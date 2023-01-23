using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceConstant : MonoBehaviour {
    public Vector2 force;
    
    private Rigidbody2D mBody;
    
    void Awake() {
        mBody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {
        mBody.AddForce(force);
    }
}
