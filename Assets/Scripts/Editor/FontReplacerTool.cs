#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FontReplacerTool : EditorWindow
{
    public Font newFont;
    public TMP_FontAsset newTMPFont;

    [MenuItem("Tools/Bayanihan/Font Replacer")]
    public static void ShowWindow()
    {
        GetWindow<FontReplacerTool>("Font Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Consistency Enforcer", EditorStyles.boldLabel);
        
        newFont = (Font)EditorGUILayout.ObjectField("Legacy TTF Font", newFont, typeof(Font), false);
        newTMPFont = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font Asset", newTMPFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Replace Fonts in ALL Scenes & Prefabs"))
        {
            if (EditorUtility.DisplayDialog("Confirm", "This will modify all scenes and prefabs. Save your work first!", "Go Ahead", "Cancel"))
            {
                ReplaceFonts();
            }
        }
    }

    private void ReplaceFonts()
    {
        if (newFont == null && newTMPFont == null)
        {
            Debug.LogError("Please assign at least one font to replace with.");
            return;
        }

        int count = 0;

        // 1. Process Prefabs
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            if (ProcessGameObject(prefab, true))
            {
                EditorUtility.SetDirty(prefab);
                count++;
            }
        }

        // 2. Process Scenes
        // Note: This iterates over scenes in Build Settings for safety/relevance
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            
            Scene s = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
            bool sceneModified = false;
            
            foreach (GameObject root in s.GetRootGameObjects())
            {
                if (ProcessGameObject(root, false)) sceneModified = true;
            }

            if (sceneModified)
            {
                EditorSceneManager.MarkSceneDirty(s);
                EditorSceneManager.SaveScene(s);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Font Replacement Complete! Modified {count} assets/scenes.");
    }

    private bool ProcessGameObject(GameObject go, bool isPrefab)
    {
        bool modified = false;

        // Replace Legacy Text
        if (newFont != null)
        {
            Text[] texts = go.GetComponentsInChildren<Text>(true);
            foreach (Text t in texts)
            {
                if (t.font != newFont)
                {
                    if(!isPrefab) Undo.RecordObject(t, "Replace Font");
                    t.font = newFont;
                    modified = true;
                }
            }
        }

        // Replace TMP
        if (newTMPFont != null)
        {
            TextMeshProUGUI[] tmps = go.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI t in tmps)
            {
                if (t.font != newTMPFont)
                {
                    if(!isPrefab) Undo.RecordObject(t, "Replace Font TMP");
                    t.font = newTMPFont;
                    modified = true;
                }
            }
        }

        return modified;
    }
}
#endif
