using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class Interactable : MonoBehaviour
{
    [BoxGroup("Food")]
    [Tooltip("If food, will destroy and increase food count instead of being picked up.")]
    public bool isFood = false;

    [ShowIf("isFood")]
    [BoxGroup("Food")]
    [Tooltip("How many rations does this fill?")]
    public int rationCount = 0;

    [HideIf("isFood")]
    [BoxGroup("Interactable Details")]
    [Tooltip("Where item will be held from if it can be held.")]
    [SerializeField]
    Transform pickupPoint;

    [HideIf("isFood")]
    [BoxGroup("Interactable Details")]
    [Tooltip("How fast should the item fall to the ground/target?")]
    [SerializeField]
    float dropSpeed = .5f;

    [HideIf("isFood")]
    [BoxGroup("Interactable Details")]
    [Tooltip("How close should the item get to the ground/target?")]
    [SerializeField]
    float dropThreshold = .01f;

    float dropPoint;
    List<Collider> colliders;
    bool isDropping = false;
    NavMeshObstacle obstacle;


    void Awake()
    {
        InitializeInteractable();
    }

    public void InitializeInteractable()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        Collider[] collidersArray = GetComponentsInChildren<Collider>();
        colliders = collidersArray.Where(collider => collider.enabled).ToList();
        if (pickupPoint == null)
        {
            pickupPoint = transform;
        }
        dropPoint = transform.position.y;
    }

    private void Update()
    {
        if (isDropping)
        {
            Vector3 target = new Vector3(transform.position.x, dropPoint, transform.position.z);
            float dropStep = dropSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, dropStep);
            if (Vector3.Distance(transform.position, target) < dropThreshold) isDropping = false;
        }
    }

    public void PickUp(Transform grabPoint)
    {
        isDropping = false;
        // Disable collider and NavMeshObstacle on the item when picked up
        obstacle.enabled = false;
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        Vector3 offsetFromRoot = pickupPoint.position - transform.position;
        transform.position = grabPoint.position - offsetFromRoot;

        transform.parent = grabPoint;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
    }

    public void Drop()
    {
        transform.parent = null;
        isDropping = true;

        // Re-enable all colliders and NavMeshObstacle on the item when dropped
        obstacle.enabled = true;
        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetNearbyComponents(this.gameObject, true);
        }
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetNearbyComponents(this.gameObject, false);
        }
    }
}