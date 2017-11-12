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

    public bool isSelected { get { return GameMapController.instance.blockNameActive == mBlockName; } }

    public event Action<PaletteItemWidget> releaseCallback;

    private string mBlockName;

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
        //select if we are the new block
        if(mBlockName == newBlockName)
            ApplySelected(true);
        //deselect if we were the previous block, and there is no new block active
        else if(mBlockName == prevBlockName)
            ApplySelected(false);
    }
        
    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        GameMapController.instance.paletteUpdateCallback += OnPaletteUpdate;
        GameMapController.instance.blockActiveChangeCallback += OnBlockActiveChange;

        BlockInfo blockInf = parms.GetValue<BlockInfo>(paramBlockInfo);
        bool showIntro = parms.GetValue<bool>(paramShowIntro);

        //setup data
        mBlockName = blockInf.name;

        //setup state

        //setup display
        blockNameText.text = M8.Localize.Get(blockInf.nameDisplayRef);

        blockImage.sprite = blockInf.icon;

        UpdateCount(GameMapController.instance.PaletteCount(mBlockName));

        ApplySelected(isSelected);

        //show intro?
        if(showIntro) {
            //animation
        }
    }

    void M8.IPoolDespawn.OnDespawned() {
        CleanUpCallbacks();

        if(releaseCallback != null)
            releaseCallback(this);
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        //toggle select
        if(isSelected)
            GameMapController.instance.blockNameActive = "";
        else
            GameMapController.instance.blockNameActive = mBlockName;
    }

    private void ApplySelected(bool select) {
        //select if we are the new block
        if(select) {
            //do animation thing

            if(selectActiveGO)
                selectActiveGO.SetActive(true);
        }
        //deselect if we were the previous block, and there is no new block active
        else {
            //do animation thing

            if(selectActiveGO)
                selectActiveGO.SetActive(false);
        }
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
