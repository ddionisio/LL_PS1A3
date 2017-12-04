using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening.Core.Easing;
using DG.Tweening;

public class TutorialPaletteDragToWorld : MonoBehaviour {
    public Transform pointEnd;
    public Transform pointer;

    public RectTransform arrowLine; //this will be set to go from point start to end.  Make sure to set pivot to bottom

    public int paletteIndex; //index of item in palette panel
    public string paletteName; //set paletteIndex = -1, get palette item by blockName

    [Header("Anim")]
    public Image pointerImage;
    public Sprite pointerSpriteUp;
    public Sprite pointerSpriteDown;
    public float pointerFadeDelay;
    public float pointerWaitDelay;
    public float pointerMoveDelay;

    public float pointerT { //[0, 1] - use animator to animate
        get { return mPointerT; }
        set {
            if(mPointerT != value) {
                mPointerTUpdate = true;
                mPointerT = value;
            }
        }
    }

    private bool mPointerTUpdate;

    private float mPointerT;
    private Vector2 mPointStartPos;
    private Vector2 mPointEndPos;

    private Color mPointerImageColorDefault;
    private Color mPointerImageColorFadeOut;

    private int mPointerAnimState;
    private float mPointerAnimLastTime;
    private EaseFunction mPointerAnimMoveEaseFunc;

    void OnEnable() {
        mPointStartPos = Vector2.zero;
        mPointEndPos = Vector2.zero;

        mPointerT = 0f;
        mPointerTUpdate = true;

        mPointerAnimState = -1;

        Update();

        mPointerAnimState = 0;
        mPointerAnimLastTime = Time.realtimeSinceStartup;
        pointerImage.color = mPointerImageColorFadeOut;
        pointerImage.sprite = pointerSpriteUp;
        pointerImage.SetNativeSize();
    }

    void Awake() {
        mPointerAnimMoveEaseFunc = EaseManager.ToEaseFunction(Ease.InOutSine);

        mPointerImageColorDefault = pointerImage.color;
        mPointerImageColorFadeOut = new Color(mPointerImageColorDefault.r, mPointerImageColorDefault.g, mPointerImageColorDefault.b, 0f);
    }

    void Update() {
        //animation
        switch(mPointerAnimState) {
            case -1:
                break;

            case 0: {
                    float time = Time.realtimeSinceStartup;
                    float curTime = time - mPointerAnimLastTime;
                    if(curTime > pointerFadeDelay) {
                        curTime = pointerFadeDelay;
                        mPointerAnimLastTime = time;                        
                        mPointerAnimState++;
                    }

                    pointerImage.color = Color.Lerp(mPointerImageColorFadeOut, mPointerImageColorDefault, curTime / pointerFadeDelay);
                }
                break;

            case 1: {
                    float time = Time.realtimeSinceStartup;
                    float curTime = time - mPointerAnimLastTime;
                    if(curTime > pointerWaitDelay) {
                        mPointerAnimLastTime = time;
                        mPointerAnimState++;
                        pointerImage.sprite = pointerSpriteDown;
                        pointerImage.SetNativeSize();
                    }
                }
                break;

            case 2: {
                    float time = Time.realtimeSinceStartup;
                    float curTime = time - mPointerAnimLastTime;
                    if(curTime > pointerWaitDelay) {
                        mPointerAnimLastTime = time;
                        mPointerAnimState++;
                    }
                }
                break;

            case 3: {
                    float time = Time.realtimeSinceStartup;
                    float curTime = time - mPointerAnimLastTime;
                    if(curTime > pointerMoveDelay) {
                        curTime = pointerMoveDelay;
                        mPointerAnimLastTime = time;
                        mPointerAnimState++;
                        pointerImage.sprite = pointerSpriteUp;
                        pointerImage.SetNativeSize();
                    }

                    pointerT = mPointerAnimMoveEaseFunc(curTime, pointerMoveDelay, 0f, 0f);
                }
                break;

            case 4: {
                    float time = Time.realtimeSinceStartup;
                    float curTime = time - mPointerAnimLastTime;
                    if(curTime > pointerWaitDelay) {
                        mPointerAnimLastTime = time;
                        mPointerAnimState++;
                    }
                }
                break;

            case 5: {
                    float time = Time.realtimeSinceStartup;
                    float curTime = time - mPointerAnimLastTime;
                    if(curTime > pointerFadeDelay) {
                        curTime = pointerFadeDelay;
                        mPointerAnimLastTime = time;
                        mPointerAnimState = 0;
                        pointerT = 0f;
                    }

                    pointerImage.color = Color.Lerp(mPointerImageColorDefault, mPointerImageColorFadeOut, curTime / pointerFadeDelay);
                }
                break;
        }

        Vector2 _pointStart = GetPointStart();
        Vector2 _pointEnd = pointEnd.position;

        bool updateArrowLine = false;

        if(mPointStartPos != _pointStart) {
            mPointStartPos = _pointStart;
            updateArrowLine = true;
        }

        if(mPointEndPos != _pointEnd) {
            mPointEndPos = _pointEnd;
            updateArrowLine = true;
        }

        if(mPointerTUpdate || updateArrowLine) {
            mPointerTUpdate = false;

            pointer.position = Vector2.Lerp(mPointStartPos, mPointEndPos, pointerT);
        }
                
        if(updateArrowLine) {
            Vector2 up = mPointEndPos - mPointStartPos;
            float dist = up.magnitude;
            if(dist > 0f)
                up /= dist;

            arrowLine.position = mPointStartPos;

            var arrowLineSize = arrowLine.sizeDelta;
            arrowLineSize.y = dist;
            arrowLine.sizeDelta = arrowLineSize;

            arrowLine.up = up;
        }
    }

    private Vector2 GetPointStart() {
        if(!HUD.instance.palettePanel.isShown)
            return transform.position;

        PaletteItemWidget palItem = null;

        if(paletteIndex != -1)
            palItem = HUD.instance.palettePanel.GetActiveWidget(paletteIndex);
        else if(!string.IsNullOrEmpty(paletteName))
            palItem = HUD.instance.palettePanel.GetActiveWidget(paletteName);

        return palItem.transform.position;
    }
}
