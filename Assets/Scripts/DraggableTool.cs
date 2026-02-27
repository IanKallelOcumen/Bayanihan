using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class DraggableTool : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] public ToolType toolType; 

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Vector3 originalScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalScale = transform.localScale;
        
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.rootCanvas != null)
        {
            canvas = canvas.rootCanvas;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;

        // Scale up effect
        StartCoroutine(AnimateScale(originalScale * 1.2f));

        if (canvas != null)
        {
            transform.SetParent(canvas.transform, true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        // Scale back down
        StartCoroutine(AnimateScale(originalScale));

        if (CheckForRepairTarget(eventData))
        {
            ReturnToOriginalPosition();
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }

    private bool CheckForRepairTarget(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == gameObject) continue;

            RepairTarget target = result.gameObject.GetComponent<RepairTarget>();
            
            if (target == null)
            {
                target = result.gameObject.GetComponentInParent<RepairTarget>();
            }

            if (target != null)
            {
                if (!target.IsFixed && target.requiredTool == this.toolType)
                {
                    target.Fix();
                    return true;
                }
            }
        }

        return false;
    }

    private void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalPosition;
    }

    private IEnumerator AnimateScale(Vector3 target)
    {
        float t = 0;
        float duration = 0.1f;
        Vector3 start = transform.localScale;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        transform.localScale = target;
    }
}
