using UnityEngine;

[CreateAssetMenu(fileName = "NewUITheme", menuName = "Game/UI Theme")]
public class UITheme : ScriptableObject
{
    [Header("Colors")]
    public Color primaryColor = new Color(0.2f, 0.6f, 1.0f);   // Blue
    public Color secondaryColor = new Color(0.1f, 0.1f, 0.15f); // Dark Blue/Grey
    public Color accentColor = new Color(1.0f, 0.8f, 0.2f);    // Gold/Yellow
    public Color textColor = Color.white;
    public Color panelBackgroundColor = new Color(0f, 0f, 0f, 0.8f);

    [Header("Typography")]
    public Font headerFont; // Legacy font reference
    public Font bodyFont;   // Legacy font reference
    public int headerSize = 32;
    public int bodySize = 14;

    [Header("Layout")]
    public float padding = 16f;
    public float spacing = 8f;
    public float cornerRadius = 8f; // For Sprite generation if needed
}
