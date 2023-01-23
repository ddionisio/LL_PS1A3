using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalInfo : M8.UIModal.Controller, M8.UIModal.Interface.IPush, M8.UIModal.Interface.IPop {
    public const string parmBlockName = "bName";
    
    [Header("Title")]
    public Text titleLabel;
    public bool titleAutoSpeech = true;
    public float titleAutoSpeechDelay = 0.3f;
    public string titleAutoSpeechGroup = "info_auto";

    [Header("Pages")]
    public Transform pagesRoot;

    private bool mIsPaused;

    private string mBlockName;
    private string mBlockTextRef;

    private Dictionary<string, GameObject> mPages;

    private bool mIsFocus;

    public void PlayTitleSpeech() {
        if(!string.IsNullOrEmpty(mBlockTextRef))
            LoLManager.instance.SpeakText(mBlockTextRef, titleAutoSpeechGroup);
    }

    void OnApplicationFocus(bool focus) {
        mIsFocus = focus;
    }

    void Awake() {
        mPages = new Dictionary<string, GameObject>();

        if(pagesRoot) {
            for(int i = 0; i < pagesRoot.childCount; i++) {
                var page = pagesRoot.GetChild(i);
                var pageGO = page.gameObject;

                pageGO.SetActive(false);

                mPages.Add(page.name, pageGO);
            }
        }
    }

    void M8.UIModal.Interface.IPush.Push(M8.GenericParams parms) {
        Pause(true);

        HideCurrentPage();

        mIsFocus = Application.isFocused;

        string blockName = parms.GetValue<string>(parmBlockName);

        if(!string.IsNullOrEmpty(blockName)) {
            var blockInfo = GameData.instance.GetBlockInfo(blockName);
            if(blockInfo != null) {
                mBlockName = blockInfo.name;
                mBlockTextRef = blockInfo.nameDisplayRef;

                //setup title
                if(titleLabel)
                    titleLabel.text = M8.Localize.Get(mBlockTextRef);

                //play title text speech
                if(titleAutoSpeech)
                    StartCoroutine(DoPlayTitleSpeechDelay());

                //activate the correct panel
                GameObject pageGO;
                if(mPages.TryGetValue(mBlockName, out pageGO))
                    pageGO.SetActive(true);
            }
        }
    }

    void M8.UIModal.Interface.IPop.Pop() {
        HideCurrentPage();

        Pause(false);
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

    void HideCurrentPage() {
        if(!string.IsNullOrEmpty(mBlockName)) {
            GameObject pageGO;
            if(mPages.TryGetValue(mBlockName, out pageGO))
                pageGO.SetActive(false);

            mBlockName = null;
        }
    }

    IEnumerator DoPlayTitleSpeechDelay() {
        float lastTime = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup - lastTime < titleAutoSpeechDelay) {
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

        PlayTitleSpeech();
    }
}
