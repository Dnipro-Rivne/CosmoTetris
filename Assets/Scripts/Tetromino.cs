using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino
{
    I, J, L, O, S, T, Z
}

[System.Serializable]
public struct TetrominoData
{
    public Tile tile;
    public Tetromino tetromino;
    public Vector2Int[] cells { get; private set; }
    public Vector2Int[,] wallKicks { get; private set; }
    public Color color; // Додаємо поле для кольору
    public void Initialize()
    {
        cells = Data.Cells[tetromino];
        wallKicks = Data.WallKicks[tetromino];
    }

    // Додаємо оператори порівняння
    public override bool Equals(object obj)
    {
        if (obj is TetrominoData)
        {
            TetrominoData other = (TetrominoData)obj;
            return tile == other.tile && tetromino == other.tetromino;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return tile.GetHashCode() ^ tetromino.GetHashCode();
    }

    public static bool operator ==(TetrominoData left, TetrominoData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TetrominoData left, TetrominoData right)
    {
        return !left.Equals(right);
    }
}