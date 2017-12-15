using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoLSpeakTextFromLocalizer : MonoBehaviour {

    M8.UI.Texts.Localizer localizer;

    public void Play() {
        if(!string.IsNullOrEmpty(localizer.key))
            LoLManager.instance.SpeakText(localizer.key);
    }

    void Awake() {
        if(!localizer)
            localizer = GetComponent<M8.UI.Texts.Localizer>();
    }
}
