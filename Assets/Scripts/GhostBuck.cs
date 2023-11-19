using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class GhostBuck : MonoBehaviour
{

    [BoxGroup("Idle Settings")]
    [SerializeField]
    Transform spriteTransform;

    [BoxGroup("Idle Settings")]
    [SerializeField]
    float spinSpeed = 2f;

    [BoxGroup("Idle Settings")]
    [SerializeField]
    float hoverSpeed = 1f;

    [BoxGroup("Idle Settings")]
    [SerializeField]
    float hoverHeight = .1f;

    [BoxGroup("Idle Settings")]
    [SerializeField]
    Ease idleEaseType = Ease.InOutCubic;



    void Start()
    {
        if (spriteTransform == null)
        {
            spriteTransform = transform;
        }
        spriteTransform.DOMoveY(spriteTransform.position.y + hoverHeight, hoverSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutCubic);
        spriteTransform.DORotate(new Vector3(0, 360, 0), spinSpeed, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(idleEaseType);
    }

    void Pickup()
    {
        spriteTransform.DOKill();
        // do full spin
        spriteTransform.DOMoveY(spriteTransform.position.y + hoverHeight, hoverSpeed * .1f).SetLoops(3, LoopType.Yoyo).SetEase(Ease.InOutCubic);
        spriteTransform.DORotate(new Vector3(0, 360, 0), spinSpeed * .1f, RotateMode.FastBeyond360)
            .SetEase(idleEaseType)
            .SetLoops(3, LoopType.Restart)
            .OnComplete(() => Destroy(gameObject));
        FMODUnity.RuntimeManager.PlayOneShot("event:/Coin");
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.PickupGhostBuck();
            Pickup();
        }
    }
}
