using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

public class FoodBowl : InteractableContainer
{
    [BoxGroup("Food Bowl")]
    [SerializeField]
    int foodAmount;

    [BoxGroup("UI")]
    [SerializeField]
    TextMeshProUGUI currentRationCount;

    private void Start()
    {
        currentRationCount.text = foodAmount.ToString();
    }

    public void AddFood(int amount)
    {
        foodAmount += amount;
        currentRationCount.text = foodAmount.ToString();
    }

    //public void TakeFood()
    //{
    //    if (CanCritterInteract()) foodAmount--;
    //    currentRationCount.text = foodAmount.ToString();
    //}

    internal override bool CanCritterInteract()
    {
        return foodAmount > 0;
    }

    internal override void CritterInteract()
    {
        foodAmount--;
        currentRationCount.text = foodAmount.ToString();
    }
}
