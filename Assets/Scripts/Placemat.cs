using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.AI;

public class Placemat : InteractableContainer
{
    NavMeshObstacle obstacle;
    ToyAnimation toyAnimation;

    protected override void Awake()
    {
        base.Awake();
        obstacle = GetComponent<NavMeshObstacle>();
        obstacle.enabled = false;
    }

    public override void SetObject(ItemData newObject, Interactable interactable)
    {
        currentObject = interactable;
        currentObject.PickUp(interactionPoint, true);
        highlight.color = defaultHighlight;
        obstacle.enabled = true;
    }

    public override void RemoveObject(PlayerController player)
    {
        if (currentObject != null)
        {
            obstacle.enabled = false;
            currentObject.PickUp(player.grabPoint, true);
            currentObject = null;
            highlight.color = actionHighlight;
        }
    }

    internal override bool CanCritterInteract()
    {
        return currentObject != null;
    }

    internal override float CritterInteract(float need)
    {
        if (currentObject == null) return 0;
        int needAmount = Mathf.RoundToInt(need / 25);

        return Mathf.Min(needAmount, currentObject.itemData.entertainmentValue) * 25f;
    }

    public override void CritterCountChange(int critters)
    {
        int previousCritters = currentCritters;
        base.CritterCountChange(critters);
        if (currentObject != null)
        {
            toyAnimation = currentObject.GetComponent<ToyAnimation>();
        }
        if (currentCritters <= 0)
        {
            currentCritters = 0;
            toyAnimation.Stop();
        }
        else if (currentCritters == 1 && previousCritters == 0)
        {
            toyAnimation.RollnJump();
        }
    }
}
