using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLine : ClearableCandy
{
    public bool isRow;

    public override void Clear()
    {
        base.Clear();
        {
            base.Clear();

            if (isRow) // row �����
            {
                piece.GridRef.ClearRow(piece.Y);
            }
            else  // col �����
            {
                piece.GridRef.ClearCol(piece.X);
            }
        }
    }
}
