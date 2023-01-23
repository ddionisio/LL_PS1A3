using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHeroAnimation : MonoBehaviour {
    public EntityHero heroEntity;
    public SpriteRenderer sprite;
    public M8.Animator.AnimatorData anim;
    public float velYThreshold = 0.5f;

    [Header("Takes")]
    public string takeIdle = "idle";
    public string takeMove = "move";
    public string takeUp = "up";
    public string takeDown = "down";

    private int mTakeIdleInd;
    private int mTakeMoveInd;
    private int mTakeUpInd;
    private int mTakeDownInd;

    private EntityState mState;
    private bool mIsGrounded;
    private EntityHero.MoveState mMoveState;
    private float mVelY;

    void OnDestroy() {
        if(heroEntity) {
            heroEntity.spawnStartCallback -= OnEntityHeroSpawnStart;
            heroEntity.setStateCallback -= OnEntityChangeState;
        }
    }

    void Awake() {
        heroEntity.spawnStartCallback += OnEntityHeroSpawnStart;
        heroEntity.setStateCallback += OnEntityChangeState;

        mTakeIdleInd = anim.GetTakeIndex(takeIdle);
        mTakeMoveInd = anim.GetTakeIndex(takeMove);
        mTakeUpInd = anim.GetTakeIndex(takeUp);
        mTakeDownInd = anim.GetTakeIndex(takeDown);
    }

    void Update() {
        if(GameMapController.instance.mode == GameMapController.Mode.Edit)
            return;

        switch(mState) {
            case EntityState.Normal:
            case EntityState.Victory:
                //check if we need to update animation
                bool isGrounded = heroEntity.moveCtrl.isGrounded;
                float velY = isGrounded ? mVelY : heroEntity.moveCtrl.localVelocity.y;
                EntityHero.MoveState moveState = heroEntity.moveState;

                if(mIsGrounded != isGrounded || mVelY != velY || mMoveState != moveState) {
                    mIsGrounded = isGrounded;
                    mVelY = velY;
                    mMoveState = moveState;

                    UpdateAnim();
                }
                break;
        }
    }

    void OnEntityChangeState(M8.EntityBase ent) {
        mState = (EntityState)ent.state;
        
        switch(mState) {
            case EntityState.Spawn:
                sprite.gameObject.SetActive(false);
                break;

            case EntityState.Normal:
            case EntityState.Victory:
                if((EntityState)ent.prevState == EntityState.Spawn)
                    mIsGrounded = true;
                else
                    mIsGrounded = heroEntity.moveCtrl.isGrounded;

                mMoveState = heroEntity.moveState;
                mVelY = Mathf.Sign(heroEntity.moveCtrl.localVelocity.y)*velYThreshold;

                UpdateAnim();
                break;
        }
    }

    void OnEntityHeroSpawnStart() {
        sprite.gameObject.SetActive(true);
    }

    private void UpdateAnim() {
        switch(mMoveState) {
            case EntityHero.MoveState.Left:
                sprite.flipX = true;
                break;
            case EntityHero.MoveState.Right:
                sprite.flipX = false;
                break;
        }

        if(mIsGrounded) {
            switch(mMoveState) {
                case EntityHero.MoveState.Stop:
                    anim.Play(mTakeIdleInd);
                    break;
                case EntityHero.MoveState.Left:
                case EntityHero.MoveState.Right:
                    anim.Play(mTakeMoveInd);
                    break;
            }
        }
        else {
            if(mVelY > velYThreshold)
                anim.Play(mTakeUpInd);
            else if(mVelY < -velYThreshold)
                anim.Play(mTakeDownInd);
            else if(anim.currentPlayingTakeIndex != mTakeUpInd || anim.currentPlayingTakeIndex != mTakeDownInd) {
                if(mVelY > 0f)
                    anim.Play(mTakeUpInd);
                else
                    anim.Play(mTakeDownInd);
            }
        }
    }
}
