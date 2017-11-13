using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenModal : MonoBehaviour {

    public void Open(string modal) {
        M8.UIModal.Manager.instance.ModalOpen(modal);
    }
}
