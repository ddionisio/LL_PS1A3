using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using fastJSON;

public class LoLManagerMockup : LoLManager {
    [Header("Mockup")]
    public GameObject audioRoot;
    public TextAsset localizeText;

    private Dictionary<string, AudioSource> mAudioItems;

    protected override void ApplyVolumes(float sound, float music, float fade) {
        //update playing audios
        foreach(var pair in mAudioItems) {
            var audio = pair.Value;

            var isBackground = pair.Key == mLastSoundBackgroundPath;

            if(audio.isPlaying) {
                audio.volume = isBackground ? music : sound;
            }
        }
    }

    public override void PlaySound(string path, bool background, bool loop) {
        if(background && !string.IsNullOrEmpty(mLastSoundBackgroundPath)) {
            AudioSource bkgrndAudioSrc;
            if(mAudioItems.TryGetValue(mLastSoundBackgroundPath, out bkgrndAudioSrc)) {
                bkgrndAudioSrc.Stop();

                //Debug.Log("Stop Background: " + mLastSoundBackgroundPath);
            }
            else
                Debug.LogWarning("Last background path not found? " + mLastSoundBackgroundPath);
        }

        if(!_ignorePlaySoundOnMute || (background ? mMusicVolume > 0f : mSoundVolume > 0f)) {
            AudioSource audioSrc;
            if(mAudioItems.TryGetValue(path, out audioSrc)) {
                audioSrc.volume = background ? mMusicVolume : mSoundVolume;
                audioSrc.loop = loop;
                audioSrc.Play();
            }
        }

        if(background) {
            //Debug.Log("Played Background: " + path);

            mLastSoundBackgroundPath = path;
        }
    }

    public override void StopSound(string path) {
        AudioSource audioSrc;
        if(mAudioItems.TryGetValue(path, out audioSrc))
            audioSrc.Stop();

        if(mLastSoundBackgroundPath == path)
            mLastSoundBackgroundPath = null;
    }

    public override void SpeakText(string key, string group) {
        SpeakCalled(key, group);
    }

    public override void StopCurrentBackgroundSound() {
        if(!string.IsNullOrEmpty(mLastSoundBackgroundPath)) {
            var bkgrndAudioSrc = mAudioItems[mLastSoundBackgroundPath];
            bkgrndAudioSrc.Stop();

            mLastSoundBackgroundPath = null;
        }
    }
    
    public override void ApplyProgress(int progress, int score) {

        mCurProgress = Mathf.Clamp(progress, 0, progressMax);
        
        ProgressCallback();
    }

    protected override IEnumerator Start() {
        mLangCode = "en";
        mCurProgress = 0;

        //setup audio sources
        var audioSources = audioRoot.GetComponentsInChildren<AudioSource>();
        mAudioItems = new Dictionary<string, AudioSource>();
        for(int i = 0; i < audioSources.Length; i++) {
            mAudioItems.Add(audioSources[i].name, audioSources[i]);
        }

        SetupVolumes();
                                
        if(localizeText) {
            string json = localizeText.text;

            Dictionary<string, object> langDefs = JSON.Parse(json) as Dictionary<string, object>;
            ParseLanguage(JSON.ToJSON(langDefs[mLangCode]));
        }

        //ParseGameStart("");

        mIsReady = true;

        yield return null;
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.P)) {
            if(mPaused)
                HandleGameStateChange(LoLSDK.GameState.Resumed);
            else
                HandleGameStateChange(LoLSDK.GameState.Paused);
        }
    }
}
