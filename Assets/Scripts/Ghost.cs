using UnityEngine;

public class Ghost : MonoBehaviour
{
    public Piece trackingPiece; // Ensure this is set in the Inspector

    private void Awake()
    {
        if (trackingPiece == null)
        {
            Debug.LogError("Tracking piece is not assigned.");
        }
    }

    private void LateUpdate()
    {
        Copy();
    }

    private void Copy()
    {
        if (trackingPiece == null || trackingPiece.cells == null)
        {
            Debug.LogError("Tracking piece or cells data not initialized.");
            return;
        }

        // Logic for copying the piece
    }
}