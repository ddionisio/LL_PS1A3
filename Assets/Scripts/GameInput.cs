using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [Header("Input")]
    public float dragScale = 0.3f;
    
    private BoxCollider2D mBoxColl;

    private Vector2 mDragLastPos;
    
    void OnDestroy() {
        
    }

    void Awake() {
        mBoxColl = GetComponent<BoxCollider2D>();
        
    }
        
    void Start () {
        //setup box collider size to be fullscreen        
        mBoxColl.offset = Vector2.zero;
        mBoxColl.size = (Vector2)GameCamera.instance.cameraViewBounds.size + new Vector2(1f, 1f);
    }
    
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        var gameCam = GameCamera.instance;
        mDragLastPos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        var gameCam = GameCamera.instance;
        Vector2 curPos = gameCam.camera2D.unityCamera.ScreenToWorldPoint(eventData.position);

        Vector2 delta = (curPos - mDragLastPos) * dragScale;

        mDragLastPos = curPos;

        gameCam.SetPosition(gameCam.position - delta);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        
    }
}
