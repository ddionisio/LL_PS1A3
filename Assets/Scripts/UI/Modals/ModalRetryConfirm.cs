using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalRetryConfirm : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {

    public void Confirm() {
        Close();
        M8.SceneManager.instance.Reload();
    }

    void OnDestroy() {
        //fail-safe
        UnhookInput();
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {

        M8.InputManager.instance.AddButtonCall(0, InputAction.Escape, OnInputEscape);
    }

    void M8.UIModal.Interface.IPop.Pop() {

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
}
