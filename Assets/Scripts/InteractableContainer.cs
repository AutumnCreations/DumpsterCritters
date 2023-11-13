using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.AI;

public class InteractableContainer : MonoBehaviour
{
    [BoxGroup("Configuration")]
    [SerializeField]
    protected Transform interactionPoint;
    [BoxGroup("Configuration")]
    [SerializeField]
    protected SpriteRenderer highlight;
    [BoxGroup("Colors")]
    [SerializeField]
    protected Color defaultHighlight = Color.white;
    [BoxGroup("Colors")]
    [SerializeField]
    protected Color actionHighlight = Color.cyan;

    [HideInInspector]
    public Interactable currentObject = null;


    protected virtual void Awake()
    {
        if (highlight == null)
        {
            highlight = GetComponentInChildren<SpriteRenderer>();
        }
        if (interactionPoint == null)
        {
            interactionPoint = transform;
        }

        highlight.color = defaultHighlight;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetNearbyComponents(this.gameObject, true);
            if (currentObject == null && (this is not FoodContainer && this is not FoodBowl))
            {
                highlight.color = actionHighlight;
            }
        }
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetNearbyComponents(this.gameObject, false);
            highlight.color = defaultHighlight;
        }
    }

    public virtual void SetObject(Interactable newObject)
    {
        currentObject = newObject;
    }

    public virtual void RemoveObject(PlayerController player)
    {
        currentObject = null;
    }
}
