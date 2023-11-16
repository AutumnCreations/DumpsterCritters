using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

public class FoodBowl : InteractableContainer
{
    [BoxGroup("Food Bowl")]
    [SerializeField]
    int foodAmount;

    [BoxGroup("Food Bowl")]
    [Required]
    public Transform foodDropPoint;

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

    internal override bool CanCritterInteract()
    {
        return foodAmount > 0;
    }

    internal override float CritterInteract(float need)
    {
        int needAmount = Mathf.RoundToInt(need / 25);

        // Check how much of the need can be fulfilled
        int amountFed = Mathf.Min(needAmount, foodAmount);

        // Deduct the amount fed from the food bowl
        foodAmount -= amountFed;
        currentRationCount.text = foodAmount.ToString();

        // Return the actual amount fed
        return amountFed * 25;
    }
}
