using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Based on Code Otter's multi-scene builder class.
/// </summary>
public class SceneLoadPlan {
    public Dictionary<SceneID, string> ScenesToLoad { get; } = new();
    public List<SceneID> ScenesToUnload { get; } = new();
    public string ActiveSceneName { get; private set; }
    public SceneID ActiveSceneID { get; private set;}

    public SceneLoadPlan Load(SceneID id, string sceneName, bool setActive = false) {
        ScenesToLoad[id] = sceneName;

        if (setActive) {
            ActiveSceneName = sceneName;
            ActiveSceneID = id;
        }
        return this;
    }

    public SceneLoadPlan Unload(SceneID id) {
        ScenesToUnload.Add(id);
        return this;
    }

    public Coroutine Execute() {
        return SceneLoader.Instance.ExecutePlan(this);
    }
}
