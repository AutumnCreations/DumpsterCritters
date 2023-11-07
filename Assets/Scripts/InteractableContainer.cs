using UnityEngine;
using Sirenix.OdinInspector;

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
            player.nearbyContainer = this;
            if (currentObject == null)
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
            highlight.color = defaultHighlight;
            player.nearbyContainer = null;
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
