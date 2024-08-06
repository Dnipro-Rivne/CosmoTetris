using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections;

public class Board : MonoBehaviour
{
    public GameObject gameHolder;

    public Grid grid; // Add a reference to the Grid component
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    public int failCount = 0;
    public int maxFails = 5;

    public TMP_Text levelText;
    public TMP_Text goalText;
    public TMP_Text scoreText;
    public TMP_Text winText; // Додано для відображення повідомлення про перемогу
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
    [SerializeField] private int totalLevels = 5; // Загальна кількість рівнів
    [SerializeField] private int targetCount;
    [SerializeField] private int pieceCounter = 0;
    [SerializeField] private Color targetColor;

    [FormerlySerializedAs("currentCount")] [SerializeField]
    private int currentCollectedCount;

    [SerializeField] private List<Piece> activePieces = new List<Piece>(); // Список для збереження активних фігур

    [SerializeField] private List<LevelConfig> levelConfigs;

    public List<float> aresiboImages;
    public Image aresiboImage;

    public bool isUA;

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
        activePiece = GetComponentInChildren<Piece>();
        //levelConfigs[0].pieces[0]
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
    }

    public void StartLevel(int level)
    {
        gameStartWindow.SetActive(false);
        activePieces.Clear();
        currentCollectedCount = 0;
        LoadLevelConfig(currentLevel);
        UpdateLevelText();
        ClearBoard();
        SpawnNextPiece();
        //StartCoroutine(newpiece());
    }

    public void RestartLevel()
    {
        //StartLevel(currentLevel);
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    void LoadLevelConfig(int level)
    {
        // Load level configuration from LevelConfigManager
        // LevelConfig config = LevelConfigManager.GetLevelConfig(level);
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

            //Debug.Log($"Loaded level config: Level {level}, Target Color: {ColorUtility.ToHtmlStringRGB(targetColor)}, Target Count: {targetCount}");
        }
        else
        {
            //Debug.LogError($"Level config for level {level} not found.");
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
        // if (levelText != null)
        // {
        //     winText.text =
        //         $"Рівень {currentLevel}";
        //    
        // }
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

        // Get TetrominoData from levelConfigs
        TetrominoData data = levelConfigs[currentLevel].pieces[pieceCounter];
        // TetrominoData data = tetrominoes[0];
        if (pieceCounter >= levelConfigs[currentLevel].pieces.Length - 1)
            pieceCounter = 0;
        else
            pieceCounter++;

        activePiece.Initialize(this, spawnPoint, data);

        if (IsValidPosition(activePiece, spawnPoint))
        {
            Set(activePiece);
            activePieces.Add(activePiece); // Add the active piece to the list
            DebugActivePieces(); // Log active pieces
        }
        else
        {
            GameOver();
        }
    }

    IEnumerator newpiece()
    {
        yield return new WaitForSeconds(1);
        SpawnNextPiece();
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
        if (piece.data.isCollecteble) // Assuming 'color' is part of TetrominoData
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
        // Show level completion message and part of the code
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
        // Показуємо повідомлення про перемогу
        winText.gameObject.SetActive(true);
        if (isUA)
            winText.text = "Вітаємо! Ви повністю розшифрували послання Аресібо!";
        else
            winText.text = "Cool! You have successfully decoded the Arecibo message!";

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

    public TMP_Text StartButton;

    public void ChangeLanguage()
    {
        isUA = !isUA;
        if (isUA)
        {
            StartButton.text = "Почати гру";
        }
        else
        {
            StartButton.text = "Start game";
        }

        LoadLevelConfig(currentLevel);
    }
}