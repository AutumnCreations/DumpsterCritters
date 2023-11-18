using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class ToyAnimation : MonoBehaviour
{
    [BoxGroup("Settings")]
    [SerializeField]
    Transform baseSprite;

    [BoxGroup("Settings/Preferences")]
    [SerializeField]
    bool wiggle = false;

    [BoxGroup("Settings/Preferences")]
    [SerializeField]
    bool roll = false;

    [BoxGroup("Settings/Preferences")]
    [SerializeField]
    bool jump = false;

    #region Wiggle

    [BoxGroup("Wiggle Settings")]
    [ShowIf("wiggle")]
    [SerializeField]
    float wiggleRotation = 15f;

    [BoxGroup("Wiggle Settings")]
    [ShowIf("wiggle")]
    [SerializeField]
    Ease wiggleEaseType = Ease.InOutCubic;

    [BoxGroup("Wiggle Settings")]
    [ShowIf("wiggle")]
    [SerializeField]
    float wiggleDuration = .5f;

    #endregion

    #region Roll
    [BoxGroup("Roll Settings")]
    [ShowIf("roll")]
    [SerializeField]
    float rollDistance = 1f;

    [BoxGroup("Roll Settings")]
    [ShowIf("roll")]
    [SerializeField]
    float rollDuration = .5f;

    [BoxGroup("Roll Settings")]
    [ShowIf("roll")]
    [SerializeField]
    Ease rollEaseType = Ease.InOutCubic;

    #endregion

    #region Jump

    [BoxGroup("Jump Settings")]
    [ShowIf("jump")]
    [SerializeField]
    float jumpDistance;

    [BoxGroup("Jump Settings")]
    [ShowIf("jump")]
    [SerializeField]
    float jumpHeight = .05f;

    [BoxGroup("Jump Settings")]
    [ShowIf("jump")]
    [SerializeField]
    float jumpDuration = .5f;

    [BoxGroup("Jump Settings")]
    [ShowIf("jump")]
    [SerializeField]
    int jumps = 2;

    [BoxGroup("Jump Settings")]
    [ShowIf("jump")]
    [SerializeField]
    Ease jumpEaseType = Ease.InOutCubic;

    #endregion

    Sequence sequence;
    Vector3 initialPosition;
    Quaternion initialRotation;

    void Start()
    {
        if (baseSprite == null)
        {
            baseSprite = transform;
        }
        initialPosition = baseSprite.localPosition;
        initialRotation = baseSprite.localRotation;

        if (wiggle) Wiggle();
    }

    public void RollnJump()
    {
        if (roll && jump)
        {

            // Main sequence
            sequence = DOTween.Sequence();

            // Non-repeating part: Roll back halfway
            sequence.Append(baseSprite.DOLocalMoveX(initialPosition.x - (rollDistance * .5f), rollDuration * .5f).SetEase(rollEaseType));
            sequence.Join(baseSprite.DOLocalRotate(new Vector3(baseSprite.localRotation.eulerAngles.x,
                           baseSprite.localRotation.eulerAngles.y, 360), rollDuration * .5f).SetEase(rollEaseType));

            // Nested sequence for repeating part
            Sequence nestedSequence = DOTween.Sequence();

            // Jump forward
            nestedSequence.Append(baseSprite.DOLocalJump(new Vector3(initialPosition.x + jumpDistance, initialPosition.y, initialPosition.z),
                          jumpHeight, jumps, jumpDuration).SetEase(jumpEaseType));
            //nestedSequence.Join(baseSprite.DOLocalRotate(new Vector3(baseSprite.localRotation.eulerAngles.x,
            //               baseSprite.localRotation.eulerAngles.y, 0), jumpDuration).SetEase(rollEaseType));

            // Roll back the rest of the way
            nestedSequence.Append(baseSprite.DOLocalMoveX(initialPosition.x - (rollDistance * .5f), rollDuration).SetEase(rollEaseType));
            nestedSequence.Join(baseSprite.DOLocalRotate(new Vector3(baseSprite.localRotation.eulerAngles.x,
                           baseSprite.localRotation.eulerAngles.y, 0), rollDuration).SetEase(rollEaseType));

            nestedSequence.SetLoops(-1, LoopType.Restart);

            sequence.Append(nestedSequence);

            //mainSequence.OnComplete(() =>
            //{
            //});
        }
        else
        {
            Debug.Log("You need to enable both Roll and Jump to use RollnJump");
        }
    }

    public void Wiggle()
    {
        // Set the initial rotation
        baseSprite.localRotation = Quaternion.Euler(baseSprite.localRotation.eulerAngles.x,
        baseSprite.localRotation.eulerAngles.y, -wiggleRotation);

        // Tween rotation side to side
        baseSprite.DOLocalRotate(new Vector3(baseSprite.localRotation.eulerAngles.x,
            baseSprite.localRotation.eulerAngles.y, wiggleRotation), wiggleDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(wiggleEaseType);
    }

    internal void Stop()
    {
        baseSprite.DOKill();
        sequence.Kill();
        baseSprite.DOLocalMove(initialPosition, .5f).SetEase(Ease.Linear);
        baseSprite.DOLocalRotate(initialRotation.eulerAngles, .5f).SetEase(Ease.Linear);
    }
}
