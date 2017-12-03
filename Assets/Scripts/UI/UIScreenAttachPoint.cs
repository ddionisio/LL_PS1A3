using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScreenAttachPoint : MonoBehaviour {
    public Camera cameraAttach; //if null, grab main camera
    public string attachPointName;

    private Vector2 mAttachPoint;
    private Camera mCam;

    public void Update() {
        transform.position = mCam.WorldToScreenPoint(mAttachPoint);
    }

    void OnEnable() {
        mCam = cameraAttach != null ? cameraAttach : Camera.main;

        var attachPoint = AttachPoint.Get(attachPointName);
        if(attachPoint)
            mAttachPoint = attachPoint.transform.position;
        else {
            Debug.LogWarning("Unable to find attach point: " + attachPointName);
            mAttachPoint = Vector2.zero;
        }

        Update();
    }
}
