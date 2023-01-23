using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add as part of the block, or just on its own
/// </summary>
public class ConductiveController : MonoBehaviour {
    public const int connectCapacity = 8;

    public struct Contact {
        public Collider2D coll;
        public ConductiveController ctrl;

        public Contact(Collider2D aColl, ConductiveController aCtrl) {
            coll = aColl;
            ctrl = aCtrl;
        }
    }

    [Header("Stats")]
    public float energyInitial;

    public float energyReceiveScale;

    public float energyTransferScale;
    public float energyTransferDelay = 0.2f;

    public float energyCapacity;  //set to zero for UNLIMITED POWER
    public bool energyIsFixed; //stored energy is fixed

    public bool explodeOnOverCapacity; //if true, kill the block when we reach above capacity

    public string killTag = "Player";
    public float killBodyExpand = 1f;
    
    [Header("Hook Ups")]
    public Block block;

    public BoxCollider2D colliderBody; //the body to determine trigger dimension, if null, our trigger will be fixed
    public float colliderBodyExpand = 2f;

    public float curEnergy {
        get { return mCurEnergy; }
        set {
            if(energyIsFixed)
                return;

            if(mCurEnergy != value) {
                if(energyCapacity > 0f)
                    mCurEnergy = Mathf.Clamp(value, 0f, energyCapacity);
                else {
                    mCurEnergy = value;
                    if(mCurEnergy < 0f)
                        mCurEnergy = 0f;
                }
            }
        }
    }

    public bool isCapacityReached {
        get {
            if(energyCapacity <= 0f)
                return false;

            return mCurEnergy >= energyCapacity;
        }
    }

    public event System.Action<ConductiveController, float> receivedCallback; //from, amt

    private BoxCollider2D mTriggerBoxColl;

    private bool mIsTriggerActive;

    private float mCurEnergy;

    private float mLastUpdateTime;

    private Collider2D[] mCollContacts;
    private M8.CacheList<Contact> mReceivers; //conductives to transfer energy
    private Vector2 mKillExt;

    public bool IsReceiver(ConductiveController other) {
        for(int i = 0; i < mReceivers.Count; i++) {
            if(mReceivers[i].ctrl == other)
                return true;
        }

        return false;
    }

    public bool IsReceiver(Collider2D otherColl) {
        for(int i = 0; i < mReceivers.Count; i++) {
            if(mReceivers[i].coll == otherColl)
                return true;
        }

        return false;
    }
    
    void Update() {
        if(curEnergy <= 0f || energyTransferScale == 0f || !mTriggerBoxColl || (block && block.state == (int)EntityState.Dead)) {
            return;
        }

        float curTime = Time.time;
        if(curTime - mLastUpdateTime >= energyTransferDelay) {
            mLastUpdateTime = curTime;

            //setup energy to transfer
            bool isDeath = explodeOnOverCapacity && isCapacityReached;

            float energyTransfer = curEnergy * energyTransferScale;
            curEnergy -= energyTransfer;

            mReceivers.Clear();

            int contactCount = Physics2D.GetContacts(mTriggerBoxColl, mCollContacts);
            if(contactCount > 0) {
                M8.EntityBase killEnt = null;

                for(int i = 0; i < contactCount; i++) {
                    var coll = mCollContacts[i];

                    //kill?
                    if(!string.IsNullOrEmpty(killTag) && !killEnt && coll.gameObject.CompareTag(killTag)) {
                        killEnt = coll.GetComponent<M8.EntityBase>();
                        continue;
                    }

                    var conductive = coll.GetComponent<ConductiveController>();

                    //check criterias
                    if(coll == mTriggerBoxColl)
                        continue;
                    if(conductive.energyReceiveScale == 0f) //cannot receive energy?
                        continue;
                    if(conductive.isCapacityReached) //capacity already reached
                        continue;
                    if(conductive.IsReceiver(this)) //are we already a receiver from this conductor?
                        continue;

                    mReceivers.Add(new Contact(coll, conductive));
                }
                                
                //distribute energy transfer
                if(mReceivers.Count > 0) {
                    float energyPerReceiver = energyTransfer / mReceivers.Count;

                    for(int i = 0; i < mReceivers.Count; i++) {
                        var ctrl = mReceivers[i].ctrl;

                        ctrl.curEnergy += energyPerReceiver;

                        if(ctrl.receivedCallback != null)
                            ctrl.receivedCallback(this, energyPerReceiver);
                    }
                }

                //kill entity?
                if(killEnt) {
                    //check if it is within kill bounds
                    if(colliderBody) {
                        Vector2 pos = transform.worldToLocalMatrix.MultiplyPoint3x4(killEnt.transform.position);
                        Vector2 boxPos = mTriggerBoxColl.offset;
                        Vector2 min = boxPos - mKillExt, max = boxPos + mKillExt;
                        if(pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y)
                            killEnt.state = (int)EntityState.Dead;
                    }
                    else
                        killEnt.state = (int)EntityState.Dead;
                }
            }

            //die from capacity reached?
            if(isDeath && block && !block.isReleased)
                block.state = (int)EntityState.Dead;
        }
    }
    
