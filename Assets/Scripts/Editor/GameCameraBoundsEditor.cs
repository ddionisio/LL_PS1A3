using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEditor.IMGUI.Controls;

[CustomEditor(typeof(GameMapData))]
public class GameCameraBoundsEditor : Editor {
    BoxBoundsHandle mBoxHandle = new BoxBoundsHandle();

    void OnSceneGUI() {
        var dat = target as GameMapData;
        if(dat == null)
            return;

        using(new Handles.DrawingScope(GameMapData.editBoundsColor)) {
            mBoxHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;

            Bounds b = dat.bounds;

            mBoxHandle.center = new Vector3(b.center.x, b.center.y, 0f);
            mBoxHandle.size = new Vector3(b.size.x, b.size.y, 0f);

            EditorGUI.BeginChangeCheck();
            mBoxHandle.DrawHandle();
            if(EditorGUI.EndChangeCheck()) {
                b.center = mBoxHandle.center;
                b.size = mBoxHandle.size;

                Undo.RecordObject(dat, "Change Game Camera Bounds");
                dat.bounds = b;
            }
        }
    }
}
