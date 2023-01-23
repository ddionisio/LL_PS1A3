using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening.Core.Easing;
using DG.Tweening;

public class ConductiveReceiverBodyMove : ConductiveReceiver {
    public Rigidbody2D targetBody;

    public Vector2 endPosition;
    public bool endIsLocal;

    public Ease tweenStyle;
    public float startDelay = 0.3f;
    public float delay = 1.5f;

    private Coroutine mRout;
    private float mCurTime;

    private Vector2 mStartPosition;
    private Vector2 mEndPosition;

    void OnDisable() {
        mRout = null;
    }

    protected override void Awake() {
        base.Awake();

        mStartPosition = targetBody.position;

        mEndPosition = endIsLocal ? (Vector2)targetBody.transform.localToWorldMatrix.MultiplyPoint3x4(endPosition) : endPosition;
    }

    protected override void OnPowerChanged() {
        if(mRout != null)
            StopCoroutine(mRout);

        StartCoroutine(DoMove());
    }

    IEnumerator DoMove() {
        if(startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        var easeFunc = EaseManager.ToEaseFunction(tweenStyle);
        var wait = new WaitForFixedUpdate();

        while(mCurTime < delay) {
            mCurTime += Time.fixedDeltaTime;
            if(mCurTime > delay)
                mCurTime = delay;

            if(isPowered) {
                float t = easeFunc(mCurTime, delay, 0f, 0f);
                targetBody.MovePosition(Vector2.Lerp(mStartPosition, mEndPosition, t));
            }
            else {
                float t = easeFunc(delay - mCurTime, delay, 0f, 0f);
                targetBody.MovePosition(Vector2.Lerp(mEndPosition, mStartPosition, t));
            }

            yield return wait;
        }

        mRout = null;
    }

    private void OnDrawGizmos() {
        if(targetBody) {
            Vector2 startPos = targetBody.position;
            Vector2 endPos = endIsLocal ? (Vector2)targetBody.transform.localToWorldMatrix.MultiplyPoint3x4(endPosition) : endPosition;

            Gizmos.color = Color.green;

            M8.Gizmo.ArrowLine2D(transform.position, startPos);

            Gizmos.DrawLine(startPos, endPos);

            Gizmos.color = Color.cyan * 0.5f;

            Gizmos.DrawSphere(endPos, 0.25f);
        }
    }
}
