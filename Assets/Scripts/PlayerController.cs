using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System;

public class PlayerController : MonoBehaviour
{
    [BoxGroup("Movement")]
    [SerializeField]
    [Tooltip("Set at initialization.")]
    [Range(.5f, 20f)]
    float movementSpeed = 5f;

    [BoxGroup("Movement")]
    [SerializeField]
    [Tooltip("Target distance from player when using input movement.")]
    [ValidateInput("DistanceCheck", "Stopping distance must be less than Target Distance from Player.")]
    [Range(.25f, 5f)]
    float distanceFromPlayer = 1f;

    [BoxGroup("Movement")]
    [SerializeField]
    [Tooltip("Set at initialization. Distance from target that player will stop.")]
    [ValidateInput("DistanceCheck", "Stopping distance must be less than Target Distance from Player.")]
    [Range(0f, 2f)]
    float stoppingDistance = .5f;

    [BoxGroup("Movement")]
    [Tooltip("Likely won't need to be changed in a flat game.")]
    [SerializeField] float maxRayDistance = 500;

    [BoxGroup("Interactions")]
    [Tooltip("How far can the character be from an interactable object/NPC to interact with it.")]
    [SerializeField] float interactRange = 2f;

    [BoxGroup("Interactions")]
    [Tooltip("Where objects will be held relative to the character.")]
    [Required]
    public Transform grabPoint;

    [BoxGroup("Interactions")]
    [Required]
    public GameObject pickupIcon;

    [HideInInspector]
    public InteractableContainer nearbyContainer = null;

    [HideInInspector]
    public Interactable nearbyInteractable = null;

    bool isClickToMove = false;
    PlayerInventory inventory;
    Interactable currentHeldItem;
    NavMeshAgent navMeshAgent;

    Vector2 currentMovementInput;
    LayerMask raycastLayerMask;
    Player_Input playerInputActions;
    InputAction clickAction;

    InputAction moveAction;
    InputAction interactAction;
    InputAction cancelAction;

    #region Debug Variables
#if UNITY_EDITOR
    [BoxGroup("Debug")]
    [SerializeField] float debugRayTime = 1.0f;
    [BoxGroup("Debug")]
    [SerializeField] Color debugRayColor = Color.blue;
#endif
    #endregion

    #region Inspector Validation
    bool DistanceCheck()
    {
        return distanceFromPlayer > stoppingDistance;
    }
    #endregion

    #region Assignment
    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        playerInputActions = new Player_Input();
        inventory = GetComponent<PlayerInventory>();

        // Setup Click to Move/Interact
        clickAction = playerInputActions.Player.MoveInteract;
        clickAction.performed += OnClickPerformed;
        clickAction.Enable();

        // Setup Movement
        moveAction = playerInputActions.Player.Move;
        moveAction.performed += OnMovementInput;
        moveAction.canceled += OnMovementStop;
        moveAction.Enable();

        // Setup Interact
        interactAction = playerInputActions.Player.Interact;
        interactAction.performed += OnInteract;
        interactAction.Enable();

        // Setup Cancel
        cancelAction = playerInputActions.Player.Cancel;
        cancelAction.performed += OnCancel;
        cancelAction.Enable();

