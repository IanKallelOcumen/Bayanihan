#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class TriggerSetup
{
    static TriggerSetup()
    {
        // Run setup once after recompilation
        EditorApplication.delayCall += Run;
    }

    static void Run()
    {
        if (!SessionState.GetBool("PitstopSetupRan", false))
        {
            Debug.Log("Triggering Pitstop Panic Setup...");
            PitstopSetup.SetupAll();
            SessionState.SetBool("PitstopSetupRan", true);
            Debug.Log("Setup Triggered.");
        }
    }
}
#endif
