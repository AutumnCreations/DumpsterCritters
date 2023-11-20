using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem.EnhancedTouch;

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
    float foodRespawn = 10;

    [BoxGroup("UI")]
    [SerializeField]
    Image fillTimer;

    [BoxGroup("Ghost Bucks")]
    [SerializeField]
    [Range(0, 1)]
    float ghostBuckDropChance = 0.1f;

    [BoxGroup("Ghost Bucks")]
    [SerializeField]
    GameObject ghostBuckPrefab;

    [BoxGroup("Audio")]
    public string PickupEvent;
    [BoxGroup("Audio")]
    public string RespawnEvent;

    float timePassed = 0;

    private void Start()
    {
        if (foodItem != null)
        {
            SetObject(foodItem);
            timePassed = foodRespawn + 1;
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
            if (ShouldDropGhostBuck())
            {
                DropGhostBuck();
            }
            currentObject.PickUp(player.grabPoint, true);
            FMODUnity.RuntimeManager.PlayOneShot(PickupEvent, transform.position);
            currentObject = null;
            StartCoroutine(RespawnFood(foodRespawn));
        }
    }

    protected virtual IEnumerator RespawnFood(float time)
    {
        if (time == 0) yield break;
        Debug.Log("Respawning Fruit...");
        ToggleUI(true);
        timePassed = 0;

        while (timePassed < time)
        {
            timePassed += Time.deltaTime;
            UpdateFillBar((time - timePassed) / time);
            yield return null;
        }
        ToggleUI(false);
        SetObject(foodItem);
        FMODUnity.RuntimeManager.PlayOneShot(RespawnEvent, transform.position);
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
            if (timePassed < foodRespawn)
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

    private bool ShouldDropGhostBuck()
    {
        return Random.value < ghostBuckDropChance;
    }

    private void DropGhostBuck()
    {
        if (ghostBuckPrefab != null)
        {
            Instantiate(ghostBuckPrefab, transform.position, Quaternion.identity);
        }
    }
}
