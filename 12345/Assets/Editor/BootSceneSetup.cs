using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BootSceneSetup
{
    [MenuItem("Tools/Boot/Ensure BootLoader in _Boot")] 
    public static void EnsureBootLoaderInBootScene()
    {
        // Ask to save any modified scenes first
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("BootSceneSetup: User cancelled save. Aborting.");
            return;
        }

        // Find _Boot scene in project
        string[] guids = AssetDatabase.FindAssets("_Boot t:Scene");
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("BootSceneSetup: Could not find a scene named '_Boot' in the project.");
            return;
        }

        string bootScenePath = null;
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == "_Boot")
            {
                bootScenePath = path;
                break;
            }
        }
        if (bootScenePath == null && guids.Length > 0)
            bootScenePath = AssetDatabase.GUIDToAssetPath(guids[0]);

        if (string.IsNullOrEmpty(bootScenePath))
        {
            Debug.LogError("BootSceneSetup: Failed to resolve _Boot scene path.");
            return;
        }

        // Remember current scene so we can restore it
        var currentScene = EditorSceneManager.GetActiveScene();

        // Open _Boot scene
        var bootScene = EditorSceneManager.OpenScene(bootScenePath, OpenSceneMode.Single);

        // Check if a BootLoader already exists in the scene
        var existing = Object.FindObjectOfType<BootLoader>();
        if (existing != null)
        {
            Debug.Log("BootSceneSetup: BootLoader already present in _Boot scene.");
        }
        else
        {
            // Create a named GameObject and attach BootLoader
            var go = new GameObject("BootManager");
            go.AddComponent<BootLoader>();
            Debug.Log("BootSceneSetup: Added BootLoader to _Boot scene.");
        }

        // Ensure there's an AudioManager in the Boot scene so AudioListener/logs appear immediately
        var existingAudio = Object.FindObjectOfType<AudioManager>();
        if (existingAudio == null)
        {
            var am = new GameObject("AudioManager");
            am.AddComponent<AudioManager>();
            Debug.Log("BootSceneSetup: Added AudioManager to _Boot scene.");
        }

        // Mark scene dirty and save
        EditorSceneManager.MarkSceneDirty(bootScene);
        EditorSceneManager.SaveScene(bootScene);
        Debug.Log("BootSceneSetup: Saved _Boot scene (with BootLoader/AudioManager if newly added).");

        // Restore previously open scene (if it had a valid path)
        if (!string.IsNullOrEmpty(currentScene.path))
        {
            EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
        }
    }
}


