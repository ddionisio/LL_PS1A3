using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductiveEnergyToggleGOActive : ConductiveEnergyToggle {
    public GameObject[] targets;

    protected override void OnEnable() {
        base.OnEnable();

        for(int i = 0; i < targets.Length; i++)
            targets[i].SetActive(false);
    }

    protected override void OnPowerChanged() {
        for(int i = 0; i < targets.Length; i++)
            targets[i].SetActive(isPowered);
    }
}
