using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconAnimation : MonoBehaviour
{
    #region Shake
    [BoxGroup("Shake Settings")]
    [SerializeField]
    Ease shakeEaseType = Ease.InOutCubic;

    [BoxGroup("Shake Settings")]
    [SerializeField]
    float shakeDuration = .5f;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Shake();
    }

    public void Shake()
    {
        transform.DOShakeRotation(shakeDuration, 50, 10, 50, true)
    .SetLoops(2, LoopType.Yoyo)
    .SetEase(shakeEaseType);
    }



}
