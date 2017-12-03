using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPaletteDragToWorld : MonoBehaviour {
    public Transform pointEnd;
    public Transform pointer;

    public RectTransform arrowLine; //this will be set to go from point start to end.  Make sure to set pivot to bottom

    public int paletteIndex; //index of item in palette panel
    public string paletteName; //set paletteIndex = -1, get palette item by blockName

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

    void OnEnable() {
        mPointStartPos = Vector2.zero;
        mPointEndPos = Vector2.zero;

        mPointerT = 0f;
        mPointerTUpdate = true;

        Update();
    }

    void Update() {
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
