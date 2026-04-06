using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Load BootInfo from Resources (created by editor script)
        var info = Resources.Load<BootInfo>("BootInfo");
        if (info == null)
        {
            Debug.LogWarning("BootLoader: BootInfo not found in Resources. No scene will be loaded.");
            yield break;
        }

        int buildIndex = info.previousSceneBuildIndex;
        Debug.Log($"BootLoader: BootInfo loaded -> path='{info.previousScenePath}', name='{info.previousSceneName}', buildIndex={buildIndex}");

        // Try load by build index first (works in player and editor when scene is in Build Settings)
        if (buildIndex >= 0)
        {
            var op = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
            if (op == null)
            {
                Debug.LogError($"BootLoader: Failed to start loading scene by build index {buildIndex}.");
                yield break;
            }
            while (!op.isDone) yield return null;

            var loadedScene = SceneManager.GetSceneByBuildIndex(buildIndex);
            if (loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
                yield return SceneManager.UnloadSceneAsync(gameObject.scene);
                yield break;
            }
            else
            {
                Debug.LogError($"BootLoader: Scene with build index {buildIndex} was not valid after load.");
            }
        }

#if UNITY_EDITOR
        // In the Editor, if the scene wasn't in Build Settings, load it into Play mode by path.
        bool editorPathLoaded = false;
        if (!string.IsNullOrEmpty(info.previousScenePath))
        {
            // LoadSceneInPlayMode allows loading a scene asset into Play mode even if it's not in Build Settings
            var loaded = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                info.previousScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive)
            );

            // wait a few frames to let the scene become valid
            int attempts = 0;
            while (!loaded.IsValid() && attempts++ < 30)
                yield return null;

            if (loaded.IsValid())
            {
                Debug.Log($"BootLoader (Editor): Loaded scene by path '{info.previousScenePath}' into Play mode.");
                SceneManager.SetActiveScene(loaded);
                yield return SceneManager.UnloadSceneAsync(gameObject.scene);
                yield break;
            }
            else
            {
                Debug.LogWarning($"BootLoader (Editor): Failed to load scene by path '{info.previousScenePath}' into Play mode. Will try name-based search.");
            }
        }

        // If loading by path failed, try to find a scene asset with the same name and load it by path
        if (!string.IsNullOrEmpty(info.previousSceneName))
        {
            Debug.Log($"BootLoader (Editor): Trying to find scene asset by name '{info.previousSceneName}'");
            var guids = UnityEditor.AssetDatabase.FindAssets($"{info.previousSceneName} t:Scene");
            if (guids != null && guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                Debug.Log($"BootLoader (Editor): Found scene asset at path '{path}', attempting to load in Play mode.");
                var loaded2 = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
                int attempts2 = 0;
                while (!loaded2.IsValid() && attempts2++ < 30)
                    yield return null;
                if (loaded2.IsValid())
                {
                    Debug.Log($"BootLoader (Editor): Successfully loaded scene '{path}' by name search.");
                    SceneManager.SetActiveScene(loaded2);
                    yield return SceneManager.UnloadSceneAsync(gameObject.scene);
                    yield break;
                }
                else
                {
                    Debug.LogError($"BootLoader (Editor): Failed to load scene found at '{path}' into Play mode.");
                }
            }
            else
            {
                Debug.LogWarning($"BootLoader (Editor): No scene assets found matching name '{info.previousSceneName}'.");
            }
        }
#endif

        // Fallback: try by name (may fail in Editor if scene not in Build Settings)
        string sceneToLoadName = info.previousSceneName;
        if (!string.IsNullOrEmpty(sceneToLoadName))
        {
            var op2 = SceneManager.LoadSceneAsync(sceneToLoadName, LoadSceneMode.Additive);
            if (op2 == null)
            {
                Debug.LogError($"BootLoader: Failed to start loading scene '{sceneToLoadName}'.");
                yield break;
            }
            while (!op2.isDone) yield return null;

            var loadedScene = SceneManager.GetSceneByName(sceneToLoadName);
            if (loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
                yield return SceneManager.UnloadSceneAsync(gameObject.scene);
                yield break;
            }
            else
            {
                Debug.LogError($"BootLoader: Scene '{sceneToLoadName}' was not valid after load.");
            }
        }

        Debug.LogWarning("BootLoader: No previous scene information available or failed to load previous scene.");
    }
}




