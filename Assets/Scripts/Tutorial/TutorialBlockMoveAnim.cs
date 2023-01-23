using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialBlockMoveAnim : MonoBehaviour {
    public RectTransform block;
        
    public Image pointer;
    public Sprite pointerSpriteUp;
    public Sprite pointerSpriteDown;

    public GameObject expandArrowRoot;

    public float moveAmount = 32f;

    public float blockMoveDelay;
    public float pointerDelay;

    public TutorialBlockExpandAnim blockExpandAnim;

    public GameObject shade;

    public int curState {
        get { return mState; }
        set {
            mState = value;
            if(mState == 0)
                OnEnable();
        }
    }

    private Vector2 mBlockStartPos;

    private int mState;
    private float mLastTime;

    void OnEnable() {
        block.localPosition = mBlockStartPos;

        pointer.sprite = pointerSpriteUp;
        pointer.SetNativeSize();

        expandArrowRoot.SetActive(true);

        shade.SetActive(false);

        mState = 0;

        mLastTime = Time.realtimeSinceStartup;
    }

    void Awake() {
        mBlockStartPos = block.localPosition;
    }

    // Update is called once per frame
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

                        Vector2 pos = block.localPosition;
                        pos.x += moveAmount;
                        block.localPosition = pos;

                        expandArrowRoot.SetActive(false);
                    }
                }
                break;

            case 2: {
                    float time = Time.realtimeSinceStartup;
                    float curTime = time - mLastTime;
                    if(curTime >= blockMoveDelay) {
                        mLastTime = time;
                        mState++;

                        Vector2 pos = block.localPosition;
                        pos.y -= moveAmount;
                        block.localPosition = pos;
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

                        expandArrowRoot.SetActive(true);

                        shade.SetActive(true);

                        //play expand anim
                        blockExpandAnim.curState = 0;
                    }
                }
                break;
        }
	}
}
