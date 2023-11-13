using UnityEngine;
using Sirenix.OdinInspector;

public class Food : Interactable
{
    [Tooltip("How many rations does this fill?")]
    [Range(1, 50)]
    public int rationCount = 0;

}
