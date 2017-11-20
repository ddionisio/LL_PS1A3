using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HeatController))]
public class HeatControllerEditor : Editor {
    float mLastHeat;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(Application.isPlaying) {
            M8.EditorExt.Utility.DrawSeparator();

            var dat = this.target as HeatController;

            EditorGUILayout.LabelField("Heat Amount", string.Format("{0}/{1}", dat.amountCurrent, dat.amountCapacity));

            if(mLastHeat != dat.amountCurrent) {
                mLastHeat = dat.amountCurrent;
                Repaint();
            }
        }
    }
}
