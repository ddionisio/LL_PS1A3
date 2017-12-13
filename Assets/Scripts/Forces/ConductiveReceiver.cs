using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConductiveReceiver : MonoBehaviour {
    public ConductiveController controller;

    [Header("Stats")]
    public float energyRequire; //at what energy do we need to be powered on
    public float energyRate; //how much energy to consume per second

    public bool isPowered {
        get { return mIsPowered; }
        set {
            if(mIsPowered != value) {
                mIsPowered = value;
                OnPowerChanged();
            }
        }
    }

    private bool mIsPowered;

    protected abstract void OnPowerChanged();

    protected virtual void OnEnable() {
        mIsPowered = false;
    }

    protected virtual void Awake() {
        if(!controller)
            controller = GetComponent<ConductiveController>();
    }

    protected virtual void Update() {
        if(controller.curEnergy > 0f) {
            if(energyRate > 0f) {
                float amt = energyRate * Time.deltaTime;
                controller.curEnergy -= amt;
            }

            isPowered = controller.curEnergy >= energyRequire;
        }
        else
            isPowered = false;
    }
}
