using UnityEngine;
using Sirenix.OdinInspector;

public class FoodContainer : InteractableContainer
{
    [BoxGroup("Food")]
    [SerializeField]
    Interactable foodItem;

    [BoxGroup("Food")]
    [SerializeField]
    int quantity = 1;

    [BoxGroup("Interactables")]
    [SerializeField]
    bool canStoreItems = false;

    [BoxGroup("Audio")]
    public string PickupEvent;

    private void Start()
    {
        if (foodItem != null)
        {
            SetObject(foodItem);
        }
    }

    public override void SetObject(Interactable newObject)
    {
        currentObject = Instantiate(newObject);
        currentObject.PickUp(interactionPoint);
        highlight.color = defaultHighlight;
    }

    public override void RemoveObject(PlayerController player)
    {
        if (currentObject != null)
        {
            player.PickupFood(foodItem);
            Destroy(currentObject.gameObject);
            currentObject = null;
            highlight.color = canStoreItems ? actionHighlight : defaultHighlight;
            FMODUnity.RuntimeManager.PlayOneShot(PickupEvent, transform.position);
        }
    }
}
