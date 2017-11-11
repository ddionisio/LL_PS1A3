using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CellIndex {
    public int row;
    public int col;

    public CellIndex(int row, int col) {
        this.row = row;
        this.col = col;
    }
}
