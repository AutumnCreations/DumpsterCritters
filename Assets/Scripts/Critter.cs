using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Critter : MonoBehaviour
{
    private enum CritterState
    {
        Idle,
        Roaming,
        SeekingFood,
        Eating,
        ReceivingAttention,
        SeekingStimulation,
        Playing
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

    [ShowInInspector, ReadOnly]
    [BoxGroup("Debug")]
    CritterState currentState;

    [ShowInInspector, ReadOnly]
    [BoxGroup("Debug")]
    InteractableContainer targetInteraction;

    NavMeshAgent agent;

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
        }
    }

    private void OnDisable()
    {
        if (GameStateManager.Instance != null)
        {
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
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ToggleUI(false);
    }

    private void Start()
    {
        // Start the roaming coroutine
        ChangeState(CritterState.Roaming);
        GameStateManager.Instance.onGameStateChange += OnGameStateChange;
    }

    private void Update()
    {
        if (isPaused) return;

        IncreaseHunger();
        DecreaseMood();

        switch (currentState)
        {
            case CritterState.Idle:
                CheckNeeds();
                break;
            case CritterState.SeekingFood:
                SeekInteraction<FoodBowl>(CritterState.Eating);
                break;
            case CritterState.Eating:
                Eat();
                break;
            case CritterState.SeekingStimulation:
                SeekInteraction<Placemat>(CritterState.Playing);
                break;
            case CritterState.Playing:
                Play();
                break;
            case CritterState.ReceivingAttention:
                ReceiveAttention();
                break;
            case CritterState.Roaming:
                CheckNeeds();
                break;
            default:
                break;
        }
    }

    private void CheckNeeds()
    {
        if (hunger >= needFood) ChangeState(CritterState.SeekingFood);
        else if (mood <= needAttention) ChangeState(CritterState.SeekingStimulation);
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
            // When done with the specific activity, evaluate the next state
            StopAllCoroutines();
            StartCoroutine(WaitAndEvaluate());
        }
        else if (currentState == CritterState.Roaming && newState != CritterState.Roaming)
        {
            // Stop roaming coroutine if transitioning out of Roaming state
            StopAllCoroutines();
        }

        // Entering new state logic
        switch (newState)
        {
            case CritterState.Eating:
                StopAllCoroutines();
                StartCoroutine(WaitAndChangeState(CritterState.Idle, eatingDuration));
                break;
            case CritterState.Playing:
                StopAllCoroutines();
                StartCoroutine(WaitAndChangeState(CritterState.Idle, playDuration));
                break;
            case CritterState.Idle:
                // Start idle time before beginning to roam again
                StartCoroutine(StartIdleRoamingTime());
                break;
            case CritterState.Roaming:
                StartCoroutine(Roam());
                break;
            case CritterState.SeekingFood:
                SeekInteraction<FoodBowl>(CritterState.Eating);
                break;
            case CritterState.SeekingStimulation:
                SeekInteraction<Placemat>(CritterState.Playing);
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
        Vector3 directionAwayFromBowl = (transform.position - targetInteraction.transform.position).normalized;
        Vector3 newDestination = transform.position + directionAwayFromBowl * moveAwayDistance;
        agent.SetDestination(newDestination);
        agent.isStopped = false;
    }

    private IEnumerator WaitAndChangeState(CritterState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        ChangeState(newState);
    }


    private void Eat()
    {
        hunger = Mathf.Max(0, hunger - 50f);
        Debug.Log($"{this.name} ate and is now at {hunger} hunger");
    }

    private void Play()
    {
        mood = Mathf.Max(0, mood + 50f);
        Debug.Log($"{this.name} played and is now at {mood} mood");
    }

    private void SeekInteraction<T>(CritterState onSuccessState) where T : InteractableContainer, new()
    {
        if (targetInteraction != null && targetInteraction.CanCritterInteract())
        {
            agent.SetDestination(targetInteraction.transform.position);
            Debug.Log($"{this.name} is seeking a {targetInteraction.name} with {agent.remainingDistance} left to reach it.");
            if (Vector3.Distance(transform.position, targetInteraction.transform.position) <= interactionDistance)
            {
                Debug.Log($"{this.name} reached interaction {targetInteraction.name}");
                agent.isStopped = true;
                targetInteraction.CritterInteract();
                targetInteraction.currentCritters--;
                ChangeState(onSuccessState);
            }
        }
        else
        {
            Debug.Log($"{this.name} has needs and could not find an interaction to fulfill {onSuccessState}");
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
            if (container.CanCritterInteract() && container.currentCritters < container.maxCritters)
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
            closestContainer.currentCritters++;
            targetInteraction = closestContainer;
        }
    }

    // Call this from the PlayerController when the player interacts with the critter
    public void ReceivePlayerInteraction()
    {
        //TODO: Add timer logic for time between player interactions
        mood = Mathf.Min(100, mood + 10f);
        if (currentState == CritterState.SeekingStimulation)
        {
            ChangeState(CritterState.ReceivingAttention);
        }
    }

    // Called when giving food to the critter
    public void Feed()
    {
        hunger = Mathf.Max(0, hunger - 50f); // Decrease hunger
        ChangeState(CritterState.Idle);
    }

    private void ReceiveAttention()
    {
        // Logic for what happens when receiving attention
        mood = Mathf.Min(100, mood + 50f);
        ChangeState(CritterState.Idle);
    }

    private IEnumerator Roam()
    {
        while (true) // Keep roaming indefinitely
        {
            // Wait for a random time within the specified range before choosing a new destination
            yield return new WaitForSeconds(Random.Range(minRoamIdleTime, maxRoamIdleTime));

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

    #region UI

    protected virtual void ToggleUI(bool active)
    {
        if (worldSpaceUI != null) worldSpaceUI.SetActive(active);
    }

    private void UpdateFillAmount(float amount, Image fillBar)
    {
        fillBar.fillAmount = amount / 100;
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
