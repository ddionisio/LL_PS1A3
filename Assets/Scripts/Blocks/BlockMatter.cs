using System;
using System.Collections;
using System.Collections.Generic;
using M8;
using UnityEngine;

public class BlockMatter : Block {
    public const float overlapThreshold = 0.1f;

    [Header("Matter")]

    [Range(0f, 1f)]
    public float ghostAlpha;

    public Color invalidColor = Color.red;

    public override Type type { get { return Type.Matter; } }

    public override CellIndex cellSize { get { return mCellSize; } }

    public override Transform editAttach { get { return transform; } }

    public override Bounds gatherBounds { get { return editBounds; } }

    public override Bounds editBounds {
        get {
            var center = mColl.transform.position;
            var size = mColl.size;

            return new Bounds(center, size);
        }
    }

    private SpriteRenderer mSpriteRender;
    private Rigidbody2D mBody;
    private BoxCollider2D mColl;

    private Color mSpriteDefaultColor;

    private CellIndex mCellSize;

    private Vector2 mEditStartPos;
    private CellIndex mEditStartCell;
    private bool mEditIsValid;

    public override bool EditIsExpandable() {
        return true;
    }

    public override void EditStart(Vector2 pos) {
        mEditStartPos = pos;
        mEditStartCell = GameMapController.instance.mapData.GetCellIndex(mEditStartPos);

        //initialize dimension as 1x1
        EditDragUpdate(pos);
    }

    public override void EditDragUpdate(Vector2 pos) {
        var mapData = GameMapController.instance.mapData;
        
        //update dimensions
        CellIndex curCell = mapData.GetCellIndex(pos);

        CellIndex minCell = new CellIndex() { row = Mathf.Min(curCell.row, mEditStartCell.row), col = Mathf.Min(curCell.col, mEditStartCell.col) };
        CellIndex maxCell = new CellIndex() { row = Mathf.Max(curCell.row, mEditStartCell.row), col = Mathf.Max(curCell.col, mEditStartCell.col) };

        UpdateDimensions(minCell, maxCell);
    }

    public override void EditDragEnd(Vector2 pos) {
        EditDragUpdate(pos);
    }

    public override void EditMove(Vector2 delta) {
        var pos = (Vector2)transform.position;
        pos += delta;
        transform.position = pos;

        UpdatePlacementValid();
        DimensionChanged();
    }

    public override void EditExpand(int top, int bottom, int left, int right) {
        var mapData = GameMapController.instance.mapData;

        var cellSize = GameData.instance.blockSize;
        var cellHalfSize = cellSize * 0.5f;

        //grab min and max cell position
        var extents = mColl.size * 0.5f;
        var center = (Vector2)transform.position;

        Vector2 min = (center - extents) + cellHalfSize;
        Vector2 max = (center + extents) - cellHalfSize;

        CellIndex minCell = mapData.GetCellIndex(min);
        CellIndex maxCell = mapData.GetCellIndex(max);

        if(minCell.row + bottom <= maxCell.row)
            minCell.row += bottom;

        if(minCell.col + left <= maxCell.col)
            minCell.col += left;

        if(maxCell.row + top >= minCell.row)
            maxCell.row += top;

        if(maxCell.col + right >= minCell.col)
            maxCell.col += right;

        UpdateDimensions(minCell, maxCell);
    }

    public override bool EditIsPlacementValid() {
        return mEditIsValid;
    }

    protected override void ApplyMode(Mode prevMode) {
        switch(mode) {
            case Mode.None:
                break;

            case Mode.Ghost:
                mSpriteDefaultColor = mSpriteRender.color;

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
        mSpriteDefaultColor = mSpriteRender.color;

        mCellSize = new CellIndex(1, 1);
        mBody.velocity = Vector2.zero;
        mBody.angularVelocity = 0f;

        ApplyCurrentCellSize();

        base.OnSpawned(parms);
    }

    protected override void OnDespawned() {
        mSpriteRender.color = mSpriteDefaultColor;
    }

    protected override void Awake() {
        base.Awake();

        mSpriteRender = GetComponentInChildren<SpriteRenderer>();
        mBody = GetComponent<Rigidbody2D>();
        mColl = GetComponent<BoxCollider2D>();

        mBody.useAutoMass = true;
        mColl.density = density;
    }

    private void ApplyCurrentCellSize() {
        var cellSize = GameData.instance.blockSize;

        Vector2 size = new Vector2(mCellSize.col * cellSize.x, mCellSize.row * cellSize.y);

        mSpriteRender.size = size;
        mColl.size = size;
    }

    private void UpdateDimensions(CellIndex minCell, CellIndex maxCell) {
        var mapData = GameMapController.instance.mapData;

        var cellSize = GameData.instance.blockSize;

        CellIndex newCellSize = new CellIndex(maxCell.row - minCell.row + 1, maxCell.col - minCell.col + 1);

        Vector2 min = mapData.GetPositionFromCell(minCell);
        Vector2 max = mapData.GetPositionFromCell(maxCell); max += cellSize;

        Vector2 center = Vector2.Lerp(min, max, 0.5f);

        bool sizeChanged = mCellSize != newCellSize;
        bool posChanged = (Vector2)transform.position != center;

        if(sizeChanged) {
            mCellSize = newCellSize;

            ApplyCurrentCellSize();
        }

        if(posChanged) {
            transform.position = center;
        }

        if(sizeChanged || posChanged) {
            UpdatePlacementValid();

            DimensionChanged();
        }
    }
    
    private void UpdatePlacementValid() {
        bool lastEditIsValid = mEditIsValid;
        
        //check pallete count
        //check overlap
        if(matterCount <= GameMapController.instance.PaletteCount(blockName)) {
            var checkSize = new Vector2(mColl.size.x - overlapThreshold, mColl.size.y - overlapThreshold);

            var collider = Physics2D.OverlapBox(transform.position, checkSize, 0f, GameData.instance.blockInvalidMask);

            mEditIsValid = collider == null;
        }
        else
            mEditIsValid = false;

        if(mEditIsValid != lastEditIsValid)
            UpdatePlacementValidDisplay(mEditIsValid);
    }

    private void UpdatePlacementValidDisplay(bool valid) {
        float alpha = mode == Mode.Ghost ? ghostAlpha : mSpriteDefaultColor.a;

        if(valid) {
            mSpriteRender.color = new Color(mSpriteDefaultColor.r, mSpriteDefaultColor.g, mSpriteDefaultColor.g, alpha);
        }
        else {
            mSpriteRender.color = new Color(invalidColor.r, invalidColor.g, invalidColor.g, alpha);
        }
    }
}
