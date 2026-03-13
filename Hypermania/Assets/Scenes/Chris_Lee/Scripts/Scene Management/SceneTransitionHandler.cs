using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionHandler : MonoBehaviour {
    [SerializeField] private SceneID sceneID;
    [SerializeField] private SceneID unloadSceneID;
    
    public void Transition() {
        SceneLoader.Instance.LoadNewScene()
            .Load(sceneID, SceneDataBank.SceneMap[sceneID]).Unload(unloadSceneID).Execute();
    }
}
