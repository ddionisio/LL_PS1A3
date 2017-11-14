using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityTriggerKill : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D collision) {
        DoKill(collision.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        DoKill(collision.gameObject);
    }

    void DoKill(GameObject go) {
        if(!go)
            return;

        var ent = go.GetComponent<M8.EntityBase>();
        if(!ent)
            return;

        ent.state = (int)EntityState.Dead;
    }
}
