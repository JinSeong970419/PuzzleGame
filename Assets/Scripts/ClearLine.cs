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

            if (isRow) // row 지우기
            {
                piece.GridRef.ClearRow(piece.Y);
            }
            else  // col 지우기
            {
                piece.GridRef.ClearCol(piece.X);
            }
        }
    }
}
