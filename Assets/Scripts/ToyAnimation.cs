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
    bool playOnAwake = true;

    [BoxGroup("Settings/Preferences")]
    [SerializeField]
    bool wiggle = false;

    [BoxGroup("Settings/Preferences")]
    [SerializeField]
    bool shake = false;

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


    #region Shake

    [BoxGroup("Shake Settings")]
    [ShowIf("shake")]
    [SerializeField]
    Ease shakeEaseType = Ease.InOutCubic;

    [BoxGroup("Shake Settings")]
    [ShowIf("shake")]
    [SerializeField]
    float shakeDuration = .5f;

    [BoxGroup("Shake Settings")]
    [ShowIf("shake")]
    [SerializeField, Range(1, 100)]
    float shakeStrength = 30f;

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
        if (playOnAwake) PlayAnimations();
    }

    public void PlayAnimations()
    {
        if (wiggle) Wiggle();
        if (shake) Shake();
        if (roll && jump) RollnJump();
        else if (roll) Roll();
        else if (jump) Jump();
    }

    private void Roll()
    {
        sequence = DOTween.Sequence();

        // Move to the left initially
        sequence.Append(baseSprite.DOLocalMoveX(initialPosition.x - (rollDistance * 0.5f), rollDuration).SetEase(rollEaseType));
        sequence.Join(baseSprite.DOLocalRotate(new Vector3(0, 0, 180), rollDuration, RotateMode.FastBeyond360).SetEase(rollEaseType));

        // Nested sequence for repeating part
        Sequence nestedSequence = DOTween.Sequence();

        // Move and rotate to the right and then to the left
        nestedSequence.Append(baseSprite.DOLocalMoveX(initialPosition.x + rollDistance, rollDuration).SetEase(rollEaseType));
        nestedSequence.Join(baseSprite.DOLocalRotate(new Vector3(0, 0, -360), rollDuration, RotateMode.FastBeyond360).SetEase(rollEaseType));
        nestedSequence.Append(baseSprite.DOLocalMoveX(initialPosition.x - rollDistance, rollDuration).SetEase(rollEaseType));
        nestedSequence.Join(baseSprite.DOLocalRotate(new Vector3(0, 0, 360), rollDuration, RotateMode.FastBeyond360).SetEase(rollEaseType));

        nestedSequence.SetLoops(-1, LoopType.Yoyo);

        sequence.Append(nestedSequence);
    }

    private void Jump()
    {
        baseSprite.DOLocalJump(new Vector3(initialPosition.x + jumpDistance, initialPosition.y, initialPosition.z),
                       jumpHeight, jumps, jumpDuration)
            .SetEase(jumpEaseType)
            .SetLoops(-1, LoopType.Yoyo);
        baseSprite.DOLocalRotate(new Vector3(0, 0, -360), jumpDuration, RotateMode.FastBeyond360).SetEase(jumpEaseType)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void RollnJump()
    {

        // Main sequence
        sequence = DOTween.Sequence();

        // Non-repeating part: Roll back halfway
        sequence.Append(baseSprite.DOLocalMoveX(initialPosition.x + (rollDistance * 0.5f), rollDuration).SetEase(rollEaseType));
        sequence.Join(baseSprite.DOLocalRotate(new Vector3(0, 0, 180), rollDuration, RotateMode.FastBeyond360).SetEase(rollEaseType));

        // Nested sequence for repeating part
        Sequence nestedSequence = DOTween.Sequence();

        // Jump forward
        nestedSequence.Append(baseSprite.DOLocalJump(new Vector3(baseSprite.position.x + jumpDistance, initialPosition.y, initialPosition.z),
                      jumpHeight, jumps, jumpDuration).SetEase(jumpEaseType));
        nestedSequence.Join(baseSprite.DOLocalRotate(new Vector3(0, 0, 360), rollDuration, RotateMode.FastBeyond360).SetEase(rollEaseType));

        // Roll back the rest of the way
        nestedSequence.Append(baseSprite.DOLocalMoveX(initialPosition.x + rollDistance, rollDuration).SetEase(rollEaseType));
        nestedSequence.Join(baseSprite.DOLocalRotate(new Vector3(0, 0, -360), rollDuration, RotateMode.FastBeyond360).SetEase(rollEaseType));

        nestedSequence.SetLoops(-1, LoopType.Restart);

        sequence.Append(nestedSequence);
    }

    private void Wiggle()
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

    private void Shake()
    {
        baseSprite.DOShakeRotation(shakeDuration, shakeStrength, 10, 50, true)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(shakeEaseType);
    }

    internal void Stop()
    {
        Debug.Log($"Stopping toy animation {gameObject.name}");
        baseSprite.DOKill();
        sequence.Kill();
        baseSprite.DOLocalMove(initialPosition, .5f).SetEase(Ease.Linear);
        baseSprite.DOLocalRotate(initialRotation.eulerAngles, .5f).SetEase(Ease.Linear);
    }
}
