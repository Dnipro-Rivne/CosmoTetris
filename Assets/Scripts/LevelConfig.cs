using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Tetris/LevelConfig", order = 1)]
public class LevelConfig : ScriptableObject
{
    public int level;
    public string levelStartText;
    public string levelGoalText;
    public string levelEndText;
    
    public TetrominoData[] pieces;
}