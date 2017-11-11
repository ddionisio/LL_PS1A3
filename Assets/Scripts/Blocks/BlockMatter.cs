using System;
using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public class BlockMatter : Block {
    [Header("Matter")]

    [Range(0f, 1f)]
    public float ghostAlpha;

    public Color invalidColor = Color.red;

    public override Type type { get { return Type.Matter; } }

    public override int matterCount { get { return mCellSize.row * mCellSize.col; } }

    private SpriteRenderer mSpriteRender;
    private Rigidbody2D mBody;
    private BoxCollider2D mColl;

    private Color mSpriteDefaultColor;

    private CellIndex mCellSize;

    private Vector2 mEditStartPos;
    private CellIndex mEditStartCell;
    private bool mEditIsValid;

    public override void EditStart(Vector2 pos) {
        mEditStartPos = pos;
        mEditStartCell = GameMapData.instance.GetCellIndex(mEditStartPos);

        //initialize dimension as 1x1
        EditUpdate(pos);
    }

    public override void EditUpdate(Vector2 pos) {
        var cellSize = GameData.instance.blockSize;

        //update dimensions
        CellIndex curCell = GameMapData.instance.GetCellIndex(pos);

        CellIndex minCell = new CellIndex() { row = Mathf.Min(curCell.row, mEditStartCell.row), col = Mathf.Min(curCell.col, mEditStartCell.col) };
        CellIndex maxCell = new CellIndex() { row = Mathf.Max(curCell.row, mEditStartCell.row), col = Mathf.Max(curCell.col, mEditStartCell.col) };

        Vector2 min = GameMapData.instance.GetPositionFromCell(minCell);
        Vector2 max = GameMapData.instance.GetPositionFromCell(maxCell); max += cellSize;
        Vector2 size = new Vector2(max.x - min.x, max.y - min.y);
        Vector2 center = Vector2.Lerp(min, max, 0.5f);

        mSpriteRender.size = size;
        mColl.size = size;

        transform.position = center;

        mCellSize.row = maxCell.row - minCell.row + 1;
        mCellSize.col = maxCell.col - minCell.col + 1;

        mBody.mass = mass * matterCount;

        UpdatePlacementValid();
    }

    public override void EditEnd(Vector2 pos) {
        EditUpdate(pos);
    }

    public override bool EditIsPlacementValid() {
        return mEditIsValid;
    }

    protected override void ApplyMode() {
        switch(mode) {
            case Mode.None:
                break;

            case Mode.Ghost:
                mSpriteRender.color = new Color(mSpriteDefaultColor.r, mSpriteDefaultColor.g, mSpriteDefaultColor.b, ghostAlpha);

                mBody.simulated = false;

                mColl.enabled = false;
                break;

            case Mode.Solid:
                mSpriteRender.color = mSpriteDefaultColor;

                mBody.simulated = true;

                mColl.enabled = true;
                break;
        }
    }

    protected override void OnSpawned(GenericParams parms) {
        //default values
        mCellSize = new CellIndex(1, 1);

        base.OnSpawned(parms);
    }

    protected override void Awake() {
        base.Awake();

        mSpriteRender = GetComponentInChildren<SpriteRenderer>();
        mBody = GetComponent<Rigidbody2D>();
        mColl = GetComponent<BoxCollider2D>();

        mSpriteDefaultColor = mSpriteRender.color;
    }

    private void UpdatePlacementValid() {        
        mEditIsValid = false;

        //check pallete count
        //check overlap
        if(matterCount > GameMapData.instance.PaletteCount(blockName)) {
            var collider = Physics2D.OverlapBox(transform.position, mColl.size, 0f, GameData.instance.blockInvalidMask);

            mEditIsValid = collider == null;
        }

        UpdatePlacementValidDisplay(mEditIsValid);
    }

    private void UpdatePlacementValidDisplay(bool valid) {
        float alpha = mode == Mode.Ghost ? ghostAlpha : mSpriteDefaultColor.a;

        if(valid) {
            mSpriteRender.color = new Color(invalidColor.r, invalidColor.g, invalidColor.g, alpha);
        }
        else {
            mSpriteRender.color = new Color(mSpriteDefaultColor.r, mSpriteDefaultColor.g, mSpriteDefaultColor.g, alpha);
        }
    }
}
