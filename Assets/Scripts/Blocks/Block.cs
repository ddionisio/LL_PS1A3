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
        Conductive = 0x1, //can transmit electricity
        Buoyant = 0x2, //can float on water
        Metallic = 0x4, //for magnets
        Dissolvable = 0x8 //things that dissolve under water
    }
        
    [Header("Info")]
    public float mass = 1f;
    public Flags propertyFlags;

    public abstract Type type { get; }

    public abstract int matterCount { get; }

    public abstract Collider2D mainCollider { get; }

    public string blockName { get { return mBlockName; } }

    public Mode mode {
        get { return mCurMode; }
        set {
            if(mCurMode != value) {
                mCurMode = value;

                ApplyMode();

                if(modeChangedCallback != null)
                    modeChangedCallback(this);
            }
        }
    }

    public event System.Action<Block> modeChangedCallback;

    private string mBlockName;
    private Mode mCurMode = Mode.None;

    //during edit
    public abstract void EditStart(Vector2 pos);
    public abstract void EditUpdate(Vector2 pos);
    public abstract void EditEnd(Vector2 pos);
    public abstract bool EditIsPlacementValid();

    protected abstract void ApplyMode();

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
