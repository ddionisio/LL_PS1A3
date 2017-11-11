using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHero : M8.EntityBase {
    public enum MoveState {
        Stop,
        Left,
        Right
    }

    public EntityHeroMoveController moveCtrl { get { return mMoveCtrl; } }

    public MoveState moveState {
        get { return mMoveState; }
        set {
            if(mMoveState != value) {
                if(mMoveState != MoveState.Stop)
                    mMoveStatePrev = mMoveState;

                mMoveState = value;

                switch(mMoveState) {
                    case MoveState.Stop:
                        mMoveCtrl.moveHorizontal = 0f;
                        break;
                    case MoveState.Left:
                        mMoveCtrl.moveHorizontal = -1f;
                        break;
                    case MoveState.Right:
                        mMoveCtrl.moveHorizontal = 1f;
                        break;
                }
            }
        }
    }

    public MoveState moveStatePrev { get { return mMoveStatePrev; } }

    private EntityHeroMoveController mMoveCtrl;

    private MoveState mMoveState;
    private MoveState mMoveStatePrev;

    protected override void OnDespawned() {
        //reset stuff here
        mMoveCtrl.ResetCollision();
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //initial states
        mMoveStatePrev = MoveState.Right; //TODO: could be set by the map
        mMoveState = MoveState.Stop;

        //populate data/state for ai, player control, etc.

        //start ai, player control, etc        
    }

    protected override void OnDestroy() {
        //dealloc here
        if(mMoveCtrl) {
            mMoveCtrl.collisionEnterCallback -= OnMoveCollisionEnter;
            mMoveCtrl.triggerEnterCallback -= OnMoveTriggerEnter;
            mMoveCtrl.triggerExitCallback -= OnMoveTriggerExit;
        }

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        mMoveCtrl = GetComponentInChildren<EntityHeroMoveController>();

        mMoveCtrl.collisionEnterCallback += OnMoveCollisionEnter;
        mMoveCtrl.triggerEnterCallback += OnMoveTriggerEnter;
        mMoveCtrl.triggerExitCallback += OnMoveTriggerExit;
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void OnMoveCollisionEnter(EntityHeroMoveController ctrl, Collision2D coll) {
        //check if it's a side collision
        if(!ctrl.isSlopSlide && (ctrl.collisionFlags & CollisionFlags.Sides) != CollisionFlags.None)
            ctrl.moveHorizontal *= -1.0f;
    }

    void OnMoveTriggerEnter(EntityHeroMoveController ctrl, Collider2D coll) {

    }

    void OnMoveTriggerExit(EntityHeroMoveController ctrl, Collider2D coll) {

    }
}
