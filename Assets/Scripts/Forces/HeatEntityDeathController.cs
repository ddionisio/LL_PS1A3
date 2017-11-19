using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Set attached entity to "death" when we reach heat capacity
/// </summary>
public class HeatEntityDeathController : MonoBehaviour {
    public EntityState entityDeathState;

    private HeatController mHeatCtrl;
    private bool mIsDead;

    void OnEnable() {
        mIsDead = false;
    }

    void OnDestroy() {
        if(mHeatCtrl) {
            mHeatCtrl.amountChangedCallback -= OnHeatAmountUpdate;
        }
    }

    void Awake() {
        mHeatCtrl = GetComponent<HeatController>();
        mHeatCtrl.amountChangedCallback += OnHeatAmountUpdate;
    }

    void OnHeatAmountUpdate(HeatController heat, float prevAmt) {
        if(!mIsDead) {
            if(heat.entity)
                heat.entity.state = (int)entityDeathState;

            mIsDead = true;
        }
    }
}
