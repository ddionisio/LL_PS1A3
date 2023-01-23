using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameMapRoot))]
public class GameMapRootEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        M8.EditorExt.Utility.DrawSeparator();

        var dat = target as GameMapRoot;

        //refresh all box 2d and polygon 2d colliders
        if(GUILayout.Button("Reconstruct Colliders")) {
            Collider2D[] colls = dat.GetComponentsInChildren<Collider2D>();
            Undo.RecordObjects(colls, "Game Map Root Reconstruct Colliders");

            for(int i = 0; i < colls.Length; i++) {
                var meshFilter = colls[i].GetComponent<MeshFilter>();
                if(!meshFilter)
                    continue;

                if(colls[i] is PolygonCollider2D)
                    M8.PolygonCollider2DEditHelper.Reconstruct((PolygonCollider2D)colls[i], meshFilter);
                else if(colls[i] is BoxCollider2D)
                    M8.BoxCollider2DEditHelper.Reconstruct((BoxCollider2D)colls[i], meshFilter);
            }
        }
    }
}