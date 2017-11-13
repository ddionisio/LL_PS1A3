using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScreenAttachToWorld : MonoBehaviour {
    public Camera cameraAttach; //if null, grab main camera
    public Transform worldAttach;
    public Vector2 position; //if worldAttach is valid, this is the local offset

    private Camera mCam;

    void OnEnable() {
        mCam = cameraAttach != null ? cameraAttach : Camera.main;
    }

    void Update () {
        Vector2 pos = GetWorldPosition();

        transform.position = mCam.WorldToScreenPoint(pos);
	}

    private Vector2 GetWorldPosition() {
        if(!worldAttach)
            return position;

        Vector2 pos = worldAttach.position;

        if(position != Vector2.zero) {
            pos += (Vector2)worldAttach.localToWorldMatrix.MultiplyPoint3x4(position);
        }

        return pos;
    }
}
