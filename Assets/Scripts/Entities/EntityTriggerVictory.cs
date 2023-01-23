using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityTriggerVictory : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D collision) {
        GameMapController.instance.Victory();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        GameMapController.instance.Victory();
    }
}
