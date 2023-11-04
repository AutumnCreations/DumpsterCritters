using UnityEngine;

public class Interactable : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] Transform pickupPoint;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PickUp(Transform grabPoint)
    {
        // Disable physics on the item when picked up
        //rb.isKinematic = true; 

        Vector3 offsetFromRoot = pickupPoint.position - transform.position;
        transform.position = grabPoint.position - offsetFromRoot;

        transform.parent = grabPoint;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
    }

    public void Drop()
    {
        transform.parent = null;
        // Re-enable physics for the item to fall
        //rb.isKinematic = false; 
    }
}