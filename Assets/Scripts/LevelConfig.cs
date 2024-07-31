using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Tetris/LevelConfig", order = 1)]
public class LevelConfig : ScriptableObject
{
    public int level;
    public List<PieceConfig> pieces;

    [System.Serializable]
    public class RowData
    {
        public List<int> row;
    }

    [System.Serializable]
    public class PieceConfig
    {
        public TetrominoData data;
        public bool catchThisPiece;
    }

    public static LevelConfig LoadFromJson(string json)
    {
        return JsonUtility.FromJson<LevelConfig>(json);
    }
}