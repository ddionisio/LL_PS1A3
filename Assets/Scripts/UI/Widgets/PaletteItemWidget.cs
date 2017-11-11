using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class PaletteItemWidget : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn, IPointerClickHandler {
    public const string paramBlockInfo = "blockInf";
    public const string paramShowIntro = "showIntro";

    public Image blockImage;
    public Text blockNameText;
    public Text blockCountText;
    public GameObject selectActiveGO;

    public string blockName { get { return mBlockName; } }

    private string mBlockName;
    private bool mIsSelected;
        
    public void Select(bool aSelect) {
        if(mIsSelected != aSelect) {
            mIsSelected = aSelect;

            if(mIsSelected) {
                //do animation thing

                if(selectActiveGO)
                    selectActiveGO.SetActive(true);

                //activate edit mode
                GameMapController.instance.mode = GameMapController.Mode.Edit;
                GameMapController.instance.blockNameActive = mBlockName;
            }
            else {
                //do animation thing

                if(selectActiveGO)
                    selectActiveGO.SetActive(false);

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

        //setup data
        mBlockName = blockInf.name;

        //setup state
        mIsSelected = false;

        //setup display
        blockNameText.text = M8.Localize.Get(blockInf.nameDisplayRef);

        blockImage.sprite = blockInf.icon;

        UpdateCount(GameMapController.instance.PaletteCount(mBlockName));

        if(selectActiveGO)
            selectActiveGO.SetActive(false);

        //show intro?
        if(showIntro) {
            //animation
        }
    }

    void M8.IPoolDespawn.OnDespawned() {
        CleanUpCallbacks();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        Select(!mIsSelected);
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
