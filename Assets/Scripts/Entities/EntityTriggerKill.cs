using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityTriggerKill : MonoBehaviour {
    public string tagFilter;

    void OnTriggerEnter2D(Collider2D collision) {
        DoKill(collision.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        DoKill(collision.gameObject);
    }

    void DoKill(GameObject go) {
        if(!go)
            return;

        if(!string.IsNullOrEmpty(tagFilter) && !go.CompareTag(tagFilter))
            return;

        var ent = go.GetComponent<M8.EntityBase>();
        if(!ent)
            return;

        ent.state = (int)EntityState.Dead;
    }
}
