using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class EntityHeroInput : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public EntityHero hero;
    
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
        
    }

    void OnHeroSpawn(M8.EntityBase ent) {
        //reset states
    }

    void OnHeroRelease(M8.EntityBase ent) {

    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        //only stop if we are on ground
        if(hero.moveCtrl.isGrounded)
            hero.moveState = EntityHero.MoveState.Stop;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        //stop if we are now grounded
        if(hero.moveCtrl.isGrounded)
            hero.moveState = EntityHero.MoveState.Stop;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
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
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(hero.moveState != EntityHero.MoveState.Stop)
            hero.moveState = EntityHero.MoveState.Stop;
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
}
