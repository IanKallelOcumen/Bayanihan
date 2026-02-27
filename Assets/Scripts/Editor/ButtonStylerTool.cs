#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ButtonStylerTool : EditorWindow
{
    public Sprite buttonSprite; // Should be a 9-sliced sprite

    [MenuItem("Tools/Bayanihan/Button Styler")]
    public static void ShowWindow()
    {
        GetWindow<ButtonStylerTool>("Button Styler");
    }

    private void OnGUI()
    {
        buttonSprite = (Sprite)EditorGUILayout.ObjectField("9-Slice Sprite", buttonSprite, typeof(Sprite), false);

        if (GUILayout.Button("Style All Buttons"))
        {
            StyleButtons();
        }
    }

    private void StyleButtons()
    {
        if (buttonSprite == null)
        {
            Debug.LogError("Assign a sprite first!");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            Button[] buttons = prefab.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                Image img = btn.GetComponent<Image>();
                if (img != null)
                {
                    Undo.RecordObject(img, "Style Button");
                    img.sprite = buttonSprite;
                    img.type = Image.Type.Sliced;
                }
                
                // Set Transitions
                Undo.RecordObject(btn, "Style Button Trans");
                btn.transition = Selectable.Transition.SpriteSwap;
                
                SpriteState ss = new SpriteState();
                ss.highlightedSprite = buttonSprite; // Ideally different
                ss.pressedSprite = buttonSprite; // Ideally different
                ss.disabledSprite = buttonSprite; // Ideally different
                btn.spriteState = ss;
            }
        }
        Debug.Log("Buttons Styled.");
    }
}
#endif
