using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RocketHUDController : MonoBehaviour {
    [System.Serializable]
    public struct ChecklistItem {
        public string id;
        public Text textWidget;
        public GameObject correctGO;

        private bool mIsConnected;

        public bool isConnected { get { return mIsConnected; } }

        public void UpdateConnected(bool connected, Color textColor) {
            mIsConnected = connected;
            textWidget.color = textColor;

            if(correctGO)
                correctGO.SetActive(connected);
        }
    }

    public ChecklistItem[] checklist;
    public Color checklistItemValidColor = Color.green;
    public Color checklistItemInvalidColor = Color.red;

    public Text launchConsoleText;
    public int launchConsoleMaxLines = 4;
    public Button launchButton;

    public string soundConsoleBeepPath;
    public string soundRocketIgniteFirstPath;
    public string soundRocketIgniteSecondPath;

    private int mValidCount;

    private System.Text.StringBuilder mConsoleStringBuffer = new System.Text.StringBuilder();
    private Queue<string> mConsoleLineStrings = new Queue<string>();

    public void Launch() {
        launchButton.interactable = false;

        //hide palette hud
        HUD.instance.palettePanel.Show(false);

        StartCoroutine(DoLaunch());
    }

    void OnDisable() {
        if(BlockConnectController.instance)
            BlockConnectController.instance.connectUpdateCallback -= OnBlockConnectUpdate;

        if(GameMapController.instance)
            GameMapController.instance.modeChangeCallback -= OnGameModeChange;

        StopAllCoroutines();
    }

    void OnEnable() {
        BlockConnectController.instance.connectUpdateCallback += OnBlockConnectUpdate;
        GameMapController.instance.modeChangeCallback += OnGameModeChange;
    }

    void Awake() {
        launchButton.interactable = false;

        for(int i = 0; i < checklist.Length; i++) {
            checklist[i].UpdateConnected(false, checklistItemInvalidColor);
        }

        launchConsoleText.text = "";
    }

    void OnGameModeChange(GameMapController.Mode mode) {
        switch(mode) {
            case GameMapController.Mode.Edit:
                launchButton.interactable = false;
                break;
            case GameMapController.Mode.Play:
                launchButton.interactable = mValidCount == checklist.Length;
                break;
        }
    }

    void OnBlockConnectUpdate(string id, bool isConnect) {
        mValidCount = 0;

        for(int i = 0; i < checklist.Length; i++) {
            if(checklist[i].id == id) {
                if(isConnect)
                    checklist[i].UpdateConnected(true, checklistItemValidColor);
                else
                    checklist[i].UpdateConnected(false, checklistItemInvalidColor);
            }

            if(checklist[i].isConnected)
                mValidCount++;
        }

        launchButton.interactable = mValidCount == checklist.Length;
    }

    private void AddConsoleText(string locRef) {
        string str = LoLLocalize.Get(locRef);

        if(mConsoleLineStrings.Count == launchConsoleMaxLines) {
            //pop oldest string
            mConsoleLineStrings.Dequeue();
        }

        mConsoleLineStrings.Enqueue(str);

        mConsoleStringBuffer.Remove(0, mConsoleStringBuffer.Length);

        bool isFirst = true;
        foreach(var line in mConsoleLineStrings) {
            if(isFirst) {
                isFirst = false;
                mConsoleStringBuffer.Append(line);
            }
            else
                mConsoleStringBuffer.Append('\n').Append(line);
        }
        
        launchConsoleText.text = mConsoleStringBuffer.ToString();
    }

    IEnumerator DoLaunch() {
        var ctrl = GameRocketLaunchController.instance;

        var waitSecond = new WaitForSeconds(1f);

        LoLMusicPlaylist.instance.Stop();
        
        AddConsoleText("launchSequence1"); //prep

        ctrl.StartPump();
        while(ctrl.isPumping)
            yield return null;
                
        AddConsoleText("launchSequence2"); //ignitions start

        ctrl.StartIgnition();

        LoLManager.instance.PlaySound(soundRocketIgniteFirstPath, false, true);

        yield return waitSecond;

        AddConsoleText("launchSequence3"); //start countdown

        yield return waitSecond;

        //countdown
        LoLManager.instance.PlaySound(soundConsoleBeepPath, false, false);
        AddConsoleText("launchSequence4");
        yield return waitSecond;
        LoLManager.instance.PlaySound(soundConsoleBeepPath, false, false);
        AddConsoleText("launchSequence5");
        yield return waitSecond;
        LoLManager.instance.PlaySound(soundConsoleBeepPath, false, false);
        AddConsoleText("launchSequence6");
        yield return waitSecond;
        LoLManager.instance.PlaySound(soundConsoleBeepPath, false, false);
        AddConsoleText("launchSequence7");
        yield return waitSecond;
        LoLManager.instance.PlaySound(soundConsoleBeepPath, false, false);
        AddConsoleText("launchSequence8");
        yield return waitSecond;
        LoLManager.instance.PlaySound(soundConsoleBeepPath, false, false);
        AddConsoleText("launchSequence9");
        yield return waitSecond;

        AddConsoleText("launchSequence10"); //engines are go!

        ctrl.LiftOff();

        LoLManager.instance.StopSound(soundRocketIgniteFirstPath);
        LoLManager.instance.PlaySound(soundRocketIgniteSecondPath, false, true);

        yield return waitSecond;

        LoLMusicPlaylist.instance.PlayStartMusic();

        AddConsoleText("launchSequence11"); //lift-off! we have a lift-off!

        //wait for launch to finish
        while(ctrl.isLiftingOff)
            yield return null;

        LoLManager.instance.StopSound(soundRocketIgniteSecondPath);

        AddConsoleText("launchSequence12");

        yield return new WaitForSeconds(2.0f);
                
        //go to ending
        if(GameFlowController.isInstantiated) {
            GameFlowController.ProgressAndLoadNextScene();
        }
        else {
            var endScene = GameData.instance.endScene;
            endScene.Load();
        }
    }
}
