using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multi_piece : MonoBehaviour
{
    public Multi_board board;
    public TetrominoData data { get; private set; }
    // 해당 Tetromino에 대한 좌표정보. TetrominoData 클래스에도 좌표정보가 있지만, Tile이 Vectoc3에 대한 정보를 받기 때문에 다시 설정해준다.
    public Vector3Int[] cells { get; set; } 
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    private float stepDelay = 1f;
    public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float moveTime;
    private float lockTime;

    private int softCount;
    private int hardCount;



    public void Initialize(Multi_board board, Vector3Int position, TetrominoData data) {
        
        this.data = data;
        this.board = board;
        this.position = position;
        this.rotationIndex = 0;

        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

        softCount = 0;
        hardCount = 0;
        
        if (cells == null) {
            cells = new Vector3Int[data.cells.Length]; // 메모리 할당 (모든 미노의 크기는 4이긴 함)
        }

        for (int i = 0; i < cells.Length; i++) {
            cells[i] = (Vector3Int)data.cells[i]; // 해당 테트리미노의 좌표값들을 할당해준다. 이유는 Tile이 Vector3를 해당 정보로 받기 때문이다.
        }

        
        
    }
    
    private void Start() {
        
    }

    public void Update() {

            
        if(this.board.isPlayerGameOver) {
            return;          
        }

        if(this.board.isGameOver) {
          return;
        }

        if(!this.board.multiTetrisManager.isMultiplayGameStart) {
            return;          
        }

        this.board.Clear(this); // 떨어지고 있는 미노들의 타일을 모두 지워준다.

         lockTime += Time.deltaTime;
        

        if(Input.GetKeyDown(KeyCode.LeftShift)) {
            Hold();
        }

        // 스페이스바를 누르면 범위 또는 해당 타일이 있는 곳까지 내려간다.
        if(Input.GetKeyDown(KeyCode.Space)) {
            HardDrop();                                  
        }

        if(Input.GetKeyDown(KeyCode.Z)) {
            Rotate(-1);         
        } else if(Input.GetKeyDown(KeyCode.X)) {
            Rotate(1);             
        }

        // 아이템전이면
        
        if(this.board.multiTetrisManager.isItemMode()) {


            // 유저 숫자 누르기 검사
            if(Input.GetKeyDown(KeyCode.Alpha1)) {
                this.board.multiTetrisManager.select_player_person_num(1);          
            } else if(Input.GetKeyDown(KeyCode.Alpha2)) {
                this.board.multiTetrisManager.select_player_person_num(2);
            } else if(Input.GetKeyDown(KeyCode.Alpha3)) {
                this.board.multiTetrisManager.select_player_person_num(3);
            } else if(Input.GetKeyDown(KeyCode.Alpha4)) {
                this.board.multiTetrisManager.select_player_person_num(4);
            } else if(Input.GetKeyDown(KeyCode.Alpha5)) {
                this.board.multiTetrisManager.select_player_person_num(5);
            } else if(Input.GetKeyDown(KeyCode.Alpha6)) {
                this.board.multiTetrisManager.select_player_person_num(6);
            }

            // 아이템 사용 여부 검사
            if(Input.GetKeyDown(KeyCode.Q)) {
              this.board.multiTetrisManager.use_item();
            }
          
        }
        

        // Advance the piece to the next row every x seconds

                // Allow the player to hold movement keys but only after a move delay
        // so it does not move too fast
        if (Time.time > moveTime) {
            HandleMoveInputs();
        }

        // Advance the piece to the next row every x seconds
        if (Time.time > stepTime) {
            Step();
        }

        if(!this.board.isPlayerGameOver || !this.board.isGameOver ||
            this.board.multiTetrisManager.isMultiplayGameStart) {
        
            this.board.Set(this); // 다시 타일을 그려준다.          
        }
        
        
    } 

    private void Hold() {
        board.hold_piece(this.data); 
    }

    
    private void HandleMoveInputs()
    {
        // Soft drop movement
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (Move(Vector2Int.down)) {
                // Update the step time to prevent double movement
                stepTime = Time.time + stepDelay;
                softCount++;
            }
        }

        // Left/right movement
        if (Input.GetKey(KeyCode.LeftArrow)) {
            Move(Vector2Int.left);
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            Move(Vector2Int.right);
        }
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;


        // Step down to the next row
        Move(Vector2Int.down);

        // Once the piece has been inactive for too long it becomes locked
        if (lockTime >= lockDelay) {
            Lock();
        }
    }

    private void Lock()
    {
        // Debug.Log("soft count :"+softCount+" , hard count : "+hardCount);
        board.Set(this);
        board.ClearLines();
        board.get_tile_info(false); // 타일맵의 정보 추출.
        


        board.newSpawnPiece();
        board.isHoldBtn = false; // hold 버튼 초기화.
    }

    private void HardDrop() {

        // Move 메서드가 유효하지 않은 움직임을 출력해주기 전까지 밑으로 계혹 내린다.
        while (Move(Vector2Int.down))
        {
            hardCount++;
            continue;            
        }

        // Debug.Log("hardCount : "+hardCount);
    }

    private bool Move(Vector2Int translation) {
        
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.IsValidPosition(this, newPosition); // 해당 움직임이 유효한 것인가를 판단해준다.

        // 유효하다면 위치를 옮겨준다.
        if(valid) {
            this.position = newPosition;          
            moveTime = Time.time + moveDelay;
            lockTime = 0f; // reset
        }

        return valid; // 유효함을 출력해준다. (HardDrop에서 쓰임)
    }

    // 회전을 시켜주는 메서드. 1은 오른쪽방향, -1은 왼쪽 방향으로 회전시켜준다.
    private void Rotate(int direction){
        
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

            if (Move(translation)) {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0) {
            wallKickIndex--;
        }

        // 0부터 7까지의 범위로 wallKickIndex를 조정해준다.
        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    // 인덱스의 범위를 벗어나면, 그걸 다시 인덱스의 범위로 포함시켜주는 알고리즘. 이해는 나중에 하자.
    private int Wrap(int input, int min, int max)
    {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }

    public void chang_stepDelay(int speed) {
        stepDelay = (float)(1.0 - (float) (speed-1) / 19);
        // Debug.Log("stepDelay : "+stepDelay);
    }

    

}
