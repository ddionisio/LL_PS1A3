using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper Behaviour to hookup calls to GameFlowController
/// </summary>
public class GameFlowProxy : MonoBehaviour {
    public void LoadScene(string toScene) {
        M8.SceneManager.instance.LoadScene(toScene);
    }

    public void ProgressBegin(string introScene) {
        if(LoLManager.instance.curProgress == 0)
            M8.SceneManager.instance.LoadScene(introScene);
        else {
            LoLMusicPlaylist.instance.PlayStartMusic();
            GameFlowController.LoadCurrentProgressScene();
        }
    }

    public void ProgressStart() {
        GameFlowController.LoadCurrentProgressScene();
    }

    public void Progress() {
        GameFlowController.ProgressAndLoadNextScene();
    }

    public void Complete() {
        GameFlowController.Complete();
    }
}
