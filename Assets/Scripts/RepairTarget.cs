using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class RepairTarget : MonoBehaviour
{
    [Header("Repair Configuration")]
    [SerializeField] public ToolType requiredTool; 

    [Header("Visual Feedback")]
    [SerializeField] private GameObject brokenVisual; 
    [SerializeField] private GameObject fixedVisual;  
    [SerializeField] private ParticleSystem repairParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip repairSound;

    private bool isFixed = false;

    public bool IsFixed => isFixed;

    private void Start()
    {
        if (brokenVisual != null) brokenVisual.SetActive(true);
        if (fixedVisual != null) fixedVisual.SetActive(false);
    }

    public void Fix()
    {
        if (isFixed) return;

        isFixed = true;

        if (brokenVisual != null) brokenVisual.SetActive(false);
        if (fixedVisual != null) fixedVisual.SetActive(true);

        if (repairParticles != null)
        {
            repairParticles.Play();
        }

        if (audioSource != null && repairSound != null)
        {
            audioSource.PlayOneShot(repairSound);
        }

        // Add shake/punch effect
        StartCoroutine(PunchScale());

        if (RepairGameManager.Instance != null)
        {
            RepairGameManager.Instance.OnPartFixed();
        }
    }

    private IEnumerator PunchScale()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 punchScale = originalScale * 1.3f;
        float duration = 0.15f;
        float t = 0;

        // Scale Up
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, punchScale, t / duration);
            yield return null;
        }

        t = 0;
        // Scale Down
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(punchScale, originalScale, t / duration);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
