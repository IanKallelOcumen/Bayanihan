#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Events; // Needed for UnityEventTools
using UnityEngine.UI;

namespace Bayanihan.Tools
{
    public class MenuSetupTool : EditorWindow
    {
        [MenuItem("Tools/Bayanihan/Fix Main Menu")]
        public static void ShowWindow()
        {
            GetWindow<MenuSetupTool>("Main Menu Fixer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Main Menu One-Click Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Fix Main Menu Scene"))
            {
                FixMainMenu();
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Instructions:", EditorStyles.boldLabel);
            GUILayout.Label("1. Open 'Main_Menu' scene.");
            GUILayout.Label("2. Click the button above.");
            GUILayout.Label("3. It will find 'MenuManager' object, swap scripts,");
            GUILayout.Label("   and rewire all buttons automatically.");
        }

        private void FixMainMenu()
        {
            // 1. Find the Menu Manager GameObject
            GameObject menuManagerObj = GameObject.Find("MenuManager");
            if (menuManagerObj == null)
            {
                // Try finding by type if name changed
                MainMenuController existing = FindFirstObjectByType<MainMenuController>();
                if (existing != null) menuManagerObj = existing.gameObject;
            }

            if (menuManagerObj == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find GameObject named 'MenuManager' or existing 'MainMenuController'. Please rename your manager object to 'MenuManager'.", "OK");
                return;
            }

            Undo.RegisterCompleteObjectUndo(menuManagerObj, "Fix Main Menu");

            // 2. Ensure MainMenuController is attached
            MainMenuController controller = menuManagerObj.GetComponent<MainMenuController>();
            if (controller == null)
            {
                // Check for missing script components and remove them
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(menuManagerObj);
                
                // Add new controller
                controller = Undo.AddComponent<MainMenuController>(menuManagerObj);
            }

            // 3. Find Panels
            // Assumption: Panels are children of a Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform mainPanelTrans = FindChildRecursive(canvas.transform, "Main_Menu_Panel");
                Transform settingsPanelTrans = FindChildRecursive(canvas.transform, "Setting_Panel"); // Note: User screenshot showed "Setting_Panel"

                if (mainPanelTrans != null)
                {
                    CanvasGroup mainCg = mainPanelTrans.GetComponent<CanvasGroup>();
                    if (mainCg == null) mainCg = Undo.AddComponent<CanvasGroup>(mainPanelTrans.gameObject);
                    
                    SerializedObject so = new SerializedObject(controller);
                    so.FindProperty("mainMenuPanel").objectReferenceValue = mainCg;
                    so.ApplyModifiedProperties();
                }
                else Debug.LogWarning("Could not find 'Main_Menu_Panel'");

                if (settingsPanelTrans != null)
                {
                    CanvasGroup setCg = settingsPanelTrans.GetComponent<CanvasGroup>();
                    if (setCg == null) setCg = Undo.AddComponent<CanvasGroup>(settingsPanelTrans.gameObject);
                    
                    SerializedObject so = new SerializedObject(controller);
                    so.FindProperty("settingsPanel").objectReferenceValue = setCg;
                    so.ApplyModifiedProperties();
                }
                else Debug.LogWarning("Could not find 'Setting_Panel'");

                // 4. Wire Buttons
                // Start Button
                WireButton(canvas.transform, "Start_Button", controller, "OnStartGame");
                
                // Settings Button (Main Menu)
                WireButton(canvas.transform, "Settings_Button", controller, "OnSettings"); // Name guess
                if (FindChildRecursive(canvas.transform, "Settings_Button") == null)
                     WireButton(canvas.transform, "Setting_Button", controller, "OnSettings"); // Try singular

                // Quit Button
                WireButton(canvas.transform, "Quit_Button", controller, "OnExitGame");

                // Back Button (inside settings)
                WireButton(canvas.transform, "Back_Button", controller, "OnBackFromSettings");
            }

            EditorUtility.SetDirty(menuManagerObj);
            Debug.Log("Main Menu Fix Complete!");
        }

        private void WireButton(Transform root, string buttonName, MainMenuController target, string methodName)
        {
            Transform btnTrans = FindChildRecursive(root, buttonName);
            if (btnTrans != null)
            {
                Button btn = btnTrans.GetComponent<Button>();
                if (btn != null)
                {
                    // Clear existing persistent listeners to avoid duplicates
                    UnityEventTools.RemovePersistentListener(btn.onClick, 0); 
                    // Note: This removes the first one. A safer way is needed if multiple exist, 
                    // but usually we just want to reset it.
                    while (btn.onClick.GetPersistentEventCount() > 0)
                    {
                         UnityEventTools.RemovePersistentListener(btn.onClick, 0);
                    }

                    // Add new listener
                    // Use UnityEditor.Events.UnityEventTools to add persistent listener
                    System.Reflection.MethodInfo method = typeof(MainMenuController).GetMethod(methodName);
                    if (method != null)
                    {
                        var action = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), target, method) as UnityEngine.Events.UnityAction;
                        UnityEventTools.AddPersistentListener(btn.onClick, action);
                        EditorUtility.SetDirty(btn);
                        Debug.Log($"Wired {buttonName} to {methodName}");
                    }
                    else
                    {
                        Debug.LogError($"Method {methodName} not found on MainMenuController!");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Button '{buttonName}' not found in hierarchy.");
            }
        }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            foreach (Transform child in parent)
            {
                Transform result = FindChildRecursive(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
#endif
