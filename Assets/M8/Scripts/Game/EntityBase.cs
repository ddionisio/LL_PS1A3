using UnityEngine;
using System.Collections;

/*
Basic Framework would be something like this:
    protected override void OnDespawned() {
        //reset stuff here
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //populate data/state for ai, player control, etc.

        //start ai, player control, etc
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }
    
    protected override void Awake() {
        base.Awake();

        //initialize data/variables
    }

    // Use this for one-time initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }
*/

namespace M8 {
    [AddComponentMenu("M8/Entity/EntityBase")]
    public class EntityBase : MonoBehaviour, IPoolSpawn, IPoolDespawn {
        public const int StateInvalid = -1;

        public delegate void OnGenericCall(EntityBase ent);
        public delegate void OnSetBool(EntityBase ent, bool b);
        
        /// <summary>
        /// if we want FSM/other stuff to activate on start (when placing entities on scene).
        /// Set this to true if the entity is not gonna be spawned via Pool
        /// </summary>
        public bool activateOnStart = false;

        public EntityActivator activator;

        public event OnGenericCall setStateCallback;
        public event OnGenericCall spawnCallback; //called after a slight delay during OnSpawned (at least after one fixed-update)
        public event OnGenericCall releaseCallback;

        private int mState = StateInvalid;
        private int mPrevState = StateInvalid;
        
        private bool mIsSpawned = false;
        private bool mIsStarted = false;
        private GenericParams mSpawnParams; //when spawn hasn't happened because of activator

        private SceneSerializer mSerializer = null;
        
        private PoolDataController mPoolData;
        protected PoolDataController poolData {
            get {
                if(mPoolData == null) mPoolData = GetComponent<PoolDataController>();
                return mPoolData;
            }
        }

        public string spawnType {
            get {
                if(poolData == null) {
                    return name;
                }

                return poolData.factoryKey;
            }
        }

        /// <summary>
        /// Set by call from Spawn, so use that instead of directly spawning via pool manager.
        /// </summary>
        public string spawnGroup { get { return poolData == null ? "" : poolData.group; } }

        ///<summary>entity is instantiated with activator set to disable on start, call spawn after activator sends a wakeup call.</summary>
        public bool isSpawned {
            get { return mIsSpawned; }
        }

        public int state {
            get { return mState; }

            set {
                if(mState != value) {
                    if(mState != StateInvalid)
                        mPrevState = mState;

                    mState = value;

                    if(setStateCallback != null) {
                        setStateCallback(this);
                    }

                    StateChanged();
                }
            }
        }

        public int prevState {
            get { return mPrevState; }
        }

        public bool isReleased {
            get {
                if(poolData == null)
                    return false;

                return poolData.claimed;
            }
        }
        
        public SceneSerializer serializer {
            get { return mSerializer; }
        }
        
        /// <summary>
        /// Explicit call to remove entity.
        /// </summary>
        public virtual void Release() {
            if(poolData != null) {
#if POOLMANAGER
            transform.parent = PoolManager.Pools[poolData.group].group;
            PoolManager.Pools[poolData.group].Despawn(transform);
#else
                PoolController.ReleaseFromGroup(poolData.group, poolData);
#endif
            }
            else {
                //just disable the object, really no need to destroy
                _OnDespawned();
                gameObject.SetActive(false);
                /*
                if(gameObject.activeInHierarchy)
                    StartCoroutine(DestroyDelay());
                else {
                    Destroy(gameObject);
                }*/
            }
        }

        /// <summary>
        /// Force change to the same state, will also set prevState to the current state
        /// </summary>
        public void RestartState() {
            mPrevState = mState;

            if(setStateCallback != null) {
                setStateCallback(this);
            }

            StateChanged();
        }

        protected virtual void OnDespawned() {
        }

        protected virtual void ActivatorWakeUp() {
            if(!gameObject.activeInHierarchy)
                return;

            if(!mIsSpawned) //we didn't get a chance to start properly before being inactivated
                ExecuteSpawn();
        }

        protected virtual void ActivatorSleep() {
        }

        protected virtual void OnDestroy() {
            if(activator != null && activator.defaultParent == transform) {
                activator.Release(true);
            }

            setStateCallback = null;
            spawnCallback = null;
            releaseCallback = null;
        }

        protected virtual void OnEnable() {
            //we didn't get a chance to start properly before being inactivated
            if(mIsStarted && (activator == null || activator.isActive) && !mIsSpawned)
                ExecuteSpawn();
        }

        protected virtual void Awake() {
            mPoolData = GetComponent<PoolDataController>();

            mSerializer = GetComponent<SceneSerializer>();

            if(activator == null)
                activator = GetComponentInChildren<EntityActivator>();

            if(activator != null) {
                activator.awakeCallback += ActivatorWakeUp;
                activator.sleepCallback += ActivatorSleep;
            }
        }

        // Use this for initialization
        protected virtual void Start() {
            mIsStarted = true;

            //for when putting entities on scene, skip the spawning state
            if(activateOnStart) {
                Spawn(mSpawnParams);
            }
        }

        protected virtual void StateChanged() {
        }
        
        //////////internal

        /// <summary>
        /// Spawn this entity, resets stats, set action to start, etc.
        /// </summary>
        protected virtual void OnSpawned(GenericParams parms) {
        }
        
        /// <summary>
        /// Call this if you want to manually "spawn" an entity on scene (make sure to set activateOnStart=false), will not do anything if Spawn has already been called
        /// </summary>
        public void Spawn(GenericParams parms) {
            if(mIsSpawned)
                return;

            // initialize data
            if(mPoolData == null)
                mPoolData = GetComponent<PoolDataController>();

            mState = mPrevState = StateInvalid; //avoid invalid updates
            
            mSpawnParams = parms;
            //
            
            //allow activator to start and check if we need to spawn now or later
            //ensure start is called before spawning if we are freshly allocated from entity manager
            if(activator != null) {
                activator.Start();

                //do it later when we wake up
                if(!activator.deactivateOnStart) {
                    ExecuteSpawn();
                }
            }
            else
                ExecuteSpawn();
        }

        void ExecuteSpawn() {
            mIsSpawned = true;

            OnSpawned(mSpawnParams);

            mSpawnParams = null;

            if(spawnCallback != null) {
                spawnCallback(this);
            }
        }

        void IPoolSpawn.OnSpawned(GenericParams parms) {
            Spawn(parms);
        }

        void IPoolDespawn.OnDespawned() {
            _OnDespawned();
        }

        void _OnDespawned() {
            OnDespawned();

            if(activator != null && activator.defaultParent == transform) {
                activator.Release(false);
            }

            if(releaseCallback != null) {
                releaseCallback(this);
            }

            mIsSpawned = false;
            mIsStarted = false;
            mSpawnParams = null;

            StopAllCoroutines();
        }
    }
}