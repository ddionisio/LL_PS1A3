using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHero : M8.EntityBase {

    public EntityHeroMoveController moveCtrl { get { return mMoveCtrl; } }

    private EntityHeroMoveController mMoveCtrl;

    protected override void OnDespawned() {
        //reset stuff here
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        //start ai, player control, etc
        mMoveCtrl.moveHorizontal = 1.0f;
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        //initialize data/variables
        mMoveCtrl = GetComponentInChildren<EntityHeroMoveController>();
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }
}
