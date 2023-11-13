using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

public class FoodBowl : InteractableContainer
{
    [BoxGroup("Food Bowl")]
    [SerializeField]
    int foodAmount;

    [BoxGroup("Food Bowl")]
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

    public bool HasFood()
    {
        return foodAmount > 0;
    }

    public void TakeFood()
    {
        if (HasFood()) foodAmount--;
        currentRationCount.text = foodAmount.ToString();
    }
}
