using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using SimpleJSON;

using LoLSDK;

public class LoLManager : M8.SingletonBehaviour<LoLManager> {
    public class QuestionAnswered {
        public int questionIndex;
        public int alternativeIndex;
        public int correctAlternativeIndex;

        public MultipleChoiceAnswer answer;

        private bool mIsSubmitted;

        public QuestionAnswered(int aQuestionIndex, string questionId, int aAlternativeIndex, string alternativeId, int aCorrectAlternativeIndex) {
            questionIndex = aQuestionIndex;
            alternativeIndex = aAlternativeIndex;
            correctAlternativeIndex = aCorrectAlternativeIndex;

            answer = new MultipleChoiceAnswer();
            answer.questionId = questionId;
            answer.alternativeId = alternativeId;

            mIsSubmitted = false;
        }

        public void Submit() {
            if(!mIsSubmitted) {
                LOLSDK.Instance.SubmitAnswer(answer);
                mIsSubmitted = true;
            }
        }
    }
        
    public const string userDataSettingsKey = "settings";

    public const string settingsMusicVolumeKey = "mv";
    public const string settingsSoundVolumeKey = "sv";
    public const string settingsFadeVolumeKey = "fv";

    private const string questionsJSONFilePath = "questions.json";
    private const string startGameJSONFilePath = "startGame.json";

    public delegate void OnChanged(LoLManager mgr, int delta);
    public delegate void OnCallback(LoLManager mgr);

    [SerializeField]
    string _gameID = "com.daviddionisio.LoLGame";
    [SerializeField]
    int _progressMax;
    [SerializeField]
    protected bool _pauseEnabled;
    [SerializeField]
    protected string _pauseModal;
    [SerializeField]
    protected bool _pauseModalPopOnResume = true;
    [SerializeField]
    bool _useFadeMusicScale;
    [SerializeField]
    float _fadeMusicScale = 1.0f;

    protected int mCurProgress;

    protected bool mPaused;

    protected string mLangCode;

    public string gameID { get { return _gameID; } }

    public bool isReady { get { return mIsReady; } }

    public int progressMax { get { return _progressMax; } set { _progressMax = value; } }

    public int curProgress { get { return mCurProgress; } }

    public float musicVolume { get { return mMusicVolume; } }
    public float soundVolume { get { return mSoundVolume; } }
    public float fadeVolume { get { return mFadeVolume; } }

    public bool isQuestionsReceived { get { return mIsQuestionsReceived; } }

    public bool isQuestionsAllAnswered {
        get {
            if(mQuestionsList == null)
                return false;

            return mCurQuestionIndex >= mQuestionsList.questions.Length;
        }
    }

    public int questionCount {
        get {
            if(mQuestionsList == null)
                return 0;

            return mQuestionsList.questions.Length;
        }
    }

    public List<QuestionAnswered> questionAnsweredList {
        get { return mQuestionsAnsweredList; }
    }

    public int questionAnsweredCount {
        get {
            if(mQuestionsAnsweredList == null)
                return 0;

            return mQuestionsAnsweredList.Count;
        }
    }

    public int questionCurrentIndex {
        get {
            return mCurQuestionIndex;
        }
    }

    public event OnCallback startCallback;
    public event OnCallback progressCallback;
    public event OnCallback completeCallback;

    protected float mMusicVolume;
    protected float mSoundVolume;
    protected float mFadeVolume;

    protected bool mIsQuestionsReceived;
    protected MultipleChoiceQuestionList mQuestionsList;
    protected List<QuestionAnswered> mQuestionsAnsweredList;

    protected int mCurQuestionIndex;

    protected string mLastSoundBackgroundPath;

    private bool mIsReady;

    public virtual void PlaySound(string path, bool background, bool loop) {
        if(background && !string.IsNullOrEmpty(mLastSoundBackgroundPath)) {
            LOLSDK.Instance.StopSound(mLastSoundBackgroundPath);
        }

        LOLSDK.Instance.PlaySound(path, background, loop);

        if(background)
            mLastSoundBackgroundPath = path;
    }

    public virtual void SpeakText(string key) {
        LOLSDK.Instance.SpeakText(key);
    }

    public virtual void StopCurrentBackgroundSound() {
        if(!string.IsNullOrEmpty(mLastSoundBackgroundPath)) {
            LOLSDK.Instance.StopSound(mLastSoundBackgroundPath);
            mLastSoundBackgroundPath = null;
        }
    }

