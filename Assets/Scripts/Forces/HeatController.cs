using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Can add this to blocks, triggers
/// </summary>
public class HeatController : MonoBehaviour {
    public const int contactCapacity = 8;

    public struct Contact {
        public Collider2D coll;
        public HeatController heat;

        public Contact(Collider2D aColl, HeatController aHeat) {
            coll = aColl;
            heat = aHeat;
        }

        public void TransferHeat(HeatController source, float amount) {
            heat.ReceiveHeat(source, amount);
        }
    }

    public float amountStart; //starting amount of heat
    public float amountCapacityPerMass; //maximum amount of heat it can absorb before able to transfer (if no body, assume mass = 1)
    public bool amountIsFixed; //if true, amount remains constant (cannot add or remove)

    public float absorptionScale; //amount of heat per second to absorb from source transfer

    public float transferCap;   //amount to cap transfer based on heat amount
    public float transferScale; //amount of heat scaled from amount (capped by transferCap) to send to contacts

    public float updateDelay; //delay to process heat per update
    
    public float amountCurrent { get { return mCurAmount; } }
    public float amountCapacity {
        get {            
            if(mBody)
                return amountCapacityPerMass * mBody.mass;
            else
                return amountCapacityPerMass;
        }
    }

    public M8.EntityBase entity { get { return mEntity; } }
    public Rigidbody2D body { get { return mBody; } }

    public event System.Action<HeatController, float> amountChangedCallback; //self, prev. amount

    private float mCurAmount;

    private M8.EntityBase mEntity;
    private Rigidbody2D mBody;

    private M8.CacheList<Contact> mTriggerContacts;
    private M8.CacheList<Contact> mCollisionContacts;

    private float mLastUpdateTime;
    private float mLastAmount;

    public void ReceiveHeat(HeatController source, float amount) {
        if(!amountIsFixed && absorptionScale != 0f) {
            float delta = amount * absorptionScale;
            mCurAmount = Mathf.Clamp(mCurAmount + delta, 0f, amountCapacity);
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        //check if already contained
        for(int i = 0; i < mTriggerContacts.Count; i++) {
            if(mTriggerContacts[i].coll == collision)
                return;
        }

        //add
        HeatController heat = collision.GetComponent<HeatController>();
        if(heat) {
            if(mTriggerContacts.IsFull) mTriggerContacts.Expand();

            mTriggerContacts.Add(new Contact(collision, heat));
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        for(int i = 0; i < mTriggerContacts.Count; i++) {
            if(mTriggerContacts[i].coll == collision) {
                mTriggerContacts.RemoveAt(i);
                return;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        var coll = collision.collider;

        //check if already contained
        for(int i = 0; i < mCollisionContacts.Count; i++) {
            if(mCollisionContacts[i].coll == coll)
                return;
        }

        //add
        HeatController heat = coll.GetComponent<HeatController>();
        if(heat) {
            if(mCollisionContacts.IsFull) mCollisionContacts.Expand();

            mCollisionContacts.Add(new Contact(coll, heat));
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        var coll = collision.collider;

        for(int i = 0; i < mCollisionContacts.Count; i++) {
            if(mCollisionContacts[i].coll == coll) {
                mCollisionContacts.RemoveAt(i);
                return;
            }
        }
    }

    void Update() {
        float curTime = Time.time;

        //transfer heat
        if(curTime - mLastUpdateTime >= updateDelay) {
            mLastUpdateTime = curTime;

            //calculate transfer heat
            float transferAmount = mCurAmount * transferScale;

            if(transferCap > 0f && transferAmount > transferCap)
                transferAmount = transferCap;

            //transfer
            if(transferAmount > 0f) {
                for(int i = 0; i < mTriggerContacts.Count; i++)
                    mTriggerContacts[i].TransferHeat(this, transferAmount);

                for(int i = 0; i < mCollisionContacts.Count; i++)
                    mCollisionContacts[i].TransferHeat(this, transferAmount);

                transferAmount = 0f;
            }
            
            if(mLastAmount != mCurAmount) {
                if(amountChangedCallback != null)
                    amountChangedCallback(this, mLastAmount);

                mLastAmount = mCurAmount;
            }
        }
    }

    void OnDestroy() {
        if(mEntity) {
            mEntity.spawnCallback -= OnEntitySpawn;
            mEntity.releaseCallback -= OnEntityRelease;
        }
    }

    void OnDisable() {
        ResetContacts();
    }

    void OnEnable() {
        mLastUpdateTime = Time.time;
    }

    void Awake() {
        mEntity = GetComponent<M8.EntityBase>();
        if(mEntity) {
            mEntity.spawnCallback += OnEntitySpawn;
            mEntity.releaseCallback += OnEntityRelease;
        }

        mBody = GetComponent<Rigidbody2D>();

        mTriggerContacts = new M8.CacheList<Contact>(contactCapacity);
        mCollisionContacts = new M8.CacheList<Contact>(contactCapacity);
    }

    void Start() {
        if(!mEntity) {
            mLastAmount = mCurAmount = amountStart;
            mLastUpdateTime = Time.time;
        }
    }

    void OnEntitySpawn(M8.EntityBase ent) {
        mLastAmount = mCurAmount = amountStart;
        mLastUpdateTime = Time.time;
    }

    void OnEntityRelease(M8.EntityBase ent) {
        ResetAll();
    }

    void ResetAll() {
        //reset properties
        mCurAmount = 0f;

        ResetContacts();
    }

    void ResetContacts() {
        mTriggerContacts.Clear();
        mCollisionContacts.Clear();
    }
}
