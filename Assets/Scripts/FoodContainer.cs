using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.UI;

public class FoodContainer : InteractableContainer
{
    [AssetList(AutoPopulate = true, Path = "Assets/Game/Interactables/Food")]
    [InlineEditor]
    [Title("Food Item")]
    [HideLabel]
    [BoxGroup("Food")]
    [SerializeField]
    ItemData foodItem;

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

    [BoxGroup("Audio")]
    public string FMODEvent;

    float timePassed = 0;

    private void Start()
    {
        if (foodItem != null)
        {
            SetObject(foodItem);
            timePassed = respawnTime + 1;
        }
    }

    public override void SetObject(ItemData food, Interactable interactable = null)
    {
        currentObject = Instantiate(food.itemPrefab);
        currentObject.PickUp(interactionPoint);
        highlight.color = defaultHighlight;
    }

    public override void RemoveObject(PlayerController player)
    {
        if (currentObject != null)
        {
            currentObject.PickUp(player.grabPoint, true);
            FMODUnity.RuntimeManager.PlayOneShot(FMODEvent, transform.position);
            currentObject = null;
            StartCoroutine(RespawnFruit());
        }
    }

    private IEnumerator RespawnFruit()
    {
        if (respawnTime == 0) yield break;
        Debug.Log("Respawning Fruit...");
        ToggleUI(true);
        timePassed = 0;

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

    protected override void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetNearbyComponents(this.gameObject, true);
            if (timePassed < respawnTime)
            {
                ToggleUI(true);
            }
        }
    }
    protected override void OnTriggerExit(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetNearbyComponents(this.gameObject, false);
            ToggleUI(false);
        }
    }
}
