using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductiveReceiverGOActive : ConductiveReceiver {
    public GameObject target;

    protected override void OnEnable() {
        base.OnEnable();

        target.SetActive(false);
    }

    protected override void OnPowerChanged() {
        target.SetActive(isPowered);
    }
}
