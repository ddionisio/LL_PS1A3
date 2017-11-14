using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.UIModal.Controller {

    public void Proceed() {
        Close();

        if(GameFlowController.isInstantiated) {
            GameFlowController.ProgressAndLoadNextScene();
        }
        else {
            //this is for test in editor if we started the scene on the level
            int progressInd = GameData.instance.GetProgressFromCurrentScene();
            if(progressInd == -1) {
                if(string.IsNullOrEmpty(GameData.instance.endScene.name))
                    M8.SceneManager.instance.LoadRoot();
                else
                    M8.SceneManager.instance.LoadScene(GameData.instance.endScene.name);
            }
            else
                M8.SceneManager.instance.LoadScene(GameData.instance.scenes[progressInd].name);
        }
    }    
}
