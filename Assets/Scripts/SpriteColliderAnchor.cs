using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only functions in scene editor
/// </summary>
[ExecuteInEditMode]
public class SpriteColliderAnchor : MonoBehaviour {
    public enum Anchor {
        Left,
        Right,
        Top,
        Bottom,
    }

    public Collider2D toCollider;
    public Anchor anchor;

#if UNITY_EDITOR
    private SpriteRenderer mSpriteRender;

    void Update () {
        if(!toCollider) return;
        if(!mSpriteRender) mSpriteRender = GetComponent<SpriteRenderer>();
        if(!mSpriteRender) return;

        var collBounds = toCollider.bounds;

        Vector2 toPos = transform.position;
        Vector2 toSize = mSpriteRender.drawMode == SpriteDrawMode.Simple ? (Vector2)transform.localScale : mSpriteRender.size;

        switch(anchor) {
            case Anchor.Bottom:
                toPos.x = collBounds.min.x;
                toPos.y = collBounds.min.y;

                toSize.x = mSpriteRender.drawMode == SpriteDrawMode.Simple ? collBounds.size.x / (mSpriteRender.sprite.rect.size.x / mSpriteRender.sprite.pixelsPerUnit) : collBounds.size.x;
                break;
        }

        transform.position = toPos;

        if(mSpriteRender.drawMode == SpriteDrawMode.Simple)
            transform.localScale = toSize;
        else
            mSpriteRender.size = toSize;
    }
#endif
}
