using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintButton : MonoBehaviour {
    
    private M8.GenericParams mModalParms = new M8.GenericParams();

    public void OpenHint() {
        M8.UIModal.Manager.instance.ModalOpen(Modals.hint, mModalParms);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    void OnDestroy() {
        if(M8.SceneManager.isInstantiated)
            M8.SceneManager.instance.sceneChangePostCallback -= OnSceneChanged;
    }

    void Awake() {
        Hide();

        M8.SceneManager.instance.sceneChangePostCallback += OnSceneChanged;
    }

    void OnSceneChanged() {
        Hide(); //let game controller activate

        mModalParms[ModalHint.parmLevelName] = M8.SceneManager.instance.curScene.name;
    }
}
