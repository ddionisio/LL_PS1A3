using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteItemDragWidget : MonoBehaviour {
    public RectTransform palettePanel;

    public Image icon;

    public bool isShown { get { return mIsShown; } }

    private RectTransform mRectTrans;

    private bool mIsShown;
        
    public void Activate(Sprite iconSprite) {
        gameObject.SetActive(true);

        icon.gameObject.SetActive(true);
        icon.sprite = iconSprite;
        icon.SetNativeSize();

        mIsShown = true;
    }

    public void Deactivate() {
        icon.sprite = null;

        gameObject.SetActive(false);
    }

    void Awake() {
        mRectTrans = GetComponent<RectTransform>();
    }

    void Update() {
        //check if we need to hide, if we are outside the panel
        //NOTE: assumes no rotation/scale
        var palettePanelRect = palettePanel.rect;
        palettePanelRect.position += (Vector2)palettePanel.position;

        var pos = (Vector2)mRectTrans.position;

        bool show = palettePanelRect.Contains(pos);

        SetShow(show);
    }

    private void SetShow(bool show) {
        if(mIsShown != show) {
            //TODO: fancy tween?

            mIsShown = show;
            icon.gameObject.SetActive(mIsShown);
        }
    }
}
