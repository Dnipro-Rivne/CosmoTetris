using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Tetris/LevelConfig", order = 1)]
public class LevelConfig : ScriptableObject
{
    public int level;
    public multilanguadgeText levelStartText;
    public multilanguadgeText levelGoalText;
    public multilanguadgeText levelEndText;
    
    public TetrominoData[] pieces;
}

[System.Serializable]
public class multilanguadgeText
{
    public string EN;
    public string UA;
}
