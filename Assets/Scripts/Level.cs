using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Level")]
public class Level : ScriptableObject
{
    public int Rows;
    public int Columns;
    public int moveIndex;
    public Piece winPiece;
    public List<Piece> Pieces;
}

[Serializable]
public struct Piece
{
    public int type;
    public bool IsVertical;
    public int Size;
    public Vector2Int startPos;// hang ? cot ?
}
