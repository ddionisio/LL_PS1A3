using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only functions in scene editor
/// </summary>
[ExecuteInEditMode]
public class SpriteAnchor : MonoBehaviour {
    public enum Anchor {
        Left,
        Right,
        Top,
        Bottom,
    }

    public SpriteRenderer toSprite;
    public Anchor anchor;

#if UNITY_EDITOR
    void Update () {
        if(Application.isPlaying)
            return;

        if(!toSprite)
            return;

        var toSpritePos = toSprite.transform.position;
        var toSpriteSize = toSprite.size;
        
        Vector2 pos = transform.position;

        switch(anchor) {
            case Anchor.Left:
                pos.x = toSpritePos.x;
                break;
            case Anchor.Right:
                pos.x = toSpritePos.x + toSpriteSize.x;
                break;
            case Anchor.Top:
                pos.y = toSpritePos.y;
                break;
            case Anchor.Bottom:
                pos.y = toSpritePos.y + toSpriteSize.y;
                break;
        }

        transform.position = pos;
	}
#endif
}
