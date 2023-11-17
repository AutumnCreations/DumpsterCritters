using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;

public class PlayerAnimation : MonoBehaviour
{
    [BoxGroup("Hover Settings")]
    [SerializeField]
    float maxHoverHeight = 0.2f;

    [BoxGroup("Hover Settings")]
    [SerializeField]
    float minHoverHeight = -0.2f;

    [BoxGroup("Hover Settings")]
    [SerializeField]
    float hoverDuration = 1f;

    [BoxGroup("Hover Settings")]
    [SerializeField]
    Transform hoverObject;

    [BoxGroup("Hover Settings")]
    [SerializeField]
    Ease hoverEaseType = Ease.InOutCubic;

    [BoxGroup("Arm Settings")]
    [SerializeField]
    float armAnimationDuration = 0.5f;

    [BoxGroup("Arm Settings")]
    [SerializeField]
    Transform leftArm;

    [BoxGroup("Arm Settings")]
    [SerializeField]
    Vector3 leftArmHoldPosition;

    [BoxGroup("Arm Settings")]
    [SerializeField]
    Vector3 leftArmPocketPosition;

    [BoxGroup("Arm Settings")]
    [SerializeField]
    Transform rightArm;

    [BoxGroup("Arm Settings")]
    [SerializeField]
    Vector3 rightArmHoldPosition;

    [BoxGroup("Arm Settings")]
    [SerializeField]
    Vector3 rightArmPocketPosition;

    [BoxGroup("Arm Settings")]
    [SerializeField]
    Ease armEaseType = Ease.OutBack;

    [BoxGroup("Arm Settings/Petting")]
    [SerializeField]
    Vector3 leftArmPetPosition;

    [BoxGroup("Arm Settings/Petting")]
    [SerializeField]
    Vector3 rightArmPetPosition;

    [BoxGroup("Arm Settings/Petting")]
    [SerializeField]
    float sideMotionDistance;

    [BoxGroup("Arm Settings/Petting")]
    [SerializeField]
    int numberOfPetLoops;

    [BoxGroup("Arm Settings/Petting")]
    [SerializeField]
    float petMotionDuration;

    [BoxGroup("Arm Settings/Petting")]
    [SerializeField]
    Ease petMotionEase;

    [BoxGroup("Eye Settings")]
    [SerializeField]
    Transform leftEye;

    [BoxGroup("Eye Settings")]
    [SerializeField]
    Transform rightEye;

    [BoxGroup("Eye Settings")]
    [SerializeField]
    float eyeAnimationDuration = 0.1f;

    [BoxGroup("Eye Settings")]
    [SerializeField]
    float eyeBlinkDistance = .1f;

    [BoxGroup("Eye Settings")]
    [SerializeField]
    float timeBetweenBlinksMin = 3f;

    [BoxGroup("Eye Settings")]
    [SerializeField]
    float timeBetweenBlinksMax = 10f;

    Vector3 leftArmStart;
    Vector3 rightArmStart;

    void Start()
    {
        leftArmStart = leftArm.localPosition;
        rightArmStart = rightArm.localPosition;

        if (hoverObject == null)
        {
            hoverObject = transform;
        }
        hoverObject.localPosition = new Vector3(hoverObject.localPosition.x, minHoverHeight, hoverObject.localPosition.z);
        //tween transform up and down
        hoverObject.DOLocalMoveY(maxHoverHeight, hoverDuration).SetLoops(-1, LoopType.Yoyo).SetEase(hoverEaseType);

        StartCoroutine(BlinkRoutine());
    }

    public void ArmsHold()
    {
        //if holding item, move arms to hold item
        if (leftArm != null && rightArm != null)
        {
            leftArm.DOLocalMove(leftArmHoldPosition, armAnimationDuration).SetEase(armEaseType);
            rightArm.DOLocalMove(rightArmHoldPosition, armAnimationDuration).SetEase(armEaseType);
        }
    }
    public void ArmsReturn()
    {
        //if not holding item, return arms to original position
        if (leftArm != null && rightArm != null)
        {
            leftArm.DOLocalMove(leftArmStart, armAnimationDuration).SetEase(armEaseType);
            rightArm.DOLocalMove(rightArmStart, armAnimationDuration).SetEase(armEaseType);
        }
    }
    public void ArmsPocket()
    {
        if (leftArm != null && rightArm != null)
        {
            Sequence armSequence = DOTween.Sequence();
            armSequence.Append(leftArm.DOLocalMove(leftArmPocketPosition, armAnimationDuration).SetEase(armEaseType));
            armSequence.Join(rightArm.DOLocalMove(rightArmPocketPosition, armAnimationDuration).SetEase(armEaseType));

            armSequence.OnComplete(() => ArmsReturn());
        }
    }
    public void ArmsPet()
    {
        if (leftArm != null && rightArm != null)
        {
            Sequence armSequence = DOTween.Sequence();
            armSequence.Append(leftArm.DOLocalMove(leftArmPetPosition, armAnimationDuration * .75f).SetEase(armEaseType));
            armSequence.Join(rightArm.DOLocalMove(rightArmPetPosition, armAnimationDuration * .75f).SetEase(armEaseType));

            //Move left arm and right arm side to side
            armSequence.Append(leftArm.DOLocalMoveZ(leftArmPetPosition.z + sideMotionDistance, petMotionDuration).SetLoops(numberOfPetLoops, LoopType.Restart).SetEase(petMotionEase));
            armSequence.Join(rightArm.DOLocalMoveZ(rightArmPetPosition.z - sideMotionDistance, petMotionDuration).SetLoops(numberOfPetLoops, LoopType.Restart).SetEase(petMotionEase));

            armSequence.OnComplete(() => ArmsReturn());
        }
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // Wait for a random time before blinking
            yield return new WaitForSeconds(UnityEngine.Random.Range(timeBetweenBlinksMin, timeBetweenBlinksMax));

            // Execute the blink sequence
            Sequence eyeSequence = DOTween.Sequence();
            eyeSequence.Append(leftEye.DOLocalMoveY(leftEye.localPosition.y - eyeBlinkDistance, eyeAnimationDuration).SetEase(Ease.Linear));
            eyeSequence.Join(rightEye.DOLocalMoveY(rightEye.localPosition.y - eyeBlinkDistance, eyeAnimationDuration).SetEase(Ease.Linear));
            eyeSequence.Append(leftEye.DOLocalMoveY(leftEye.localPosition.y, eyeAnimationDuration).SetEase(Ease.Linear));
            eyeSequence.Join(rightEye.DOLocalMoveY(rightEye.localPosition.y, eyeAnimationDuration).SetEase(Ease.Linear));

            eyeSequence.Append(leftEye.DOLocalMoveY(leftEye.localPosition.y - eyeBlinkDistance, eyeAnimationDuration).SetEase(Ease.Linear));
            eyeSequence.Join(rightEye.DOLocalMoveY(rightEye.localPosition.y - eyeBlinkDistance, eyeAnimationDuration).SetEase(Ease.Linear));
            eyeSequence.Append(leftEye.DOLocalMoveY(leftEye.localPosition.y, eyeAnimationDuration).SetEase(Ease.Linear));
            eyeSequence.Join(rightEye.DOLocalMoveY(rightEye.localPosition.y, eyeAnimationDuration).SetEase(Ease.Linear));

            // Wait for the blink sequence to complete
            yield return eyeSequence.WaitForCompletion();
        }
    }


}
