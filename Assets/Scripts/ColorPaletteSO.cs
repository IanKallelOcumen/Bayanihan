using UnityEngine;

[CreateAssetMenu(fileName = "NewColorPalette", menuName = "UI/Color Palette")]
public class ColorPaletteSO : ScriptableObject
{
    public Color primaryColor = Color.white;
    public Color secondaryColor = Color.gray;
    public Color accentColor = Color.cyan;
    public Color textColor = Color.black;
    public Color backgroundColor = Color.blue;
}
