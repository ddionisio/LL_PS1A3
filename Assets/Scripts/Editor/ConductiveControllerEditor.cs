using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConductiveController))]
public class ConductiveControllerEditor : Editor {
    float mLastEnergy;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(Application.isPlaying) {
            M8.EditorExt.Utility.DrawSeparator();

            var dat = this.target as ConductiveController;

            if(dat.energyCapacity > 0)
                EditorGUILayout.LabelField("Energy Amount", string.Format("{0}/{1}", dat.curEnergy, dat.energyCapacity));
            else
                EditorGUILayout.LabelField("Energy Amount", dat.curEnergy.ToString());

            if(mLastEnergy != dat.curEnergy) {
                mLastEnergy = dat.curEnergy;
                Repaint();
            }
        }
    }
}
