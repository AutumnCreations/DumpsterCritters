using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.AI;

public class Placemat : InteractableContainer
{
    NavMeshObstacle obstacle;

    protected override void Awake()
    {
        base.Awake();
        obstacle = GetComponent<NavMeshObstacle>();
        obstacle.enabled = false;
    }

    public override void SetObject(Interactable newObject)
    {
        currentObject = newObject;
        currentObject.PickUp(interactionPoint);
        highlight.color = defaultHighlight;
        obstacle.enabled = true;
    }

    public override void RemoveObject(PlayerController player)
    {
        if (currentObject != null)
        {
            obstacle.enabled = false;
            currentObject.PickUp(player.grabPoint);
            currentObject = null;
            highlight.color = actionHighlight;
        }
    }

    internal override bool CanCritterInteract()
    {
        return currentObject != null;
    }
}
