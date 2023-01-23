using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class EntityHeroInput : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public EntityHero hero;

    public SpriteRenderer heroSprite;

    public GameObject stopGO;
    public GameObject leftGO;
    public GameObject rightGO;

    private bool mIsStopOnLanding;
    private bool mIsDragging;
    
    void OnDestroy() {
        if(hero) {
            hero.spawnCallback -= OnHeroSpawn;
            hero.releaseCallback -= OnHeroRelease;
            hero.setStateCallback -= OnHeroChangeState;
        }

        if(GameMapController.instance)
            GameMapController.instance.modeChangeCallback -= OnGameChangeMode;
    }

    void Awake() {
        hero.spawnCallback += OnHeroSpawn;
        hero.releaseCallback += OnHeroRelease;
        hero.setStateCallback += OnHeroChangeState;

        GameMapController.instance.modeChangeCallback += OnGameChangeMode;

        HideInterfaceDisplays();
    }

    void OnDisable() {
        mIsStopOnLanding = false;
        mIsDragging = false;
        HideInterfaceDisplays();
    }

    void Update() {
        if(mIsStopOnLanding) {
            if(hero.moveCtrl.isGrounded) {
                hero.moveState = EntityHero.MoveState.Stop;
                mIsStopOnLanding = false;

                HideInterfaceDisplays();
            }
        }
    }

    void OnHeroSpawn(M8.EntityBase ent) {
        //reset states
    }

    void OnHeroRelease(M8.EntityBase ent) {

    }

    void OnHeroChangeState(M8.EntityBase ent) {
        if((EntityState)ent.state != EntityState.Normal) {
            mIsDragging = false;
            mIsStopOnLanding = false;
            HideInterfaceDisplays();
        }
    }

    void OnApplicationFocus(bool focus) {
        if(!focus) {
            mIsDragging = false;
            HideInterfaceDisplays();
        }
    }

    void OnGameChangeMode(GameMapController.Mode mode) {
        switch(mode) {
            case GameMapController.Mode.Edit:
                HideInterfaceDisplays();
                break;
        }
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        //only stop if we are on ground
        if(hero.moveCtrl.isGrounded)
            hero.moveState = EntityHero.MoveState.Stop;
        else
            mIsStopOnLanding = true;

        ShowInterface(EntityHero.MoveState.Stop);

        mIsDragging = true;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        //stop if we are now grounded
        if(hero.moveCtrl.isGrounded) {
            hero.moveState = EntityHero.MoveState.Stop;

            //determine interface display
            var gameCam = GameCamera.instance;

            Vector2 curDragPos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);

            //check delta based on hero's current position, apply position if drag is outside hero's radius
            float delta = curDragPos.x - hero.transform.position.x;
            float dragLen = Mathf.Abs(delta);

            if(dragLen >= hero.moveCtrl.radius) {
                if(delta > 0f) {
                    ShowInterface(EntityHero.MoveState.Right);
                    
                    heroSprite.flipX = false;
                }
                else {
                    ShowInterface(EntityHero.MoveState.Left);
                    
                    heroSprite.flipX = true;
                }
            }
            else
                ShowInterface(EntityHero.MoveState.Stop);
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(!hero.moveCtrl.isGrounded || !mIsDragging)
            return;

        var gameCam = GameCamera.instance;

        Vector2 curDragPos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);

        //check delta based on hero's current position, apply position if drag is outside hero's radius
        float delta = curDragPos.x - hero.transform.position.x;
        float dragLen = Mathf.Abs(delta);

        if(dragLen >= hero.moveCtrl.radius) {
            if(delta < 0f) //go left
                hero.moveState = EntityHero.MoveState.Left;
            else
                hero.moveState = EntityHero.MoveState.Right;
        }
        //just apply pointer click
        else if(hero.moveState != EntityHero.MoveState.Stop)
            hero.moveState = EntityHero.MoveState.Stop;
        else
            hero.moveState = hero.moveStatePrev;

        HideInterfaceDisplays();

        mIsDragging = false;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(hero.moveState != EntityHero.MoveState.Stop) {
            if(hero.moveCtrl.isGrounded)
                hero.moveState = EntityHero.MoveState.Stop;
            else {
                mIsStopOnLanding = true;

                ShowInterface(EntityHero.MoveState.Stop);
            }
        }
        else
            hero.moveState = hero.moveStatePrev;

        //move the opposite
        /*switch(hero.moveState) {
            case EntityHero.MoveState.Left:
                hero.moveState = EntityHero.MoveState.Right;
                break;
            case EntityHero.MoveState.Right:
                hero.moveState = EntityHero.MoveState.Left;
                break;
        }*/
    }

    private void HideInterfaceDisplays() {
        stopGO.SetActive(false);
        rightGO.SetActive(false);
        leftGO.SetActive(false);
    }

    private void ShowInterface(EntityHero.MoveState state) {
        switch(state) {
            case EntityHero.MoveState.Stop:
                stopGO.SetActive(true);
                rightGO.SetActive(false);
                leftGO.SetActive(false);
                break;
            case EntityHero.MoveState.Left:
                stopGO.SetActive(false);
                rightGO.SetActive(false);
                leftGO.SetActive(true);
                break;
            case EntityHero.MoveState.Right:
                stopGO.SetActive(false);
                rightGO.SetActive(true);
                leftGO.SetActive(false);
                break;
        }
    }
}
