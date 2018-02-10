using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalOptions : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    [Header("Slider Mode")]
    public Slider musicSlider;
    public Slider soundSlider;

    [Header("Toggle Mode")]
    [M8.Localize]
    public string onStringRef;
    [M8.Localize]
    public string offStringRef;

    public Text musicToggleLabel;
    public Text soundToggleLabel;

    public bool toggleRefreshMusic; //turn off music and save path, replay on turn on

    private bool mIsPaused;

    private float mLastMusicVolume;
    private float mLastSoundVolume;
    private string mLastMusicPlayingPath;
    private bool mLastMusicIsLoop;

    void OnDestroy() {
        //fail-safe
        Pause(false);

        UnhookInput();
    }

    void Awake() {
        if(musicSlider) musicSlider.onValueChanged.AddListener(OnMusicSliderValue);
        if(soundSlider) soundSlider.onValueChanged.AddListener(OnSoundSliderValue);
    }

    public override void SetActive(bool aActive) {
        base.SetActive(aActive);

        if(aActive) {
            if(musicSlider && musicSlider.gameObject.activeInHierarchy) musicSlider.normalizedValue = LoLManager.instance.musicVolume;
            if(soundSlider && soundSlider.gameObject.activeInHierarchy) soundSlider.normalizedValue = LoLManager.instance.soundVolume;

            mLastMusicVolume = LoLManager.instance.musicVolume;
            mLastSoundVolume = LoLManager.instance.soundVolume;

            UpdateToggleStates();
        }
    }

    public void ToggleMusic() {
        bool isOn = LoLManager.instance.musicVolume > 0f;

        if(isOn) { //turn off
            mLastMusicVolume = LoLManager.instance.musicVolume;

            //save music path playing
            if(toggleRefreshMusic) {
                mLastMusicPlayingPath = LoLManager.instance.lastSoundBackgroundPath;
                mLastMusicIsLoop = LoLManager.instance.lastSoundBackgroundIsLoop;

                LoLManager.instance.StopCurrentBackgroundSound();
            }

            LoLManager.instance.ApplyVolumes(LoLManager.instance.soundVolume, 0f, true);
        }
        else { //turn on
            if(mLastMusicVolume == 0f) //need to set to default
                mLastMusicVolume = LoLManager.musicVolumeDefault;

            LoLManager.instance.ApplyVolumes(LoLManager.instance.soundVolume, mLastMusicVolume, true);

            //play back last music if there's no music playing
            if(toggleRefreshMusic) {
                if(!string.IsNullOrEmpty(LoLManager.instance.lastSoundBackgroundPath)) {
                    LoLManager.instance.PlaySound(LoLManager.instance.lastSoundBackgroundPath, true, LoLManager.instance.lastSoundBackgroundIsLoop);
                }
                else if(!string.IsNullOrEmpty(mLastMusicPlayingPath)) {
                    LoLManager.instance.PlaySound(mLastMusicPlayingPath, true, mLastMusicIsLoop);
                }

                mLastMusicPlayingPath = null;
            }
        }

        UpdateToggleStates();
    }

    public void ToggleSound() {
        bool isOn = LoLManager.instance.soundVolume > 0f;

        if(isOn) { //turn off
            mLastSoundVolume = LoLManager.instance.soundVolume;

            LoLManager.instance.ApplyVolumes(0f, LoLManager.instance.musicVolume, true);
        }
        else { //turn on
            if(mLastSoundVolume == 0f) //need to set to default
                mLastSoundVolume = LoLManager.soundVolumeDefault;

            LoLManager.instance.ApplyVolumes(mLastSoundVolume, LoLManager.instance.musicVolume, true);
        }

        UpdateToggleStates();
    }

    void UpdateToggleStates() {
        if(musicToggleLabel) {
            string txt = LoLManager.instance.musicVolume > 0f ? M8.Localize.Get(onStringRef) : M8.Localize.Get(offStringRef);
            musicToggleLabel.text = txt;
        }

        if(soundToggleLabel) {
            string txt = LoLManager.instance.soundVolume > 0f ? M8.Localize.Get(onStringRef) : M8.Localize.Get(offStringRef);
            soundToggleLabel.text = txt;
        }
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        Pause(true);

        M8.InputManager.instance.AddButtonCall(0, InputAction.Escape, OnInputEscape);
    }

    void M8.UIModal.Interface.IPop.Pop() {
        if(soundSlider && soundSlider.gameObject.activeInHierarchy && musicSlider && musicSlider.gameObject.activeInHierarchy)
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
