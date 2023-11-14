using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.UI;

public class FoodContainer : InteractableContainer
{
    [BoxGroup("Food")]
    [SerializeField]
    Interactable foodItem;

    [BoxGroup("Food")]
    [SerializeField]
    int quantity = 1;

    [BoxGroup("Food")]
    [Tooltip("How long does it take for the food to respawn in seconds? 0 = No Respawn")]
    [Range(0, 120)]
    [SerializeField]
    float respawnTime = 10;

    [BoxGroup("UI")]
    [SerializeField]
    Image fillTimer;

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
            currentObject.PickUp(player.grabPoint);
            currentObject = null;
            StartCoroutine(RespawnFruit());
            //player.PickupFood(foodItem);
            //Destroy(currentObject.gameObject);
            //currentObject = null;
        }
    }

    private IEnumerator RespawnFruit()
    {
        Debug.Log("Respawning Fruit...");
        if (respawnTime == 0) yield break;
        ToggleUI(true);
        float timePassed = 0;

        while (timePassed < respawnTime)
        {
            timePassed += Time.deltaTime;
            UpdateFillBar((respawnTime - timePassed) / respawnTime);
            yield return null;
        }
        ToggleUI(false);
        SetObject(foodItem);
        Debug.Log("Fruit Respawned!");
    }

    private void UpdateFillBar(float amount)
    {
        fillTimer.fillAmount = amount;
    }
}