    public MultipleChoiceQuestion GetQuestion(int index) {
        if(mQuestionsList == null)
            return null;

        return mQuestionsList.questions[index];
    }

    public MultipleChoiceQuestion GetCurrentQuestion() {
        return GetQuestion(mCurQuestionIndex);
    }

    /// <summary>
    /// This will move the current question index by 1
    /// </summary>
    public virtual QuestionAnswered AnswerCurrentQuestion(int alternativeIndex) {
        if(isQuestionsAllAnswered)
            return null;

        var curQuestion = GetCurrentQuestion();

        if(curQuestion == null) {
            Debug.LogWarning("No question found for index: " + mCurQuestionIndex);
            return null;
        }

        int correctAltIndex = -1;
        string correctAltId = curQuestion.correctAlternativeId;
        for(int i = 0; i < curQuestion.alternatives.Length; i++) {
            if(curQuestion.alternatives[i].alternativeId == correctAltId) {
                correctAltIndex = i;
                break;
            }
        }

        var newAnswered = new QuestionAnswered(mCurQuestionIndex, curQuestion.questionId, alternativeIndex, curQuestion.alternatives[alternativeIndex].alternativeId, correctAltIndex);

        //don't submit if it's already answered
        int questionInd = -1;
        for(int i = 0; i < mQuestionsAnsweredList.Count; i++) {
            if(mQuestionsAnsweredList[i].answer.questionId == newAnswered.answer.questionId) {
                questionInd = i;
                break;
            }
        }

        if(questionInd == -1) {
            newAnswered.Submit();

            mQuestionsAnsweredList.Add(newAnswered);
        }

        mCurQuestionIndex++;

        return newAnswered;
    }

    /// <summary>
    /// Call this if you want to cycle back
    /// </summary>
    /// <param name="ind"></param>
    public void ResetCurrentQuestionIndex() {
        mCurQuestionIndex = 0;
    }

    public virtual void ApplyScore(int score) {
        LOLSDK.Instance.SubmitProgress(score, mCurProgress, _progressMax);
    }

    protected void ProgressCallback() {
        if(progressCallback != null)
            progressCallback(this);
    }

    public virtual void ApplyProgress(int progress, int score) {

        mCurProgress = Mathf.Clamp(progress, 0, _progressMax);

        ApplyScore(score);

        ProgressCallback();
    }

    protected virtual void ApplyVolumes(float sound, float music, float fade) {
        LOLSDK.Instance.ConfigureSound(mSoundVolume, mMusicVolume, mFadeVolume);
    }

    public void ApplyCurrentVolumes() {
        ApplyVolumes(mSoundVolume, mMusicVolume, mFadeVolume);
    }

    public void ApplyVolumes(float sound, float music, bool save) {
        if(_useFadeMusicScale)
            ApplyVolumes(sound, music, music * _fadeMusicScale, save);
        else
            ApplyVolumes(sound, music, mFadeVolume, save);
    }

    public void ApplyVolumes(float sound, float music, float fade, bool save) {
        ApplyVolumes(sound, music, fade);

        if(save) {
            mSoundVolume = sound;
            mMusicVolume = music;
            mFadeVolume = fade;

            var settings = M8.UserData.GetInstance(userDataSettingsKey);
            settings.SetFloat(settingsSoundVolumeKey, mSoundVolume);
            settings.SetFloat(settingsMusicVolumeKey, mMusicVolume);
            settings.SetFloat(settingsFadeVolumeKey, mFadeVolume);
        }
    }

    /// <summary>
    /// Call this when player quits, or finishes
    /// </summary>
    public virtual void Complete() {
        LOLSDK.Instance.CompleteGame();

        if(completeCallback != null)
            completeCallback(this);
    }

    protected virtual void Start() {
        mLangCode = "en";
        mIsReady = false;

        // Create the WebGL (or mock) object
#if UNITY_EDITOR
        ILOLSDK webGL = new LoLSDK.MockWebGL();
#elif UNITY_WEBGL
		ILOLSDK webGL = new LoLSDK.WebGL();
#endif

        // Initialize the object, passing in the WebGL
        LOLSDK.Init(webGL, _gameID);

        // Register event handlers
#if !UNITY_EDITOR
        LOLSDK.Instance.StartGameReceived += new StartGameReceivedHandler(this.HandleStartGame);
        LOLSDK.Instance.GameStateChanged += new GameStateChangedHandler(this.HandleGameStateChange);
        LOLSDK.Instance.QuestionsReceived += new QuestionListReceivedHandler(this.HandleQuestions);
        LOLSDK.Instance.LanguageDefsReceived += new LanguageDefsReceivedHandler(this.HandleLanguageDefs);
#endif

        mCurProgress = 0;

        SetupVolumes();

        // Mock the platform-to-game messages when in the Unity editor.
#if UNITY_EDITOR
        LoadMockData();
#endif

        // Then, tell the platform the game is ready.
        LOLSDK.Instance.GameIsReady();
    }

