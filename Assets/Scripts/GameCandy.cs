using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCandy : MonoBehaviour
{
    private int x;
    public int X 
    {
        get { return x; }
        set 
        {
            if (IsMovable())
            {
                x = value;
            }
        }
    }

    private int y;
    public int Y 
    { 
        get { return y; }
        set
        {
            if (IsMovable())
            {
                y = value;
            }
        }
    }

    private CandyBoard grid;
    public CandyBoard GridRef { get { return grid; } }

    private CandyBoard.CandyType type;
    public CandyBoard.CandyType Type 
    {
        get { return type; }
        set { type = value; }
    }

    private MovableCandy movableComponent;
    public MovableCandy MovableComponent
    {
        get { return movableComponent; }
    }

    private ColorCandys colorComponent;
    public ColorCandys ColorComponent
    {
        get { return colorComponent; }
    }

    private ClearableCandy clearableComponent;
    public ClearableCandy ClearableComponent
    {
        get { return clearableComponent; }
    }

    private void Awake()
    {
        movableComponent = GetComponent<MovableCandy>();
        colorComponent = GetComponent<ColorCandys>();
        clearableComponent = GetComponent<ClearableCandy>();
    }

    public void Init(int _x, int _y, CandyBoard _grid, CandyBoard.CandyType _type)
    {
        x = _x;
        y= _y;
        grid = _grid;
        type = _type;
    }

    private void OnMouseEnter()
    {
        grid.EnterPiece(this);
    }

    private void OnMouseDown()
    {
        grid.PressPiece(this);
    }

    private void OnMouseUp()
    {
        grid.CandyRelease();
    }

    public bool IsMovable()
    {
        return movableComponent != null;
    }

    public bool IsColored()
    {
        return colorComponent != null;
    }

    public bool IsClearable()
    {
        return clearableComponent != null;
    }
}
