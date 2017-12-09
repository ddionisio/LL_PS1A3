using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockWidgetBalloon : BlockWidget {
    public const int raycastHitCapacity = 8;

    [Header("Balloon Widget")]
    [Range(0f, 1f)]
    public float ghostAlpha;

    public Color invalidColor = Color.red;

    [Header("Balloon Physics")]
    public float force;
    public float ropeLength;
    public float forceStartDelay = 0.1f;

    public float ropeCheckRadius;
    public LayerMask ropeCheckLayerMask;

    [Header("Balloon Parts")]
    public SpriteRenderer balloonSprite;
    public SpriteRenderer ropeSprite;
    public SpriteRenderer widgetSprite;

    public float ropeLengthOfs;
        
    public Transform widgetRoot;

    public override Rigidbody2D mainBody { get { return mBody; } }

    public override Bounds editBounds {
        get {
            return new Bounds(mEditPos, GameData.instance.blockSize);
        }
    }
    
    private CircleCollider2D mColl;
    private SpringJoint2D mJoint;
    private Rigidbody2D mBody;
    private M8.EntityBase mEntAttach;
    private float mRopePixelRatio;

    private bool mEditIsValid;
    private Vector2 mEditPos;

    private Color mBalloonSpriteDefaultColor;
    private Color mRopeSpriteDefaultColor;
    private Color mWidgetDefaultColor;

    private bool mIsForceActive;
    private float mLastTime;

    private RaycastHit2D[] mRaycastHits = new RaycastHit2D[raycastHitCapacity];
    
    public override void EditSetPosition(Vector2 pos) {
        if(mEditPos != pos) {
            mEditPos = pos;
            UpdatePositionFromEditPos();
            DimensionChanged();
        }
    }

    public override void EditMove(Vector2 delta) {
        var lastEditPos = mEditPos;
        mEditPos += delta;
        
        if(mEditPos != lastEditPos) {
            UpdatePositionFromEditPos();
            DimensionChanged();
        }
    }

    public override bool EditIsPlacementValid() {
        //check balloon's collision
        //check line to make sure it isn't occluded towards balloon
        return mEditIsValid;
    }

    public override void EditEnableCollision(bool aEnable) {
        mBody.simulated = aEnable;
    }

    protected override void ApplyMode(Mode prevMode) {
        switch(mode) {
            case Mode.Ghost:
                mBalloonSpriteDefaultColor = balloonSprite.color;
                mRopeSpriteDefaultColor = ropeSprite.color;
                mWidgetDefaultColor = widgetSprite.color;

                widgetRoot.gameObject.SetActive(true);

                mBody.simulated = false;

                mJoint.enabled = false;

                UpdatePositionFromEditPos();
                break;

            case Mode.Solid:
                widgetRoot.gameObject.SetActive(false);

                balloonSprite.color = mBalloonSpriteDefaultColor;
                ropeSprite.color = mRopeSpriteDefaultColor;
                widgetSprite.color = mWidgetDefaultColor;

                //setup physics
                //setup joint
                var rayPt = (Vector2)transform.position - new Vector2(0f, mColl.radius + 0.001f);

                int numHit = Physics2D.CircleCastNonAlloc(rayPt, ropeCheckRadius, Vector2.down, mRaycastHits, ropeLength, ropeCheckLayerMask);
                if(numHit > 0) {
                    var hit = mRaycastHits[numHit - 1];

                    var hitBody = hit.rigidbody;
                    var hitColl = hit.collider;

                    if(hitBody) {
                        //check if it's an entity
                        mEntAttach = hitBody.GetComponent<M8.EntityBase>();
                        if(mEntAttach)
                            mEntAttach.releaseCallback += OnAttachDespawn;

                        mJoint.connectedBody = hitBody;
                        mJoint.connectedAnchor = hitBody.transform.worldToLocalMatrix.MultiplyPoint3x4(hit.point);
                    }
                    else {
                        //just setup a fixed point
                        mJoint.connectedBody = null;
                        mJoint.connectedAnchor = hit.point;
                    }

                    mJoint.enabled = true;
                }
                else {
                    mJoint.enabled = false;
                }

                mBody.simulated = true;

                mIsForceActive = false;
                mLastTime = Time.fixedTime;
                break;
        }
    }

    protected override void OnDespawned() {
        mBody.velocity = Vector2.zero;
        mBody.angularVelocity = 0f;

        mIsForceActive = false;

        if(mEntAttach) {
            mEntAttach.releaseCallback -= OnAttachDespawn;
            mEntAttach = null;
        }
                
        mJoint.connectedBody = null;

        ropeSprite.transform.localRotation = Quaternion.identity;

        balloonSprite.color = mBalloonSpriteDefaultColor;
        ropeSprite.color = mRopeSpriteDefaultColor;
        widgetSprite.color = mWidgetDefaultColor;
    }

    protected override void OnSpawned(M8.GenericParams parms) {
        //default values
        mBalloonSpriteDefaultColor = balloonSprite.color;
        mRopeSpriteDefaultColor = ropeSprite.color;
        mWidgetDefaultColor = widgetSprite.color;

        mBody.velocity = Vector2.zero;
        mBody.angularVelocity = 0f;

        mEditPos = transform.position;
        
        base.OnSpawned(parms);
    }

    protected override void Awake() {
        base.Awake();

        mColl = GetComponent<CircleCollider2D>();
        mJoint = GetComponent<SpringJoint2D>();
        mBody = GetComponent<Rigidbody2D>();

        mBody.useAutoMass = true;
        mColl.density = density;

        mJoint.autoConfigureDistance = false;
        mJoint.distance = ropeLength;
        mJoint.anchor = new Vector2(0f, -mColl.radius);

        mRopePixelRatio = ropeSprite.sprite.textureRect.size.y / ropeSprite.sprite.pixelsPerUnit;
        if(mRopePixelRatio <= 0f)
            mRopePixelRatio = 1f;
    }
    
    void Update() {
        if(!isSpawned)
            return;

        /*switch(GameMapController.instance.mode) {
            case GameMapController.Mode.Edit:
                UpdateRopeDisplayStatic();
                break;

            case GameMapController.Mode.Play:
                if(mJoint.enabled)
                    UpdateRopeDisplayDynamic();
                else
                    UpdateRopeDisplayStatic();
                break;
        }*/
        switch(mode) {
            case Mode.Solid:
                if(mJoint.enabled)
                    UpdateRopeDisplayDynamic();
                else
                    UpdateRopeDisplayStatic();
                break;
            case Mode.Ghost:
                UpdateRopeDisplayStatic();
                break;
        }
    }

    void UpdateRopeDisplayStatic() {
        float len = ropeLength + mColl.radius + ropeLengthOfs;

        if(ropeSprite.drawMode == SpriteDrawMode.Simple) {
            var newRopeSpriteSize = new Vector2(ropeSprite.transform.localScale.x, len / mRopePixelRatio);
            ropeSprite.transform.localScale = newRopeSpriteSize;
        }
        else {
            var newRopeSpriteSize = new Vector2(ropeSprite.size.x, len);
            ropeSprite.size = newRopeSpriteSize;
        }

        ropeSprite.transform.up = Vector2.up;
        ropeSprite.transform.position = (Vector2)transform.position - new Vector2(0f, len * 0.5f);
    }

    void UpdateRopeDisplayDynamic() {
        Vector2 jointWorldPos;
        if(mJoint.connectedBody != null)
            jointWorldPos = mJoint.connectedBody.transform.localToWorldMatrix.MultiplyPoint3x4(mJoint.connectedAnchor);
        else
            jointWorldPos = mJoint.connectedAnchor;

        Vector2 dirToBalloon = ((Vector2)transform.position - jointWorldPos);
        float dist = dirToBalloon.magnitude;
        if(dist > 0f) {
            ropeSprite.gameObject.SetActive(true);

            dirToBalloon /= dist;

            ropeSprite.transform.up = dirToBalloon;

            dist += ropeLengthOfs;
            
            if(ropeSprite.drawMode == SpriteDrawMode.Simple) {
                var newRopeSpriteSize = new Vector2(ropeSprite.transform.localScale.x, dist / mRopePixelRatio);
                ropeSprite.transform.localScale = newRopeSpriteSize;
            }
            else {
                var newRopeSpriteSize = new Vector2(ropeSprite.size.x, dist);
                ropeSprite.size = newRopeSpriteSize;
            }

            ropeSprite.transform.position = jointWorldPos + dirToBalloon * (dist * 0.5f);
        }
        else {
            ropeSprite.gameObject.SetActive(false);
        }
    }

    void FixedUpdate() {
        if(mode != Mode.Solid)
            return;

        if(mIsForceActive) {
            //add force
            mBody.AddForce(new Vector2(0f, force));
        }
        else {
            if(Time.fixedTime - mLastTime >= forceStartDelay)
                mIsForceActive = true;
        }
    }

    void OnDrawGizmos() {
        if(ropeCheckRadius > 0f) {            
            var t = widgetRoot ? widgetRoot : transform;

            Gizmos.color = Color.blue * 0.5f;
            Gizmos.DrawSphere(t.position, ropeCheckRadius);
        }
    }

    void OnAttachDespawn(M8.EntityBase ent) {
        mEntAttach.releaseCallback -= OnAttachDespawn;
        mEntAttach = null;

        mJoint.enabled = false;
        mJoint.connectedBody = null;
    }
    
    private void UpdatePositionFromEditPos() {        
        transform.position = mEditPos + new Vector2(0f, ropeLength + mColl.radius);

        widgetRoot.localPosition = widgetRoot.parent.worldToLocalMatrix.MultiplyPoint3x4(mEditPos);

        //check to make sure balloon has room
        if(IsCountValid()) {
            var collider = Physics2D.OverlapCircle(transform.position, mColl.radius, GameData.instance.blockInvalidMask);

            mEditIsValid = collider == null;
        }
        else
            mEditIsValid = false;

        //update color
        if(mEditIsValid) {
            balloonSprite.color = new Color(mBalloonSpriteDefaultColor.r, mBalloonSpriteDefaultColor.g, mBalloonSpriteDefaultColor.b, ghostAlpha);
            ropeSprite.color = new Color(mRopeSpriteDefaultColor.r, mRopeSpriteDefaultColor.g, mRopeSpriteDefaultColor.b, ghostAlpha);
            widgetSprite.color = new Color(mWidgetDefaultColor.r, mWidgetDefaultColor.g, mWidgetDefaultColor.b, ghostAlpha);
        }
        else {
            balloonSprite.color = new Color(invalidColor.r, invalidColor.g, invalidColor.b, ghostAlpha);
            ropeSprite.color = new Color(invalidColor.r, invalidColor.g, invalidColor.b, ghostAlpha);
            widgetSprite.color = new Color(invalidColor.r, invalidColor.g, invalidColor.b, ghostAlpha);
        }
    }
}