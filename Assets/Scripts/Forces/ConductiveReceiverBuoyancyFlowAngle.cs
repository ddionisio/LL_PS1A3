using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductiveReceiverBuoyancyFlowAngle : ConductiveReceiver {
    public BuoyancyEffector2D buoyancy;
    public float toAngle;

    private float mDefaultAngle;

    void OnDisable() {
        buoyancy.flowAngle = mDefaultAngle;
    }

    protected override void Awake() {
        base.Awake();

        mDefaultAngle = buoyancy.flowAngle;
    }

    protected override void OnPowerChanged() {
        if(isPowered)
            buoyancy.flowAngle = toAngle;
        else
            buoyancy.flowAngle = mDefaultAngle;
    }
}
