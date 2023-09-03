
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MyTetrisServer;

public class Multi_board : MonoBehaviour
{
    public Tilemap tilemap {get; private set;}
    public Tilemap ghost_tilemap;
    public Multi_piece activePiece { get; private set; }
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

    public bool isPlayerGameOver; // 플레이어의 게임 오버
    public bool isGameOver; // 게임 전체 오버 종료

    public multiTetris multiTetrisManager;
    public GameObject img_hold_ban;
    public Tile ghost_tile;


    
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
        activePiece = GetComponent<Multi_piece>(); // 떨어질 조각을 만들어주는 스크립트 초기화

        isPlayerGameOver = false;
        
        block_speed = 1;
        exper = 0;

        for(int i = 0; i < this.tetrominos.Length; i++) {
            this.tetrominos[i].Initialize(); //i, o, z, s 등의 좌표를 초기화            
        }
    }
    private void Start() {
        
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
            Debug.Log("gameover newSpawnPiece");
            playerGameOver();
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
            Debug.Log("gameover replaceSpawnPiece");
            playerGameOver();
            return;
        }
    }

    public void playerGameOver() {
        this.isPlayerGameOver = true;
        // tilemap.ClearAllTiles();
        
        multiTetrisManager.show_playerGameOver_screen(); // 패널을 보여준다.
        multiTetrisManager.send_player_game_over(); // 게임 오버 관련 요청을 보내준다.
        this.stop_create_item(); // 아이템 생성을 멈춘다.
        
        
        // Do anything else you want on game over here..
    }

    // Next Piece를 화면에 보여주는 메서드
    public void init_NextPiece() {       

        // next_tetrominos 리스트에 블록들에 대한 데이터 할당.
        next_tetrominos.Clear();
        for(int i = 0; i < 6; i++) {
            int random = Random.Range(0, this.tetrominos.Length); // 무작위 숫자 할당. 끝값은 제외되므로, 0부터 6의 데이터가 나옴.
            TetrominoData data = this.tetrominos[random]; // i, o, z 중 무작위의 테트로미노가 나온다.
            next_tetrominos.Add(data);

            set_NextPiece(data, i);            
        }

        newSpawnPiece();
        
        // 아이템 모드면 아이템 생성 로직
        if(this.multiTetrisManager.isItemMode()) {
            InvokeRepeating("create_item", 0f, 3f); // 3초에 한번씩 아이템 생성 메서드 호출
            // InvokeRepeating("create_item", 0f, 4f); // 3초에 한번씩 아이템 생성 메서드 호출
        }
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
    public void Set(Multi_piece piece) {
        if(isPlayerGameOver || isGameOver || !this.multiTetrisManager.isMultiplayGameStart) {
            return;          
        }
        

        for(int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + piece.position; // 테트리미노의 각 타일의 위치는 기존 위치 + spqwn position으로 해준다.
            this.tilemap.SetTile(tilePosition, piece.data.tile); // setTile 즉, tile을 화면에 띄어준다.
        }
    }

    // TileMap에 Tile을 지워주는 메서드
    public void Clear(Multi_piece piece) {
        for(int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + piece.position; // 테트리미노의 각 타일의 위치는 기존 위치 + spqwn position으로 해준다.
            this.tilemap.SetTile(tilePosition, null); // setTile - null. 해당 위치에 있는 타일을 없애준다.
        }
    }

    // 타당성 검사를 하는 메서드.
    public bool IsValidPosition(Multi_piece piece, Vector3Int position) {

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
                LineClear(false, row);
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

        multiTetrisManager.print_scoreEvent(lineCount, comboCount);
        
        if(lineCount != 0 || comboCount != 0) {
            int comboCount_line = 0;
            switch(comboCount){
                case 1:
                    comboCount_line = 0;
                    break;
                
                case 2:
                case 3:
                    comboCount_line = 1;
                    break;

                case 4:
                case 5:
                    comboCount_line = 2;
                    break;

                case 6:
                case 7:
                    comboCount_line = 3;
                    break;

                case 8:
                case 9:
                case 10:
                    comboCount_line = 3;
                    break;                
            }

            if(comboCount >= 11) {
                comboCount_line = 4;              
            }

            multiTetrisManager.send_garbage_line_req(lineCount+comboCount_line); // 상대에게 가비지라인을 보내준다. 
            GameManager.Instance.send_packet(PROTOCOL.SET_TARGET_BLOCK_REQ , new UserIdData("타겟 블록 요청")); // 타겟 블록을 변경해준다.         
        }
        
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

    public void LineClear(bool isItemUse , int row)
    {
        // 아이템 얻기를 위한 타일맵의 정보 생성
        TileMapInfo tileMapInfo = new TileMapInfo();
        tileMapInfo.tilemap_user_id = GameManager.Instance.get_userId();


        RectInt bounds = Bounds;

        // 해당 줄을 없애준다.
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            
            TileBase tile = tilemap.GetTile(position);
            if (tile != null)
            {
                Sprite sprite = (tile as Tile).sprite;
                CoordinateColorData coordinateColorData = new CoordinateColorData(
                    new int[] {position.x, position.y, position.z }, sprite.name);
                
                tileMapInfo.vector_tileColor.Add(coordinateColorData);
            }
            
            tilemap.SetTile(position, null); // 해당 타일들을 없애준다.
        }


        // 아이템 모드면, 해당 row의 정보를 바탕으로 아이템을 추가해준다.
        if(this.multiTetrisManager.isItemMode() && !isItemUse) {
            get_item(tileMapInfo);
        }


        // 위의 줄을 아래로 내려준다.
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

    // 클리어한 라인의 타일맵 정보를 바탕으로 아이템을 추가해주는 메서드
    private void get_item(TileMapInfo tileMapInfo) {

        List<CoordinateColorData> vector_list = tileMapInfo.vector_tileColor;

        for(int i = 0; i < vector_list.Count; i++) {
            //해당 타일이 아이템 이면 추가함
            string sprite_name = vector_list[i].color_name;
            if(sprite_name == "item_plus" || sprite_name == "item_minus" || 
                sprite_name == "item_change" || sprite_name == "item_clean" ||
                sprite_name == "item_dark") 
            {
                Debug.Log("아이템로그-아이템 추가 : "+sprite_name);
                this.multiTetrisManager.add_item(sprite_name);
            }
        }
    }

    //
    public void LineUp(GarbageLineAckData garbageLineAckData) {
        
        int count_garbage_line = garbageLineAckData.count_garbage_line;
        
        RectInt bounds = Bounds;

        int max_row = bounds.yMax; 
        int min_row = bounds.yMin;

        // int row = max_row;

        Dictionary<Vector3Int, TileBase> existing_info = new Dictionary<Vector3Int, TileBase>();

        // 기존 타일의 가비지 라인 수만큼 더한 값을 리스트에 저장하고 삭제한다.
        // 이때 떨어지고 있는 조각의 좌표값은 배제해야한다.

        for(int row = max_row; min_row <= row  ; row--) {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row, 0);

                // 해당 자리에 떨어지고 있는 조각이 있다면 
                if(isPiecePosition(position)) {
                    continue;                  
                }

                TileBase under = tilemap.GetTile(position);
                tilemap.SetTile(position, null); // 기존 값을 삭제해준다.

                position = new Vector3Int(col, row+count_garbage_line, 0);
                existing_info.Add(position, under);
            }
        }

        // 저장한 값을 재배치 한다.
        foreach (var item in existing_info)
        {
            tilemap.SetTile(item.Key, item.Value);
        }
        


        // 그리고 올린 자리에 가비지 타일을 채워준다.
        // 하나를 제외하고
        
        int randomNumber = Random.Range(-5, 5);

        for(int row = bounds.yMin; row < bounds.yMin+count_garbage_line; row++) {
            for(int col = bounds.xMin; col < bounds.xMax; col++) {
                
                if(col != randomNumber) {
                    Vector3Int position = new Vector3Int(col, row, 0);
                    tilemap.SetTile(position, ghost_tile);                                      
                }
            }
        }

        // 다시 그려준다.
        get_tile_info(true);

        //일단 테스트부터 해보자.


    }

    private bool isPiecePosition(Vector3Int vectorPosition) {
        for(int i = 0; i < this.activePiece.cells.Length; i++) {

            Vector3Int tilePosition = this.activePiece.cells[i] + this.activePiece.position; // 테트리미노의 각 타일의 위치는 기존 위치 + spqwn position으로 해준다.
            if(vectorPosition == tilePosition) {
                return true; // 해당 위치에 piece가 존재함
            }
            
        }
        return false; // 해당 위치에 piece가 존재하지 않음                  
    }


    public void get_tile_info(bool isGarbage) 
    {

        TileMapInfo tileMapInfo = new TileMapInfo();
        tileMapInfo.tilemap_user_id = GameManager.Instance.get_userId();
        
        // Get the bounds of the tilemap
        RectInt bounds = Bounds;

        // Loop over all positions in the tilemap
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                
                //떨어지고 있는 조각이면 해당 정보를 할당하지 않는다.
                if(isGarbage) {
                    if(isPiecePosition(position)) {
                        continue;                  
                    }           
                }
                     
                TileBase tile = tilemap.GetTile(position);

                if (tile != null)
                {
                    // If the tile is not null, it exists at the current position
                    // Debug.Log("Tile exists at position " + position.ToString());

                    // Do something with the tile information, such as getting its sprite
                    Sprite sprite = (tile as Tile).sprite;
                    // Debug.Log("Tile sprite: " + sprite.name);

                    CoordinateColorData coordinateColorData = new CoordinateColorData(
                        new int[] {position.x, position.y, position.z }, sprite.name);
                    tileMapInfo.vector_tileColor.Add(coordinateColorData);

                    
                    // Debug.Log("Tile : " + DataForJson<TileMapInfo>.get_toJson(tileMapInfo));
                }
            }
        }
        
        
        
        this.multiTetrisManager.send_tileMap_info(tileMapInfo);

    }

    // 아이템 생성
    public void create_item() {

        if(isPlayerGameOver || isGameOver || !this.multiTetrisManager.isMultiplayGameStart) {
            return;          
        }

        
        // 1. 우선 타일들이 있는 지 확인. 없으면 return.
        
        TileMapInfo tileMapInfo = new TileMapInfo();
        tileMapInfo.tilemap_user_id = GameManager.Instance.get_userId();
        
        
        RectInt bounds = Bounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                
                //떨어지고 있는 조각이면 해당 정보를 할당하지 않는다.
                if(isPiecePosition(position)) {
                    continue;                  
                }           
                
                     
                TileBase tile = tilemap.GetTile(position);
                if (tile != null)
                {
                    Sprite sprite = (tile as Tile).sprite;

                    // 해당 타일이 아이템 이면 추가 안함.
                    if(sprite.name == "item_plus" || sprite.name == "item_minus" || 
                        sprite.name == "item_change" || sprite.name == "item_clean" ||
                        sprite.name == "item_dark") 
                    {
                      continue;
                    }

                    CoordinateColorData coordinateColorData = new CoordinateColorData(
                        new int[] {position.x, position.y, position.z }, sprite.name);
                    
                    tileMapInfo.vector_tileColor.Add(coordinateColorData);
                }
            }
        }


        // 2. 확률. 1/2 확률로 가자. 그리고 아이템 5개 중에 랜덤.
        int createRanNum = Random.Range(0, 2);
        if(createRanNum != 0) {
            // 0이 아니라면 리턴.
            return;          
        }
        int itemRanNum = Random.Range(8, 13);
        // itemRanNum = 12;
        // Debug.Log("아이템로그-아이템종류 : "+itemRanNum);




        // 3. 현재 블록들의 좌표값 중 랜덤 배치
        int block_count = tileMapInfo.vector_tileColor.Count;
        if(block_count == 0) {
            // 현재 블록의 좌표들이 없다면 return            
            // Debug.Log("아이템로그-좌표없음 : ");
            return;          
        }
        int blockRanNum = Random.Range(0, block_count);

        int vector1 = tileMapInfo.vector_tileColor[blockRanNum].vector[0];
        int vector2 = tileMapInfo.vector_tileColor[blockRanNum].vector[1];
        int vector3 = tileMapInfo.vector_tileColor[blockRanNum].vector[2];

        // Debug.Log("아이템로그-좌표값 : "+vector1+" , "+vector2+" , "+vector3);

        this.tilemap.SetTile(
            new Vector3Int(vector1, vector2, vector3), 
            this.multiTetrisManager.tetrominos[itemRanNum].tile);


        
        // 맵 정보 갱신        
        get_tile_info(true);
    }

    public void stop_create_item() {
        CancelInvoke("create_item");
    }

    public void clean_allLine() {
        TileMapInfo tileMapInfo = new TileMapInfo();
        tileMapInfo.tilemap_user_id = GameManager.Instance.get_userId();
        
        // Get the bounds of the tilemap
        RectInt bounds = Bounds;

        // Loop over all positions in the tilemap
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                
                //떨어지고 있는 조각이면 해당 정보를 할당하지 않는다.
                if(isPiecePosition(position)) {
                        continue;                  
                }   
                this.tilemap.SetTile(position, null);
            }
        }
        
    
        get_tile_info(true);
    }

    public TileMapInfo get_myTilemapInfo() {
        TileMapInfo tileMapInfo = new TileMapInfo();
        tileMapInfo.tilemap_user_id = GameManager.Instance.get_userId();
        
        // Get the bounds of the tilemap
        RectInt bounds = Bounds;

        // Loop over all positions in the tilemap
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                
                //떨어지고 있는 조각이면 해당 정보를 할당하지 않는다.
                if(isPiecePosition(position)) {
                        continue;                  
                }         
                
                     
                TileBase tile = tilemap.GetTile(position);

                if (tile != null)
                {
                    
                    Sprite sprite = (tile as Tile).sprite;
                    
                    CoordinateColorData coordinateColorData = new CoordinateColorData(
                        new int[] {position.x, position.y, position.z }, sprite.name);
                    tileMapInfo.vector_tileColor.Add(coordinateColorData);
                }
            }
        }

        return tileMapInfo;
    }

    public TileMapInfo get_otherTilemapInfo(Tilemap otherTilemap) {
        TileMapInfo tileMapInfo = new TileMapInfo();
        tileMapInfo.tilemap_user_id = GameManager.Instance.get_userId();
        
        // Get the bounds of the tilemap
        RectInt bounds = Bounds;

        // Loop over all positions in the tilemap
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                
                //떨어지고 있는 조각이면 해당 정보를 할당하지 않는다.
                if(isPiecePosition(position)) {
                        continue;                  
                }         
                
                     
                TileBase tile = otherTilemap.GetTile(position);

                if (tile != null)
                {
                    
                    Sprite sprite = (tile as Tile).sprite;
                    
                    CoordinateColorData coordinateColorData = new CoordinateColorData(
                        new int[] {position.x, position.y, position.z }, sprite.name);
                    tileMapInfo.vector_tileColor.Add(coordinateColorData);
                }
            }
        }

        return tileMapInfo;
    }


    public void set_myTileMap(TileMapInfo tileMapInfo) {
        
        RectInt bounds = Bounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                
                if(!isPiecePosition(position)) {
                    tilemap.SetTile(position, null);
                } 
            }
        }
        
        // 보내준 좌표값에 따라서 순회해야함
        for(int i = 0; i < tileMapInfo.vector_tileColor.Count; i++) {
            int vector1 = tileMapInfo.vector_tileColor[i].vector[0];
            int vector2 = tileMapInfo.vector_tileColor[i].vector[1];
            int vector3 = tileMapInfo.vector_tileColor[i].vector[2];

            string color_name = tileMapInfo.vector_tileColor[i].color_name;
            int color_num = this.multiTetrisManager.get_color_num(color_name);
            
            tilemap.SetTile(new Vector3Int(vector1, vector2, vector3), this.multiTetrisManager.tetrominos[color_num].tile);  
        }
    }



    

}