        raycastLayerMask = ~(1 << LayerMask.NameToLayer("Player"));
    }

    void OnDestroy()
    {
        clickAction.performed -= OnClickPerformed;
        clickAction.Disable();

        moveAction.performed -= OnMovementInput;
        moveAction.canceled -= OnMovementStop;
        moveAction.Disable();

        interactAction.performed -= OnInteract;
        interactAction.Disable();

        cancelAction.performed -= OnCancel;
        cancelAction.Disable();
    }

    void Start()
    {
        navMeshAgent.speed = movementSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;
        pickupIcon.SetActive(false);
    }
    #endregion

    #region Movement
    public void ClickToMove(Vector3 destination)
    {
        navMeshAgent.destination = destination;
        navMeshAgent.isStopped = false;
    }

    private void StartMoving(Vector2 input)
    {
        if (navMeshAgent == null) return;
        Transform cameraTransform = Camera.main.transform;

        Vector3 cameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

        Vector3 direction = (cameraRight * input.x + cameraForward * input.y).normalized;

        Vector3 destination = transform.position + direction * distanceFromPlayer;
        navMeshAgent.SetDestination(destination);
    }

    private void StopMoving()
    {
        if (navMeshAgent.hasPath)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }
    }

    #endregion

    #region Interactions
    private void HandleInteraction()
    {
        //TODO: Need to account for closest item/npc/animal within range first
        if (currentHeldItem == null)
        {
            if (nearbyInteractable != null) AttemptPickup();
            else if (nearbyContainer != null && nearbyContainer.currentObject != null)
            {
                currentHeldItem = nearbyContainer.currentObject;
                nearbyContainer.RemoveObject(this);
                pickupIcon.SetActive(false);
            }
        }
        else if (nearbyContainer != null && nearbyContainer.currentObject == null && nearbyContainer is not FoodContainer)
        {
            pickupIcon.SetActive(true);
            nearbyContainer.SetObject(currentHeldItem);
            currentHeldItem = null;
            StopMoving();
        }
        else
        {
            DropItem();
        }
        //If talking to NPC, petting animal, etc. should stop movement
    }

    private void AttemptPickup()
    {
        if (nearbyInteractable.isFood)
        {
            inventory.FoodRations += nearbyInteractable.rationCount;
            Debug.Log(inventory.FoodRations);
            Destroy(nearbyInteractable.gameObject);
            nearbyInteractable = null;
        }
        else
        {
            currentHeldItem = nearbyInteractable;
            nearbyInteractable = null;
            currentHeldItem.PickUp(grabPoint);
        }
        pickupIcon.SetActive(false);
    }

    private void DropItem()
    {
        if (currentHeldItem != null)
        {
            currentHeldItem.Drop();
            currentHeldItem = null;
        }
    }

    public void PickupFood(Interactable foodItem)
    {
        inventory.FoodRations += foodItem.rationCount;
        Debug.Log(inventory.FoodRations);
    }

    internal void SetNearbyComponents(GameObject component, bool active)
    {
        Interactable interactable = component.GetComponent<Interactable>();
        InteractableContainer interactableContainer = component.GetComponent<InteractableContainer>();

        if (interactable != null)
        {
            if (active)
            {
                nearbyInteractable = interactable;
                if (currentHeldItem == null) pickupIcon.SetActive(active);
            }
            else
            {
                nearbyInteractable = nearbyInteractable == interactable ? null : nearbyInteractable;
                pickupIcon.SetActive(active);
            }
        }
        else if (interactableContainer != null)
        {
            if (active)
            {
                nearbyContainer = interactableContainer;
                if (currentHeldItem == null && interactableContainer.currentObject != null) pickupIcon.SetActive(active);
            }
            else
            {
                nearbyContainer = nearbyContainer == interactableContainer ? null : nearbyContainer;
                pickupIcon.SetActive(active);
            }
        }

    }

    #endregion

    #region Events
    void OnClickPerformed(InputAction.CallbackContext context)
    {
        //Debug.Log($"Click Action Performed: {context.phase}");
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxRayDistance, raycastLayerMask))
            {
#if UNITY_EDITOR
                Debug.DrawLine(ray.origin, hit.point, debugRayColor, debugRayTime);
#endif
                Interactable interactable = hit.collider.GetComponent<Interactable>();
                InteractableContainer interactableContainer = hit.collider.GetComponent<InteractableContainer>();
                if (interactable != null || interactableContainer != null)
                {
                    Debug.Log("Hit Something");
                    if ((interactable != null && interactable == nearbyInteractable) ||
                        (interactableContainer != null && interactableContainer == nearbyContainer))
                    {
                        Debug.Log($"Hit Nearby");
                        HandleInteraction();
                    }
                    else
                    {
                        Debug.Log("Hit Interactable");
                        Vector3 closestPoint = hit.collider.ClosestPoint(hit.point);
                        ClickToMove(closestPoint);
                    }
                }
                else
                {
                    isClickToMove = true;
                    ClickToMove(hit.point);
                }
            }
        }
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        isClickToMove = false;
        StartMoving(currentMovementInput);
    }

    private void OnMovementStop(InputAction.CallbackContext context)
    {
        currentMovementInput = Vector2.zero;
        StopMoving();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        HandleInteraction();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        // Should first check for context, for now just drop item
        DropItem();
    }
    #endregion

    void Update()
    {
        if (nearbyInteractable != null) Debug.Log(nearbyInteractable);
        // If there's input and the player is not using click to move, keep updating the destination.
        if (!isClickToMove && currentMovementInput != Vector2.zero)
        {
            StartMoving(currentMovementInput);
        }
    }
}

