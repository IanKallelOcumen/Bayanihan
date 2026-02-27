using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutGroup))]
public class LayoutSpacingEnforcer : MonoBehaviour
{
    public SpacingSystemSO spacingSystem;
    public PaddingSize paddingSize = PaddingSize.Medium;
    public PaddingSize spacingSize = PaddingSize.Medium;

    private void OnValidate()
    {
        if (spacingSystem == null) return;

        LayoutGroup layoutGroup = GetComponent<LayoutGroup>();
        if (layoutGroup != null)
        {
            int pad = spacingSystem.GetPadding(paddingSize);
            layoutGroup.padding = new RectOffset(pad, pad, pad, pad);
            
            if (layoutGroup is HorizontalLayoutGroup hGroup)
            {
                hGroup.spacing = spacingSystem.GetPadding(spacingSize);
            }
            else if (layoutGroup is VerticalLayoutGroup vGroup)
            {
                vGroup.spacing = spacingSystem.GetPadding(spacingSize);
            }
            else if (layoutGroup is GridLayoutGroup gGroup)
            {
                int space = spacingSystem.GetPadding(spacingSize);
                gGroup.spacing = new Vector2(space, space);
            }
        }
    }
}
