using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General access to UI related to HUD
/// </summary>
public class HUD : M8.SingletonBehaviour<HUD> {
    public PalettePanel palettePanel;

    protected override void OnInstanceInit() {
        //default turned off
        palettePanel.gameObject.SetActive(false);
    }
}
