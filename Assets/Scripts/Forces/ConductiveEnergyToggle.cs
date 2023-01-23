using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConductiveEnergyToggle : MonoBehaviour {
    public ConductiveController controller;

    public float inactiveDelay = 0.5f; //if no energy received for this long, turn off

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
    private float mLastTimeEnergyReceived;

    protected abstract void OnPowerChanged();

    protected virtual void OnEnable() {
        mIsPowered = false;
    }

    protected virtual void OnDestroy() {
        if(controller)
            controller.receivedCallback -= OnEnergyReceived;
    }

    protected virtual void Awake() {
        if(!controller)
            controller = GetComponent<ConductiveController>();

        if(controller)
            controller.receivedCallback += OnEnergyReceived;
    }
    
    void Update () {
        if(mIsPowered) {
            float time = Time.time - mLastTimeEnergyReceived;
            if(time >= inactiveDelay) {
                mIsPowered = false;
                OnPowerChanged();
            }
        }
    }

    void OnEnergyReceived(ConductiveController other, float amt) {
        if(amt > 0f) {
            if(!mIsPowered) {
                mIsPowered = true;
                OnPowerChanged();
            }

            mLastTimeEnergyReceived = Time.time;
        }
    }
}
