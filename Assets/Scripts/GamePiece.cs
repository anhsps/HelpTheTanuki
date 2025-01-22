using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private BoxCollider2D box;

    [HideInInspector] public bool IsVertical;
    [HideInInspector] public int Size;
    [HideInInspector] public Vector2Int CurrentGridPos;
    [HideInInspector] public Vector2 CurrentPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(Piece piece)
    {
        CurrentGridPos = piece.startPos;
        IsVertical = piece.IsVertical;
        Size = piece.Size;
        CurrentPos = new Vector2(CurrentGridPos.y + 0.5f, CurrentGridPos.x + 0.5f);

        if (piece.IsVertical)
        {
            sr.transform.localPosition = new Vector3(0, Size - 1, 0) * 0.5f;
            sr.size = new Vector2(1, Size);
            box.size = new Vector2(1, Size);
        }
        else
        {
            sr.transform.localPosition = new Vector3(Size - 1, 0, 0) * 0.5f;
            sr.size = new Vector2(Size, 1);
            box.size = new Vector2(Size, 1);
        }
    }

    public void UpdatePos(float offset)
    {
        CurrentPos += (IsVertical ? Vector2.up : Vector2.right) * offset;
        transform.position = CurrentPos;
    }
}
