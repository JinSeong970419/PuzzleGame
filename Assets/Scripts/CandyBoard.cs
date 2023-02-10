using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyBoard : MonoBehaviour
{
    public enum CandyType
    {
        EMPTY,
        NORMAL,
        HOLE,
        ROWCLEAR,
        COLCLEAR,
        TWOBYTWO,
        COUNT,
    };

    [System.Serializable]
    public struct CandyPrefab
    {
        public CandyType type;
        public GameObject prefab;
    };

    [Range(1, 11)] public int xDim;
    [Range(1, 11)] public int yDim;
    public float fillTime;
    public int addScore;

    private bool fillLimit = false;
    private float space = 0.25f; // ��� �� ĵ�� ����
    private float height = 1f; // ��� �� ĵ�� ���� ����

    public CandyPrefab[] candyPrefabs;
    public GameObject backgroundPrefab;

    private Dictionary<CandyType, GameObject> candyPrefabDict;

    private GameCandy[,] candys;
    private GameCandy pressed;
    private GameCandy entered;

    public UI ui;

    void Start()
    {
        candyPrefabDict = new Dictionary<CandyType, GameObject>();

        ui.movableText.text = GameManager.Instance.movable.ToString();
        ui.targetText.text = GameManager.Instance.target.ToString();
        ui.scoreText.text = GameManager.Instance.score.ToString();

        for (int i = 0; i < candyPrefabs.Length; i++)
        {
            if (!candyPrefabDict.ContainsKey(candyPrefabs[i].type))
            {
                candyPrefabDict.Add(candyPrefabs[i].type, candyPrefabs[i].prefab);
            }
        }

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                // ������ �ٴ�
                if ((x == 0 || x == xDim - 1) && (y == 0 || y == yDim - 1))
                {
                    continue;
                }
                GameObject background = (GameObject)Instantiate(backgroundPrefab, GetWorldPosition(x, y), Quaternion.identity);
                background.transform.parent = transform;
            }
        }

        candys = new GameCandy[xDim, yDim];
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                NewCandySpawn(x, y, CandyType.EMPTY);
            }
        }

        // ���� ����
        for (int i = 0; i < xDim; i++)
        {
            for (int j = 0; j < yDim; j++)
            {
                if((i==0 && j==0) || (i == 0 && j == yDim-1) || (i == xDim-1 && j == yDim-1)|| (i == xDim-1 && j == 0))
                {
                    continue;
                }

                if(i == 0 || i == xDim-1 || j == 0 || j == yDim - 1)
                {
                    Destroy(candys[i, j].gameObject);
                    NewCandySpawn(i, j, CandyType.HOLE);
                }
            }
        }

        StartCoroutine(Fill());
    }

    // �׸��� ä��� �Լ�
    public IEnumerator Fill()
    {
        bool Refillable = true;

        while (Refillable)
        {
            yield return new WaitForSeconds(fillTime);
            while (FillChecker())
            {
                fillLimit = true;
                yield return new WaitForSeconds(fillTime);
            }
            fillLimit = false;
            Refillable = ClearAllMatch();
        }
    }

    public bool FillChecker()
    {
        bool movedCandy = false;

        // ���� �Ʒ� ���� ������ �� ������ ������ ĭ�� ���� ĭ������ -2
        // �Ʒ� ������ ��� �ִ��� Ȯ�� �� �̵�
        for (int y = yDim - 2; y >= 0; y--)
        {
            for (int x = 0; x < xDim; x++)
            {
                GameCandy candy = candys[x, y];

                if (candy.IsMovable())
                {
                    GameCandy piece = candys[x, y + 1];
                    if (piece.Type == CandyType.EMPTY)
                    {
                        Destroy(piece.gameObject); // ��� �� �� ������Ʈ ����
                        candy.MovableComponent.Move(x, y + 1, fillTime);
                        candys[x, y + 1] = candy;
                        NewCandySpawn(x, y, CandyType.EMPTY);
                        movedCandy = true;
                    }
                }
            }
        }

        // ���� ���� ���� Ȯ�� �� ��� �ִٸ� ä���
        // Ư�� ĵ�� ���� �߰��ϱ�
        for (int x = 0; x < xDim; x++)
        {
            GameCandy piece = candys[x, 1];

            if (piece.Type == CandyType.EMPTY)
            {
                Destroy(piece.gameObject); // ��� �� �� ������Ʈ ����
                GameObject newPiece = (GameObject)Instantiate(candyPrefabDict[CandyType.NORMAL], GetWorldPosition(x, 0), Quaternion.identity);
                newPiece.transform.parent = transform;

                candys[x, 1] = newPiece.GetComponent<GameCandy>();
                candys[x, 1].Init(x, 0, this, CandyType.NORMAL);
                candys[x, 1].MovableComponent.Move(x, 1, fillTime);
                candys[x, 1].ColorComponent.SetColor((ColorCandys.ColorType)Random.Range(0, candys[x, 1].ColorComponent.NumColors));
                movedCandy = true;
            }
        }

        return movedCandy;
    }

    // ��ǥ ���� ��ġ ����
    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - xDim * 0.25f + space + x * 0.5f, transform.position.y + yDim * 0.25f - height - y * 0.5f);
    }

    // ���ο� ĵ�� ����
    public GameCandy NewCandySpawn(int x, int y, CandyType type)
    {
        GameObject newPiece = (GameObject)Instantiate(candyPrefabDict[type], GetWorldPosition(x, y), Quaternion.identity);
        newPiece.transform.parent = transform;

        candys[x, y] = newPiece.GetComponent<GameCandy>();
        candys[x, y].Init(x, y, this, type);

        return candys[x, y];
    }

    // ���� ���� Ȯ�� �Լ�
    public bool IsAdjacent(GameCandy candy1, GameCandy candy2)
    {
        return (candy1.X == candy2.X && (int)Mathf.Abs(candy1.Y - candy2.Y) == 1
            || (candy1.Y == candy2.Y && (int)Mathf.Abs(candy1.X - candy2.X) == 1));
    }

    // ĵ�� ��ġ�� �ٲٴ� �Լ�
    public void SwapCandy(GameCandy candy1, GameCandy candy2)
    {
        if (candy1.IsMovable() && candy2.IsMovable())
        {
            GameManager.Instance.movable--;
            ui.SetMovable(GameManager.Instance.movable);

            if(GameManager.Instance.movable == 0)
            {
                GameManager.Instance.GameClear();
            }

            candys[candy1.X, candy1.Y] = candy2;
            candys[candy2.X, candy2.Y] = candy1;

            if (GetMatch(candy1, candy2.X, candy2.Y) != null || GetMatch(candy2, candy1.X, candy1.Y) != null)
            {
                int candy1X = candy1.X;
                int candy1Y = candy1.Y;

                candy1.MovableComponent.Move(candy2.X, candy2.Y, fillTime);
                candy2.MovableComponent.Move(candy1X, candy1Y, fillTime);

                ClearAllMatch();

                pressed = null;
                entered = null;

                StartCoroutine(Fill());
            }
            else if (candy1.Type == CandyType.TWOBYTWO && !fillLimit)
            {
                if (candy1.Type == CandyType.TWOBYTWO && candy1.IsClearable())
                {
                    ClearTwobyTwo twoBytwo = candy1.GetComponent<ClearTwobyTwo>();
                    Vector2Int dir = new Vector2Int(candy2.X - candy1.X, candy2.Y - candy1.Y);

                    ClearCandy(candy1.X, candy1.Y);

                    StartCoroutine(RollTwoByTwo(candy1, dir, fillTime *0.5f));
                }

                StartCoroutine(Fill());
            }
            else  // ��Ī ���н� ���� ��ġ��
            {
                candys[candy1.X, candy1.Y] = candy1;
                candys[candy2.X, candy2.Y] = candy2;
            }

        }
    }

    public IEnumerator RollTwoByTwo(GameCandy candy, Vector2Int dir, float time)
    {
        int Delx = candy.X + dir.x;
        int Dely = candy.Y + dir.y;
        ClearCandy(Delx, Dely);
        while (true)
        {
            candy.MovableComponent.Move(candy.X + dir.x, candy.Y + dir.y, time);

            if(candys[candy.X + dir.x, candy.Y + dir.y].Type != CandyType.HOLE )
            {
                ClearCandy(candy.X + dir.x, candy.Y + dir.y);
            }
            else // ���� ��ġ�� ������ ���
            {
                candy.MovableComponent.Move(candy.X + dir.x, candy.Y + dir.y, time);

                GameManager.Instance.target--;
                ui.SetTarget(GameManager.Instance.target);
                if(GameManager.Instance.target == 0)
                {
                    GameManager.Instance.GameClear();
                }
                break;
            }
            yield return new WaitForSeconds(time);
        }
    }

    public void PressPiece(GameCandy candy)
    {
        pressed = candy;
    }

    public void EnterPiece(GameCandy candy)
    {
        entered = candy;
    }

    public void CandyRelease()
    {
        if (IsAdjacent(pressed, entered))
        {
            SwapCandy(pressed, entered);
        }
    }

    // ��Ī Ȯ��
    public List<GameCandy> GetMatch(GameCandy candy, int newX, int newY)
    {
        if (candy.IsColored())
        {
            List<GameCandy> horizontal = GetHorizonMatch(candy, newX, newY);
            List<GameCandy> vertical = GetverticalMatch(candy, newX, newY);
            List<GameCandy> square = GetsquareMatch(candy, newX, newY);
            List<GameCandy> matchingCandy = new List<GameCandy>();

            // �簢��
            if (square.Count >= 4)
            {
                for (int i = 0; i < square.Count; i++)
                {
                    square[i].Type = CandyType.TWOBYTWO;
                    square[i].Type = CandyType.TWOBYTWO;
                    matchingCandy.Add(square[i]);
                }
                return matchingCandy;
            }

            // ���θ� Ȯ��
            else if (horizontal.Count >= 3)
            {
                for (int i = 0; i < horizontal.Count; i++)
                {
                    matchingCandy.Add(horizontal[i]);
                }
                return horizontal;
            }

            // ����
            else if(vertical.Count >= 3)
            {
                for (int i = 0; i < vertical.Count; i++)
                {
                    matchingCandy.Add(vertical[i]);
                }
                return vertical;
            }
        }
        return null;
    }

    public List<GameCandy> GetHorizonMatch(GameCandy candy, int newX, int newY)
    {
        List<GameCandy> horizontalPieces = new List<GameCandy>();

        horizontalPieces.Add(candy);

        for (int dir = 0; dir <= 1; dir++)
        {
            for (int xOffset = 1; xOffset < xDim; xOffset++)
            {
                int x;

                if (dir == 0) // left
                {
                    x = newX - xOffset;
                }
                else // right
                {
                    x = newX + xOffset;
                }

                if (x < 0 || x >= xDim) { break; }

                if (candys[x, newY].IsColored() && candys[x, newY].ColorComponent.Color == candy.ColorComponent.Color)
                {
                    horizontalPieces.Add(candys[x, newY]);
                }
                else { break; }
            }
        }

        return horizontalPieces;
    }
    
    public List<GameCandy> GetverticalMatch(GameCandy candy, int newX, int newY)
    {
        List<GameCandy> verticalPieces = new List<GameCandy>();

        verticalPieces.Add(candy);

        for (int dir = 0; dir <= 1; dir++)
        {
            for (int yOffset = 1; yOffset < yDim; yOffset++)
            {
                int y;

                if (dir == 0) //up
                {
                    y = newY - yOffset;
                }
                else // down
                {
                    y = newY + yOffset;
                }

                if (y < 0 || y >= yDim) { break; }

                if (candys[newX, y].IsColored() && candys[newX, y].ColorComponent.Color == candy.ColorComponent.Color)
                {
                    verticalPieces.Add(candys[newX, y]);
                }
                else { break; }
            }
        }

        return verticalPieces;
    }

    public List<GameCandy> GetsquareMatch(GameCandy candy, int newX, int newY)
    {
        List<GameCandy> squareCandy = new List<GameCandy>();

        squareCandy.Add(candys[newX,newY]);
        if (newX - 1 > 0 && candys[newX - 1, newY].IsColored() && candys[newX - 1, newY].ColorComponent.Color == squareCandy[0].ColorComponent.Color) // ����
        {
            if (candys[newX - 1, newY-1].IsColored() && newX < yDim - 1 && candys[newX - 1, newY - 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color &&  // ��
                candys[newX, newY-1].IsColored() && newX < yDim - 1 && candys[newX, newY - 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color)
            {
                squareCandy.Add(candys[newX - 1, newY]);
                squareCandy.Add(candys[newX - 1, newY - 1]);
                squareCandy.Add(candys[newX, newY - 1]);
            }

            if (candys[newX - 1, newY + 1].IsColored() && newX < yDim + 1 && candys[newX - 1, newY + 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color &&  // �Ʒ�
                candys[newX, newY + 1].IsColored() && newX < yDim + 1 && candys[newX, newY + 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color)
            {
                squareCandy.Add(candys[newX - 1, newY]);
                squareCandy.Add(candys[newX - 1, newY + 1]);
                squareCandy.Add(candys[newX, newY + 1]);
            }
        }

        if (newX < xDim - 1 && candys[newX + 1, newY].IsColored() && candys[newX + 1, newY].ColorComponent.Color == squareCandy[0].ColorComponent.Color) // ������
        {

            if (candys[newX , newY - 1].IsColored() && newX < yDim + 1 && candys[newX , newY - 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color &&  // ��
                candys[newX + 1 , newY - 1].IsColored() && newX < yDim + 1 && candys[newX + 1 , newY - 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color)
            {
                squareCandy.Add(candys[newX + 1, newY]);
                squareCandy.Add(candys[newX + 1, newY - 1]);
                squareCandy.Add(candys[newX, newY - 1]);
            }

            if (candys[newX + 1, newY + 1].IsColored() && newX < yDim - 1 && candys[newX + 1, newY + 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color &&  // �Ʒ�
                candys[newX, newY + 1].IsColored() && newX < yDim - 1 && candys[newX, newY + 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color)
            {
                squareCandy.Add(candys[newX + 1, newY]);
                squareCandy.Add(candys[newX + 1, newY + 1]);
                squareCandy.Add(candys[newX, newY + 1]);
            }
        }

        return squareCandy;
    }

    public bool ClearAllMatch()
    {
        bool needsRefill = false;

        for (int y = 0; y < yDim; y++)
        {
            for(int x=0; x < xDim; x++)
            {
                if (candys[x, y].IsClearable())
                {
                    List<GameCandy> match = GetMatch(candys[x, y], x, y);

                    if(match != null)
                    {
                        CandyType specialCandyType = CandyType.COUNT;
                        GameCandy candyPosition = match[0];   // �÷��̾ ���� ��ġ�� ����
                        int specialPieceX = candyPosition.X;
                        int specialPieceY = candyPosition.Y;

                        if(match.Count == 4 && match[0].Type != CandyType.TWOBYTWO)
                        {
                            // ���� �� ������� ��� ó��
                            if (pressed == null || entered == null)
                            {
                                specialCandyType = (CandyType)Random.Range((int)CandyType.ROWCLEAR, (int)CandyType.COLCLEAR);
                            }
                            else if (pressed.Y == entered.Y)
                            {
                                specialCandyType = CandyType.ROWCLEAR;
                            }
                            else
                            {
                                specialCandyType = CandyType.COLCLEAR;
                            }
                        }
                        else if(match.Count == 4 && match[0].Type == CandyType.TWOBYTWO)
                        {
                            specialCandyType = CandyType.TWOBYTWO;
                        }

                        // ��Ī �ȿ� �ǽ� ĵ�� �����
                        for (int i = 0; i < match.Count; i++)
                        {
                            if(ClearCandy(match[i].X, match[i].Y))
                            {
                                needsRefill = true;
                                if(match[i] == pressed || match[i] == entered)
                                {
                                    specialPieceX = match[i].X;
                                    specialPieceY = match[i].Y;
                                }
                                GameManager.Instance.score += addScore;
                                ui.SetScore(GameManager.Instance.score);
                            }
                        }

                        // Ư�� ���� ���� ���� Ȯ��
                        if(specialCandyType != CandyType.COUNT)
                        {
                            Destroy(candys[specialPieceX, specialPieceY]);
                            GameCandy newPiece = NewCandySpawn(specialPieceX, specialPieceY, specialCandyType);

                            if ((specialCandyType == CandyType.ROWCLEAR || specialCandyType == CandyType.COLCLEAR) && newPiece.IsColored() && match[0].IsColored())
                            {
                                newPiece.ColorComponent.SetColor(match[0].ColorComponent.Color);
                            }
                            else if(specialCandyType == CandyType.TWOBYTWO && newPiece.IsColored() && match[0].IsColored())
                            {
                                newPiece.ColorComponent.SetColor(ColorCandys.ColorType.ANY);
                            }
                        }
                    }
                }
            }
        }
        return needsRefill;
    }

    // ĵ�� ���� �ڵ�
    public bool ClearCandy(int x, int y)
    {
        if(candys[x,y].IsClearable() && !candys[x, y].ClearableComponent.IsBeingCleared)
        {
            candys[x, y].ClearableComponent.Clear();
            NewCandySpawn(x, y, CandyType.EMPTY);

            return true;
        }
        return false;
    }

    // Row �����
    public void ClearRow(int row)
    {
        for (int i = 0; i < xDim-1; i++)
        {
            if (candys[i, row].Type == CandyType.TWOBYTWO)
            {
                continue;
            }
            else
            {
                ClearCandy(i, row);
            }
        }
    }

    // col �����
    public void ClearCol(int col)
    {
        for (int i = 0; i < yDim-1; i++)
        {
            if (candys[col, i].Type == CandyType.TWOBYTWO)
            {
                continue;
            }
            else
            {
                ClearCandy(col, i);
            }
        }
    }
}
