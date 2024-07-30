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
    public TMP_Text splashText;
    public GameObject splashScreen;

    public List<LevelConfig> levelConfigs;

    private int currentLevel;
    private List<LevelConfig.PieceConfig> spawnPieces;
    private List<LevelConfig.PieceConfig> targetPieces;
    private int spawnIndex;
    private int targetCount;
    private int currentCount;

    private List<Piece> activePieces = new List<Piece>();

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
        if (levelText == null || scoreText == null || winText == null || splashText == null || splashScreen == null || tilemap == null || activePiece == null)
        {
            Debug.LogError("One or more UI components or references are not assigned in the Inspector.");
            return;
        }

        winText.gameObject.SetActive(false);
        splashScreen.SetActive(true);
        splashText.gameObject.SetActive(true);

        StartLevel(1);
    }

    void StartLevel(int level)
    {
        currentLevel = level;
        LoadLevelConfig(level);
        ShowSplashScreen();
        currentCount = 0;
        failCount = 0;
        spawnIndex = 0;
        ClearBoard();
        InitializeBoard();
        SpawnNextPiece();
    }

    void LoadLevelConfig(int level)
    {
        LevelConfig config = levelConfigs.Find(l => l.level == level);
        if (config != null)
        {
            spawnPieces = new List<LevelConfig.PieceConfig>(config.spawnPieces);
            targetPieces = new List<LevelConfig.PieceConfig>(config.targetPieces);
            targetCount = targetPieces.Count;
            Debug.Log($"Loaded level config: Level {level}, Target Count: {targetCount}");
        }
        else
        {
            Debug.LogError($"Level config for level {level} not found.");
        }
    }

    void InitializeBoard()
    {
        // You can add initial board setup logic here if needed
    }

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = $"Рівень {currentLevel}\nШукай {targetCount} фігур";
        }
    }

    public void SpawnNextPiece()
    {
        LevelConfig.PieceConfig pieceConfig;

        if (spawnIndex < spawnPieces.Count)
        {
            pieceConfig = spawnPieces[spawnIndex];
        }
        else
        {
            // If we reach the end of the list, start from the beginning
            spawnIndex = 0;
            pieceConfig = spawnPieces[spawnIndex];
        }

        spawnIndex++;

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
            RestartLevel();
        }
    }

    void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Рахунок: {currentCount}/{targetCount}";
        }
    }

    public void OnPieceStopped(Piece piece)
    {
        if (IsPieceValid(piece))
        {
            currentCount++;
            UpdateScore();

            if (currentCount >= targetCount)
            {
                ShowLevelCompletion();
            }
            else
            {
                Set(piece);
            }
        }
        else
        {
            AddFail();
            piece.ContinueFalling();
        }

        LogFixedPieces();
    }

    bool IsPieceValid(Piece piece)
    {
        foreach (var targetPiece in targetPieces)
        {
            if (piece.data == targetPiece.data && piece.data.color == targetPiece.color)
            {
                targetPieces.Remove(targetPiece); // Видаляємо зловлену фігуру зі списку
                return true;
            }
        }
        return false;
    }

    void ShowLevelCompletion()
    {
        Debug.Log("Level Completed");
        splashScreen.SetActive(true);
        splashText.text = $"Частина шифру розблокована!\nНатисніть будь-яку клавішу, щоб продовжити.";
        if (currentLevel >= levelConfigs.Count)
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
        splashScreen.SetActive(false);
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

    void RestartLevel()
    {
        Debug.Log("Level Restarted");
        StartLevel(currentLevel);
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

    void ShowSplashScreen()
    {
        splashScreen.SetActive(true);
        splashText.text = $"Рівень {currentLevel}\nШукай {targetCount} фігур";
        Invoke("HideSplashScreen", 3f); // Приховуємо сплеш-скрін через 3 секунди
    }

    void HideSplashScreen()
    {
        splashScreen.SetActive(false);
        SpawnNextPiece();
    }
}
