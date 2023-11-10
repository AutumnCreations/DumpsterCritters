using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    public Dialogue dialogue;
    public ShopItem[] itemsForSale;
    private DialogueManager dialogueManager;

    private void Awake()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    public void Interact(PlayerController player)
    {
        dialogueManager.StartDialogue(dialogue);
    }

    public void GoToShopSpot(Vector3 spot)
    {
        GetComponent<NavMeshAgent>().SetDestination(spot);
    }

    public ShopItem[] GetItemsForSale()
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
}

