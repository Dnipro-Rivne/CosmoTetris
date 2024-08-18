using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public GameObject gameHolder;
    public Grid grid;
    public Tilemap tilemap { get; set; }

    public Piece piecePrefab;
    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public int failCount = 0;
    public int maxFails = 5;

    public TMP_Text levelText;
    public TMP_Text goalText;
    public TMP_Text scoreText;
    public TMP_Text winText;
    public GameObject levelCompleteonWindow;
    public GameObject nextLevelButtonUA;
    public GameObject nextLevelButtonEN;
    public GameObject gameStartWindow;
    public GameObject AresiboMassage;
    public GameObject FinalMassageEN;
    public GameObject FinalMassageUA;
    public TMP_Text infoText;
    public GameObject buttonsContainer;

    [SerializeField] private int currentLevel;
    [SerializeField] private int totalLevels = 5;
    [SerializeField] private int targetCount;
    [SerializeField] private int pieceCounter = 0;
    [SerializeField] private Color targetColor;

    [SerializeField] private int currentCollectedCount;

    [SerializeField] private List<Piece> activePieces = new List<Piece>();
    [SerializeField] private List<LevelConfig> levelConfigs;

    public List<float> aresiboImages;
    public Image aresiboImage;

    public bool isUA;

    public float spawnDelay = 1.0f; // Delay between spawning pieces
    private float lastSpawnTime = 0f; // Time of the last spawn

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
        levelCompleteonWindow.SetActive(false);
        aresiboImage.fillAmount = 0f;

        goalText.text = " ";
        levelText.text = " ";
        scoreText.text = " ";
        AresiboMassage.SetActive(false);
        FinalMassageEN.SetActive(false);
        FinalMassageUA.SetActive(false);
        buttonsContainer.SetActive(true);
        tilemap = GetComponentInChildren<Tilemap>();

        for (int i = 0; tetrominoes != null && i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        if (levelText == null || scoreText == null || winText == null || tilemap == null || piecePrefab == null)
        {
            Debug.LogError("One or more UI components or references are not assigned in the Inspector.");
            return;
        }

        winText.gameObject.SetActive(false); // Hide win text at the start
    }

    public void StartLevel(int level)
    {
        gameStartWindow.SetActive(false);
        activePieces.Clear();
        currentCollectedCount = 0;
        LoadLevelConfig(currentLevel);
        UpdateLevelText();
        ClearBoard();
    }

    public void RestartLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    void LoadLevelConfig(int level)
    {
        LevelConfig config = levelConfigs[level];

        pieceCounter = 0;
        if (config != null)
        {
            targetCount = 0;
            for (int i = 0; i < config.pieces.Length; i++)
            {
                config.pieces[i].Initialize();
                if (config.pieces[i].isCollecteble)
                {
                    targetColor = config.pieces[i].color;
                    targetCount++;
                }
            }
        }

        if (isUA)
        {
            goalText.text = "Зберіть " + targetCount + "\n" + config.levelGoalText.UA;
            levelText.text = $"Рівень {currentLevel + 1}";
            scoreText.text = $"Зібрано {currentCollectedCount}";
            infoText.text = config.levelEndText.UA;
        }
        else
        {
            goalText.text = "Collect " + targetCount + "\n" + config.levelGoalText.EN;
            levelText.text = $"Level {currentLevel + 1}";
            scoreText.text = $"Collected {currentCollectedCount}";
            infoText.text = config.levelEndText.EN;
        }
    }

    void UpdateLevelText()
    {
    }

    private void Update()
    {
        if (Time.time >= lastSpawnTime + spawnDelay)
        {
            SpawnNextPiece();
            lastSpawnTime = Time.time;
        }

        // Обробляємо клік миші
        if (Input.GetMouseButtonDown(0)) // Перевіряємо, чи був здійснений клік лівою кнопкою миші
        {
            Vector3Int tilePosition = GetTilePositionFromMouseClick();

            if (tilePosition != Vector3Int.zero) // Перевірка, щоб уникнути виведення некоректних координат
            {
                Debug.Log($"Клік по Tilemap: {tilePosition.x}, {tilePosition.y}");

                // Знаходимо всі Piece на цій позиції та викликаємо OnStop
                foreach (Piece piece in activePieces)
                {
                    if (piece.ContainsPosition(tilePosition))
                    {
                        piece.OnStop();
                    }
                }
            }
        }
    }

    // Метод для отримання координат кліку по Tilemap
    private Vector3Int GetTilePositionFromMouseClick()
    {
        // Отримуємо координати миші
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Конвертуємо світові координати у Tilemap координати
        Vector3Int tilePosition = tilemap.WorldToCell(mouseWorldPos);

        return tilePosition;
    }

    public void SpawnNextPiece()
    {
        Vector3Int spawnPoint = new Vector3Int(Random.Range(-4, 3), 3, 0);

        if (levelConfigs == null || levelConfigs.Count == 0)
        {
            Debug.LogError("LevelConfigs are not set up correctly.");
            return;
        }

        if (levelConfigs[currentLevel].pieces == null || levelConfigs[currentLevel].pieces.Length == 0)
        {
            Debug.LogError("Pieces in LevelConfig are not set up correctly.");
            return;
        }

        TetrominoData data = levelConfigs[currentLevel].pieces[pieceCounter];

        if (pieceCounter >= levelConfigs[currentLevel].pieces.Length - 1)
            pieceCounter = 0;
        else
            pieceCounter++;

        Piece newPiece = Instantiate(piecePrefab, grid.transform);
        newPiece.Initialize(this, spawnPoint, data);

        if (IsValidPosition(newPiece, spawnPoint))
        {
            Set(newPiece, newPiece.data.tile);
            activePieces.Add(newPiece);
            DebugActivePieces();
        }
        else
        {
            GameOver();
        }
    }

    public void Set(Piece piece, Tile tile)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, tile);
        }

        CheckForLineClears(); // Check for filled lines after setting a piece
    }


    private void CheckForLineClears()
    {
        RectInt bounds = Bounds;

        int row = bounds.yMin;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
            }
            else
            {
                row++;
            }
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Зсув рядків вниз після очищення
        for (int r = row; r < bounds.yMax; r++)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, r + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, r, 0);
                tilemap.SetTile(position, above);
            }
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            //Debug.Log("SETTING 1 for " + tilePosition.x + " " + tilePosition.y);
            tilemap.SetTile(tilePosition, null);
        }
    }

    public void RemovePiece(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            Debug.Log("SETTING 0 for " + tilePosition.x + " " + tilePosition.y);
            tilemap.SetTile(tilePosition, null);
        }

        activePieces.Remove(piece);
        Destroy(piece.gameObject); // Знищуємо фігуру після її видалення
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
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
    }

    void UpdateScore()
    {
        if (scoreText != null)
        {
            if (isUA)
                scoreText.text = $"Рахунок: {currentCollectedCount}/{targetCount}";
            else
                scoreText.text = $"Score: {currentCollectedCount}/{targetCount}";
        }
    }

    public void OnPieceCaught(Piece piece)
    {
        if (piece.data.isCollecteble)
        {
            currentCollectedCount++;
            UpdateScore();

            if (currentCollectedCount >= targetCount)
            {
                ShowLevelCompletion();
            }
        }

        LogFixedPieces();
    }

    public bool CollectedPiece()
    {
        currentCollectedCount++;
        if (currentCollectedCount >= targetCount)
        {
            ShowLevelCompletion();
            gameHolder.SetActive(false);
            return true;
        }

        if (isUA)
            scoreText.text = $"Зібрано: {currentCollectedCount}";
        else
            scoreText.text = $"Collected: {currentCollectedCount}";
        return false;
    }

    void ShowLevelCompletion()
    {
        Debug.Log("Level Completed");

        if (currentLevel >= totalLevels)
        {
            if (isUA)
            {
                nextLevelButtonUA.SetActive(true);
            }
            else
            {
                nextLevelButtonEN.SetActive(true);
            }

            WinGame();
        }
        else
        {
            currentLevel++;

            levelCompleteonWindow.SetActive(true);
            if (isUA)
            {
                nextLevelButtonUA.SetActive(true);
            }
            else
            {
                nextLevelButtonEN.SetActive(true);
            }

            for (int i = 0; i < currentLevel; i++)
            {
                aresiboImage.fillAmount = aresiboImages[i];
            }

            AresiboMassage.SetActive(true);
            infoText.gameObject.SetActive(true);
            buttonsContainer.SetActive(false);
        }
    }

    private bool endCheck = false;

    public void StartNewLevel()
    {
        if (currentLevel != levelConfigs.Count)
        {
            levelCompleteonWindow.SetActive(false);
            AresiboMassage.SetActive(false);
            buttonsContainer.SetActive(true);
            nextLevelButtonUA.SetActive(false);
            nextLevelButtonEN.SetActive(false);

            Debug.Log("started new level");
            StartLevel(currentLevel);
        }
        else
        {
            if (endCheck)
            {
                string currentSceneName = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(currentSceneName);
            }
            else
            {
                levelCompleteonWindow.SetActive(true);
                for (int i = 0; i < aresiboImages.Count; i++)
                {
                    aresiboImage.fillAmount = aresiboImages[i];
                }

                infoText.gameObject.SetActive(false);
                if (isUA)
                {
                    FinalMassageUA.SetActive(true);
                    nextLevelButtonUA.SetActive(true);
                }
                else
                {
                    FinalMassageEN.SetActive(true);
                    nextLevelButtonEN.SetActive(true);
                }

                buttonsContainer.SetActive(false);
                endCheck = true;
                gameHolder.SetActive(false);
            }
        }
    }

    void WinGame()
    {
        winText.gameObject.SetActive(true);
        if (isUA)
            winText.text = "Вітаємо! Ви повністю розшифрували послання Аресібо!";
        else
            winText.text = "Cool! You have successfully decoded the Arecibo message!";

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

    public TMP_Text StartButton;

    public void ChangeLanguage()
    {
        isUA = !isUA;
        if (isUA)
        {
            StartButton.text = "Почати гру";
            OnSetUkrainian();
        }
        else
        {
            StartButton.text = "Start game";
            OnSetEnglish();
        }

        LoadLevelConfig(currentLevel);
    }

    public void OnSetUkrainian()
    {
        foreach (GameObject obj in EnglishObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        foreach (GameObject obj in UkrainianObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
    
    public void OnSetEnglish()
    {
        foreach (GameObject obj in UkrainianObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        foreach (GameObject obj in EnglishObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
    
    [SerializeField] private GameObject[] EnglishObjects;
    [SerializeField] private GameObject[] UkrainianObjects;
}
