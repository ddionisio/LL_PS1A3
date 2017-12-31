using System;
using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;
using fastJSON;

public class LoLLocalize : Localize {
    public static new LoLLocalize instance {
        get {
            return (LoLLocalize)Localize.instance;
        }
    }

#if UNITY_EDITOR
    public string debugLanguageCode = "en";
    public string debugLanguageRef = "language.json";

    public string debugLanguagePath {
        get {
            return System.IO.Path.Combine(Application.streamingAssetsPath, debugLanguageRef);
        }
    }
#endif

    private Dictionary<string, LocalizeData> mEntries;

    private string mCurLang;

    public bool isLoaded {
        get {
            return mCurLang != null;
        }
    }

    public override string[] languages {
        get {
            return new string[] { mCurLang };
        }
    }

    public override int languageCount {
        get {
            return 1;
        }
    }

    public void Load(string langCode, string json) {
        mCurLang = langCode;
        if(mCurLang == null) //langCode shouldn't be null
            mCurLang = "";

        Dictionary<string, object> defs;
        if(!string.IsNullOrEmpty(json)) {
            defs = JSON.Parse(json) as Dictionary<string, object>;
        }
        else
            defs = new Dictionary<string, object>();
        
        mEntries = new Dictionary<string, LocalizeData>(defs.Count);
        
        foreach(var item in defs) {
            string key = item.Key;
            string val = item.Value.ToString();

            LocalizeData dat = new LocalizeData(val, new string[0]);

            mEntries.Add(key, dat);
        }

        Refresh();
    }

    public override bool Exists(string key) {
        if(mEntries == null)
            return false;

        return mEntries.ContainsKey(key);
    }

    public override string[] GetKeys() {
#if UNITY_EDITOR
        if(mEntries == null)
            LoadFromReference();
#endif

        if(mEntries == null)
            return new string[0];

        var keyColl = mEntries.Keys;
        var keys = new string[keyColl.Count];
        keyColl.CopyTo(keys, 0);

        return keys;
    }

    public override void Unload() {
        mEntries = null;
    }

    public override bool IsLanguageFile(string filepath) {
#if UNITY_EDITOR
        return filepath.Contains(debugLanguageRef);
#else
        return false;
#endif
    }

    public override int GetLanguageIndex(string lang) {
        if(lang == mCurLang)
            return 0;

        return -1;
    }

    public override string GetLanguageName(int langInd) {
        if(langInd == 0)
            return mCurLang;

        return "";
    }

    protected override void HandleLanguageChanged() {
        //Language is not changed via language for LoL, use Load
    }

    protected override bool TryGetData(string key, out LocalizeData data) {
        if(mEntries == null) {
            data = new LocalizeData();
            return false;
        }

        return mEntries.TryGetValue(key, out data);
    }

#if UNITY_EDITOR
    private void LoadFromReference() {
        if(string.IsNullOrEmpty(debugLanguageRef))
            return;

        string filepath = debugLanguagePath;

        string json = System.IO.File.ReadAllText(filepath);

        var defs = JSON.Parse(json) as Dictionary<string, object>;

        Load(debugLanguageCode, JSON.ToJSON(defs[debugLanguageCode]));
    }
#endif
}
