using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General access to UI related to HUD
/// </summary>
[M8.PrefabFromResource("UI")]
public class HUD : M8.SingletonBehaviour<HUD> {
    public RectTransform root;

    [Header("World HUDs")]
    public BlockMatterExpandPanel blockMatterExpandPanel;

    [Header("Screen HUDs")]
    public PalettePanel palettePanel;
    public PaletteItemDragWidget paletteItemDrag;

    public void HideAll() {
        HUD.instance.blockMatterExpandPanel.Cancel();
        HUD.instance.palettePanel.Show(false);
    }

    protected override void OnInstanceInit() {
        //default turned off
        blockMatterExpandPanel.gameObject.SetActive(false);

        palettePanel.gameObject.SetActive(false);
        paletteItemDrag.gameObject.SetActive(false);
    }
}
