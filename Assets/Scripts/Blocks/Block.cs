using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Block : M8.EntityBase {
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

    [Header("Heat Property")]
    public float heatAbsorptionRate; //amount of heat to absorb from source transfer
    public float heatTransferRate; //amount of heat to transfer to others on contacts
    public float heatCapacityPerArea; //amount of heat it can absorb (per area)
    public float heatCritical; //amount of heat above capacity to reach in order to: melt, burn, etc. (set to -1 to ignore critical)

    [Header("Conduction")]
    public float conductionTransferScale; //scale of received energy to transfer to another (set to 0 to absorb the entire energy, e.g. rubber)
    public float conductionThreshold; //amount of energy it can withstand, if energy is higher than threshold, then the block explodes

    public abstract Type type { get; }

    public virtual int matterCount { get { return cellSize.area; } }

    public abstract CellIndex cellSize { get; }
    
    /// <summary>
    /// Where to attach the edit control
    /// </summary>
    public abstract Transform editAttach { get; }

    /// <summary>
    /// Bounds that define the dimension for edit control
    /// </summary>
    public abstract Bounds editBounds { get; }

    /// <summary>
    /// Area to allow user to gather the block
    /// </summary>
    public abstract Bounds gatherBounds { get; }

    public string blockName { get { return mBlockName; } }

    public Mode mode {
        get { return mCurMode; }
        set {
            if(mCurMode != value) {
                var prevMode = mCurMode;
                mCurMode = value;

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

    //during edit
    public abstract bool EditIsExpandable();
    public abstract void EditStart(Vector2 pos);
    public abstract void EditDragUpdate(Vector2 pos);
    public abstract void EditDragEnd(Vector2 pos);
    public abstract void EditMove(Vector2 delta);
    public abstract void EditExpand(int top, int bottom, int left, int right);
    public abstract bool EditIsPlacementValid();
    
    protected abstract void ApplyMode(Mode prevMode);

    protected void DimensionChanged() {
        if(dimensionChangedCallback != null)
            dimensionChangedCallback(this);
    }

    protected override void OnDespawned() {
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
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }
}
