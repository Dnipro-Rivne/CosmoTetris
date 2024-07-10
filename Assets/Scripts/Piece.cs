using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float moveDelay = 0.1f;
    public float destroyDelay = 1f; // Delay before the piece is destroyed

    private float stepTime;
    private float moveTime;
    private float destroyTime;

    private bool isStopped;
    private bool isAtBottom;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        this.position = position;
        this.isStopped = false;
        this.isAtBottom = false;

        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;

        if (cells == null)
        {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        if (isStopped) return;

        board.Clear(this);

        // Handle rotation
        if (Input.GetKeyDown(KeyCode.Q) || rotationLeft)
        {
            Rotate(-1);
            rotationLeft = false;
        }
        else if (Input.GetKeyDown(KeyCode.E) || rotationRight)
        {
            Rotate(1);
            rotationRight = false;
        }

        // Handle hard drop
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }

        // Allow the player to hold movement keys but only after a move delay
        // so it does not move too fast
        if (Time.time > moveTime)
        {
            HandleMoveInputs();
        }

        // Advance the piece to the next row every x seconds
        if (Time.time > stepTime)
        {
            Step();
        }

        // Stop the piece when the specific key is pressed
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Stop();
        }

        if (isAtBottom && Time.time > destroyTime)
        {
            DestroyPiece();
        }

        board.Set(this);
    }

    private void HandleMoveInputs()
    {
        // Soft drop movement
        if (Input.GetKey(KeyCode.S) || softDrop)
        {
            if (Move(Vector2Int.down))
            {
                // Update the step time to prevent double movement
                stepTime = Time.time + stepDelay;
            }
            softDrop = false;
        }

        // Left/right movement
        if (Input.GetKey(KeyCode.A) || moveLeft)
        {
            Move(Vector2Int.left);
            moveLeft = false;
        }
        else if (Input.GetKey(KeyCode.D) || moveRight)
        {
            Move(Vector2Int.right);
            moveRight = false;
        }
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;

        // Step down to the next row
        if (!Move(Vector2Int.down))
        {
            if (position.y < -board.boardSize.y / 2)
            {
                board.SpawnPiece();
            }
            else
            {
                isAtBottom = true;
                destroyTime = Time.time + destroyDelay;
            }
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        isAtBottom = true;
        destroyTime = Time.time + destroyDelay;
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        // Only save the movement if the new position is valid
        if (valid)
        {
            position = newPosition;
            moveTime = Time.time + moveDelay;
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        // Store the current rotation in case the rotation fails
        // and we need to revert
        int originalRotation = rotationIndex;

        // Rotate all of the cells using a rotation matrix
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        // Revert the rotation if the wall kick tests fail
        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        // Rotate all of the cells using the rotation matrix
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // "I" and "O" are rotated from an offset center point
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }

    private void DestroyPiece()
    {
        isStopped = true;
        board.Clear(this);
        board.SpawnPiece();
    }

    public void OnMoveLeft()
    {
        moveLeft = true;
    }

    public void OnMoveRight()
    {
        moveRight = true;
    }

    public void OnSoftDrop()
    {
        softDrop = true;
    }

    public void OnRotateLeft()
    {
        rotationLeft = true;
    }

    public void OnRotateRight()
    {
        rotationRight = true;
    }

    public void OnStop()
    {
        Stop();
    }

    public void Stop()
    {
        isStopped = true;
        board.SpawnPiece();
    }

    private bool moveLeft;
    private bool moveRight;
    private bool softDrop;
    private bool rotationLeft;
    private bool rotationRight;
}