    protected void SetupVolumes() {
        var settings = M8.UserData.GetInstance(userDataSettingsKey);

        const float musicDefault = 0.3f;
        const float soundDefault = 0.5f;
        const float fadeDefault = 0.1f;

        mMusicVolume = settings.GetFloat(settingsMusicVolumeKey, musicDefault);
        mSoundVolume = settings.GetFloat(settingsSoundVolumeKey, soundDefault);
        mFadeVolume = settings.GetFloat(settingsFadeVolumeKey, _useFadeMusicScale ? musicDefault * _fadeMusicScale : fadeDefault);

        ApplyCurrentVolumes();
    }

    // Start the game here
    protected void HandleStartGame(string json) {
        //SharedState.StartGameData = JSON.Parse(json);

        mIsReady = true;

        if(startCallback != null)
            startCallback(this);
    }

    // Handle pause / resume
    protected void HandleGameStateChange(GameState gameState) {
        // Either GameState.Paused or GameState.Resumed
        switch(gameState) {
            case GameState.Paused:
                if(_pauseEnabled && !mPaused) {
                    mPaused = true;

                    if(!string.IsNullOrEmpty(_pauseModal)) {
                        if(M8.UIModal.Manager.isInstantiated) {
                            if(!M8.UIModal.Manager.instance.ModalIsInStack(_pauseModal))
                                M8.UIModal.Manager.instance.ModalOpen(_pauseModal);
                        }
                    }
                    else
                        M8.SceneManager.instance.Pause();
                }
                break;

            case GameState.Resumed:
                if(_pauseEnabled && mPaused) {
                    mPaused = false;

                    if(!string.IsNullOrEmpty(_pauseModal)) {
                        if(_pauseModalPopOnResume) {
                            if(M8.UIModal.Manager.instance.ModalIsInStack(_pauseModal))
                                M8.UIModal.Manager.instance.ModalCloseUpTo(_pauseModal, true);
                        }
                    }
                    else
                        M8.SceneManager.instance.Resume();
                }
                break;
        }
    }

    // Store the questions and show them in order based on your game flow.
    protected void HandleQuestions(MultipleChoiceQuestionList questionList) {
        mIsQuestionsReceived = true;

        mQuestionsList = questionList;
        mQuestionsAnsweredList = new List<QuestionAnswered>(mQuestionsList.questions.Length);
    }

    // Use language to populate UI
    protected void HandleLanguageDefs(string json) {
        LoLLocalize.instance.Load(mLangCode, json);
    }

#if UNITY_EDITOR
    void LoadMockData() {
        mLangCode = LoLLocalize.instance.debugLanguageCode;

        //apply language
        string langFilePath = LoLLocalize.instance.debugLanguagePath;
        if(File.Exists(langFilePath)) {
            string json = File.ReadAllText(langFilePath);

            JSONNode langDefs = JSON.Parse(json);

            HandleLanguageDefs(langDefs[mLangCode].ToString());
        }
        else
            HandleLanguageDefs("");
        //

        //apply questions
        string questionsFilePath = Path.Combine(Application.streamingAssetsPath, questionsJSONFilePath);
        if(File.Exists(questionsFilePath)) {
            string questionsDataAsJson = File.ReadAllText(questionsFilePath);
            MultipleChoiceQuestionList qs = MultipleChoiceQuestionList.CreateFromJSON(questionsDataAsJson);
            HandleQuestions(qs);
        }
        //

        //apply start data
        string startDataFilePath = Path.Combine(Application.streamingAssetsPath, startGameJSONFilePath);
        if(File.Exists(startDataFilePath)) {
            string startDataAsJSON = File.ReadAllText(startDataFilePath);
            JSONNode startGamePayload = JSON.Parse(startDataAsJSON);
            // Capture the language code from the start payload. Use this to switch fontss
            mLangCode = startGamePayload["languageCode"];
            HandleStartGame(startDataAsJSON);
        }
        else
            HandleStartGame("");
        //
    }
#endif
}
