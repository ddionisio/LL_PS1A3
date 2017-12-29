﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SimpleJSON;

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
            if(mAudioItems.TryGetValue(mLastSoundBackgroundPath, out bkgrndAudioSrc))
                bkgrndAudioSrc.Stop();
            else
                Debug.LogWarning("Last background path not found? " + mLastSoundBackgroundPath);
        }

        AudioSource audioSrc;
        if(mAudioItems.TryGetValue(path, out audioSrc)) {
            audioSrc.volume = background ? mMusicVolume : mSoundVolume;
            audioSrc.loop = loop;
            audioSrc.Play();
        }

        if(background)
            mLastSoundBackgroundPath = path;
    }

    public override void SpeakText(string key) {
        
    }

    public override void StopCurrentBackgroundSound() {
        if(!string.IsNullOrEmpty(mLastSoundBackgroundPath)) {
            var bkgrndAudioSrc = mAudioItems[mLastSoundBackgroundPath];
            bkgrndAudioSrc.Stop();

            mLastSoundBackgroundPath = null;
        }
    }

    public override void ApplyScore(int score) {
        
    }

    public override void ApplyProgress(int progress, int score) {

        mCurProgress = Mathf.Clamp(progress, 0, progressMax);

        ApplyScore(score);

        ProgressCallback();
    }

    protected override void Start() {
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

            JSONNode langDefs = JSON.Parse(json);

            HandleLanguageDefs(langDefs[mLangCode].ToString());
        }
        else
            HandleLanguageDefs("");

        HandleStartGame("");
    }
}
