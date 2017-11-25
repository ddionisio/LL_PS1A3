using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UIEventEnterExitTweenColor : TweenerBase, IPointerEnterHandler, IPointerExitHandler {
    public Graphic target;
    public Color exitColor = Color.clear;
    public Color enterColor = Color.white;

    protected override void Apply(float t) {
        target.color = Color.LerpUnclamped(exitColor, enterColor, t);
    }
    
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        Enter(true);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        Enter(false);
    }
}
