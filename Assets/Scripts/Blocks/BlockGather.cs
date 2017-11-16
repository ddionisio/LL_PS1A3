using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Use this to make the block gatherable (all of them)
/// </summary>
public class BlockGather : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public enum State {
        Start,
        End,
    }

    public Block block;
    public float sizeOfs = 0.25f;

    public System.Action<State> stateChangeCallback;

    //we assume the collider to be box to simplify things
    private BoxCollider2D mColl;
    private Coroutine mRout;

    void OnDestroy() {
        if(block) {
            block.spawnCallback -= OnBlockSpawned;
            block.releaseCallback -= OnBlockDespawned;
        }

        ClearCallbacks();
    }

    void Awake() {
        mColl = GetComponent<BoxCollider2D>();
        if(!mColl) {
            Debug.LogError("Missing Box Collider 2D!");
            return;
        }

        //hook ups
        block.spawnCallback += OnBlockSpawned;
        block.releaseCallback += OnBlockDespawned;
    }

    void OnBlockSpawned(M8.EntityBase ent) {
        block.modeChangedCallback += OnBlockModeChanged;

        GameMapController.instance.modeChangeCallback += OnGameModeChanged;

        UpdateCollider();
    }

    void OnBlockDespawned(M8.EntityBase ent) {
        ClearCallbacks();

        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }

    void OnBlockModeChanged(Block b) {
        UpdateCollider();
    }

    void OnGameModeChanged(GameMapController.Mode mode) {
        UpdateCollider();
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        if(mRout == null) {
            mRout = StartCoroutine(DoGather());
        }
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
        CancelGather();
    }

    IEnumerator DoGather() {
        if(stateChangeCallback != null)
            stateChangeCallback(State.Start);

        float lastTime = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup - lastTime < GameData.instance.blockPickupDelay)
            yield return null;

        if(stateChangeCallback != null)
            stateChangeCallback(State.End);

        mRout = null;

        //restore palette amount
        GameMapController.instance.PaletteChange(block.blockName, block.matterCount);

        //release block
        M8.PoolController.ReleaseAuto(block.gameObject);
    }

    private void CancelGather() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;

            if(stateChangeCallback != null)
                stateChangeCallback(State.End);
        }
    }

    private void UpdateCollider() {
        bool collActive = block.mode == Block.Mode.Solid && GameMapController.instance.mode == GameMapController.Mode.Edit;

        mColl.enabled = collActive;

        if(collActive) {
            //setup box size based on bound size of block
            var blockCollBounds = block.gatherBounds;

            var size = (Vector2)blockCollBounds.size;
            size.x += sizeOfs;
            size.y += sizeOfs;

            //TODO: may require changing offset for special case block types

            mColl.size = size;
        }
        else
            CancelGather(); //just in case it was active for some reason
    }
    
    private void ClearCallbacks() {
        if(block) {
            block.modeChangedCallback -= OnBlockModeChanged;
        }

        if(GameMapController.instance) {
            GameMapController.instance.modeChangeCallback -= OnGameModeChanged;
        }
    }
}
