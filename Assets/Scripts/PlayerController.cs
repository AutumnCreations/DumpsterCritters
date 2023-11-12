using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [BoxGroup("Settings")]
    [SerializeField]
    [Tooltip("Usually controlled by the GameStateManager, will pause functionality")]
    bool isPaused = false;

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


    [HideInInspector]
    public NPC nearbyNPC = null;

    bool isClickToMove = false;
    PlayerInventory inventory;
    InventorySystem inventorySystem;
    Interactable currentHeldItem;
    NavMeshAgent navMeshAgent;

    Vector2 currentMovementInput;
    LayerMask raycastLayerMask;
    Player_Input playerInputActions;

    InputAction clickAction;
    InputAction moveAction;
    InputAction interactAction;
    InputAction cancelAction;
    InputAction pauseAction;

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
        inventorySystem = FindObjectOfType<InventorySystem>();

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

        // Setup Pause
        pauseAction = playerInputActions.Player.Pause;
        pauseAction.performed += OnPause;
        pauseAction.Enable();

        GameStateManager.Instance.onGameStateChange += OnGameStateChange;

        raycastLayerMask = ~(1 << LayerMask.NameToLayer("Player"));
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
            else if (nearbyNPC != null)
            {
                nearbyNPC.Interact(this);
                StopMoving();
            }
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
        NPC npc = component.GetComponent<NPC>();

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
        else if (npc != null)
        {
            if (active)
            {
                nearbyNPC = npc;
            }
            else
            {
                nearbyNPC = nearbyNPC == npc ? null : nearbyNPC;
            }
        }

    }

    #endregion

    #region Events
    void OnClickPerformed(InputAction.CallbackContext context)
    {
        //Should set up a check for UI elements to click through dialogue, etc.
        if (isPaused || EventSystem.current.IsPointerOverGameObject()) return;


        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxRayDistance, raycastLayerMask))
            {
                Debug.Log("Hit layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
#if UNITY_EDITOR
                Debug.DrawLine(ray.origin, hit.point, debugRayColor, debugRayTime);
#endif
                Interactable interactable = hit.collider.GetComponent<Interactable>();
                InteractableContainer interactableContainer = hit.collider.GetComponent<InteractableContainer>();
                NPC npc = hit.collider.GetComponent<NPC>();
                if (interactable != null || interactableContainer != null || npc != null)
                {
                    Debug.Log("Hit Something");
                    if ((interactable != null && interactable == nearbyInteractable) ||
                        (interactableContainer != null && interactableContainer == nearbyContainer) ||
                        (npc != null && npc == nearbyNPC))
                    {
                        Debug.Log($"Hit Nearby");
                        HandleInteraction();
                    }
                    else if (npc != null)
                    {
                        // Calculate direction from player to NPC
                        Vector3 directionToNPC = hit.collider.transform.position - transform.position;
                        // Find a point slightly in front of the NPC, along the direction vector
                        Vector3 pointNearNPC = hit.collider.transform.position - directionToNPC.normalized * stoppingDistance;
                        // Now get the closest point on the NPC's collider from this point
                        Vector3 closestPoint = hit.collider.ClosestPoint(pointNearNPC);
                        Debug.Log("Moving to closest point near NPC");
                        ClickToMove(closestPoint);
                    }
                    else
                    {
                        Debug.Log("Hit something too far");
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
        if (isPaused) return;
        currentMovementInput = context.ReadValue<Vector2>();
        isClickToMove = false;
        StartMoving(currentMovementInput);
    }

    private void OnMovementStop(InputAction.CallbackContext context)
    {
        if (isPaused) return;
        currentMovementInput = Vector2.zero;
        StopMoving();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        //Should set up a check for UI elements to click through dialogue, etc.
        if (isPaused) return;
        HandleInteraction();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        // Should be able to cancel out of dialogue, shop, etc.
        if (!isPaused) return;
        DropItem();
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        inventorySystem.ToggleInventory();
    }

    private void OnGameStateChange(GameStateManager.GameState newState)
    {
        switch (newState)
        {
            case GameStateManager.GameState.Paused:
                isPaused = true;
                navMeshAgent.isStopped = true;
                break;
            case GameStateManager.GameState.Playing:
                isPaused = false;
                navMeshAgent.isStopped = false;
                break;
            case GameStateManager.GameState.Dialogue:
                isPaused = true;
                navMeshAgent.isStopped = true;
                break;
        }
    }

    private void OnDisable()
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

        pauseAction.performed -= OnPause;
        pauseAction.Disable();

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.onGameStateChange -= OnGameStateChange;
        }
    }

    #endregion

    #region Debug
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            Gizmos.color = debugRayColor;
            Gizmos.DrawWireSphere(new Vector3(navMeshAgent.destination.x, 
                navMeshAgent.destination.y + .25f, navMeshAgent.destination.z), .25f);
        }
#endif
    }
    #endregion

    void Update()
    {
        if (isPaused) return;
        if (nearbyInteractable != null) Debug.Log(nearbyInteractable);
        // If there's input and the player is not using click to move, keep updating the destination.
        if (!isClickToMove && currentMovementInput != Vector2.zero)
        {
            StartMoving(currentMovementInput);
        }
    }
}

