using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PalettePanel : MonoBehaviour {
    public const int widgetCacheCapacity = 32;

    public M8.PoolController widgetPool;
    public Transform widgetContainer;

    private M8.CacheList<PaletteItemWidget> mActiveWidgets = new M8.CacheList<PaletteItemWidget>(widgetCacheCapacity);
    private bool mIsShow = false;

    public void Show(bool aShow) {
        if(mIsShow != aShow) {
            mIsShow = aShow;

            if(mIsShow) {
                gameObject.SetActive(true);

                //add widgets
                var blocks = GameData.instance.blocks;
                for(int i = 0; i < blocks.Length; i++) {
                    var blockInfo = blocks[i];

                    if(GameMapController.instance.PaletteCount(blockInfo.name) > 0)
                        AddNewBlock(blockInfo, false);
                }

                //TODO: animation

                GameMapController.instance.paletteUpdateCallback += OnPaletteUpdate;
            }
            else {
                GameMapController.instance.paletteUpdateCallback -= OnPaletteUpdate;

                //TODO: animation, then clear and hide

                //clear up widgets
                for(int i = 0; i < mActiveWidgets.Count; i++)
                    widgetPool.Release(mActiveWidgets[i].gameObject);

                mActiveWidgets.Clear();

                gameObject.SetActive(false);
            }
        }
    }

    void AddNewBlock(BlockInfo blockInfo, bool showIntro) {
        var parms = new M8.GenericParams();
        parms[PaletteItemWidget.paramBlockInfo] = blockInfo;
        parms[PaletteItemWidget.paramShowIntro] = showIntro;

        PaletteItemWidget widget = widgetPool.Spawn<PaletteItemWidget>(blockInfo.name, widgetContainer, parms);
        mActiveWidgets.Add(widget);
    }

    void OnPaletteUpdate(string blockName, int amount, int delta) {
        //allocate new widget?
        int widgetInd = -1;
        for(int i = 0; i < mActiveWidgets.Count; i++) {
            if(mActiveWidgets[i].blockName == blockName) {
                widgetInd = i;
                break;
            }
        }

        if(widgetInd == -1) {
            var blockInfo = GameData.instance.GetBlockInfo(blockName);
            AddNewBlock(blockInfo, true);
        }
    }
}