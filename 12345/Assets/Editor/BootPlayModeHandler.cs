using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class BootPlayModeHandler
{
    static BootPlayModeHandler()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // We're about to enter play mode from edit mode
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Ask to save modified scenes
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // User cancelled save -> cancel entering play mode
                EditorApplication.isPlaying = false;
                return;
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            string previousPath = activeScene.path;
            string previousName = activeScene.name;

            // Determine build index
            int buildIndex = -1;
            if (!string.IsNullOrEmpty(previousPath))
            {
                buildIndex = SceneUtility.GetBuildIndexByScenePath(previousPath);
            }

            // If not in Build Settings, add it so runtime can load by build index
            if (buildIndex < 0 && !string.IsNullOrEmpty(previousPath))
            {
                try
                {
                    var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                    bool exists = scenes.Exists(s => s.path == previousPath);
                    if (!exists)
                    {
                        scenes.Add(new EditorBuildSettingsScene(previousPath, true));
                        EditorBuildSettings.scenes = scenes.ToArray();
                        AssetDatabase.SaveAssets();
                        buildIndex = SceneUtility.GetBuildIndexByScenePath(previousPath);
                        Debug.Log($"BootPlayModeHandler: Added '{previousPath}' to Build Settings (index {buildIndex}).");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"BootPlayModeHandler: Failed to add scene to Build Settings: {ex.Message}");
                }
            }

            // Create or update BootInfo asset in Resources
            const string resourcesDir = "Assets/Resources";
            const string bootInfoPath = "Assets/Resources/BootInfo.asset";

            BootInfo info = AssetDatabase.LoadAssetAtPath<BootInfo>(bootInfoPath);
            if (info == null)
            {
                if (!AssetDatabase.IsValidFolder(resourcesDir))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                info = ScriptableObject.CreateInstance<BootInfo>();
                AssetDatabase.CreateAsset(info, bootInfoPath);
            }

            info.previousScenePath = previousPath;
            info.previousSceneName = previousName;
            info.previousSceneBuildIndex = buildIndex;

            EditorUtility.SetDirty(info);
            AssetDatabase.SaveAssets();

            Debug.Log($"BootPlayModeHandler: Saved previous scene path='{previousPath}', name='{previousName}', buildIndex={buildIndex} to {bootInfoPath}");

            // Find _Boot scene in project
            string[] guids = AssetDatabase.FindAssets("_Boot t:Scene");
            if (guids == null || guids.Length == 0)
            {
                Debug.LogError("BootPlayModeHandler: Could not find a scene named '_Boot' in the project. Aborting play.");
                EditorApplication.isPlaying = false;
                return;
            }

            // Prefer exact name match on filename
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
            {
                bootScenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
            }

            if (string.IsNullOrEmpty(bootScenePath))
            {
                Debug.LogError("BootPlayModeHandler: Failed to resolve _Boot scene path. Aborting play.");
                EditorApplication.isPlaying = false;
                return;
            }

            Debug.Log($"BootPlayModeHandler: Opening _Boot scene at path '{bootScenePath}'");

            // Open _Boot scene (single) so play will start in it
            var bootScene = EditorSceneManager.OpenScene(bootScenePath, OpenSceneMode.Single);

            // Ensure BootLoader exists in the _Boot scene so it can load the previous scene at runtime
            var existingLoader = Object.FindObjectOfType<BootLoader>();
            if (existingLoader == null)
            {
                var go = new GameObject("BootManager");
                go.AddComponent<BootLoader>();
                Debug.Log("BootPlayModeHandler: Added BootLoader to _Boot scene automatically.");
            }

            // Ensure AudioManager exists so an AudioListener will be present and a log will appear
            var existingAudio = Object.FindObjectOfType<AudioManager>();
            if (existingAudio == null)
            {
                var am = new GameObject("AudioManager");
                am.AddComponent<AudioManager>();
                Debug.Log("BootPlayModeHandler: Added AudioManager to _Boot scene automatically.");
            }

            // Save changes to the boot scene before entering Play
            EditorSceneManager.MarkSceneDirty(bootScene);
            EditorSceneManager.SaveScene(bootScene);
        }
    }
}




