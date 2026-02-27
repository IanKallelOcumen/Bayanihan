#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class UIInventoryTool : EditorWindow
{
    [MenuItem("Tools/Bayanihan/Generate UI Inventory")]
    public static void ShowWindow()
    {
        GetWindow<UIInventoryTool>("UI Inventory");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Scan All Scenes and Generate CSV"))
        {
            GenerateInventory();
        }
    }

    private void GenerateInventory()
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Scene,ObjectType,ObjectName,Path,Status");

        string[] guids = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            List<Component> components = new List<Component>();
            
            // Find all UI components
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                components.AddRange(root.GetComponentsInChildren<Canvas>(true));
                components.AddRange(root.GetComponentsInChildren<Image>(true));
                components.AddRange(root.GetComponentsInChildren<Text>(true));
                components.AddRange(root.GetComponentsInChildren<Button>(true));
                components.AddRange(root.GetComponentsInChildren<Slider>(true));
                components.AddRange(root.GetComponentsInChildren<ScrollRect>(true));
                // Add TMPro if needed, assuming legacy for now
            }

            foreach (Component c in components)
            {
                string status = "Legacy";
                // Heuristic: If it has a specific script or naming convention, mark updated
                if (c.GetComponent<LayoutSpacingEnforcer>() != null) status = "Updated";

                csv.AppendLine($"{scene.name},{c.GetType().Name},{c.name},{GetPath(c.transform)},{status}");
            }
        }

        string outputPath = "UI_Inventory.csv";
        File.WriteAllText(outputPath, csv.ToString());
        Debug.Log($"UI Inventory generated at {Path.GetFullPath(outputPath)}");
        AssetDatabase.Refresh();
    }

    private string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
#endif
