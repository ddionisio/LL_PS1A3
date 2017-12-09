using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSizeAnchor : MonoBehaviour {
    public enum Anchor {
        Width,
        Height
    }

    public Transform point;
    public Anchor anchor;

    SpriteRenderer mSprite;
    private Vector2 mLastPointPos;
    private Vector2 mLastPos;

    void OnEnable() {
        mLastPointPos = point.position;
        mLastPos = transform.position;
        ApplySize();
    }

    void Awake() {
        mSprite = GetComponent<SpriteRenderer>();
    }

    void Update() {
        bool update = false;

        Vector2 pointPos = point.position;
        Vector2 pos = transform.position;

        if(mLastPointPos != pointPos) {
            mLastPointPos = pointPos;
            update = true;
        }

        if(mLastPos != pos) {
            mLastPos = pos;
            update = true;
        }

        if(update)
            ApplySize();
    }

    void ApplySize() {
        Vector2 size = mSprite.size;

        switch(anchor) {
            case Anchor.Width:
                float w = mLastPointPos.x - mLastPos.x;
                size.x = w;
                break;

            case Anchor.Height:
                float h = mLastPointPos.y - mLastPos.y;
                size.y = h;
                break;
        }

        mSprite.size = size;
    }
}
