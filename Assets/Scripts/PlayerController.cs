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
    public Transform grabPoint;

    Interactable currentHeldItem;
    NavMeshAgent navMeshAgent;
    Player_Input playerInputActions;
    InputAction clickAction;
    InputAction moveAction;
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

        raycastLayerMask = ~(1 << LayerMask.NameToLayer("Structure"));
    }

    void OnDestroy()
    {
        clickAction.performed -= OnClickPerformed;
        clickAction.Disable();
        moveAction.performed -= OnMovementInput;
        moveAction.canceled -= OnMovementStop;
        moveAction.Disable();
    }

    void Update()
    {
        HandleInteraction();
        // If there's input and the player is not using click to move, keep updating the destination.
        if (!isClickToMove && currentMovementInput != Vector2.zero)
        {
            StartMoving(currentMovementInput);
        }
    }

    private void HandleInteraction()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentHeldItem != null)
            {
                DropItem();
            }
            else
            {
                AttemptPickup();
            }
        }
        // Add more interaction handling here for other objects like NPCs
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
        currentHeldItem.Drop();
        currentHeldItem = null;
    }

    void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxRayDistance, raycastLayerMask))
            {
#if UNITY_EDITOR
                Debug.DrawLine(ray.origin, hit.point, debugRayColor, debugRayTime);
#endif
                isClickToMove = true;
                ClickToMove(hit.point);
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
}

