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

    [BoxGroup("UI")]
    [Tooltip("The Worlspace UI GameObject")]
    [SerializeField]
    GameObject worldSpaceUI;

    [HideInInspector]
    public Interactable currentObject = null;

    [BoxGroup("Critter Interactions")]
    [SerializeField]
    [ReadOnly]
    internal int currentCritters = 0;

    [BoxGroup("Critter Interactions")]
    [SerializeField, Range(0, 5)]
    internal int maxCritters = 0;

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
        ToggleUI(false);
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
            if (this is not FoodContainer) ToggleUI(true);
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
        if (this is not FoodContainer) ToggleUI(false);
    }

    public virtual void SetObject(Interactable newObject)
    {
        currentObject = newObject;
    }

    public virtual void RemoveObject(PlayerController player)
    {
        currentObject = null;
    }

    protected virtual void ToggleUI(bool active)
    {
        if (worldSpaceUI != null) worldSpaceUI.SetActive(active);
    }

    internal virtual bool CanCritterInteract()
    {
        return false;
    }

    internal virtual void CritterInteract() { }
}
