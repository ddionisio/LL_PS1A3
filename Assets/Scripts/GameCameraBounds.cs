using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCameraBounds : M8.SingletonBehaviour<GameCameraBounds> {
    public static Color editBoundsColor { get { return Color.cyan; } }

    [SerializeField]
    Bounds _bounds;
    
    public Bounds bounds { get { return _bounds; } set { _bounds = value; } }

    public Vector2 Clamp(Vector2 center, Vector2 ext) {
        Vector2 min = (Vector2)_bounds.min + ext;
        Vector2 max = (Vector2)_bounds.max - ext;

        center.x = Mathf.Clamp(center.x, min.x, max.x);
        center.y = Mathf.Clamp(center.y, min.y, max.y);

        return center;
    }

    void OnDrawGizmos() {
        Gizmos.color = editBoundsColor;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
