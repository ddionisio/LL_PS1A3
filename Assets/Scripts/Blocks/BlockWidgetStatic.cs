using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockWidgetStatic : BlockWidget {
    [Header("Data")]
    public string id;
    public SpriteRenderer sprite;

    [Range(0f, 1f)]
    public float ghostAlpha = 0.6f;

    public Color invalidColor = Color.red;

    private BoxCollider2D mColl;

    private bool mEditIsValid;
    private Vector2 mEditPos;

    private Color mSpriteDefaultColor;

    private bool mIsDeployed;

    public override Type type { get { return Type.Static; } }

    public override int matterCount { get { return 1; } }

    public override CellIndex cellSize { get { return new CellIndex(1, 1); } }

    public override Transform editAttach { get { return transform; } }

    public override Bounds editBounds {
        get {
            return new Bounds(mEditPos, mColl.size);
        }
    }

    public override Rigidbody2D mainBody {
        get {
            return null;
        }
    }

    public override void EditSetPosition(Vector2 pos) {
        if(mEditPos != pos) {
            mEditPos = pos;
            UpdatePositionFromEditPos();
            DimensionChanged();
        }
    }

    public override void EditMove(Vector2 delta) {
        var lastEditPos = mEditPos;
        mEditPos += delta;
        
        if(mEditPos != lastEditPos) {
            UpdatePositionFromEditPos();
            DimensionChanged();
        }
    }

    public override bool EditIsPlacementValid() {
        //check balloon's collision
        //check line to make sure it isn't occluded towards balloon
        return mEditIsValid;
    }

    public override void EditEnableCollision(bool aEnable) {
        mColl.enabled = aEnable;
    }

    public override void EditCancel() {
        //if we are deployed, add palette back
        if(mIsDeployed) {
            GameMapController.instance.PaletteChange(blockName, matterCount);
        }
    }

    protected override void ApplyMode(Mode prevMode) {
        switch(mode) {
            case Mode.Ghost:
                mSpriteDefaultColor = sprite.color;

                mColl.enabled = false;

                UpdatePositionFromEditPos();
                break;
            case Mode.Solid:
                sprite.color = mSpriteDefaultColor;

                mColl.enabled = true;

                mIsDeployed = true;
                break;
        }
    }
        
    protected override void OnDespawned() {
        sprite.color = mSpriteDefaultColor;

        GameMapController.instance.modeChangeCallback -= OnGameChangeMode;

        mIsDeployed = false;

        base.OnDespawned();
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        mSpriteDefaultColor = sprite.color;

        base.OnSpawned(parms);

        GameMapController.instance.modeChangeCallback += OnGameChangeMode;
    }

    protected override void Awake() {
        base.Awake();

        mColl = GetComponent<BoxCollider2D>();

        mEditPos = transform.position;
    }

    void OnGameChangeMode(GameMapController.Mode mode) {
        switch(mode) {
            case GameMapController.Mode.Edit:
                //revert to ghost
                this.mode = Mode.Ghost;
                break;

            case GameMapController.Mode.Play:
                if(mIsDeployed) //revert back to solid if we were deployed last time
                    this.mode = Mode.Solid;
                break;
        }
    }

    private void UpdatePositionFromEditPos() {
        transform.position = mEditPos;

        //check to make sure balloon has room
        if(IsCountValid() || mIsDeployed) {
            var collider = Physics2D.OverlapBox(mEditPos, mColl.size, 0f, GameData.instance.blockInvalidMask);

            mEditIsValid = collider == null;
        }
        else
            mEditIsValid = false;
        
        //update color
        if(mEditIsValid) {
            sprite.color = new Color(mSpriteDefaultColor.r, mSpriteDefaultColor.g, mSpriteDefaultColor.b, ghostAlpha);            
        }
        else {
            sprite.color = new Color(invalidColor.r, invalidColor.g, invalidColor.b, ghostAlpha);
        }
    }
}