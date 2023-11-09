using UnityEngine;
using Sirenix.OdinInspector;

public class FoodBowl : MonoBehaviour
{
    [BoxGroup("Food Bowl")]
    [SerializeField]
    int foodAmount;

    public void AddFood(int amount)
    {
        foodAmount += amount;
    }

    public bool HasFood()
    {
        return foodAmount > 0;
    }

    public void TakeFood()
    {
        if (HasFood()) foodAmount--;
    }
}
