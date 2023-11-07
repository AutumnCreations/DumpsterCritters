using UnityEngine;
using Sirenix.OdinInspector;
public class Placemat : InteractableContainer
{
    public override void SetObject(Interactable newObject)
    {
        currentObject = newObject;
        currentObject.PickUp(interactionPoint);
        highlight.color = defaultHighlight;
    }

    public override void RemoveObject(PlayerController player)
    {
        if (currentObject != null)
        {
            currentObject.PickUp(player.grabPoint);
            currentObject = null;
            highlight.color = actionHighlight;
        }
    }
}
