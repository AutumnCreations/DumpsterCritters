using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class CritterAnimation : MonoBehaviour
{
    [BoxGroup("Walk Settings")]
    [SerializeField]
    Transform body;

    [BoxGroup("Walk Settings")]
    [SerializeField]
    float maxWalkHeight = 0.2f;

    [BoxGroup("Walk Settings")]
    [SerializeField]
    float minWalkHeight = 0.01f;

    [BoxGroup("Walk Settings")]
    [SerializeField]
    float walkAnimationDuration = 0.5f;

    [BoxGroup("Walk Settings")]
    [SerializeField]
    Ease walkEase = Ease.InOutCubic;

    [BoxGroup("Walk Settings")]
    [SerializeField]
    float rotateAnimationDuration = 0.25f;

    [BoxGroup("Walk Settings")]
    [SerializeField]
    float walkRotation = 15f;

    [BoxGroup("Walk Settings")]
    [SerializeField]
    Ease bodyRotateEase = Ease.InOutCubic;

    [BoxGroup("Tail Settings")]
    [SerializeField]
    Transform tail;

    [BoxGroup("Tail Settings")]
    [SerializeField]
    float idleTailRotation = 8f;

    [BoxGroup("Tail Settings")]
    [SerializeField]
    float idleTailDuration = .3f;

    [BoxGroup("Tail Settings")]
    [SerializeField]
    Ease idleTailEase = Ease.InOutCubic;

    Quaternion initialRotation;
    Sequence sequence;

    void Start()
    {
        if (body == null)
        {
            body = transform;
        }

        body.localPosition = new Vector3(body.localPosition.x, body.localPosition.y, minWalkHeight);

        initialRotation = body.localRotation;

        // Set the initial rotation
        tail.localRotation = Quaternion.Euler(tail.localRotation.eulerAngles.x,
        tail.localRotation.eulerAngles.y, -idleTailRotation);

        // Tween rotation side to side
        tail.DOLocalRotate(new Vector3(tail.localRotation.eulerAngles.x,
            tail.localRotation.eulerAngles.y, idleTailRotation), idleTailDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(idleTailEase);
    }
    public void Walk()
    {
        body.DOLocalMoveY(maxWalkHeight, walkAnimationDuration).SetLoops(-1, LoopType.Yoyo).SetEase(walkEase);
        sequence = DOTween.Sequence();
        sequence.Append(body.DOLocalRotate(new Vector3(walkRotation,
                                  body.localRotation.eulerAngles.y, body.localRotation.eulerAngles.z), rotateAnimationDuration)
                                  .SetEase(bodyRotateEase));
        sequence.Append(body.DOLocalRotate(new Vector3(initialRotation.x,
                          body.localRotation.eulerAngles.y, body.localRotation.eulerAngles.z), rotateAnimationDuration)
                          .SetEase(bodyRotateEase));
        sequence.Append(body.DOLocalRotate(new Vector3(-walkRotation,
                          body.localRotation.eulerAngles.y, body.localRotation.eulerAngles.z), rotateAnimationDuration)
                          .SetEase(bodyRotateEase));
        sequence.Append(body.DOLocalRotate(new Vector3(initialRotation.x,
                  body.localRotation.eulerAngles.y, body.localRotation.eulerAngles.z), rotateAnimationDuration)
                  .SetEase(bodyRotateEase));
        sequence.SetLoops(-1, LoopType.Yoyo);
    }
    public void StopWalk()
    {
        sequence.Kill();
        body.DOKill();
        body.DOLocalMoveY(minWalkHeight, walkAnimationDuration).SetEase(walkEase);
        body.DOLocalRotate(initialRotation.eulerAngles, rotateAnimationDuration).SetEase(bodyRotateEase);
    }


}
