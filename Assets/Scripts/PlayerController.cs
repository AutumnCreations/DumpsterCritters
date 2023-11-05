using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

public class PlayerController : MonoBehaviour
{
    [BoxGroup("Character Stats")]
    [SerializeField] float movementSpeed = 5f;
    [BoxGroup("Character Stats")]
    [Tooltip("How far can the character be from an interactable object/NPC to interact with it.")]
    [SerializeField] float interactRange = 2f;
    [BoxGroup("Character Stats")]
    [Tooltip("Likely won't need to be changed in a flat game.")]
    [SerializeField] float maxRayDistance = 500;
    [BoxGroup("Character Stats")]
    [Tooltip("Where objects will be held relative to the character.")]
    [Required]
    public Transform grabPoint;

    Interactable currentHeldItem;
    NavMeshAgent navMeshAgent;
    Player_Input playerInputActions;
    InputAction clickAction;
    InputAction moveAction;
    InputAction interactAction;
    InputAction cancelAction;
    LayerMask raycastLayerMask;
    Vector2 currentMovementInput;
    bool isClickToMove = false;

#if UNITY_EDITOR
    [BoxGroup("Debug")]
    [SerializeField] float debugRayTime = 1.0f;
    [BoxGroup("Debug")]
    [SerializeField] Color debugRayColor = Color.blue;
#endif

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        playerInputActions = new Player_Input();

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

        raycastLayerMask = ~(1 << LayerMask.NameToLayer("Structure"));
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

    void Update()
    {
        // If there's input and the player is not using click to move, keep updating the destination.
        if (!isClickToMove && currentMovementInput != Vector2.zero)
        {
            StartMoving(currentMovementInput);
        }
    }

    private void HandleInteraction()
    {
        //TODO: Need to account for closest item/npc/animal within range first
        if (currentHeldItem == null)
        {
            AttemptPickup();
        }
        //If no other interactables in range
        else
        {
            DropItem();
        }
        //If talking to NPC, petting animal, etc. should stop movement
        //StopMoving();
    }

    public void ClickToMove(Vector3 destination)
    {
        if (navMeshAgent == null) return;
        navMeshAgent.destination = destination;
        navMeshAgent.isStopped = false;
    }

    private void StartMoving(Vector2 input)
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = cameraTransform.right;
        right.y = 0;
        right.Normalize();
        Vector3 direction = (forward * input.y + right * input.x).normalized;
        Vector3 destination = transform.position + direction * movementSpeed;
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

    private void AttemptPickup()
    {
        Collider[] itemsInRange = Physics.OverlapSphere(transform.position, interactRange);
        foreach (Collider item in itemsInRange)
        {
            Interactable interactable = item.GetComponent<Interactable>();
            // Ensure interactable can be interacted with
            //if (interactable && interactable.CanInteract()) 
            if (interactable)
            {
                currentHeldItem = interactable;
                interactable.PickUp(grabPoint);
                break;
            }
        }
    }

    private void DropItem()
    {
        if (currentHeldItem != null)
        {
            currentHeldItem.Drop();
            currentHeldItem = null;
        }
    }

    void OnClickPerformed(InputAction.CallbackContext context)
    {
        Debug.Log($"Click Action Performed: {context.phase}");
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
                if (interactable != null && Vector3.Distance(hit.point, transform.position) <= interactRange)
                {
                    interactable.PickUp(grabPoint);
                    currentHeldItem = interactable;
                    //StopMoving();
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
}

