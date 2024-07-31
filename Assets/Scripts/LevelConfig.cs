using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Tetris/LevelConfig", order = 1)]
public class LevelConfig : ScriptableObject
{
    public int level;
    public TetrominoData[] pieces;
}