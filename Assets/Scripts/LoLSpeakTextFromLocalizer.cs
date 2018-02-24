using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoLSpeakTextFromLocalizer : MonoBehaviour {

    M8.UI.Texts.Localizer localizer;

    public bool autoPlay;
    public float autoPlayDelay;

    private bool mIsFocus;

    public void Play() {
        if(!string.IsNullOrEmpty(localizer.key))
            LoLManager.instance.SpeakText(localizer.key);
    }

    void OnApplicationFocus(bool focus) {
        mIsFocus = focus;
    }

    void OnEnable() {
        mIsFocus = Application.isFocused;

        if(autoPlay)
            StartCoroutine(DoAutoPlay());
    }
    
    void Awake() {
        if(!localizer)
            localizer = GetComponent<M8.UI.Texts.Localizer>();
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

        Play();
    }
}
