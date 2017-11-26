using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Block : M8.EntityBase, IComparable<Block> {
    //spawn params
    public const string paramName = "name"; //Block Name
    public const string paramMode = "mode";  //Mode

    public enum Type {
        Matter, //for most things, can be resized during edit
        Widget //special block that can't be resized during edit, just placed
    }

    public enum Mode {
        None, //despawned
        Ghost, //during edit for placement and sizing
        Solid //deployed
    }

    [System.Flags]
    public enum Flags {
        None = 0x0,
        Metallic = 0x1, //for magnets
    }
        
    [Header("Info")]
    public float density = 1f;
    [M8.EnumMask]
    public Flags propertyFlags;
    public float deathDelay = 1.5f;

    [Header("Attaches")]
    public GameObject[] solidActiveGO;
    public GameObject[] ghostActiveGO;
    
    public abstract Type type { get; }

    public virtual int matterCount { get { return cellSize.area; } }

    public abstract CellIndex cellSize { get; }

    public abstract Rigidbody2D mainBody { get; }
    
    /// <summary>
    /// Where to attach the edit control
    /// </summary>
    public abstract Transform editAttach { get; }

    /// <summary>
    /// Bounds that define the dimension for edit control
    /// </summary>
    public abstract Bounds editBounds { get; }
    
    public string blockName { get { return mBlockName; } }

    public Mode mode {
        get { return mCurMode; }
        set {
            if(mCurMode != value) {
                var prevMode = mCurMode;
                mCurMode = value;

                //activate mode GOs
                for(int i = 0; i < solidActiveGO.Length; i++) {
                    if(solidActiveGO[i])
                        solidActiveGO[i].SetActive(mCurMode == Mode.Solid);
                }

                for(int i = 0; i < ghostActiveGO.Length; i++) {
                    if(ghostActiveGO[i])
                        ghostActiveGO[i].SetActive(mCurMode == Mode.Ghost);
                }

                ApplyMode(prevMode);

                if(modeChangedCallback != null)
                    modeChangedCallback(this);
            }
        }
    }

    public event System.Action<Block> dimensionChangedCallback;
    public event System.Action<Block> modeChangedCallback;

    private string mBlockName;
    private Mode mCurMode = Mode.None;

    private Coroutine mRout;

    //during edit
    public abstract bool EditIsExpandable();
    public abstract void EditSetPosition(Vector2 pos);
    public abstract void EditMove(Vector2 delta);
    public abstract void EditExpand(int top, int bottom, int left, int right);
    public abstract bool EditIsPlacementValid();
    public abstract void EditEnableCollision(bool aEnable); //used while in ghost mode (usu. enable/disable collision to allow other blocks to update placement valid properly)
    
    /// <summary>
    /// Start by snapping position to grid
    /// </summary>
    public void EditStart(Vector2 pos) {
        var mapData = GameMapController.instance.mapData;
        var cellSize = GameData.instance.blockSize;

        CellIndex curCell = mapData.GetCellIndex(pos);
        
        EditSetPosition(mapData.GetPositionFromCell(curCell) + cellSize * 0.5f);
    }

    public bool IsCountValid() {
        int paletteBlockCount = GameMapController.instance.PaletteCount(mBlockName);
        int ghostBlockCount = HUD.instance.palettePanel.GetGhostCountExclude(this);
        
        return ghostBlockCount + matterCount <= paletteBlockCount;
    }

    protected abstract void ApplyMode(Mode prevMode);

    protected void DimensionChanged() {
        if(dimensionChangedCallback != null)
            dimensionChangedCallback(this);
    }

    protected override void StateChanged() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        switch((EntityState)state) {
            case EntityState.Dead:
                mRout = StartCoroutine(DoDeath());
                break;
        }
    }

    protected override void OnDespawned() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        //reset stuff here
        mode = Mode.None;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        //start ai, player control, etc
        if(!parms.TryGetValue(paramName, out mBlockName)) {
            mBlockName = name;
            Debug.LogWarning("No block name give for: " + name);
        }

        state = (int)EntityState.Normal;

        Mode toMode;
        if(parms.TryGetValue(paramMode, out toMode))
            mode = toMode;
        else
            mode = Mode.Solid;
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        for(int i = 0; i < solidActiveGO.Length; i++) {
            if(solidActiveGO[i])
                solidActiveGO[i].SetActive(false);
        }

        for(int i = 0; i < ghostActiveGO.Length; i++) {
            if(ghostActiveGO[i])
                ghostActiveGO[i].SetActive(false);
        }
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    IEnumerator DoDeath() {
        yield return new WaitForSeconds(deathDelay);

        //refund matter count
        GameMapController.instance.PaletteChange(mBlockName, matterCount);

        Release();
    }
    
    public int CompareTo(Block other) {
        return (int)type - (int)other.type;
    }
}
