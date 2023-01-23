using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockConnectTrigger : MonoBehaviour {
    [System.Serializable]
    public struct IDData {
        public string id;
        public Color color;
        public SpriteRenderer[] spritesApplyColor;

        public void ApplyColor() {
            for(int i = 0; i < spritesApplyColor.Length; i++) {
                if(spritesApplyColor[i])
                    spritesApplyColor[i].color = color;
            }
        }
    }

    public IDData[] ids;

    public LayerMask blockLayerMask;
    public Bounds bounds;

    private Collider2D[] mColls = new Collider2D[8];
    
    private M8.CacheList<Block> mAttachedBlocks = new M8.CacheList<Block>(8);
    
    void OnDestroy() {
        if(GameMapController.instance)
            GameMapController.instance.modeChangeCallback -= OnGameModeChange;
    }

    void Awake() {
        GameMapController.instance.modeChangeCallback += OnGameModeChange;
    }
    
    void OnDrawGizmos() {
        Vector3 pos = transform.position + bounds.center;

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireCube(pos, bounds.size);
    }

    void OnGameModeChange(GameMapController.Mode mode) {
        switch(mode) {
            case GameMapController.Mode.Edit:
                StopAllCoroutines();

                //disconnect current blocks
                for(int i = 0; i < mAttachedBlocks.Count; i++)
                    BlockConnectController.instance.SetConnect(mAttachedBlocks[i].blockName, false);
                mAttachedBlocks.Clear();
                break;

            case GameMapController.Mode.Play:
                StartCoroutine(DoBlockUpdate());
                break;
        }
    }

    int GetIDData(Block b) {
        var blockName = b.blockName;

        for(int i = 0; i < ids.Length; i++) {
            if(blockName == ids[i].id)
                return i;
        }

        return -1;
    }

    IEnumerator DoBlockUpdate() {
        yield return null;

        Vector2 pos = transform.position + bounds.center;

        //grab blocks
        int collCount = Physics2D.OverlapBoxNonAlloc(pos, bounds.size, 0f, mColls, blockLayerMask);
        for(int i = 0; i < collCount; i++) {
            var b = mColls[i].GetComponent<Block>();
            if(b) {
                int ind = GetIDData(b);
                if(ind != -1) {
                    mAttachedBlocks.Add(b);

                    ids[ind].ApplyColor();
                                        
                    BlockConnectController.instance.SetConnect(b.blockName, true);
                }
            }
        }
    }
}
