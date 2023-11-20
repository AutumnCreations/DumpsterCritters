using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MouseTrap : FoodContainer
{
    [BoxGroup("Mouse Trap")]
    [SerializeField, Range(1f, 5f)]
    [Tooltip("How long does it take for the mouse trap to destroy itself?")]
    float destroyDelay = 3f;

    [BoxGroup("Mouse Trap")]
    [SerializeField, Range(30f, 500f)]
    [Tooltip("How long does it take for the mouse trap to respawn?")]
    float mouseTrapRespawn = 120f;

    [BoxGroup("Mouse Trap")]
    [SerializeField]
    SpriteRenderer spriteRenderer;

    NavMeshObstacle obstacle;

    protected override void Awake()
    {
        base.Awake();
        obstacle = GetComponent<NavMeshObstacle>();
        obstacle.enabled = true;
    }

    public override void RemoveObject(PlayerController player)
    {
        base.RemoveObject(player);
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay - 1f);
        StartCoroutine(FadeOut());  
        yield return new WaitForSeconds(1f);
        obstacle.enabled = false;
        yield return new WaitForSeconds(mouseTrapRespawn);
        obstacle.enabled = true;
        StartCoroutine(RespawnFood(1f));
        StartCoroutine(FadeIn());
        
    }
    private IEnumerator FadeOut()
    {
        if (spriteRenderer == null) yield break;

        float fadeDuration = 1f; 
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        if (spriteRenderer == null) yield break;

        float fadeDuration = 1f;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            yield return null;
        }
    }

}
