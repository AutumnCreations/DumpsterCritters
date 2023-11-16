using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    [BoxGroup("Item Details")]
    [Tooltip("GameObject that will be instantiated when used.")]
    [SerializeField]
    internal Interactable itemPrefab;

    [BoxGroup("Item Details")]
    [Tooltip("Name that will show in inventory and shop. Sets to GO name if not assigned.")]
    [SerializeField]
    internal string itemName;

    [BoxGroup("Item Details")]
    [Tooltip("Sprite that will show in inventory and shop.")]
    [SerializeField]
    internal Sprite itemSprite;

    [BoxGroup("Item Details")]
    [SerializeField]
    internal int cost = 0;

    [BoxGroup("Item Details")]
    [Tooltip("How many rations does this fill? 0 if non-food item")]
    [Range(0, 4)]
    public int rationCount = 0;

    [BoxGroup("Item Details")]
    [Tooltip("How much entertainment does this provide? 0 if food")]
    [Range(0, 4)]
    public int entertainmentValue = 0;
}
