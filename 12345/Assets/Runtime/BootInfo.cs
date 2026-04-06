using UnityEngine;

/// <summary>
/// Small ScriptableObject used to pass the editor's "previous scene" information
/// to the runtime after entering Play mode. This asset is created/updated by the
/// editor handler before switching to the _Boot scene.
/// </summary>
[CreateAssetMenu(menuName = "Boot/BootInfo")]
public class BootInfo : ScriptableObject
{
    [Tooltip("Path to the previously open scene (Assets/... .unity)")]
    public string previousScenePath;

    [Tooltip("Name of the previously open scene")]
    public string previousSceneName;

    [Tooltip("Build index of the previously open scene. -1 when not in Build Settings")]
    public int previousSceneBuildIndex = -1;
}

