using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Tetris/LevelConfig", order = 1)]
public class LevelConfig : ScriptableObject
{
    public int level;
    public List<PieceConfig> spawnPieces; // Список фігур, які спавняться на рівні
    public List<PieceConfig> targetPieces; // Список фігур, які гравець має зловити

    [System.Serializable]
    public class PieceConfig
    {
        public TetrominoData data;
        public Color color;
    }

    public static LevelConfig LoadFromJson(string json)
    {
        return JsonUtility.FromJson<LevelConfig>(json);
    }
}