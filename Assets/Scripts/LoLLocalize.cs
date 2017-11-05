using System;
using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;
using SimpleJSON;

public class LoLLocalize : Localize {
    public static new LoLLocalize instance {
        get {
            return (LoLLocalize)Localize.instance;
        }
    }

#if UNITY_EDITOR
    public string debugLanguageCode = "en";
    public string debugLanguageRef = "language.json";
#endif
        
    private Dictionary<string, LocalizeData> mEntries;

    private string mCurLang;

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

        JSONNode langDefs = JSON.Parse(json);

        mEntries = new Dictionary<string, LocalizeData>(langDefs.Count);
        
        foreach(var item in langDefs.Children) {
            string key = item.ToString();

            LocalizeData dat = new LocalizeData(item.Value, new string[0]);

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

        string filepath = System.IO.Path.Combine(Application.streamingAssetsPath, debugLanguageRef);

        string json = System.IO.File.ReadAllText(filepath);

        JSONNode langDefs = JSON.Parse(json);

        Load(debugLanguageCode, langDefs[debugLanguageCode].ToString());
    }
#endif
}
