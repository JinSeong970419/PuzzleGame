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
    private float space = 0.25f; // 배경 및 캔디 여백
    private float height = 1f; // 배경 및 캔디 시작 높이

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
                // 삭제될 바닥
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

        // 구멍 생성
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

    // 그리드 채우는 함수
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

        // 가장 아래 행은 움직일 수 없으며 마지막 칸은 구멍 칸때문에 -2
        // 아래 조각이 비어 있는지 확인 후 이동
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
                        Destroy(piece.gameObject); // 사용 후 빈 오브젝트 삭제
                        candy.MovableComponent.Move(x, y + 1, fillTime);
                        candys[x, y + 1] = candy;
                        NewCandySpawn(x, y, CandyType.EMPTY);
                        movedCandy = true;
                    }
                }
            }
        }

        // 가장 위의 조각 확인 후 비어 있다면 채우기
        // 특수 캔디 로직 추가하기
        for (int x = 0; x < xDim; x++)
        {
            GameCandy piece = candys[x, 1];

            if (piece.Type == CandyType.EMPTY)
            {
                Destroy(piece.gameObject); // 사용 후 빈 오브젝트 삭제
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

    // 좌표 생성 위치 변경
    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - xDim * 0.25f + space + x * 0.5f, transform.position.y + yDim * 0.25f - height - y * 0.5f);
    }

    // 새로운 캔디 생성
    public GameCandy NewCandySpawn(int x, int y, CandyType type)
    {
        GameObject newPiece = (GameObject)Instantiate(candyPrefabDict[type], GetWorldPosition(x, y), Quaternion.identity);
        newPiece.transform.parent = transform;

        candys[x, y] = newPiece.GetComponent<GameCandy>();
        candys[x, y].Init(x, y, this, type);

        return candys[x, y];
    }

    // 인접 여부 확인 함수
    public bool IsAdjacent(GameCandy candy1, GameCandy candy2)
    {
        return (candy1.X == candy2.X && (int)Mathf.Abs(candy1.Y - candy2.Y) == 1
            || (candy1.Y == candy2.Y && (int)Mathf.Abs(candy1.X - candy2.X) == 1));
    }

    // 캔디 위치를 바꾸는 함수
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
            else  // 매칭 실패시 원래 위치로
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
            else // 구멍 위치에 도착할 경우
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

    // 매칭 확인
    public List<GameCandy> GetMatch(GameCandy candy, int newX, int newY)
    {
        if (candy.IsColored())
        {
            List<GameCandy> horizontal = GetHorizonMatch(candy, newX, newY);
            List<GameCandy> vertical = GetverticalMatch(candy, newX, newY);
            List<GameCandy> square = GetsquareMatch(candy, newX, newY);
            List<GameCandy> matchingCandy = new List<GameCandy>();

            // 사각형
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

            // 가로만 확인
            else if (horizontal.Count >= 3)
            {
                for (int i = 0; i < horizontal.Count; i++)
                {
                    matchingCandy.Add(horizontal[i]);
                }
                return horizontal;
            }

            // 세로
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
        if (newX - 1 > 0 && candys[newX - 1, newY].IsColored() && candys[newX - 1, newY].ColorComponent.Color == squareCandy[0].ColorComponent.Color) // 왼쪽
        {
            if (candys[newX - 1, newY-1].IsColored() && newX < yDim - 1 && candys[newX - 1, newY - 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color &&  // 위
                candys[newX, newY-1].IsColored() && newX < yDim - 1 && candys[newX, newY - 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color)
            {
                squareCandy.Add(candys[newX - 1, newY]);
                squareCandy.Add(candys[newX - 1, newY - 1]);
                squareCandy.Add(candys[newX, newY - 1]);
            }

            if (candys[newX - 1, newY + 1].IsColored() && newX < yDim + 1 && candys[newX - 1, newY + 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color &&  // 아래
                candys[newX, newY + 1].IsColored() && newX < yDim + 1 && candys[newX, newY + 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color)
            {
                squareCandy.Add(candys[newX - 1, newY]);
                squareCandy.Add(candys[newX - 1, newY + 1]);
                squareCandy.Add(candys[newX, newY + 1]);
            }
        }

        if (newX < xDim - 1 && candys[newX + 1, newY].IsColored() && candys[newX + 1, newY].ColorComponent.Color == squareCandy[0].ColorComponent.Color) // 오른쪽
        {

            if (candys[newX , newY - 1].IsColored() && newX < yDim + 1 && candys[newX , newY - 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color &&  // 위
                candys[newX + 1 , newY - 1].IsColored() && newX < yDim + 1 && candys[newX + 1 , newY - 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color)
            {
                squareCandy.Add(candys[newX + 1, newY]);
                squareCandy.Add(candys[newX + 1, newY - 1]);
                squareCandy.Add(candys[newX, newY - 1]);
            }

            if (candys[newX + 1, newY + 1].IsColored() && newX < yDim - 1 && candys[newX + 1, newY + 1].ColorComponent.Color == squareCandy[0].ColorComponent.Color &&  // 아래
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
                        GameCandy candyPosition = match[0];   // 플레이어가 놓은 위치에 생성
                        int specialPieceX = candyPosition.X;
                        int specialPieceY = candyPosition.Y;

                        if(match.Count == 4 && match[0].Type != CandyType.TWOBYTWO)
                        {
                            // 리필 중 만들어질 경우 처리
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

                        // 매칭 안에 피스 캔디 지우기
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

                        // 특수 조각 생성 조건 확인
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

    // 캔디 삭제 코드
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

    // Row 지우기
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

    // col 지우기
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
