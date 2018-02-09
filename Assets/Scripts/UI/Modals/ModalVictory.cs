using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalVictory : M8.UIModal.Controller, M8.UIModal.Interface.IOpen {
    public string soundVictoryPath;

    public float newScoreDelay = 1f;

    public Text currentScoreText;
    public Text levelScoreText;
    public Text hintScoreText;
    public Text newScoreText;

    public Image[] hintIcons;

    public Color hintIconDisableColor;

    private Coroutine mPlayNewScoreRout;

    private int mCurScore;
    private int mNewScore;

    public void PlayNewScore() {
        if(mPlayNewScoreRout != null)
            StopCoroutine(mPlayNewScoreRout);

        mPlayNewScoreRout = StartCoroutine(DoPlayNewScore());
    }

    public void Proceed() {
        Close();

        HUD.instance.HideAll();

        //update score
        GameData.instance.currentScore = mNewScore;

        if(GameStart.isStarted) {
            GameFlowController.ProgressAndLoadNextScene();
        }
        else {
            //this is for test in editor if we started the scene on the level
            int progressInd = GameData.instance.GetProgressFromCurrentScene();
            if(progressInd == -1) {
                if(string.IsNullOrEmpty(GameData.instance.endScene.name))
                    M8.SceneManager.instance.LoadRoot();
                else
                    M8.SceneManager.instance.LoadScene(GameData.instance.endScene.name);
            }
            else
                M8.SceneManager.instance.LoadScene(GameData.instance.scenes[progressInd].name);
        }
    }

    void OnDisable() {
        mPlayNewScoreRout = null;
    }

    void M8.UIModal.Interface.IOpen.Open() {
        HUD.instance.HideAllMisc();

        //LoLManager.instance.PlaySound(soundVictoryPath, false, false);

        string curSceneName = M8.SceneManager.instance.curScene.name;
        int hintCount = ModalHint.GetPageCount(curSceneName);
        int hintCounter = GameData.instance.GetHintCounter(curSceneName);

        for(int i = 0; i < hintCounter; i++) {
            hintIcons[i].gameObject.SetActive(true);
            hintIcons[i].color = Color.white;
        }

        for(int i = hintCounter; i < hintCount; i++) {
            hintIcons[i].gameObject.SetActive(true);
            hintIcons[i].color = hintIconDisableColor;
        }

        for(int i = hintCount; i < hintIcons.Length; i++)
            hintIcons[i].gameObject.SetActive(false);

        //set up score
        mCurScore = GameData.instance.currentScore;

        int completeScore = GameData.instance.scorePerLevel;
        int hintPenaltyScore = GameData.instance.scoreHintPenalty * hintCounter;

        mNewScore = mCurScore + completeScore - hintPenaltyScore;

        currentScoreText.text = mCurScore.ToString();
        levelScoreText.text = "+" + completeScore.ToString();
        hintScoreText.text = "-" + hintPenaltyScore.ToString();
        newScoreText.text = "";
    }

    IEnumerator DoPlayNewScore() {
        float curTime = 0f;
        while(curTime < newScoreDelay) {

            float t = Mathf.Clamp01(curTime / newScoreDelay);

            int s = Mathf.RoundToInt(Mathf.Lerp(mCurScore, mNewScore, t));

            newScoreText.text = s.ToString();

            yield return null;

            curTime += Time.deltaTime;
        }

        newScoreText.text = mNewScore.ToString();

        mPlayNewScoreRout = null;
    }
}
