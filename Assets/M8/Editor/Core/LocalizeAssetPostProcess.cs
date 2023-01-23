﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace M8 {
    public class LocalizeAssetPostProcess : AssetPostprocessor {        
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {            
            for(int i = 0; i < importedAssets.Length; i++) {
                //check localize selector
                if(LocalizeSelector.localizeExists && LocalizeSelector.localize.IsLanguageFile(importedAssets[i]))
                    LocalizeSelector.localize.Unload();
            }
        }
    }
}