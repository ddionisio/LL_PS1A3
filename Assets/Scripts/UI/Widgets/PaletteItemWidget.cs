using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteItemWidget : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn {
    public const string paramBlockInfo = "blockInf";
    public const string paramShowIntro = "showIntro";

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
                GameMapController.instance.mode = GameMapController.Mode.Edit;
                GameMapController.instance.blockNameActive = mBlockName;
            }
            else {
                //do animation thing

                //deactivate edit mode
                GameMapController.instance.mode = GameMapController.Mode.Play;
            }
        }
    }
        
    void OnDestroy() {
        CleanUpCallbacks();
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

    void OnBlockActiveChange(string newBlockName, string prevBlockName) {
        //deselect if we were the previous block, and there is no new block active
        if(string.IsNullOrEmpty(newBlockName) && prevBlockName == mBlockName)
            Select(false);
    }

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        GameMapController.instance.paletteUpdateCallback += OnPaletteUpdate;
        GameMapController.instance.blockActiveChangeCallback += OnBlockActiveChange;

        BlockInfo blockInf = parms.GetValue<BlockInfo>(paramBlockInfo);
        bool showIntro = parms.GetValue<bool>(paramShowIntro);

        mBlockName = blockInf.name;

        //setup display
        blockNameText.text = M8.Localize.Get(blockInf.name);

        blockImage.sprite = blockInf.icon;

        UpdateCount(GameMapController.instance.PaletteCount(mBlockName));

        //show intro?
        if(showIntro) {
            //animation
        }
    }

    void M8.IPoolDespawn.OnDespawned() {
        CleanUpCallbacks();
    }

    private void UpdateCount(int amount) {
        blockCountText.text = amount.ToString("D2");
    }

    private void CleanUpCallbacks() {
        if(GameMapController.isInstantiated) {
            GameMapController.instance.paletteUpdateCallback -= OnPaletteUpdate;
            GameMapController.instance.blockActiveChangeCallback -= OnBlockActiveChange;
        }
    }
}
