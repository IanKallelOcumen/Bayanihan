using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SimpleButtonAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float duration = 0.1f;

    [Header("Idle Animation")]
    [SerializeField] private bool idlePulse = false;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.05f;

    private Vector3 originalScale;
    private Coroutine currentCoroutine;
    private bool isHovering = false;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = originalScale;
        if (idlePulse) StartCoroutine(IdlePulseRoutine());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        AnimateTo(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        AnimateTo(originalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AnimateTo(originalScale * clickScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Return to hover state if still hovering, else original
        AnimateTo(isHovering ? originalScale * hoverScale : originalScale);
    }

    private void AnimateTo(Vector3 targetScale)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ScaleOverTime(targetScale));
    }

    private IEnumerator ScaleOverTime(Vector3 target)
    {
        float t = 0;
        Vector3 start = transform.localScale;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime; 
            transform.localScale = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        transform.localScale = target;
    }

    private IEnumerator IdlePulseRoutine()
    {
        while (true)
        {
            if (!isHovering)
            {
                float scale = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
                transform.localScale = originalScale * scale;
            }
            yield return null;
        }
    }
}
