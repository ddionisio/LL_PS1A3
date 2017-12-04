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
    public GameObject retryButtonRoot;

    [Header("Misc HUDs")]
    public Transform miscContainer;

    private Dictionary<string, GameObject> mMiscHUDs;

    public void HideAll() {
        HUD.instance.blockMatterExpandPanel.Cancel();
        HUD.instance.palettePanel.Show(false);

        retryButtonRoot.SetActive(false);

        HideAllMisc();
    }

    public void HideAllMisc() {
        foreach(var pair in mMiscHUDs) {
            pair.Value.SetActive(false);
        }
    }

    public GameObject GetMiscHUD(string name) {
        GameObject go;
        mMiscHUDs.TryGetValue(name, out go);
        return go;
    }

    protected override void OnInstanceInit() {
        //default turned off
        blockMatterExpandPanel.gameObject.SetActive(false);

        palettePanel.gameObject.SetActive(false);
        paletteItemDrag.gameObject.SetActive(false);

        retryButtonRoot.SetActive(false);

        mMiscHUDs = new Dictionary<string, GameObject>();
        if(miscContainer) {
            for(int i = 0; i < miscContainer.childCount; i++) {
                var child = miscContainer.GetChild(i);
                var childGO = child.gameObject;
                mMiscHUDs.Add(childGO.name, childGO);
                childGO.SetActive(false);
            }
        }
    }
}
