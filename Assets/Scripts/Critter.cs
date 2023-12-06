using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Critter : MonoBehaviour
{
    public enum CritterState
    {
        Idle,
        Roaming,
        SeekingFood,
        Eating,
        SeekingStimulation,
        Playing,
        GroupInteract
    }

    [BoxGroup("Settings")]
    [Tooltip("Toggle to use integers instead of floats for stats")]
    [SerializeField]
    bool useIntegers = true;

    [BoxGroup("Settings")]
    [Tooltip("Usually controlled by the GameStateManager, will pause functionality")]
    [SerializeField]
    bool isPaused = false;

    [BoxGroup("Stats")]
    [GUIColor("orange")]
    [Tooltip("Current critter hunger level. Lower is good, higher is bad")]
    [SerializeField, Range(0, 100)]
    float hunger = 0f;

    [BoxGroup("Stats")]
    [GUIColor("orange")]
    [Tooltip("How hungry the critter needs to be before seeking food. Think of this as a percentage out of 100")]
    [SerializeField, Range(0, 100)]
    float needFood = 60f;

    [BoxGroup("Stats")]
    [GUIColor("orange")]
    [Tooltip("How fast the critter's hunger increases in seconds")]
    [SerializeField, Range(0, 20)]
    float hungerIncreaseRate = 1f;

    [BoxGroup("Stats")]
    [GUIColor("cyan")]
    [Tooltip("Current critter mood level. Lower is bad, higher is good")]
    [SerializeField, Range(0, 100)]
    float mood = 100f;

    [BoxGroup("Stats")]
    [GUIColor("cyan")]
    [Tooltip("How deprived the critter needs to be before seeking attention. Think of this as a percentage out of 100")]
    [SerializeField, Range(0, 100)]
    float needAttention = 30f;

    [BoxGroup("Stats")]
    [GUIColor("cyan")]
    [Tooltip("How fast the critter's mood decreases in seconds")]
    [SerializeField, Range(0, 20)]
    float moodDecreaseRate = 1f;

    [BoxGroup("Stats")]
    [Tooltip("How often can the critter be pet?")]
    [SerializeField, Range(0, 120)]
    public float petCooldown = 20f;

    [BoxGroup("Stats")]
    [Tooltip("How often can the critter be fed?")]
    [SerializeField, Range(0, 120)]
    public float feedCooldown = 20f;

    [BoxGroup("Roaming")]
    [Tooltip("The radius within which the critter will randomly roam.")]
    [SerializeField, Range(5, 100)]
    float roamRadius = 10f;

    [BoxGroup("Roaming")]
    [Tooltip("The minimum amount of time to wait between roaming.")]
    [SerializeField, Range(0, 10)]
    float minRoamIdleTime = 2f;

    [BoxGroup("Roaming")]
    [Tooltip("The maximum amount of time to wait between roaming.")]
    [SerializeField, Range(5, 30)]
    float maxRoamIdleTime = 10f;

    [BoxGroup("Roaming")]
    [Tooltip("How long the critter will eat for in seconds")]
    [SerializeField, Range(0, 10)]
    float eatingDuration = 3f;

    [BoxGroup("Roaming")]
    [Tooltip("How long the critter will play for in seconds")]
    [SerializeField, Range(0, 10)]
    float playDuration = 5f;

    [BoxGroup("Roaming")]
    [Tooltip("How far away can the critter interact with food/placemats from")]
    [SerializeField, Range(0, 10)]
    float interactionDistance = .5f;

    [BoxGroup("Roaming")]
    [Tooltip("How far away should the critter move after interacting with something")]
    [SerializeField, Range(0, 10)]
    float moveAwayDistance = 5f;

    [BoxGroup("UI")]
    [Tooltip("The Worlspace UI GameObject")]
    [SerializeField]
    GameObject worldSpaceUI;

    [BoxGroup("UI")]
    [Tooltip("The bar that will display the critter's hunger level")]
    [SerializeField]
    Image hungerFillBar;

    [BoxGroup("UI")]
    [Tooltip("The bar that will display the critter's mood level")]
    [SerializeField]
    Image moodFillBar;

    [BoxGroup("UI")]
    [SerializeField]
    Image thoughtBubble;

    [BoxGroup("UI")]
    [SerializeField]
    Image hungryIcon;

    [BoxGroup("UI")]
    [SerializeField]
    Image playIcon;

    [BoxGroup("UI")]
    [SerializeField]
    Image upsetIcon;

    [BoxGroup("UI")]
    [SerializeField]
    float upsetTime;

    [BoxGroup("VFX")]
    [Tooltip("The particle system that will play when the critter is pet")]
    [SerializeField]
    ParticleSystem petVFX;

    [BoxGroup("VFX")]
    [Tooltip("The particle system that will play when the critter is fed")]
    [SerializeField]
    ParticleSystem fedVFX;

    [BoxGroup("VFX")]
    [Tooltip("The point where food will travel to when player feeds this critter")]
    public Transform feedPoint;

    [BoxGroup("Audio")]
    public string InteractSuccessSound;

    [BoxGroup("GhostBuck Settings")]
    [SerializeField, Tooltip("Chance of dropping GhostBuck on each check while roaming")]
    [Range(0f, 1f)]
    float ghostBuckDropChance = 0.05f; // 5% chance

    [BoxGroup("GhostBuck Settings")]
    [SerializeField, Tooltip("Hunger threshold below which GhostBucks can be dropped")]
    [Range(0, 100)]
    float hungerThresholdForGhostBuck = 20f; // <=20 hunger

    [BoxGroup("GhostBuck Settings")]
    [SerializeField, Tooltip("Mood threshold above which GhostBucks can be dropped")]
    [Range(0, 100)]
    float moodThresholdForGhostBuck = 80f; // >=80 mood

    [BoxGroup("GhostBuck Settings")]
    [SerializeField, Tooltip("GhostBuck prefab to instantiate")]
    GameObject ghostBuckPrefab;

    [ShowInInspector, ReadOnly]
    [BoxGroup("Debug")]
    public CritterState currentState;

    [ShowInInspector, ReadOnly]
    [BoxGroup("Debug")]
    InteractableContainer targetInteraction;

    [ShowInInspector, ReadOnly]
    [BoxGroup("Debug")]
    float interactionRefill = 0;

    [ReadOnly]
    [BoxGroup("Debug")]
    public float lastPet = float.MaxValue;

    [ReadOnly]
    [BoxGroup("Debug")]
    public float lastFed = float.MaxValue;

    NavMeshAgent agent;
    CritterAnimation critterAnimation;
    Material material;

    private void OnValidate()
    {
        // Convert and round all float fields to their integer counterparts
        if (useIntegers)
        {
            hunger = Mathf.RoundToInt(hunger);
            needFood = Mathf.RoundToInt(needFood);
            hungerIncreaseRate = Mathf.RoundToInt(hungerIncreaseRate);
            mood = Mathf.RoundToInt(mood);
            needAttention = Mathf.RoundToInt(needAttention);
            moodDecreaseRate = Mathf.RoundToInt(moodDecreaseRate);
            roamRadius = Mathf.RoundToInt(roamRadius);
            minRoamIdleTime = Mathf.RoundToInt(minRoamIdleTime);
            maxRoamIdleTime = Mathf.RoundToInt(maxRoamIdleTime);
            eatingDuration = Mathf.RoundToInt(eatingDuration);

            upsetTime = Mathf.RoundToInt(upsetTime);
        }
    }

    private void OnDisable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UpdateCritterCount(-1);
            GameStateManager.Instance.onGameStateChange -= OnGameStateChange;
        }
    }

    private void OnGameStateChange(GameStateManager.GameState newState)
    {
        switch (newState)
        {
            case GameStateManager.GameState.Paused:
                isPaused = true;
                agent.isStopped = true;
                break;
            case GameStateManager.GameState.Playing:
                isPaused = false;
                agent.isStopped = false;
                break;
            case GameStateManager.GameState.Dialogue:
                isPaused = true;
                agent.isStopped = true;
                break;
            case GameStateManager.GameState.Tutorial:
                isPaused = true;
                agent.isStopped = true;
                break;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        critterAnimation = GetComponent<CritterAnimation>();
        material = GetComponentInChildren<SkinnedMeshRenderer>().material;
        ToggleUI(false);
        StartCoroutine(Upset(false));
        Playful(false);
        Hungry(false);
    }

    private void Start()
    {
        // Start the roaming coroutine
        ChangeState(CritterState.Roaming);
        GameStateManager.Instance.onGameStateChange += OnGameStateChange;
        GameStateManager.Instance.UpdateCritterCount(1);
        material.SetColor("_Color", Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f));
    }

    private void Update()
    {
        if (isPaused) return;
        IncreaseInteractionTimers();
        IncreaseHunger();
        DecreaseMood();

        switch (currentState)
        {
            case CritterState.Idle:
                CheckNeeds();
                break;
            case CritterState.SeekingFood:
                SeekInteraction<FoodBowl>(CritterState.Eating, hunger);
                break;
            case CritterState.Eating:
                Eat();
                break;
            case CritterState.SeekingStimulation:
                SeekInteraction<Placemat>(CritterState.Playing, 100 - mood);
                break;
            case CritterState.Playing:
                Play();
                break;
            case CritterState.Roaming:
                CheckNeeds();
                break;
            default:
                break;
        }
    }

    private void IncreaseInteractionTimers()
    {
        lastFed += Time.deltaTime;
        lastPet += Time.deltaTime;
    }

    private void CheckNeeds()
    {
        if (hunger >= needFood)
            ChangeState(CritterState.SeekingFood);

        else if (mood <= needAttention)
            ChangeState(CritterState.SeekingStimulation);
    }
    private void IncreaseHunger()
    {
        hunger += hungerIncreaseRate * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0f, 100f);
        UpdateFillAmount((hunger - 100) * -1, hungerFillBar);
    }

    private void DecreaseMood()
    {
        mood -= moodDecreaseRate * Time.deltaTime;
        mood = Mathf.Clamp(mood, 0f, 100f);
        UpdateFillAmount(mood, moodFillBar);
    }

    private void ChangeState(CritterState newState)
    {
        // Exiting current state logic
        if (currentState == CritterState.Eating && newState != CritterState.Eating ||
            currentState == CritterState.Playing && newState != CritterState.Playing)
        {
            critterAnimation.StopWalk();
            // When done with the specific activity, evaluate the next state
            StopAllCoroutines();
            StartCoroutine(WaitAndEvaluate());
        }
        else if (currentState == CritterState.Roaming && newState != CritterState.Roaming)
        {
            critterAnimation.StopWalk();
            // Stop roaming coroutine if transitioning out of Roaming state
            StopAllCoroutines();
        }

        // Entering new state logic
        switch (newState)
        {
            case CritterState.Eating:
                StopAllCoroutines();
                Hungry(false);
                critterAnimation.StopWalk();
                StartCoroutine(WaitAndChangeState(CritterState.Idle, eatingDuration));
                break;
            case CritterState.Playing:
                StopAllCoroutines();
                Playful(false);
                critterAnimation.StopWalk();
                StartCoroutine(WaitAndChangeState(CritterState.Idle, playDuration));
                break;
            case CritterState.Idle:
                // Start idle time before beginning to roam again
                critterAnimation.StopWalk();
                StartCoroutine(StartIdleRoamingTime());
                break;
            case CritterState.Roaming:
                critterAnimation.Walk();
                StartCoroutine(Roam());
                break;
            case CritterState.SeekingFood:
                Hungry(true);
                critterAnimation.Walk();
                targetInteraction = null;
                SeekInteraction<FoodBowl>(CritterState.Eating, hunger);
                break;
            case CritterState.SeekingStimulation:
                Playful(true);
                critterAnimation.Walk();
                targetInteraction = null;
                SeekInteraction<Placemat>(CritterState.Playing, mood);
                break;
        }
        currentState = newState;
    }


    private IEnumerator StartIdleRoamingTime()
    {
        float idleTime = Random.Range(minRoamIdleTime, maxRoamIdleTime);
        yield return new WaitForSeconds(idleTime);
        if (currentState == CritterState.Idle)
        {
            ChangeState(CritterState.Roaming);
        }
    }

    public void GroupInteractWithPlacemat(Placemat placemat, float waitDuration)
    {
        ChangeState(CritterState.GroupInteract);
        StartCoroutine(GroupInteractionRoutine(placemat, waitDuration));
    }

    private IEnumerator GroupInteractionRoutine(Placemat placemat, float waitDuration)
    {
        agent.isStopped = false;
        targetInteraction = placemat;
        // Move to placemat
        agent.SetDestination(placemat.transform.position);
        while (Vector3.Distance(transform.position, placemat.transform.position) > interactionDistance)
        {
            yield return null;
        }

        // Wait at placemat for duration
        yield return new WaitForSeconds(waitDuration);
        ChangeState(CritterState.Idle); // Return to idle after waiting
    }

    private IEnumerator WaitAndEvaluate()
    {
        transform.LookAt(targetInteraction.transform.position);
        yield return new WaitForSeconds(eatingDuration);
        MoveAwayFromInteraction();
        if (hunger >= needFood) ChangeState(CritterState.SeekingFood);
        else if (mood <= needAttention) ChangeState(CritterState.SeekingStimulation);
        else ChangeState(CritterState.Idle);
    }

    private void MoveAwayFromInteraction()
    {
        if (targetInteraction == null) return;
        targetInteraction.CritterCountChange(-1);
        Vector3 directionAwayFromBowl = (transform.position - targetInteraction.transform.position).normalized;
        targetInteraction = null;
        Vector3 newDestination = transform.position + directionAwayFromBowl * moveAwayDistance;
        agent.SetDestination(newDestination);
        critterAnimation.Walk();
        agent.isStopped = false;
    }

    private IEnumerator WaitAndChangeState(CritterState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        ChangeState(newState);
    }


    private void Eat()
    {
        hunger = Mathf.Max(0, hunger - interactionRefill);
        //Debug.Log($"{this.name} ate and is now at {hunger} hunger");
    }

    private void Play()
    {
        mood = Mathf.Max(0, mood + interactionRefill);
        //Debug.Log($"{this.name} played and is now at {mood} mood");
    }

    private void SeekInteraction<T>(CritterState onSuccessState, float need) where T : InteractableContainer, new()
    {
        if (targetInteraction != null && targetInteraction.CanCritterInteract())
        {
            agent.SetDestination(targetInteraction.transform.position);
            //Debug.Log($"{this.name} is seeking a {targetInteraction.name} with {agent.remainingDistance} left to reach it.");
            if (Vector3.Distance(transform.position, targetInteraction.transform.position) <= interactionDistance)
            {
                //Debug.Log($"{this.name} reached interaction {targetInteraction.name}");
                critterAnimation.StopWalk();
                agent.isStopped = true;
                transform.LookAt(targetInteraction.transform.position);
                //Either eat or play, should return a value that can be used to refill mood or hunger
                Debug.Log($"Needs: {need} for {onSuccessState} using {targetInteraction}");
                interactionRefill = targetInteraction.CritterInteract(need);

                //Might need to move this to moveaway method
                targetInteraction.CritterCountChange(1);

                ChangeState(onSuccessState);
            }
            else
            {
                agent.isStopped = false;
            }
        }
        else
        {
            //Debug.Log($"{this.name} has needs and could not find an interaction to fulfill {onSuccessState}");
            FindInteraction<T>();
        }
    }

    private void FindInteraction<T>() where T : InteractableContainer, new()
    {
        T[] containers = FindObjectsOfType<T>();
        T closestContainer = null;
        float closestDistance = Mathf.Infinity;

        foreach (T container in containers)
        {
            if (container.CanCritterInteract() && container.GetCritters() < container.maxCritters)
            {
                float distanceToContainer = Vector3.Distance(transform.position, container.transform.position);
                if (distanceToContainer < closestDistance)
                {
                    closestContainer = container;
                    closestDistance = distanceToContainer;
                }
            }
        }

        if (closestContainer != null)
        {
            //closestContainer.CritterCountChange(1);
            targetInteraction = closestContainer;
        }
    }

    // Call this from the PlayerController when the player interacts with the critter
    public void ReceivePlayerInteraction()
    {
        lastPet = 0;
        mood = Mathf.Min(100, mood + 20f);
        petVFX.Play();
        agent.isStopped = true;
        ChangeState(CritterState.Idle);
        FMODUnity.RuntimeManager.PlayOneShot(InteractSuccessSound, transform.position);
    }

    // Call this from the PlayerController when the player feeds the critter
    public void ReceivePlayerFood(int rations)
    {
        //Check needs before continuing states
        lastFed = 0;
        hunger = Mathf.Max(0, hunger - (25f * rations));
        fedVFX.Play();
        agent.isStopped = true;
        ChangeState(CritterState.Idle);
        FMODUnity.RuntimeManager.PlayOneShot(InteractSuccessSound, transform.position);
    }

    private IEnumerator Roam()
    {
        agent.isStopped = false;
        while (true) // Keep roaming indefinitely
        {
            // Wait for a random time within the specified range before choosing a new destination
            yield return new WaitForSeconds(Random.Range(minRoamIdleTime, maxRoamIdleTime));

            // Check for GhostBuck drop
            if (ShouldDropGhostBuck())
            {
                DropGhostBuck();
            }

            // Choose a new random destination and move to it
            Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, 1))
            {
                Vector3 finalPosition = hit.position;
                agent.SetDestination(finalPosition);
            }

            // Wait until the critter has reached the destination before starting the timer again
            while (!agent.pathPending && agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null;
            }
        }
    }

    private bool ShouldDropGhostBuck()
    {
        return hunger <= hungerThresholdForGhostBuck &&
               mood >= moodThresholdForGhostBuck &&
               Random.value < ghostBuckDropChance;
    }

    private void DropGhostBuck()
    {
        if (ghostBuckPrefab != null)
        {
            Instantiate(ghostBuckPrefab, transform.position, Quaternion.identity);
        }
    }

    #region UI

    protected virtual void ToggleUI(bool active)
    {
        if (worldSpaceUI != null) worldSpaceUI.SetActive(active);
    }

    private void UpdateFillAmount(float amount, Image fillBar)
    {
        fillBar.fillAmount = amount / 100;
    }

    public IEnumerator Upset(bool active, bool timed = false)
    {
        thoughtBubble.gameObject.SetActive(active);
        upsetIcon.gameObject.SetActive(active);
        if (timed)
        {
            yield return new WaitForSeconds(upsetTime);
            thoughtBubble.gameObject.SetActive(!active);
            upsetIcon.gameObject.SetActive(!active);
        }
    }

    public void Playful(bool active)
    {
        thoughtBubble.gameObject.SetActive(active);
        playIcon.gameObject.SetActive(active);
    }

    public void Hungry(bool active)
    {
        thoughtBubble.gameObject.SetActive(active);
        hungryIcon.gameObject.SetActive(active);
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            ToggleUI(true);
            player.SetNearbyComponents(this.gameObject, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            ToggleUI(false);
            player.SetNearbyComponents(this.gameObject, false);
        }
    }
}
