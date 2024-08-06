using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    [SerializeField] public Vector3Int[] cells { get; private set; }
    [SerializeField] public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float moveTime;
    private float lockTime;
    private bool isAtBottom;
    public bool isValid = true;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        this.position = position;

        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

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
        if (board == null || cells == null)
        {
            Debug.LogError("Board or cells data not initialized.");
            return;
        }

        board.Clear(this);

        lockTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) || rotationLeft)
        {
            rotationLeft = false;
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.E) || rotationRight)
        {
            rotationRight = false;
            Rotate(1);
        }

        if (Input.GetKeyDown(KeyCode.Space) || hardDown)
        {
            HardDrop();
            hardDown = false;
        }

        if (Time.time > moveTime)
        {
            HandleMoveInputs();
        }

        if (Time.time > stepTime)
        {
            Step();
        }

        if (isValid)
            board.Set(this, this.data.tile);
    }

    public void OnMoveLeft()
    {
        if (Time.time > moveTime)
        {
            left = true;
        }
    }

    public void OnMoveRight()
    {
        if (Time.time > moveTime)
        {
            right = true;
        }
    }

    public void OnSoftDrop()
    {
        if (Time.time > moveTime)
        {
            softDown = true;
        }
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

    private bool right;
    private bool left;
    private bool hardDown;
    private bool softDown;
    private bool rotationLeft;
    private bool rotationRight;

    private void HandleMoveInputs()
    {
        if (Input.GetKey(KeyCode.S) || softDown)
        {
            softDown = false;
            if (Move(Vector2Int.down))
            {
                stepTime = Time.time + stepDelay;
            }
        }

        if (Input.GetKey(KeyCode.A) || left)
        {
            Move(Vector2Int.left);
            left = false;
        }
        else if (Input.GetKey(KeyCode.D) || right)
        {
            right = false;
            Move(Vector2Int.right);
        }
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;

        if (!Move(Vector2Int.down))
        {
            isValid = false;
            Lock();
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        // Закріплюємо фігуру на дошці
        board.Set(this, null);
        //board.OnPieceCaught(this);
        board.RemovePiece(this);
        //Destroy(gameObject); // Знищуємо фігуру після її закріплення
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        if (valid)
        {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            lockTime = 0f;
        }
        else if (translation == Vector2Int.down)
        {
            isAtBottom = true;
        }

        return valid;
    }

    private void Rotate(int direction)
    {
        int originalRotation = rotationIndex;

        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
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

    private void Stop()
    {
        if (this.data.isCollecteble)
        {
            board.CollectedPiece();
        }
        else
        {
            board.AddFail();
        }

        board.RemovePiece(this);
    }

    // Метод для перевірки, чи містить фігура певну позицію
    public bool ContainsPosition(Vector3Int positionToCheck)
    {
        foreach (var cell in cells)
        {
            if (cell + position == positionToCheck)
            {
                return true;
            }
        }
        return false;
    }
}
