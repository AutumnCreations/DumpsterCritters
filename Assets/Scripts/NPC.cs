using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using FMODUnity;

public class NPC : MonoBehaviour
{
    [SerializeField]
    Dialogue dialogue;
    [SerializeField]
    private Inventory inventory = new Inventory();

    private DialogueManager dialogueManager;

    public Inventory Inventory => inventory;

    public GameObject Music;
    private MusicPlayer musicPlayer;

    private void Awake()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        musicPlayer = Music.GetComponent<MusicPlayer>();
    }

    public List<Item> GetItemsForSale()
    {
        return inventory.GetItems();
    }

    public void Interact(PlayerController player)
    {
        dialogueManager.StartDialogue(this);
        musicPlayer.PlayTrack(3);
    }

    public void GoToShopSpot(Vector3 spot)
    {
        GetComponent<NavMeshAgent>().SetDestination(spot);
    }

    //public List<Item> GetItemsForSale()
    //{
    //    return itemsForSale;
    //}

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

