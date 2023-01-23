using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialBlockExpandAnim : MonoBehaviour {
    public RectTransform block;

    public Image pointer;
    public Sprite pointerSpriteUp;
    public Sprite pointerSpriteDown;
    
    public float moveAmount = 32f;

    public float blockMoveDelay;
    public float pointerDelay;

    public TutorialBlockMoveAnim blockMoveAnim;

    public GameObject shade;

    public int curState {
        get { return mState; }
        set {
            mState = value;

            if(mState == 0) {
                ResetState();

                shade.SetActive(false);

                mLastTime = Time.realtimeSinceStartup;
            }
        }
    }

    private Vector2 mBlockStartSize;

    private int mState;
    private float mLastTime;

    void ResetState() {
        block.sizeDelta = mBlockStartSize;

        pointer.sprite = pointerSpriteUp;
        pointer.SetNativeSize();
    }

    void OnEnable() {
        ResetState();

        shade.SetActive(true);

        mState = -1; //wait for move to start us
    }

    void Awake() {
        mBlockStartSize = block.sizeDelta;
    }

    void Update () {
		switch(mState) {
            case 0: {
                    float time = Time.realtimeSinceStartup;
                    float curTime = time - mLastTime;
                    if(curTime >= pointerDelay) {
                        mLastTime = time;
                        mState++;

                        pointer.sprite = pointerSpriteDown;
                        pointer.SetNativeSize();
                    }
                }
                break;

            case 1: {
                    float time = Time.realtimeSinceStartup;
                    float curTime = time - mLastTime;
                    if(curTime >= blockMoveDelay) {
                        mLastTime = time;
                        mState++;

                        Vector2 size = block.sizeDelta;
                        size.x += moveAmount;
                        block.sizeDelta = size;
                    }
                }
                break;

            case 2: {
                    float time = Time.realtimeSinceStartup;
                    float curTime = time - mLastTime;
                    if(curTime >= blockMoveDelay) {
                        mLastTime = time;
                        mState++;

                        Vector2 size = block.sizeDelta;
                        size.y += moveAmount;
                        block.sizeDelta = size;
                    }
                }
                break;

            case 3: {
                    float time = Time.realtimeSinceStartup;
                    float curTime = time - mLastTime;
                    if(curTime >= pointerDelay) {
                        mLastTime = time;
                        mState = -1;

                        pointer.sprite = pointerSpriteUp;
                        pointer.SetNativeSize();

                        shade.SetActive(true);

                        //play move again
                        blockMoveAnim.curState = 0;
                    }
                }
                break;
        }
	}
}
