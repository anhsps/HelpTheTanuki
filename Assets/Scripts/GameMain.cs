using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameMain : MonoBehaviour
{
    [SerializeField] private AudioSource move_audio;
    [SerializeField] private Level[] levels;
    [SerializeField] private SpriteRenderer[] bgPrefab;
    [SerializeField] private GamePiece winPrefab;
    [SerializeField] private GamePiece[] piecePrefab;
    [SerializeField] private TextMeshProUGUI moveText;

    private int levelIndex = 1, moveIndex;
    private Level currentLevel;
    private List<GamePiece> gamePieces = new List<GamePiece>();
    private GamePiece winPiece, currentPiece;
    private Vector2 currentPos, previousPos, previousGridPos, startPos;
    private List<Vector2> offsets;
    private bool[,] pieceCollision;
    private bool move = true;

    void Start()
    {
        levelIndex = Mathf.Clamp(GameManager.instance.GetCurrentLevel(), 1, levels.Length);
        currentLevel = levels[levelIndex - 1];
        moveIndex = currentLevel.moveIndex;
        moveText.text = moveIndex.ToString();
        SpawnLevel();
    }

    void Update()
    {
        MobileInput();
    }

    private void MobileInput()
    {
        if (Input.touchCount > 0 && moveIndex > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos2D = Camera.main.ScreenToWorldPoint(touch.position);
            RaycastHit2D hit = Physics2D.Raycast(touchPos2D, Vector2.zero);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (!hit || !hit.collider.transform.parent.TryGetComponent(out currentPiece))
                        return;

                    move = true;
                    previousPos = currentPos = touchPos2D;
                    previousGridPos = currentPiece.CurrentGridPos;
                    startPos = new Vector2(
                                currentPiece.CurrentGridPos.y + 0.5f, currentPiece.CurrentGridPos.x + 0.5f);
                    CalculateCollision();
                    offsets = new List<Vector2>();
                    break;

                case TouchPhase.Moved:
                    if (currentPiece == null || !move) return;

                    currentPos = touchPos2D;

                    Vector2 offset = currentPos - previousPos;
                    offsets.Add(offset);

                    float moveDelta = IsMovingOpposite() ? -0.5f : 0.5f;
                    Vector2 piecePos = currentPiece.CurrentPos;

                    if (currentPiece.IsVertical)
                        piecePos.y += moveDelta;
                    else
                        piecePos.x += moveDelta;

                    Vector2Int pieceGridPos = new Vector2Int(
                        Mathf.FloorToInt(piecePos.y),
                        Mathf.FloorToInt(piecePos.x));

                    if (!CanMovePiece(pieceGridPos))
                    {
                        if (currentPiece.CurrentGridPos != previousGridPos)
                        {
                            currentPiece.CurrentPos = new Vector2(
                                currentPiece.CurrentGridPos.y + 0.5f, currentPiece.CurrentGridPos.x + 0.5f);
                        }
                        else
                        {
                            currentPiece.CurrentPos = startPos;
                        }
                        move = false;
                        return;
                    }

                    currentPiece.CurrentGridPos = pieceGridPos;
                    currentPiece.UpdatePos(currentPiece.IsVertical ? offset.y : offset.x);

                    previousPos = currentPos;
                    break;

                case TouchPhase.Ended:
                    if (currentPiece != null)
                    {
                        currentPiece.transform.position = new Vector3(
                            currentPiece.CurrentGridPos.y + 0.5f,
                            currentPiece.CurrentGridPos.x + 0.5f, 0);

                        if (currentPiece.CurrentGridPos != previousGridPos)
                            CheckLose();

                        currentPiece = null;
                        currentPos = Vector2.zero;
                        previousPos = Vector2.zero;
                        move = true;
                    }
                    break;
            }
        }
    }

    private void SpawnLevel()
    {
        // set up tiles bg
        for (int i = 0; i < currentLevel.Rows; i++)
        {
            for (int j = 0; j < currentLevel.Columns; j++)
            {
                SpriteRenderer bg;
                if (i == 3 && j == 0)
                    bg = Instantiate(bgPrefab[4]);
                else
                {
                    if (levelIndex > 10)
                        bg = Instantiate(bgPrefab[(i % 2 == 0 && j % 2 == 0) ||
                    (i % 2 != 0 && j % 2 != 0) ? 2 : 3]);
                    else
                        bg = Instantiate(bgPrefab[(i % 2 == 0 && j % 2 == 0) ||
                    (i % 2 != 0 && j % 2 != 0) ? 0 : 1]);
                }

                bg.size = Vector2.one;
                bg.transform.position = new Vector2(i + 0.5f, j + 0.5f);
            }
        }

        // spawn win piece
        winPiece = Instantiate(winPrefab);
        Vector3 spawnPos = new Vector3(currentLevel.winPiece.startPos.y + 0.5f,
            currentLevel.winPiece.startPos.x + 0.5f, 0);
        winPiece.transform.position = spawnPos;
        winPiece.Init(currentLevel.winPiece);
        gamePieces.Add(winPiece);

        // spawn all pieces
        foreach (var piece in currentLevel.Pieces)
        {
            int typeIndex = Mathf.Clamp(piece.type, 0, piecePrefab.Length - 1);
            GamePiece temp = Instantiate(piecePrefab[typeIndex]);
            spawnPos = new Vector3(piece.startPos.y + 0.5f, piece.startPos.x + 0.5f, 0);
            temp.transform.position = spawnPos;
            temp.Init(piece);
            gamePieces.Add(temp);
        }

        // set up camera
        Camera.main.orthographicSize = Mathf.Max(currentLevel.Columns, currentLevel.Rows) + 2f;
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = currentLevel.Columns * 0.5f;
        camPos.y = currentLevel.Rows * 0.5f;
        Camera.main.transform.position = camPos;
    }

    private void CalculateCollision()
    {
        pieceCollision = new bool[currentLevel.Rows, currentLevel.Columns];
        for (int i = 0; i < currentLevel.Rows; i++)
        {
            for (int j = 0; j < currentLevel.Columns; j++)
            {
                pieceCollision[i, j] = false;
            }
        }

        foreach (var piece in gamePieces)
        {
            for (int i = 0; i < piece.Size; i++)
            {
                int x = piece.CurrentGridPos.x + (piece.IsVertical ? i : 0);
                int y = piece.CurrentGridPos.y + (piece.IsVertical ? 0 : i);
                pieceCollision[x, y] = true;
            }
        }

        for (int i = 0; i < currentPiece.Size; i++)
        {
            int x = currentPiece.CurrentGridPos.x + (currentPiece.IsVertical ? i : 0);
            int y = currentPiece.CurrentGridPos.y + (currentPiece.IsVertical ? 0 : i);
            pieceCollision[x, y] = false;
        }
    }

    private bool CanMovePiece(Vector2Int pieceGridPos)
    {
        List<Vector2Int> piecePos = new List<Vector2Int>();
        for (int i = 0; i < currentPiece.Size; i++)
        {
            piecePos.Add(pieceGridPos +
                (currentPiece.IsVertical ? Vector2Int.right : Vector2Int.up) * i);
        }

        foreach (var pos in piecePos)
        {
            if (!IsValidPos(pos) || pieceCollision[pos.x, pos.y])
                return false;
        }

        return true;
    }

    //move nguoc huong
    private bool IsMovingOpposite()
    {
        Vector2 result = Vector2.zero;
        for (int i = Mathf.Max(0, offsets.Count - 20); i < offsets.Count; i++)
            result += offsets[i];

        float val = currentPiece.IsVertical ? result.y : result.x;
        if (Mathf.Abs(val) > 0.2f)
            return val < 0;

        return true;
    }

    private bool IsValidPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < currentLevel.Rows && pos.y < currentLevel.Columns;
    }

    public bool CheckWin()
    {
        if (winPiece.CurrentGridPos.x == 0)
        {
            GameManager.instance.GameWin();
            return true;
        }
        return false;
    }

    public void CheckLose()
    {
        if (SoundManager.instance.soundEnabled)
            move_audio.Play();

        moveIndex--;
        moveText.text = moveIndex.ToString();

        if (CheckWin()) return;

        if (moveIndex == 0)
            GameManager.instance.GameLose();
    }
}
