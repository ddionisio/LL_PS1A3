using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalOptions : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    public Slider musicSlider;
    public Slider soundSlider;

    private bool mIsPaused;

    void OnDestroy() {
        //fail-safe
        Pause(false);

        UnhookInput();
    }

    void Awake() {
        musicSlider.onValueChanged.AddListener(OnMusicSliderValue);
        soundSlider.onValueChanged.AddListener(OnSoundSliderValue);
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        if(aActive) {
            musicSlider.normalizedValue = LoLManager.instance.musicVolume;
            soundSlider.normalizedValue = LoLManager.instance.soundVolume;
        }
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        Pause(true);

        M8.InputManager.instance.AddButtonCall(0, InputAction.Escape, OnInputEscape);
    }

    void M8.UIModal.Interface.IPop.Pop() {
        LoLManager.instance.ApplyVolumes(soundSlider.normalizedValue, musicSlider.normalizedValue, true);

        Pause(false);

        UnhookInput();
    }

    void OnInputEscape(M8.InputManager.Info data) {
        if(data.state == M8.InputManager.State.Released)
            Close();
    }

    void OnMusicSliderValue(float val) {
        LoLManager.instance.ApplyVolumes(soundSlider.normalizedValue, musicSlider.normalizedValue, false);
    }

    void OnSoundSliderValue(float val) {
        LoLManager.instance.ApplyVolumes(soundSlider.normalizedValue, musicSlider.normalizedValue, false);
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
