using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.AI;
using TMPro;
using UnityEngine.UI;

public class Placemat : InteractableContainer
{
    NavMeshObstacle obstacle;
    ToyAnimation toyAnimation;

    [BoxGroup("Placemat")]
    public bool unlocked;

    [BoxGroup("Placemat")]
    [SerializeField]
    SpriteRenderer lockedSprite;

    [BoxGroup("Placemat")]
    [SerializeField]
    SpriteRenderer unlockedSprite;

    [BoxGroup("Placemat")]
    public Image repairIcon;

    [BoxGroup("Placemat")]
    public TextMeshProUGUI unlockText;


    [BoxGroup("Placemat")]
    [SerializeField]
    TextMeshProUGUI critterCount;

    [BoxGroup("Placemat")]
    public int requiredCritters = 0;

    [BoxGroup("Placemat")]
    public float groupInteractionDuration = 10f;

    [BoxGroup("Placemat")]
    [SerializeField]
    ParticleSystem repairVFX;

    protected override void Awake()
    {
        base.Awake();
        unlockText.gameObject.SetActive(false);
        obstacle = GetComponent<NavMeshObstacle>();
        obstacle.enabled = false;
        repairVFX.gameObject.SetActive(false);

        lockedSprite.gameObject.SetActive(!unlocked);
        unlockedSprite.gameObject.SetActive(unlocked);
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
        if (unlocked)
        {
            return currentObject != null;
        }
        else return unlocked;
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

    public void UnlockPlacemat()
    {
        unlocked = true;
        lockedSprite.gameObject.SetActive(false);
        unlockedSprite.gameObject.SetActive(true);
        repairIcon.gameObject.SetActive(false);
        highlight.color = actionHighlight;
        critterCount.text = $"{currentCritters} / {maxCritters}";
        repairVFX.gameObject.SetActive(true);
        unlockText.gameObject.SetActive(false);
        FMODUnity.RuntimeManager.PlayOneShot("event:/Env/Mat_RepairSuccess");
    }

    protected override void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetNearbyComponents(this.gameObject, true);
            if (currentObject == null && unlocked)
            {
                highlight.color = actionHighlight;
                critterCount.text = $"{currentCritters} / {maxCritters}";
                repairIcon.gameObject.SetActive(false);
                ToggleUI(true);
            }
            else if (!unlocked)
            {
                //UI To show locked state and critter requirement
                critterCount.text = $"{Mathf.Min(GameStateManager.Instance.critterCount, requiredCritters)} / {requiredCritters}";
                repairIcon.gameObject.SetActive(true);
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
            highlight.color = defaultHighlight;
            ToggleUI(false);
        }
    }
}
