using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VehicleController : MonoBehaviour
{
    [Header("Assets")]
    [SerializeField] private Sprite brokenSprite;
    [SerializeField] private Sprite fixedSprite;
    [SerializeField] private Image visualImage;
    [SerializeField] private GameObject fixedVisualObj; // The separate object for fixed state if needed

    [Header("Repair Logic")]
    [SerializeField] private List<RepairTarget> allPossibleTargets;

    public void Initialize(int minParts, int maxParts)
    {
        // Set visual
        if (visualImage != null && brokenSprite != null)
        {
            visualImage.sprite = brokenSprite;
            visualImage.SetNativeSize();
            visualImage.gameObject.SetActive(true);
        }

        if (fixedVisualObj != null)
        {
            Image fixedImg = fixedVisualObj.GetComponent<Image>();
            if (fixedImg != null && fixedSprite != null)
            {
                fixedImg.sprite = fixedSprite;
                fixedImg.SetNativeSize();
            }
            fixedVisualObj.SetActive(false);
        }

        // Randomize Problems (3-10 range as requested)
        int countToEnable = Random.Range(minParts, maxParts + 1);
        countToEnable = Mathf.Clamp(countToEnable, 1, allPossibleTargets.Count);
        
        // Shuffle
        for (int i = 0; i < allPossibleTargets.Count; i++)
        {
            RepairTarget temp = allPossibleTargets[i];
            int randomIndex = Random.Range(i, allPossibleTargets.Count);
            allPossibleTargets[i] = allPossibleTargets[randomIndex];
            allPossibleTargets[randomIndex] = temp;
        }

        // Enable/Disable
        for (int i = 0; i < allPossibleTargets.Count; i++)
        {
            if (i < countToEnable)
            {
                allPossibleTargets[i].gameObject.SetActive(true);
            }
            else
            {
                allPossibleTargets[i].gameObject.SetActive(false);
            }
        }
    }

    public IEnumerator DriveIn()
    {
        // Reset state
        if (fixedVisualObj != null) fixedVisualObj.SetActive(false);
        if (visualImage != null) visualImage.gameObject.SetActive(true);

        // Simple translation animation
        float duration = 1.0f;
        float elapsed = 0f;
        RectTransform rt = GetComponent<RectTransform>();
        
        Vector2 endPos = Vector2.zero;
        Vector2 startPos = new Vector2(-1500, 0); // Start from left off screen
        rt.anchoredPosition = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease Out Cubic
            t = 1f - Mathf.Pow(1f - t, 3);
            
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        rt.anchoredPosition = endPos;
    }

    public IEnumerator DriveAway()
    {
        // Enable fixed visual
        if (fixedVisualObj != null) fixedVisualObj.SetActive(true);
        if (visualImage != null) visualImage.gameObject.SetActive(false);

        // Simple translation animation
        float duration = 1.5f;
        float elapsed = 0f;
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(1500, 0); // Move right off screen

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease In Back (starts slow then zooms)
            t = t * t * t; 
            
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
    }
}
