using UnityEngine;
using System.Collections;

public class MouseSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject critterPrefab;

    [SerializeField]
    private Transform spawnArea;

    [SerializeField]
    private float spawnDelay = 5f; 

    [SerializeField]
    private float spawnRadius = 5f;

    private float lastSpawnTime = 0f;

    private void Update()
    {
        if (Time.time - lastSpawnTime > spawnDelay && CanSpawnCritter())
        {
            SpawnCritter();
            lastSpawnTime = Time.time;
        }
    }

    private bool CanSpawnCritter()
    {
        int maxCrittersAllowed = 0;

        // Calculate total max critters allowed from all placemats
        foreach (var placemat in FindObjectsOfType<Placemat>())
        {
            if (placemat.unlocked) maxCrittersAllowed += placemat.maxCritters;
        }

        // Allow spawning if current critter count is less than max allowed
        return GameStateManager.Instance.critterCount < maxCrittersAllowed;
    }

    private void SpawnCritter()
    {

        FMODUnity.RuntimeManager.PlayOneShot("event:/NPC/Mouse/Mouse_Squeak");
        Vector3 spawnPosition = spawnArea.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = spawnArea.position.y; 

        Instantiate(critterPrefab, spawnPosition, Quaternion.identity);
        //GameStateManager.Instance.UpdateCritterCount(1); 
    }
}
