using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

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


    List<Collider> colliders;

    void Awake()
    {
        Collider[] collidersArray = GetComponentsInChildren<Collider>();
        colliders = collidersArray.Where(collider => collider.enabled).ToList();
        if (pickupPoint == null)
        {
            pickupPoint = transform;
        }
    }

    public void PickUp(Transform grabPoint)
    {
        // Disable collider on the item when picked up
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

        // Re-enable all colliders
        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }
    }
}