using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interactable : MonoBehaviour
{

    [SerializeField] Transform pickupPoint;

    List<Collider> colliders;

    void Awake()
    {
        Collider[] collidersArray = GetComponentsInChildren<Collider>();
        colliders = collidersArray.Where(collider => collider.enabled).ToList();
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