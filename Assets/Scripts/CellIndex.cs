using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CellIndex {
    public int row;
    public int col;

    public int area { get { return row * col; } }

    public CellIndex(int row, int col) {
        this.row = row;
        this.col = col;
    }

    public override int GetHashCode() {
        return row.GetHashCode() ^ col.GetHashCode();
    }

    public override bool Equals(object obj) {
        if(obj == null || GetType() != obj.GetType())
            return false;

        CellIndex other = (CellIndex)obj;

        return row == other.row && col == other.col;
    }

    public static bool operator ==(CellIndex x, CellIndex y) {
        return x.row == y.row && x.col == y.col;
    }
    public static bool operator !=(CellIndex x, CellIndex y) {
        return !(x == y);
    }
}
