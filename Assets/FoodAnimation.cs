using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodAnimation : MonoBehaviour
{

    [BoxGroup("Idle Settings")]
    [SerializeField]
    Transform spriteTransform;

    [BoxGroup("Idle Settings")]
    [SerializeField]
    float sideToSideRotation = 15f;

    [BoxGroup("Idle Settings")]
    [SerializeField]
    Ease idleEaseType = Ease.InOutCubic;

    [BoxGroup("Idle Settings")]
    [SerializeField]
    float idleMotionDuration = .5f;


    void Start()
    {
        if (spriteTransform == null)
        {
            spriteTransform = transform;
        }

        // Set the initial rotation
        spriteTransform.localRotation = Quaternion.Euler(spriteTransform.localRotation.eulerAngles.x,
        spriteTransform.localRotation.eulerAngles.y, -sideToSideRotation);

        // Tween rotation side to side
        spriteTransform.DOLocalRotate(new Vector3(spriteTransform.localRotation.eulerAngles.x,
            spriteTransform.localRotation.eulerAngles.y, sideToSideRotation), idleMotionDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(idleEaseType);
    }

}
