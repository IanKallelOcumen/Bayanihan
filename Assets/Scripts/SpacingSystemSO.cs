using UnityEngine;

[CreateAssetMenu(fileName = "NewSpacingSystem", menuName = "UI/Spacing System")]
public class SpacingSystemSO : ScriptableObject
{
    public int paddingSmall = 4;
    public int paddingMedium = 8;
    public int paddingLarge = 16;
    public int paddingExtraLarge = 24;
    public int paddingHuge = 32;

    public int GetPadding(PaddingSize size)
    {
        switch (size)
        {
            case PaddingSize.Small: return paddingSmall;
            case PaddingSize.Medium: return paddingMedium;
            case PaddingSize.Large: return paddingLarge;
            case PaddingSize.ExtraLarge: return paddingExtraLarge;
            case PaddingSize.Huge: return paddingHuge;
            default: return paddingMedium;
        }
    }
}

public enum PaddingSize
{
    Small,
    Medium,
    Large,
    ExtraLarge,
    Huge
}
