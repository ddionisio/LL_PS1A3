using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class EntityHeroInput : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public EntityHero hero;
    
    private EntityHero.MoveState mWaitForLandingToMoveState;
    private Coroutine mWaitForLandingRout;
    
    void OnDestroy() {
        if(hero) {
            hero.spawnCallback -= OnHeroSpawn;
            hero.releaseCallback -= OnHeroRelease;
        }
    }

    void Awake() {
        hero.spawnCallback += OnHeroSpawn;
        hero.releaseCallback += OnHeroRelease;
    }

    void OnDisable() {
        StopWaitForLanding();
    }

    void OnHeroSpawn(M8.EntityBase ent) {
        //reset states
    }

    void OnHeroRelease(M8.EntityBase ent) {

    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        SetMove(EntityHero.MoveState.Stop);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        var gameCam = GameCamera.instance;

        Vector2 curDragPos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);

        //check delta based on hero's current position, apply position if drag is outside hero's radius
        float delta = curDragPos.x - hero.transform.position.x;
        float dragLen = Mathf.Abs(delta);

        if(dragLen >= hero.moveCtrl.radius) {
            if(delta < 0f) //go left
                SetMove(EntityHero.MoveState.Left);
            else
                SetMove(EntityHero.MoveState.Right);
        }
        else //just apply pointer click
            ApplyClick();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        ApplyClick();
    }

    void ApplyClick() {
        switch(hero.moveState) {
            case EntityHero.MoveState.Stop:
                //resume last movement
                SetMove(hero.moveStatePrev);
                break;

            default:
                //stop movement
                SetMove(EntityHero.MoveState.Stop);
                break;
        }
    }

    private void SetMove(EntityHero.MoveState toState) {
        if(hero.moveCtrl.isGrounded) { //move right away
            StopWaitForLanding();
            
            hero.moveState = toState;
        }
        else { //wait until we landed
            mWaitForLandingToMoveState = toState;

            if(mWaitForLandingRout == null)
                mWaitForLandingRout = StartCoroutine(DoWaitForLanding());
        }
    }

    IEnumerator DoWaitForLanding() {
        while(!hero.moveCtrl.isGrounded)
            yield return null;
        
        hero.moveState = mWaitForLandingToMoveState;

        mWaitForLandingRout = null;
    }

    private void StopWaitForLanding() {
        if(mWaitForLandingRout != null) {
            StopCoroutine(mWaitForLandingRout);
            mWaitForLandingRout = null;
        }
    }
}
