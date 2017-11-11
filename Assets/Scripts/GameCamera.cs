using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : M8.SingletonBehaviour<GameCamera> {
    public AnimationCurve moveToCurve;
    public float moveToSpeed = 10f;

    public M8.Camera2D camera2D { get { return mCam; } }

    public Vector2 position { get { return transform.position; } }
    
    public Bounds cameraViewBounds { get { return mCamViewBounds; } }

    public bool isMoving { get { return mMoveToRout != null; } }

    private M8.Camera2D mCam;
    private Bounds mCamViewBounds; //in local space, relative to world

    private Coroutine mMoveToRout;

    public void MoveTo(Vector2 dest) {
        StopMoveTo();

        var mapData = GameMapController.instance.mapData;

        //clamp
        dest = mapData.Clamp(dest, mCamViewBounds.extents);

        mMoveToRout = StartCoroutine(DoMoveTo(dest));
    }

    public void SetPosition(Vector2 pos) {
        var mapData = GameMapController.instance.mapData;

        //clamp
        pos = mapData.Clamp(pos, mCamViewBounds.extents);

        transform.position = pos;
    }

    protected override void OnInstanceInit() {
        mCam = GetComponentInChildren<M8.Camera2D>();

        var unityCam = mCam.unityCamera;

        //setup view bounds
        var minExt = unityCam.ViewportToWorldPoint(Vector3.zero);
        var maxExt = unityCam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        var mtxToLocal = transform.worldToLocalMatrix;

        var minExtL = mtxToLocal.MultiplyPoint3x4(minExt);
        var maxExtL = mtxToLocal.MultiplyPoint3x4(maxExt);

        mCamViewBounds = new Bounds(
            new Vector2(Mathf.Lerp(minExtL.x, maxExtL.x, 0.5f), Mathf.Lerp(minExtL.y, maxExtL.y, 0.5f)), 
            new Vector2(Mathf.Abs(maxExtL.x - minExtL.x), Mathf.Abs(maxExtL.y - minExtL.y)));
        //
    }

    void OnDisable() {
        StopMoveTo();
    }

    IEnumerator DoMoveTo(Vector2 dest) {
        float curTime = 0f;

        Vector2 start = transform.position;

        float dist = (dest - start).magnitude;
        float delay = dist / moveToSpeed;

        while(curTime < delay) {
            float t = Mathf.Clamp01(curTime / delay);

            transform.position = Vector2.Lerp(start, dest, moveToCurve.Evaluate(t));

            yield return null;

            curTime += Time.deltaTime;
        }

        transform.position = dest;

        mMoveToRout = null;
    }

    private void StopMoveTo() {
        if(mMoveToRout != null) {
            StopCoroutine(mMoveToRout);
            mMoveToRout = null;
        }
    }
}
