using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalRetryConfirm : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {

    private bool mIsPaused;

    public void Confirm() {
        Close();

        //show hint after reloading
        string curSceneName = M8.SceneManager.instance.curScene.name;
        if(ModalHint.GetPageCount(curSceneName) > 0) {
            GameData.instance.SetHintVisible(curSceneName, true);
        }

        M8.SceneManager.instance.Reload();
    }

    void OnDestroy() {
        //fail-safe
        Pause(false);

        UnhookInput();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        Pause(true);

        M8.InputManager.instance.AddButtonCall(0, InputAction.Escape, OnInputEscape);
    }

    void M8.UIModal.Interface.IPop.Pop() {
        Pause(false);

        UnhookInput();
    }

    void OnInputEscape(M8.InputManager.Info data) {
        if(data.state == M8.InputManager.State.Released)
            Close();
    }

    void UnhookInput() {
        if(M8.InputManager.instance)
            M8.InputManager.instance.RemoveButtonCall(OnInputEscape);
    }

    void Pause(bool pause) {
        if(mIsPaused != pause) {
            mIsPaused = pause;

            if(M8.SceneManager.instance) {
                if(mIsPaused)
                    M8.SceneManager.instance.Pause();
                else
                    M8.SceneManager.instance.Resume();
            }
        }
    }
}
