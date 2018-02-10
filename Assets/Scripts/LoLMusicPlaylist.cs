using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoLMusicPlaylist : M8.SingletonBehaviour<LoLMusicPlaylist> {
    [System.Serializable]
    public struct Item {
        public string path;
        public float duration;
        public bool disabled;
    }

    public string startMusicPath;

    public Item[] items;

    private Coroutine mRout;
    private float mLastTime;

    private float mOutOfFocusDiffTime;
    private bool mIsOutOfFocus;

    public void PlayStartMusic() {
        Stop();

        if(!string.IsNullOrEmpty(startMusicPath))
            LoLManager.instance.PlaySound(startMusicPath, true, true);
        else
            Debug.Log("intro music is not set.");
    }

    public void Play() {
        if(mRout != null)
            return; //already playing

        mRout = StartCoroutine(DoPlaylist());
    }

    public void Stop() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        LoLManager.instance.StopCurrentBackgroundSound();
    }

    private void OnApplicationFocus(bool focus) {
        mIsOutOfFocus = !focus;

        if(focus) {
            mLastTime = Time.realtimeSinceStartup - mOutOfFocusDiffTime;
        }
        else {
            mOutOfFocusDiffTime = Time.realtimeSinceStartup - mLastTime;
        }
    }

    void OnEnable() {
        mIsOutOfFocus = !Application.isFocused;
    }

    void OnDisable() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    IEnumerator DoPlaylist() {
        int index = 0;

        var waitShortDelay = new WaitForSeconds(0.3f);

        while(true) {
            var item = items[index];
            if(!item.disabled && !string.IsNullOrEmpty(item.path)) {
                LoLManager.instance.StopCurrentBackgroundSound();

                yield return waitShortDelay;

                LoLManager.instance.PlaySound(item.path, true, true);

                mLastTime = Time.realtimeSinceStartup;
                while(Time.realtimeSinceStartup - mLastTime < item.duration)
                    yield return null;

                while(mIsOutOfFocus)
                    yield return null;
            }

            index++;
            if(index == items.Length)
                index = 0;
        }
    }
}
