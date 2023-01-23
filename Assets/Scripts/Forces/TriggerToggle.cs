using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerToggle : MonoBehaviour {
    public bool stayToggled;

    public bool isToggled {
        get { return mIsToggled; }

        private set {
            if(mIsToggled != value) {
                mIsToggled = value;
                ToggleChanged();
            }
        }
    }

    private bool mIsToggled;

    protected abstract void ToggleChanged();

    void OnTriggerEnter2D(Collider2D collision) {
        isToggled = true;
    }

    void OnTriggerExit2D(Collider2D collision) {
        if(!stayToggled)
            isToggled = false;
    }
}
