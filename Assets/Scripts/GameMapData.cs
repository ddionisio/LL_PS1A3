using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapData : MonoBehaviour {
    public static Color editBoundsColor { get { return Color.cyan; } }
    
    [System.Serializable]
    public struct PaletteData {
        public string blockName;
        public int amount;
    }

    [SerializeField]
    Bounds _bounds;

    public int blockPoolStartCapacity;
    public int blockPoolMaxCapacity;

    [SerializeField]
    PaletteData[] _initialPalette;
    
    public PaletteData[] initialPalette { get { return _initialPalette; } }
    public Bounds bounds { get { return _bounds; } set { _bounds = value; } }
        
    public CellIndex GetCellIndex(Vector2 pos) {
        var cellSize = GameData.instance.blockSize;

        pos -= (Vector2)bounds.min;

        return new CellIndex(
            Mathf.FloorToInt(pos.y / cellSize.y),
            Mathf.FloorToInt(pos.x / cellSize.x));
    }

    public Vector2 GetPositionFromCell(CellIndex cell) {
        var cellSize = GameData.instance.blockSize;

        Vector2 pos = new Vector2(cell.col * cellSize.x, cell.row * cellSize.y);

        pos += (Vector2)bounds.min;

        return pos;
    }

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
