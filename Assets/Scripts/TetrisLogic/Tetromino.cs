using UnityEngine;
using UnityEngine.Tilemaps;

public enum Tetromino
{
    I, O, Z, S, J, L, T
}

[System.Serializable]
public struct TetrominoData
{
    public Tile tile; // 타일의 종류
    public Tetromino tetromino; // 테트로미노의 종류

    public Vector2Int[] cells { get; private set; }
    public Vector2Int[,] wallKicks { get; private set; }

    public void Initialize()
    {
        cells = Data.Cells[tetromino]; // 셀들의 좌표를 할당. Data.Cells는 I, O 등 테트로미노의 종류에 따른 좌표값들이 들어있음.
        wallKicks = Data.WallKicks[tetromino];
    }

}