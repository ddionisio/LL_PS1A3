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
                Vector2 min = mBoxHandle.center - mBoxHandle.size*0.5f;

                float _minX = Mathf.Round(min.x / dat.boundsStep.x);
                float _minY = Mathf.Round(min.y / dat.boundsStep.y);

                min.x = _minX * dat.boundsStep.x;
                min.y = _minY * dat.boundsStep.y;

                Vector2 max = mBoxHandle.center + mBoxHandle.size * 0.5f;

                float _maxX = Mathf.Round(max.x / dat.boundsStep.x);
                float _maxY = Mathf.Round(max.y / dat.boundsStep.y);

                max.x = _maxX * dat.boundsStep.x;
                max.y = _maxY * dat.boundsStep.y;

                b.center = Vector2.Lerp(min, max, 0.5f);
                b.size = max - min;

                Undo.RecordObject(dat, "Change Game Camera Bounds");
                dat.bounds = b;
            }
        }
    }
}
