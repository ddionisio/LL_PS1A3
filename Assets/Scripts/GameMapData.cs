using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapData : M8.SingletonBehaviour<GameMapData> {
    public static Color editBoundsColor { get { return Color.cyan; } }

    public class PaletteData {
        public string blockName;
        public int amount;
    }

    [SerializeField]
    Bounds _bounds;

    [SerializeField]
    PaletteData[] _initialPallete;
    
    public Bounds bounds { get { return _bounds; } set { _bounds = value; } }

    public event System.Action<string, int, int> paletteUpdateCallback; //block name, new amount, delta

    private Dictionary<string, int> mBlockPalette = new Dictionary<string, int>();

    public CellIndex GetCellIndex(Vector2 pos) {
        var cellSize = GameData.instance.blockSize;

        pos -= (Vector2)bounds.min;

        return new CellIndex(
            Mathf.RoundToInt(pos.y / cellSize.y),
            Mathf.RoundToInt(pos.x / cellSize.x));
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

    public int PaletteCount(string blockName) {
        int count;
        mBlockPalette.TryGetValue(blockName, out count);
        return count;
    }
    
    public void PaletteChange(string blockName, int delta) {
        int amount;

        if(mBlockPalette.ContainsKey(blockName))
            amount = mBlockPalette[blockName];
        else {
            mBlockPalette.Add(blockName, 0);
            amount = 0;
        }

        amount = Mathf.Clamp(amount + delta, 0, GameData.paletteMaxCount);

        mBlockPalette[blockName] = amount;

        if(paletteUpdateCallback != null)
            paletteUpdateCallback(blockName, amount, delta);
    }

    protected override void OnInstanceInit() {
        //setup current palette
        for(int i = 0; i < _initialPallete.Length; i++) {
            if(_initialPallete[i].amount > 0)
                mBlockPalette.Add(_initialPallete[i].blockName, _initialPallete[i].amount);
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = editBoundsColor;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
