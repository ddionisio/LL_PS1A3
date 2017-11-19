using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapData : MonoBehaviour {
    public static Color editBoundsColor { get { return Color.cyan; } }
    
    [System.Serializable]
    public struct PaletteData {
        public string blockName;
        public int amount;
        public int capacityAdd; //pool will be generated with amount + capacityAdd
    }

    [SerializeField]
    Bounds _bounds;

    [SerializeField]
    Vector2 _boundsStep = Vector2.one; //for scene editor

    [SerializeField]
    PaletteData[] _initialPalette;

    [SerializeField]
    int _paletteDefaultPoolCapacity = 8;
    
    public PaletteData[] initialPalette { get { return _initialPalette; } }
    public int paletteDefaultPoolCapacity { get { return _paletteDefaultPoolCapacity; } }
    public Bounds bounds { get { return _bounds; } set { _bounds = value; } }
    public Vector2 boundsStep { get { return _boundsStep; } }
    
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
