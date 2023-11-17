using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.AI;
using DG.Tweening;
using UnityEditor;

public class Interactable : MonoBehaviour
{
    [BoxGroup("Interactable Details")]
    [SerializeField]
    internal ItemData itemData;

    [BoxGroup("Interactable Details")]
    [Tooltip("Where item will be held from if it can be held.")]
    [SerializeField]
    Transform pickupPoint;

    [BoxGroup("Interactable Details")]
    [Tooltip("How fast should the item fall to the ground/target?")]
    [SerializeField]
    float dropSpeed = .5f;

    [BoxGroup("Interactable Details")]
    [Tooltip("How close should the item get to the ground/target?")]
    [SerializeField]
    float dropThreshold = .01f;


    List<Collider> colliders;
    bool isDropped = false;
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
        itemData.itemName = itemData.itemName == "" ? gameObject.name : itemData.itemName;
    }

    public void PickUp(Transform settlePoint, bool animateMove = false)
    {
        isDropped = false;
        // Disable collider and NavMeshObstacle on the item when picked up
        if (obstacle != null) obstacle.enabled = false;

        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
        if (!animateMove)
        {
            Vector3 offsetFromRoot = pickupPoint.position - transform.position;
            transform.position = settlePoint.position - offsetFromRoot;
        }

        transform.DOKill();

        Vector3 worldScale = transform.lossyScale;
        transform.SetParent(settlePoint, true);

        // Calculate the new local scale     
        Vector3 newLocalScale = new(
            worldScale.x / settlePoint.lossyScale.x,
            worldScale.y / settlePoint.lossyScale.y,
            worldScale.z / settlePoint.lossyScale.z);
        transform.localScale = newLocalScale;

        transform.localRotation = Quaternion.identity;
        if (animateMove)
        {
            transform.DOLocalMove(Vector3.zero, .5f).SetEase(Ease.InBack);
        }
        else
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(newLocalScale, 1f).SetEase(Ease.OutCirc);
        }
    }

    public void Drop()
    {
        transform.parent = null;
        isDropped = true;

        // Re-enable all colliders and NavMeshObstacle on the item when dropped
        obstacle.enabled = true;
        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }
        transform.DOMove(new (transform.position.x, 0, transform.position.z), .5f).SetEase(Ease.OutBounce);
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