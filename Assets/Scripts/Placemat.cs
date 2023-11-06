using UnityEngine;
using Sirenix.OdinInspector;

public class Placemat : MonoBehaviour
{
    [BoxGroup("Placemat Configuration")]
    [SerializeField]
    Transform placementPoint;
    [BoxGroup("Placemat Configuration")]
    [SerializeField]
    SpriteRenderer highlight;
    [BoxGroup("Colors")]
    [SerializeField]
    Color defaultHighlight = Color.white;
    [BoxGroup("Colors")]
    [SerializeField]
    Color actionHighlight = Color.cyan;

    [HideInInspector]
    public Interactable currentObject = null;

    private void Awake()
    {
        if (highlight == null)
        {
            highlight = GetComponentInChildren<SpriteRenderer>();
        }
        if (placementPoint == null)
        {
            placementPoint = gameObject.transform;
        }

        highlight.color = defaultHighlight;
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        //Debug.Log($"Triggered by {other.gameObject.name}");
        if (player != null)
        {
            player.nearbyPlacemat = this;
            if (currentObject == null)
            {
                highlight.color = actionHighlight;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            highlight.color = defaultHighlight;
            player.nearbyPlacemat = null;
        }
    }

    public void SetObject(Interactable newObject)
    {
        currentObject = newObject;
        currentObject.PickUp(placementPoint);
        highlight.color = defaultHighlight;
    }

    public void RemoveObject(Transform grabPoint)
    {
        currentObject.PickUp(grabPoint);
        currentObject = null;
        highlight.color = actionHighlight;
    }
}
