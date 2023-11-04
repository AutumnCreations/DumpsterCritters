using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] float maxRayDistance = 500;
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float pickupRange = 2f;
    public Transform grabPoint;
    
    Interactable currentHeldItem;
    NavMeshAgent navMeshAgent;
    Player_Input playerInputActions;
    InputAction clickAction;
    InputAction moveAction;
    LayerMask raycastLayerMask;
    Vector2 currentMovementInput;

#if UNITY_EDITOR
    [SerializeField] float debugRayTime = 1.0f;
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
        moveAction.performed += ctx => currentMovementInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => currentMovementInput = Vector2.zero;
        moveAction.Enable();

        raycastLayerMask = ~(1 << LayerMask.NameToLayer("Structure"));
    }

    void OnDestroy()
    {
        clickAction.performed -= OnClickPerformed;
        clickAction.Disable();
        moveAction.performed -= ctx => currentMovementInput = ctx.ReadValue<Vector2>();
        moveAction.canceled -= ctx => currentMovementInput = Vector2.zero;
        moveAction.Disable();
    }

    void Update()
    {
        HandleInteraction();

        if (currentMovementInput != Vector2.zero)
        {
            // WASD/Joystick movement
            Vector3 moveDirection = new Vector3(currentMovementInput.x, 0, currentMovementInput.y);
            navMeshAgent.velocity = moveDirection * movementSpeed;
        }
        else if (navMeshAgent.hasPath)
        {
            // NavMesh Movement
            navMeshAgent.velocity = Vector3.Lerp(navMeshAgent.velocity, Vector3.zero, Time.deltaTime * 5f);
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                navMeshAgent.ResetPath();
            }
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

    //private void HandleMovement()
    //{
    //    if (Mouse.current.leftButton.wasPressedThisFrame)
    //    {
    //        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
    //        RaycastHit hit;

    //        if (Physics.Raycast(ray, out hit))
    //        {
    //            agent.SetDestination(hit.point);
    //        }
    //    }
    //    // Add more input methods here for mobile taps or joystick input
    //}

    private void AttemptPickup()
    {
        Collider[] itemsInRange = Physics.OverlapSphere(transform.position, pickupRange);
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
                MoveTo(hit.point);
            }
        }
    }

    public void MoveTo(Vector3 destination)
    {
        if (navMeshAgent == null) return;
        navMeshAgent.destination = destination;
        navMeshAgent.isStopped = false;
    }
}

