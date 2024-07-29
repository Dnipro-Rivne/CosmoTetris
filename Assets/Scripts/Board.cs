using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro; // Заміна на TMP_Text
using System.Collections.Generic; // Для використання List

public class Board : MonoBehaviour
{
    public Grid grid; // Add a reference to the Grid component
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    public int failCount = 0;
    public int maxFails = 5;

    public TMP_Text levelText;
    public TMP_Text scoreText;
    public TMP_Text winText; // Додано для відображення повідомлення про перемогу

    private int currentLevel;
    private int totalLevels = 5; // Загальна кількість рівнів
    private Color targetColor;
    private int targetCount;
    private int currentCount;

    private List<Piece> activePieces = new List<Piece>(); // Список для збереження активних фігур

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
        if (levelText == null || scoreText == null || winText == null || tilemap == null || activePiece == null)
        {
            Debug.LogError("One or more UI components or references are not assigned in the Inspector.");
            return;
        }

        winText.gameObject.SetActive(false); // Ховаємо текст перемоги на початку
        StartLevel(1);
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
        // Load level configuration from LevelConfigManager
        LevelConfig config = LevelConfigManager.GetLevelConfig(level);
        if (config != null)
        {
            targetColor = config.targetColor;
            targetCount = config.targetCount;
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
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
            activePieces.Add(activePiece); // Додаємо активну фігуру до списку
            DebugActivePieces(); // Виводимо активні фігури у лог
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
        if (piece.data.color == targetColor)  // Assuming 'color' is part of TetrominoData
        {
            currentCount++;
            UpdateScore();

            if (currentCount >= targetCount)
            {
                ShowLevelCompletion();
            }
        }

        LogFixedPieces();
    }

    void ShowLevelCompletion()
    {
        // Show level completion message and part of the code
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
        // Показуємо повідомлення про перемогу
        winText.gameObject.SetActive(true);
        winText.text = "Вітаємо! Ви повністю розшифрували послання Аресібо!";
        Debug.Log("Cool! You have successfully decoded the Arecibo message!");
        // Можливо додати інші дії після виграшу, наприклад, збереження результату, перехід до меню тощо
    }

    void ClearBoard()
    {
        tilemap.ClearAllTiles();
        activePieces.Clear(); // Очищаємо список активних фігур
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();
        Debug.Log("Game Over");

        // Do anything else you want on game over here..
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
