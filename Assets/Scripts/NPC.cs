using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    [SerializeField]
    Dialogue dialogue;
    [SerializeField]
    //[AssetList(Path = "Assets/Game/Interactables/Items")]
    List<Item> itemsForSale;

    private DialogueManager dialogueManager;

    private void Awake()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    public void Interact(PlayerController player)
    {
        dialogueManager.StartDialogue(this);
    }

    public void GoToShopSpot(Vector3 spot)
    {
        GetComponent<NavMeshAgent>().SetDestination(spot);
    }

    public List<Item> GetItemsForSale()
    {
        return itemsForSale;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetNearbyComponents(this.gameObject, true);
        }
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetNearbyComponents(this.gameObject, false);
        }
    }

    internal Dialogue GetDialogue()
    {
        return dialogue;
    }
}

