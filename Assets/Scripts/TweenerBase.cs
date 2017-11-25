using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using DG.Tweening.Core.Easing;

public abstract class TweenerBase : MonoBehaviour {
    public Ease easeType;
    public float delay;
    public bool isRealTime;
    
    private bool mIsEnter;
    private EaseFunction mEaseFunc;
    private float mLastTime;
    private float mCurTime;

    public void ResetTween() {
        mIsEnter = false;
        mCurTime = 0f;
        Apply(0f);
    }

    protected abstract void Apply(float t);
        
    protected virtual void OnEnable() {
        ResetTween();

        mEaseFunc = EaseManager.ToEaseFunction(easeType);
    }

    protected void Enter(bool enter) {
        if(mIsEnter != enter) {
            mIsEnter = enter;
            mLastTime = isRealTime ? Time.realtimeSinceStartup : Time.time;
        }
    }

    void Update() {
        if(mIsEnter) {
            if(mCurTime < delay) {
                mCurTime += GetTimeDelta();
                if(mCurTime > delay)
                    mCurTime = delay;

                float t = mEaseFunc(mCurTime, delay, 0f, 0f);

                Apply(t);
            }
        }
        else {
            if(mCurTime > 0f) {
                mCurTime -= GetTimeDelta();
                if(mCurTime < 0f)
                    mCurTime = 0f;

                float t = mEaseFunc(mCurTime, delay, 0f, 0f);

                Apply(t);
            }
        }
    }

    private float GetTimeDelta() {
        float curTime = isRealTime ? Time.realtimeSinceStartup : Time.time;
        float delta = curTime - mLastTime;
        mLastTime = curTime;
        return delta;
    }
}