    void OnDestroy() {
        if(block) {
            block.modeChangedCallback -= OnBlockChangeMode;
        }
    }

    void OnDisable() {
        if(GameMapController.isInstantiated) {
            GameMapController.instance.modeChangeCallback -= OnGameChangeMode;
        }

        ClearConnections();
    }

    void OnEnable() {
        mLastUpdateTime = Time.time;
        mCurEnergy = energyInitial;

        if(GameMapController.isInstantiated) {
            GameMapController.instance.modeChangeCallback += OnGameChangeMode;
        }
    }

    void Awake() {
        if(block) {
            block.modeChangedCallback += OnBlockChangeMode;
                        
            mIsTriggerActive = false; //allow block's mode to determine when to activate trigger
        }
        else
            mIsTriggerActive = true; //activate trigger on start
                
        mCollContacts = new Collider2D[connectCapacity];
        mReceivers = new M8.CacheList<Contact>(connectCapacity);

        mTriggerBoxColl = GetComponent<BoxCollider2D>();

        UpdateTriggerCollider();
    }
        
    void OnBlockChangeMode(Block block) {
        switch(block.mode) {
            case Block.Mode.Solid:
                SetTriggerActive(GameMapController.instance.mode == GameMapController.Mode.Play);
                break;
            case Block.Mode.None:
            case Block.Mode.Ghost:
                SetTriggerActive(false);
                ClearConnections();
                break;
        }
    }

    void OnGameChangeMode(GameMapController.Mode mode) {
        switch(mode) {
            case GameMapController.Mode.Play:
                SetTriggerActive(true); //enable trigger if it hasn't yet (after placements)
                break;
            case GameMapController.Mode.Edit:
                break;
        }
    }

    void SetTriggerActive(bool aActive) {
        if(mIsTriggerActive != aActive) {
            mIsTriggerActive = aActive;
            UpdateTriggerCollider();
        }
    }

    void UpdateTriggerCollider() {
        if(mIsTriggerActive) {
            if(mTriggerBoxColl) {
                if(colliderBody) {
                    float ext = colliderBodyExpand * 2.0f;
                    mTriggerBoxColl.size = colliderBody.size + new Vector2(ext, ext);

                    float killExt = killBodyExpand * 2.0f;
                    mKillExt = (colliderBody.size + new Vector2(killExt, killExt)) * 0.5f;
                }

                mTriggerBoxColl.enabled = true;
            }
        }
        else {
            if(mTriggerBoxColl) mTriggerBoxColl.enabled = false;
        }
    }

    void ClearConnections() {
        mCurEnergy = 0f;
        mReceivers.Clear();
    }
}
