using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatGODeactivate : MonoBehaviour {

    private HeatController mHeatCtrl;
    
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
        if(heat.amountCurrent >= heat.amountCapacity) {
            gameObject.SetActive(false);
        }
    }
}
