using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;
using System.Collections;

public class Critter : MonoBehaviour
{
    private enum CritterState
    {
        Idle,
        Roaming,
        SeekingFood,
        Eating,
        SeekingAttention,
        ReceivingAttention
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
    [GUIColor("orange")]
    [Tooltip("How long the critter will eat for in seconds")]
    [SerializeField, Range(0, 10)]
    float eatingDuration = 3f;

    NavMeshAgent agent;
    CritterState currentState;
    FoodBowl targetFoodBowl;
    Transform targetAttentionLocation;

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

    //private void OnEnable()
    //{
    //    GameStateManager.Instance.onGameStateChange += OnGameStateChange;
    //}

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
                SeekFood();
                break;
            case CritterState.Eating:
                Eat();
                break;
            case CritterState.SeekingAttention:
                SeekAttention();
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
        else if (mood <= needAttention) ChangeState(CritterState.SeekingAttention);
    }

    private void IncreaseHunger()
    {
        hunger += hungerIncreaseRate * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0f, 100f);
    }

    private void DecreaseMood()
    {
        mood -= moodDecreaseRate * Time.deltaTime;
        mood = Mathf.Clamp(mood, 0f, 100f);
    }

    private void ChangeState(CritterState newState)
    {
        // Exiting current state logic
        if (currentState == CritterState.Eating && newState != CritterState.Eating)
        {
            // When done eating, we want to evaluate if we should roam, seek attention, or stay idle
            StopAllCoroutines();
            StartCoroutine(WaitAndEvaluate());
        }
        else if (currentState == CritterState.Roaming && newState != CritterState.Roaming)
        {
            // If we're currently roaming and transitioning to another state, stop the roaming coroutine
            StopAllCoroutines();
        }

        // If we're entering Eating state, handle the eating duration
        if (newState == CritterState.Eating)
        {
            StopAllCoroutines();
            StartCoroutine(WaitAndChangeState(CritterState.Idle, eatingDuration));
        }

        currentState = newState;

        // Handle any entry logic for new states as needed
        switch (newState)
        {
            case CritterState.Idle:
                // Start idle time before beginning to roam again
                StartCoroutine(StartIdleRoamingTime());
                break;
            case CritterState.Roaming:
                // Start the roaming coroutine
                StartCoroutine(Roam());
                break;
            case CritterState.SeekingFood:
                FindFoodBowl();
                break;
            case CritterState.SeekingAttention:
                GetComponent<MeshRenderer>().material.color = Color.blue;
                if (mood > needAttention)
                {
                    GetComponent<MeshRenderer>().material.color = Color.white;
                    ChangeState(CritterState.Idle);
                }
                // Likely should add some UI prompts here. Maybe go to interactable placemat that has a toy?
                break;
        }

        //Debug.Log($"{name} state: {currentState} mood: {mood} hunger: {hunger}");
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
        yield return new WaitForSeconds(eatingDuration); // Or however long you want to wait after eating
        if (hunger >= needFood) ChangeState(CritterState.SeekingFood);
        else if (mood <= needAttention) ChangeState(CritterState.SeekingAttention);
        else ChangeState(CritterState.Idle);
    }

    private IEnumerator WaitAndChangeState(CritterState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        ChangeState(newState);
    }


    private void Eat()
    {
        hunger = Mathf.Max(0, hunger - 50f);
    }

    private void SeekFood()
    {
        // Assume targetFoodBowl is already set by FindFoodBowl()
        if (targetFoodBowl != null && targetFoodBowl.HasFood())
        {
            agent.SetDestination(targetFoodBowl.transform.position);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
            {
                targetFoodBowl.TakeFood();
                ChangeState(CritterState.Eating);
            }
        }
    }

    private void FindFoodBowl()
    {
        FoodBowl[] foodBowls = FindObjectsOfType<FoodBowl>();
        FoodBowl closestBowl = null;
        float closestDistance = Mathf.Infinity;

        foreach (FoodBowl bowl in foodBowls)
        {
            if (bowl.HasFood())
            {
                float distanceToBowl = Vector3.Distance(transform.position, bowl.transform.position);
                if (distanceToBowl < closestDistance)
                {
                    closestBowl = bowl;
                    closestDistance = distanceToBowl;
                }
            }
        }

        targetFoodBowl = closestBowl;
    }

    private void SeekAttention()
    {
        // Logic for the critter to seek attention from the player or a designated attention point
        // Can set the targetAttentionLocation when the player is close or based on some game logic
        if (targetAttentionLocation != null)
        {
            agent.SetDestination(targetAttentionLocation.position);

            // Check if the critter is close enough to the player or attention point (player or placemat) to receive attention
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
            {
                // Reached the attention point
                ChangeState(CritterState.ReceivingAttention);
            }
        }
    }

    // Call this from the PlayerController when the player interacts with the critter
    public void ReceivePlayerInteraction()
    {
        mood = Mathf.Min(100, mood + 10f);
        if (currentState == CritterState.SeekingAttention)
        {
            targetAttentionLocation = GameObject.FindGameObjectWithTag("Player").transform;
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

}
