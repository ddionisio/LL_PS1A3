using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for widget type blocks
/// </summary>
public abstract class BlockWidget : Block {
    public override Type type { get { return Type.Widget; } }

    public override int matterCount { get { return 1; } }

    public override CellIndex cellSize { get { return new CellIndex(1, 1); } }
    
    public override Transform editAttach { get { return transform; } }
    
    public override Bounds editBounds {
        get {
            return new Bounds(transform.position, GameData.instance.blockSize);
        }
    }
    
    public override bool EditIsExpandable() { return false; }
    
    public override void EditSetPosition(Vector2 pos) {
        transform.position = pos;
    }

    public override void EditMove(Vector2 delta) {
        var pos = (Vector2)transform.position;
        pos += delta;
        transform.position = pos;
    }

    public override void EditExpand(int top, int bottom, int left, int right) {

    }
}
