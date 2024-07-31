using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    public Grid grid;
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    public int failCount = 0;
    public int maxFails = 5;

    public TMP_Text levelText;
    public TMP_Text scoreText;
    public TMP_Text winText;

    [SerializeField] private int currentLevel;
    private int totalLevels = 5;
    private Color targetColor;
    private int targetCount;
    private int currentCount;

    private List<Piece> activePieces = new List<Piece>();
    [SerializeField] private List<LevelConfig.PieceConfig> levelPieces;
    [SerializeField] private List<LevelConfig.PieceConfig> caughtPieces;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; tetrominoes != null && i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        caughtPieces = new List<LevelConfig.PieceConfig>();

        if (levelText == null || scoreText == null || winText == null || tilemap == null || activePiece == null)
        {
            Debug.LogError("One or more UI components or references are not assigned in the Inspector.");
            return;
        }

        winText.gameObject.SetActive(false);
        StartLevel(0);
    }

    void StartLevel(int level)
    {
        currentLevel = level;
        LoadLevelConfig(level);
        UpdateLevelText();
        currentCount = 0;
        ClearBoard();
        SpawnNextPiece();
    }

    void LoadLevelConfig(int level)
    {
        LevelConfig config = LevelConfigManager.Instance.GetLevelConfig(level);
        if (config != null)
        {
            levelPieces = new List<LevelConfig.PieceConfig>(config.pieces);
            Debug.Log($"Loaded level config: Level {level}, Target Color: {ColorUtility.ToHtmlStringRGB(targetColor)}, Target Count: {targetCount}");
        }
        else
        {
            Debug.LogError($"Level config for level {level} not found.");
        }
    }

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = $"Рівень {currentLevel}\nШукай {targetCount} фігур кольору {ColorUtility.ToHtmlStringRGB(targetColor)}";
        }
    }

    public void SpawnNextPiece()
    {
        if (levelPieces.Count == 0)
        {
            Debug.LogError("No pieces configured for the level.");
            return;
        }

        LevelConfig.PieceConfig pieceConfig = levelPieces[0];
        levelPieces.RemoveAt(0);
        levelPieces.Add(pieceConfig);

        activePiece.Initialize(this, spawnPosition, pieceConfig.data);

        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
            activePieces.Add(activePiece);
            DebugActivePieces();
        }
        else
        {
            GameOver();
        }
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void AddFail()
    {
        failCount++;
        if (failCount >= maxFails)
        {
            Debug.Log("YOU LOSE");
            GameOver();
        }
    }

    void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Рахунок: {currentCount}/{targetCount}";
        }
    }

    public void OnPieceCaught(Piece piece)
    {
        LevelConfig.PieceConfig caughtPieceConfig = levelPieces.Find(p => p.data.tetromino == piece.data.tetromino && p.data.color == piece.data.color);
        if (caughtPieceConfig != null && caughtPieceConfig.catchThisPiece)
        {
            caughtPieces.Add(caughtPieceConfig);
            currentCount++;
            UpdateScore();

            if (currentCount >= targetCount)
            {
                ShowLevelCompletion();
            }
        }
        else
        {
            AddFail();
        }

        LogFixedPieces();
    }

    void ShowLevelCompletion()
    {
        Debug.Log("Level Completed");
        
        if (currentLevel >= totalLevels)
        {
            WinGame();
        }
        else
        {
            StartLevel(currentLevel + 1);
        }
    }

    void WinGame()
    {
        winText.gameObject.SetActive(true);
        winText.text = "Вітаємо! Ви повністю розшифрували послання Аресібо!";
        Debug.Log("Cool! You have successfully decoded the Arecibo message!");
    }

    void ClearBoard()
    {
        tilemap.ClearAllTiles();
        activePieces.Clear();
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();
        Debug.Log("Game Over");
    }

    void DebugActivePieces()
    {
        Debug.Log("Active pieces:");
        foreach (Piece piece in activePieces)
        {
            Debug.Log($"Piece {piece.data.tetromino} with color {piece.data.color}");
        }
    }

    void LogFixedPieces()
    {
        var pieceGroups = new Dictionary<string, int>();

        foreach (Piece piece in activePieces)
        {
            string key = $"{piece.data.tetromino} - {ColorUtility.ToHtmlStringRGB(piece.data.color)}";
            if (pieceGroups.ContainsKey(key))
            {
                pieceGroups[key]++;
            }
            else
            {
                pieceGroups[key] = 1;
            }
        }

        Debug.Log("Fixed pieces count and types:");
        foreach (var group in pieceGroups)
        {
            Debug.Log($"{group.Key}: {group.Value}");
        }
    }
}
