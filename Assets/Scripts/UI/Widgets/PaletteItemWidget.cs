using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteItemWidget : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn {
    public const string paramBlockInfo = "blockInf";

    public Image blockImage;
    public Text blockNameText;
    public Text blockCountText;

    public string blockName { get { return mBlockName; } }

    private string mBlockName;
    private bool mIsSelected;
        
    public void Select(bool aSelect) {
        if(mIsSelected != aSelect) {
            mIsSelected = aSelect;

            if(mIsSelected) {
                //do animation thing

                //activate edit mode
            }
            else {
                //do animation thing

                //deactivate edit mode
            }
        }
    }

    private void UpdateCount(int amount) {
        blockCountText.text = amount.ToString("D2");
    }

    void OnPaletteUpdate(string blockName, int amount, int delta) {
        if(amount > 0) {
            UpdateCount(amount);
        }
        else {
            //do animation thing
            //delete after animation thing
            M8.PoolController.ReleaseAuto(gameObject);
        }
    }

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        GameMapData.instance.paletteUpdateCallback += OnPaletteUpdate;

        BlockInfo blockInf = parms.GetValue<BlockInfo>(paramBlockInfo);

        mBlockName = blockInf.name;

        //setup display
        blockNameText.text = M8.Localize.Get(blockInf.name);

        blockImage.sprite = blockInf.icon;

        UpdateCount(GameMapData.instance.PaletteCount(mBlockName));
    }

    void M8.IPoolDespawn.OnDespawned() {
        GameMapData.instance.paletteUpdateCallback -= OnPaletteUpdate;
    }
}
