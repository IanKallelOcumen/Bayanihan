using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animation))]
public class LowFuelIndicator : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] float maxWarningLevel = 0.2f;

    Image image;
    Animation blinkingAnimation;

    GameManager gameManager;

    void Start()
    {
        image = GetComponent<Image>();
        blinkingAnimation = GetComponent<Animation>();
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        if (gameManager == null) return;
        float level = gameManager.GetFuelLevel();
        if (level <= maxWarningLevel && level > 0f) Blink(true);
        else Blink(false);
    }

    void Blink(bool blink)
    {
        if (image == null) return;
        Color color = image.color;
        if (blink)
        {
            color.a = 1f;
            if (blinkingAnimation != null) blinkingAnimation.Play();
        }
        else
        {
            color.a = 0f;
            if (blinkingAnimation != null) blinkingAnimation.Stop();
        }
        image.color = color;
    }
}
