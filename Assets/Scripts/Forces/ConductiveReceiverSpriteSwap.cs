using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConductiveReceiverSpriteSwap : ConductiveReceiver {
    [System.Serializable]
    public struct Data {
        public SpriteRenderer spriteRender;
        public Sprite spriteOn;
        public Sprite spriteOff;

        public void Apply(bool isOn) {
            spriteRender.sprite = isOn ? spriteOn : spriteOff;
        }
    }

    public Data[] targets;

    protected override void OnEnable() {
        base.OnEnable();

        for(int i = 0; i < targets.Length; i++)
            targets[i].Apply(false);
    }

    protected override void OnPowerChanged() {
        for(int i = 0; i < targets.Length; i++)
            targets[i].Apply(isPowered);
    }
}
