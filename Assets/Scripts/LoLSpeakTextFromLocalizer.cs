using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoLSpeakTextFromLocalizer : MonoBehaviour {

    M8.UI.Texts.Localizer localizer;
    public string group = "default";
    
    public bool autoPlay;
    public float autoPlayDelay;

    private bool mIsFocus;
    private bool mIsSpeakCalled;

    private string mGroupAuto;

    public void Play() {
        StopAllCoroutines();

        _Play(false);
    }

    private void _Play(bool isAuto) {
        if(localizer && !string.IsNullOrEmpty(localizer.key)) {
            LoLManager.instance.SpeakText(localizer.key, isAuto ? mGroupAuto : group);
        }
    }

    void OnApplicationFocus(bool focus) {
        mIsFocus = focus;
    }

    void OnEnable() {
        mIsFocus = Application.isFocused;

        if(autoPlay)
            StartCoroutine(DoAutoPlay());
    }

    void OnDestroy() {
        if(autoPlay) {
            if(LoLManager.isInstantiated)
                LoLManager.instance.speakCallback -= OnSpeakCalled;
        }
    }

    void Awake() {
        if(!localizer)
            localizer = GetComponent<M8.UI.Texts.Localizer>();

        if(autoPlay) {
            mGroupAuto = string.IsNullOrEmpty(group) ? "auto" : group + "_auto";

            if(LoLManager.isInstantiated)
                LoLManager.instance.speakCallback += OnSpeakCalled;
        }
    }

    void OnSpeakCalled(LoLManager lolMgr, string key, string group) {
        if(group != mGroupAuto) {
            StopAllCoroutines();
        }
    }

    IEnumerator DoAutoPlay() {
        float lastTime = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup - lastTime < autoPlayDelay) {
            //focus lost
            if(!mIsFocus) {
                float timePassed = Time.realtimeSinceStartup - lastTime;

                //wait for focus to return
                while(!mIsFocus)
                    yield return null;

                //refresh lastTime
                lastTime = Time.realtimeSinceStartup - timePassed;
            }

            yield return null;
        }

        _Play(true);
    }
}
