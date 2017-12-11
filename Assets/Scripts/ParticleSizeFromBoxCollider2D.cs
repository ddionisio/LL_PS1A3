using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only functions in scene editor
/// </summary>
[ExecuteInEditMode]
public class ParticleSizeFromBoxCollider2D : MonoBehaviour {
    public BoxCollider2D toBoxCollider;
    public bool updateOnEnable;

    private ParticleSystem mParticle;

    void OnEnable() {
        if(updateOnEnable)
            UpdateShape();
    }

#if UNITY_EDITOR
    void Update () {
        UpdateShape();
    }
#endif

    void UpdateShape() {
        if(!toBoxCollider) return;
        if(!mParticle) mParticle = GetComponent<ParticleSystem>();
        if(!mParticle) return;

        Bounds bounds = toBoxCollider.bounds;

        var shape = mParticle.shape;

        shape.shapeType = ParticleSystemShapeType.Box;
        shape.position = transform.worldToLocalMatrix.MultiplyPoint3x4(bounds.center);
        shape.scale = bounds.size;
    }
}
