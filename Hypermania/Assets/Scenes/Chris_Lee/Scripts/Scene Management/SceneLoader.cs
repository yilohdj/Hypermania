using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    #region Global
    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        //We manually load the first scene
        _sceneIDMap.Add(SceneID.MainMenu, SceneDataBank.MENU);
    }
    #endregion Global

    #region Scene Mapping

    private Dictionary<SceneID, string> _sceneIDMap = new();

    private bool _isBusy = false;

    #endregion Scene Mapping

    public delegate void SceneLoad(SceneID loaded);
    public event SceneLoad OnSceneLoad;

    public SceneLoadPlan LoadNewScene() {
        return new SceneLoadPlan();
    }

    public Coroutine ExecutePlan(SceneLoadPlan plan) {
        if (_isBusy) return null;
        _isBusy = true;

        return StartCoroutine(DoSceneTransition(plan));
    }

    private IEnumerator DoSceneTransition(SceneLoadPlan plan) {
        foreach (var idToScene in plan.ScenesToLoad) {
            if (_sceneIDMap.ContainsKey(idToScene.Key)) {
                yield return DoUnloadScene(idToScene.Key);
            }

            yield return DoLoadScene(idToScene.Key, idToScene.Value, plan.ActiveSceneName == idToScene.Value);
        }

        foreach (SceneID id in plan.ScenesToUnload) {
            yield return DoUnloadScene(id);
        }

        _isBusy = false;
    }

    private IEnumerator DoLoadScene(SceneID id, string sceneName, bool setActive) {
        AsyncOperation loadAction = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        
        if (loadAction == null) yield break;
        loadAction.allowSceneActivation = false;

        while (loadAction.progress < 0.9f) {
            yield return null;
        }

        loadAction.allowSceneActivation = true;

        while (!loadAction.isDone) {
            yield return null;
        }

        if (setActive) {
            Scene loadingScene = SceneManager.GetSceneByName(sceneName);
            if (loadingScene.IsValid() && loadingScene.isLoaded) {
                SceneManager.SetActiveScene(loadingScene);
            }
        }

        _sceneIDMap[id] = sceneName;
        OnSceneLoad?.Invoke(id);
    }

    private IEnumerator DoUnloadScene(SceneID id) {
        if (!_sceneIDMap.TryGetValue(id, out string sceneName)) yield break;
        if (string.IsNullOrEmpty(sceneName)) yield break;

        AsyncOperation unloadAction = SceneManager.UnloadSceneAsync(sceneName);
        if (unloadAction != null) {
            while (!unloadAction.isDone) {
                yield return null;
            }
        }
        
        _sceneIDMap.Remove(id);

    }
}
