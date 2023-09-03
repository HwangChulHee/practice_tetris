using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Board : MonoBehaviour
{
    public Tilemap tilemap {get; private set;}
    public Piece activePiece { get; private set; }
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    public TetrominoData[] tetrominos; // i, o, z, s 등.. 7개를 할당.
    private List<TetrominoData> next_tetrominos = new List<TetrominoData>(); // 랜덤한 테트로미노가 6개 할당됨.
    public Vector3Int[] next_tetriminos_position;
    
    private TetrominoData? hold_tetromino = null;
    public bool isHoldBtn = false;
    public Vector3Int hold_tetriminos_position;

    public Vector2Int boardSize = new Vector2Int(10, 20);


    public int block_speed; // 블록 스피드
    public int exper; // 경험치
    int lineCount; // 한 piece로 인해 클리어된 라인의 개수

    public int lineClearCount = 0;
    private bool isPreviousLineClear;
    private int comboCount = 0;

    public bool isGameOver;
    public GameObject gameOver;

    Scene scene;
    public singleMode_manager gameManager;
    public GameObject img_hold_ban;


    
    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake() {
        this.tilemap = GetComponentInChildren<Tilemap>(); // ? 이 부분 이해좀 안된듯.
        activePiece = GetComponent<Piece>(); // 떨어질 조각을 만들어주는 스크립트 초기화

        isGameOver = false;
        
        block_speed = 1;
        exper = 0;

        for(int i = 0; i < this.tetrominos.Length; i++) {
            this.tetrominos[i].Initialize(); //i, o, z, s 등의 좌표를 초기화            
        }
    }

    private void Start() {
        init_NextPiece();   
    }

    public void SpawnPiece() {
        int random = Random.Range(0, this.tetrominos.Length); // 무작위 숫자 할당. 끝값은 제외되므로, 0부터 6의 데이터가 나옴.
        TetrominoData data = this.tetrominos[random]; // i, o, z 중 무작위의 테트로미노가 나온다.

         activePiece.Initialize(this, spawnPosition, data); // piece 스크립트안에 해당 테트리미노를 할당해주고, spawn 위치도 저장해준다.
         Set(this.activePiece); // piece 에 할당된 정보를 화면에 띄어준다.
    }

    public void newSpawnPiece() {

        activePiece.Initialize(this, spawnPosition, this.next_tetrominos[0]); // piece 스크립트안에 해당 테트리미노를 할당해주고, spawn 위치도 저장해준다.
        
        if (IsValidPosition(activePiece, spawnPosition)) {
            Set(this.activePiece); // piece 에 할당된 정보를 화면에 띄어준다.
        } else {
            GameOver();
            return;
        }
        

        clear_NextPiece();

        next_tetrominos.RemoveAt(0);
        int random = Random.Range(0, this.tetrominos.Length); // 무작위 숫자 할당. 끝값은 제외되므로, 0부터 6의 데이터가 나옴.
        TetrominoData data = this.tetrominos[random]; // i, o, z 중 무작위의 테트로미노가 나온다.
        next_tetrominos.Add(data); // 마지막에 새로운 테트로미노 추가

        // 다시 타일맵에 테트로미노 그려줌.
        for(int i = 0; i < next_tetrominos.Count; i++) {
            set_NextPiece(next_tetrominos[i] , i);            
        }        
    }

    public void replaceSpawnPiece() {
         activePiece.Initialize(this, spawnPosition, (TetrominoData)hold_tetromino); // piece 스크립트안에 해당 테트리미노를 할당해주고, spawn 위치도 저장해준다.
        
        if (IsValidPosition(activePiece, spawnPosition)) {
            Set(this.activePiece); // piece 에 할당된 정보를 화면에 띄어준다.
        } else {
            GameOver();
            return;
        }
    }

    public void GameOver() {
        gameManager.modeGameOver();
        tilemap.ClearAllTiles();
        // Do anything else you want on game over here..
    }

    // Next Piece를 화면에 보여주는 메서드
    public void init_NextPiece() {       

        // next_tetrominos 리스트에 블록들에 대한 데이터 할당.
        for(int i = 0; i < 6; i++) {
            int random = Random.Range(0, this.tetrominos.Length); // 무작위 숫자 할당. 끝값은 제외되므로, 0부터 6의 데이터가 나옴.
            TetrominoData data = this.tetrominos[random]; // i, o, z 중 무작위의 테트로미노가 나온다.
            next_tetrominos.Add(data);

            set_NextPiece(data, i);            
        }

        newSpawnPiece();
    }

    public void set_NextPiece(TetrominoData tetrominoData, int tetromino_num) {
        for(int i = 0; i < tetrominoData.cells.Length; i++) {
            Vector3Int tilePosition = ((Vector3Int)tetrominoData.cells[i]) + next_tetriminos_position[tetromino_num]; // 테트리미노의 각 타일의 위치는 기존 위치 + spqwn position으로 해준다.
            this.tilemap.SetTile(tilePosition, tetrominoData.tile); // setTile 즉, tile을 화면에 띄어준다.
        }
    }

    // Next_tetrominos의 타일들을 다 지우는 메서드
    public void clear_NextPiece() {

        // 모든 netx_tetrominos에 대하여...
        for(int i = 0; i < next_tetrominos.Count; i++) {

            TetrominoData tetrominoData = next_tetrominos[i];

            // 각 tetromino의 cell들에 할당된 tile을 지워준다.
            for(int j = 0; j < tetrominoData.cells.Length; j++) {
                Vector3Int tilePosition = ((Vector3Int)tetrominoData.cells[j]) + next_tetriminos_position[i]; // 테트리미노의 각 타일의 위치는 기존 위치 + spqwn position으로 해준다.
                this.tilemap.SetTile(tilePosition, null); // setTile - null. 해당 위치에 있는 타일을 없애준다.                                        
            }
        }
    }

    public void hold_piece(TetrominoData tetrominoData) {

        //이미 Hold가 눌린적이 있으면 hold 기능은 동작 안한다.
        if(isHoldBtn) {
            img_hold_ban.SetActive(true);
            Invoke("hold_ban",2f);
            return;          
        }

        isHoldBtn = true;
        
        if(hold_tetromino.HasValue) {
            TetrominoData removeTetromino = (TetrominoData) hold_tetromino;
            for(int i = 0; i < removeTetromino.cells.Length; i++) {
                Vector3Int tilePosition = ((Vector3Int)removeTetromino.cells[i]) + hold_tetriminos_position; // 테트리미노의 각 타일의 위치는 기존 위치 + spqwn position으로 해준다.
                this.tilemap.SetTile(tilePosition, null); // setTile - null. 해당 위치에 있는 타일을 없애준다.                                                        
            }             
        }

        if(!hold_tetromino.HasValue) {
            newSpawnPiece();          
        } else {
            replaceSpawnPiece();
        }
                          
    
        hold_tetromino = tetrominoData;
        for(int i = 0; i < tetrominoData.cells.Length; i++) {
            Vector3Int tilePosition = ((Vector3Int)tetrominoData.cells[i]) + hold_tetriminos_position; // 테트리미노의 각 타일의 위치는 기존 위치 + spqwn position으로 해준다.
            this.tilemap.SetTile(tilePosition, tetrominoData.tile); // setTile 즉, tile을 화면에 띄어준다.   
        }
    }

    public void hold_ban() {
        img_hold_ban.SetActive(false);
    }


    // TileMap에 Tile을 그려주는 메서드
    public void Set(Piece piece) {
        if(isGameOver) {
            return;          
        }
        

        for(int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + piece.position; // 테트리미노의 각 타일의 위치는 기존 위치 + spqwn position으로 해준다.
            this.tilemap.SetTile(tilePosition, piece.data.tile); // setTile 즉, tile을 화면에 띄어준다.
        }
    }

    // TileMap에 Tile을 지워주는 메서드
    public void Clear(Piece piece) {
        for(int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + piece.position; // 테트리미노의 각 타일의 위치는 기존 위치 + spqwn position으로 해준다.
            this.tilemap.SetTile(tilePosition, null); // setTile - null. 해당 위치에 있는 타일을 없애준다.
        }
    }

    // 타당성 검사를 하는 메서드.
    public bool IsValidPosition(Piece piece, Vector3Int position) {

        RectInt bounds = this.Bounds;
        
        for(int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + position;

            // 테트리스 판안에 해당 타일이 있는지 없는지 판단. 없으면 유효하지 않은 것
            if(!bounds.Contains((Vector2Int) tilePosition)) {
                return false;              
            }

            // 해당 위치에 이미 타일이 존재하면, 유효하지 않은 것.
            if(this.tilemap.HasTile(tilePosition)) {
                return false;              
            }            
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;

        lineCount = 0;

        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row)) {
                LineClear(row);
                lineCount++;
            } else {
                row++;
            }
        }

        lineClearCount += lineCount;

        // 라인클리어 수에 따라서 콤보를 0으로 초기화 해주거나, comboCount를 1 증가시켜준다.
        if(lineCount != 0) {
            if(isPreviousLineClear) {
                comboCount++;         
            }
            isPreviousLineClear = true;             
        } else {
            comboCount = 0;
            isPreviousLineClear = false;
        }

        gameManager.printScoreEvent(lineCount, comboCount);

        // Debug.Log("콤보 카운트 : "+comboCount);
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position)) {
                return false;
            }
        }

        return true;
    }

    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // Clear all tiles in the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Shift every row above down one
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }

    // 경험치를 더해주고, 조건에 따라 블록 스피드를 증가시켜준다.
    public void add_exper(int line) {
        for(int i = 0; i < line; i++) {
            exper++;
            if(exper >= 10) {
                change_blockSpeed(block_speed+1);
                exper = 0;
            }
        }
        gameManager.print_exper();
    }


    // 블록 스피드를 변화시켜준다.
    public void change_blockSpeed(int speed) {
        block_speed = speed; // 블록스피드를 변화시켜준다.
        activePiece.chang_stepDelay(block_speed); // 블럭이 떨어지는 속도를 조정해준다.
    }

    public void add_score(int softCount, int hardCount) {
        int gainScore = 0;
        gainScore += softCount + (hardCount * 2); // 1. softCount와 hardCount를 더해준다.


        int lineClearScore = 0;
        switch (lineCount) {
            case 1:
                lineClearScore = 100 * block_speed;
                break;
            case 2:
                lineClearScore = 300 * block_speed;
                break;
            case 3:
                lineClearScore = 500 * block_speed;
                break;
            case 4:
                lineClearScore = 800 * block_speed;
                break;
        }

        gainScore += lineClearScore;
        gainScore += 50 * comboCount * block_speed;
        
        Debug.Log("soft drop : "+softCount+" , hard drop :"+hardCount +", level : "+block_speed
        +" , lineClearScore : "+lineClearScore+" , comboCount : "+comboCount+ ", gaine score : "+gainScore);

        gameManager.print_score(gainScore);
        add_exper(lineCount); // 점수 계산 후, 경험치를 증가시켜준다.

        //

    }

    public void test_piece(int tetromino_num) {
        Clear(this.activePiece);
        TetrominoData data = this.tetrominos[tetromino_num]; // i, o, z... 중의 테트로미노가 나온다.

        activePiece.cells = null;
        activePiece.Initialize(this, spawnPosition, data); // piece 스크립트안에 해당 테트리미노를 할당해주고, spawn 위치도 저장해준다.
        Set(this.activePiece); // piece 에 할당된 정보를 화면에 띄어준다.
    }
}
