using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyTetrisServer;
using UnityEngine.Tilemaps;

public class multiTetris : MonoBehaviour
{
    /* UI 관련 변수들 */
    public Text txt_room_name; // 방 이름
    public Text txt_room_mode; // 방 모드
    public GameObject group_password; // 패스워드 그룹
    public Text txt_room_password; // 방 비밀번호

    public Button btn_start; // 시작하기 버튼 (방장만 보임)
    public Button btn_exit; // 나가기 버튼

    public GameObject panel_afterRemove; // 게임방 종료시 띄울 패널
    public GameObject group_afterRemove; // 게임방 종료시 띄울 화면
    public Button btn_afterRemove; // 게임방 종료시 (호스트가 나가서) 확인 버튼

    public GameObject panel_rejectStart; // 게임 시작 거부 시, 띄울 패널
    public GameObject group_rejectStart; // 게임 시작 거부 시, 띄울 화면
    public Button btn_rejectStart; // 게임 시작 거부 시, (호스트가 나가서) 확인 버튼

    
    
    public GameObject group_teamSelect;
    public Button btn_team_1; // 팀 선택 버튼 1
    public Button btn_team_2; // 팀 선택 버튼 2
    public Button btn_team_3; // 팀 선택 버튼 3
    public Button btn_team_4; // 팀 선택 버튼 4
    public Button btn_team_5; // 팀 선택 버튼 5
    public Button btn_team_6; // 팀 선택 버튼 6

    public Text txt_scoreEvent; // 라인클리어 이벤트를 띄어주는 텍스트

    public GameObject panel_playerGameOver; // 플레이어의 게임 종료시 띄울 패널
    public GameObject group_playerGameOver; // 플레이어의 게임 종료시 띄울 화면 

    public Button btn_playerGameOver_remain; // 플레이어 게임 종료 시 구경하기 버튼
    public Button btn_playerGameOver_exit; // 플레이어 게임 종료 시 나가기 버튼 

    public GameObject panel_gameOver; // 게임 종료시 띄울 패널
    public GameObject group_gameOver; // 게임 종료시 띄울 화면

    public Button btn_gameOver_replay; // 게임 종료시 다시하기 버튼
    public Button btn_gameOver_exit; // 게임 종료시 나가기 버튼




    /* 각종 변수들 */

    // 멀티 보드 객체
    public Multi_board multi_Board;

    // 자신 자신에 대한 정보들
    public GameObject myPlayerUi;

    // 방에 대한 정보
    RoomData roomData;

    // 게임 시작 정보
    public bool isMultiplayGameStart;
    
    // 다른 플레이어의 정보들
    class OtherPlayerData {
        public bool isPlayerDead;
        public string plyaer_id;
        public int team_num;
        public int person_num;
        public int ui_num; // 해당 ui의 숫자
        public GameObject ui_player; // 플레이어의 아이디가 할당되는 ui (패널 색깔, id, 플레이어의 team 할당)
        public Tilemap tilemap;

        public OtherPlayerData(string plyaer_id, int team_num, int person_num, 
                                int ui_num, GameObject ui_player, Tilemap tilemap) {
            
            this.isPlayerDead = false;
            this.plyaer_id = plyaer_id;
            this.team_num = team_num;
            this.person_num = person_num;
            this.ui_num = ui_num+1;
            this.ui_player = ui_player;
            this.tilemap = tilemap;
        }
    }
    // 다른 플레이어에 관한 정보. key 값은 plyaer의 id값이다.
    private Dictionary<string, OtherPlayerData> other_player_list = new Dictionary<string, OtherPlayerData>(); 
    // 플레이어가 사용하는 ui의 사용여부
    public Dictionary<GameObject, bool> isPlayerUiUsed = new Dictionary<GameObject, bool>(); 
    public List<GameObject> playerUi_list; // 플레이어의 아이디가 할당되는 ui (패널 색깔, id, 플레이어의 team 할당)
    public List<Tilemap> otherPlayer_tilemap_list; // 다른 플레이어의 타일맵 리스트
    public TetrominoData[] tetrominos; // i, o, z, s 등.. 7개를 할당.

    int my_person_num; // 내 플레이어 숫자
    int my_team_num; // 내 팀 숫자.
    string target_block_id; // 블록 타겟의 id
    string target_item_id; // 아이템 타겟의 id
    bool isHost; // 호스트 여부


    /*아이템 관련 변수 */
    public class Tetris_item {
        public string item_name; // 아이템의 이름 : 플러스(plus), 마이너스(minus), 올클리어(clear), 교환(change), 시야가리기(dark)
        public Tile item_tile; // 아이템의 타일

        public Tetris_item(string item_name, Tile item_tile) {
            this.item_name = item_name;
            this.item_tile = item_tile;
        }
    }

    public Tilemap tilemap_item;

    public Tile tile_plus;
    public Tile tile_minus;
    public Tile tile_clean;
    public Tile tile_change;
    public Tile tile_dark;
    private List<Tetris_item> item_list; // 유저가 가지고 있는 아이템 목록

    public List<Image> dark_image_list;

    


   private void Awake() {

     // 수신 시 콜백 메서드 설정
    GameManager.Instance.scene_callback = on_message;

    offer_room_info_req(); // 방 정보 제공해달라는 요청

    // ui 할당. 처음에는 할당이 안되었으니 모두 false로.
    for(int i = 0; i < playerUi_list.Count; i++) {
        isPlayerUiUsed.Add(playerUi_list[i], false);
        playerUi_list[i].SetActive(false);
    }

    // 호스트가 나갔을 때 활성화되는 객체들
    group_afterRemove.SetActive(false); 
    panel_afterRemove.SetActive(false);

    // 호스트가 나갔을 때 누르는 버튼
    btn_afterRemove.onClick.AddListener(exit_room);

    // 시작하기 버튼 눌렀을 때
    btn_start.onClick.AddListener(start_game);

    // 나가기 버튼 눌렀을 때
    btn_exit.onClick.AddListener(exit_room);

    // 팀 선택 버튼 눌렀을 때
    btn_team_1.onClick.AddListener(() => req_click_my_team_select_btn(1));
    btn_team_2.onClick.AddListener(() => req_click_my_team_select_btn(2));
    btn_team_3.onClick.AddListener(() => req_click_my_team_select_btn(3));
    btn_team_4.onClick.AddListener(() => req_click_my_team_select_btn(4));
    btn_team_5.onClick.AddListener(() => req_click_my_team_select_btn(5));
    btn_team_6.onClick.AddListener(() => req_click_my_team_select_btn(6));

    // 라인 클리어 이벤트는 처음에는 아무것도 없음.
    txt_scoreEvent.text = null;

    // 플레이어의 게임 종료시 활성화 되는 객체들
    group_playerGameOver.SetActive(false);
    panel_playerGameOver.SetActive(false);

    // 플레이어의 게임 종료시 버튼들
    btn_playerGameOver_remain.onClick.AddListener(remain_room);
    btn_playerGameOver_exit.onClick.AddListener(exit_room);


    // 게임 종료시 활성화 되는 객체들
    group_gameOver.SetActive(false);
    panel_gameOver.SetActive(false);

    // 게임 종료 후, 다시하기 및 나가기
    btn_gameOver_replay.onClick.AddListener(replay_game);
    btn_gameOver_exit.onClick.AddListener(exit_room);

    // 게임 시작 거부시 활성화 되는 객체들
    panel_rejectStart.SetActive(false);
    group_rejectStart.SetActive(false);
    btn_rejectStart.onClick.AddListener(inactive_reject);

    //테트로미노에 대한 값 초기화
    for(int i = 0; i < this.tetrominos.Length; i++) {
            this.tetrominos[i].Initialize(); //i, o, z, s 등의 좌표를 초기화            
    }

    // 블록 및 아이템 타겟, 게임오버 오브젝트를 비활성화
    for(int i = 0; i < playerUi_list.Count; i++) {
        playerUi_list[i].transform.Find("target_block").gameObject.SetActive(false);
        playerUi_list[i].transform.Find("target_item").gameObject.SetActive(false);
        playerUi_list[i].transform.Find("player_game_over").gameObject.SetActive(false);
    }

    // 아이템 초기화
    item_list = new List<Tetris_item>();
    
   }



    // 방 정보 제공해달라는 요청
   private void offer_room_info_req() {
    GameManager.Instance.send_packet(PROTOCOL.OFFER_ROOM_INFO_REQ , new UserIdData(GameManager.Instance.get_userId()));
   }



    // 수신 시 콜백 메서드
    public void on_message(PROTOCOL protocol, string jsonData) {
        
           

		// 프로토콜에 따른 분기 처리.
		switch (protocol)
		{   
            // 방 갱신 응답
			case PROTOCOL.OFFER_ROOM_INFO_ACK:
                OfferRoomData offerRoomData = DataForJson<OfferRoomData>.get_fromJson(jsonData);
                set_room_info(offerRoomData);
				break;
            
            // 플레이어가 들어왔을 때, ui 갱신            
            case PROTOCOL.ENTER_PLAYER_ACK:
                PlayerData playerData = DataForJson<PlayerData>.get_fromJson(jsonData);
                set_other_player_info(playerData);
                break;

            // 플레이어가 나갔을 때, ui 갱신            
            case PROTOCOL.EXIT_GAME_ROOM_ACK:
                UserIdData userIdData = DataForJson<UserIdData>.get_fromJson(jsonData);
                update_other_player_info(userIdData.id);
                break;

            // 방장 양도에 대한 응답
            case PROTOCOL.ASSIGN_HOST_ACK:
                ack_assign_host();
                break;

            // 다른 플레이어가 팀을 바꿨을 때에 대한 응답
            case PROTOCOL.CHANGE_TEAM_ACK:
                PlayerData playerData2 = DataForJson<PlayerData>.get_fromJson(jsonData);
                ack_team_select_btn(playerData2);
                break;

            // 게임 시작했다는 응답
            case PROTOCOL.START_GAME_ACK:
                UserIdData userIdData2 = DataForJson<UserIdData>.get_fromJson(jsonData);
                ack_started_game(userIdData2);
                break;

            // 다른 플레이어의 타일 맵에 대한 응답
            case PROTOCOL.SEND_TILE_MAP_INFO_ACK:
                TileMapInfo tileMapInfo = DataForJson<TileMapInfo>.get_fromJson(jsonData);
                ack_tilemap_info(tileMapInfo);
                break;

            // 타겟 블록에 설정에 대한 응답
            case PROTOCOL.SET_TARGET_BLOCK_ACK:
                TargetData targetData = DataForJson<TargetData>.get_fromJson(jsonData);
                set_target_block(targetData);
                break;

            // 타겟 아이템 설정에 대한 응답
            case PROTOCOL.SET_TARGET_ITEM_ACK:
                TargetData targetData1 = DataForJson<TargetData>.get_fromJson(jsonData);
                set_target_item(targetData1);
                break;

            // 가비지 라인 요청에 대한 응답
            case PROTOCOL.SEND_GARBAGE_LINE_ACK:
                GarbageLineAckData garbageLineAckData = DataForJson<GarbageLineAckData>.get_fromJson(jsonData);
                ack_garbage_line(garbageLineAckData);
                break;

            // 플레이어의 게임 종료에 대한 응답(상대)
            case PROTOCOL.PLAYER_GAME_OVER_ACK:
                UserIdData userIdData1 = DataForJson<UserIdData>.get_fromJson(jsonData);
                ack_other_player_game_over(userIdData1);
                break;
            
            // 게임 종료에 대한 응답
            case PROTOCOL.GAME_OVER_ACK:
                GameOverData gameOverData = DataForJson<GameOverData>.get_fromJson(jsonData);
                ack_game_over(gameOverData);
                break;

            // 아이템-교환에 대한 응답
            case PROTOCOL.ITEM_CHANGE_ACK:
                TileMapInfo tileMapInfo1 = DataForJson<TileMapInfo>.get_fromJson(jsonData);
                ack_use_item_change(tileMapInfo1);
                break;

            // 아이템-다크, 화면 가리기에 대한 응답
            case PROTOCOL.ITEM_DARK_ACK:
                ack_item_dark();
                break;


		}
    }

    // 방에 관한 정보를 설정해준다. (제목, 모드, 비밀번호, 시작버튼)
    private void set_room_info(OfferRoomData offerRoomData) {
        this.roomData = offerRoomData.roomData;

        RoomData roomData = offerRoomData.roomData;
        bool isHost = offerRoomData.isHost;
        this.isHost = isHost;
        int team_num = offerRoomData.player_team_num;
        int person_num = offerRoomData.player_person_num;
        List<PlayerData> otherPlayerData_list = offerRoomData.otherPlayers;
        
        
        txt_room_name.text = roomData.room_name;
        
        if(roomData.room_mode == 0) {
            txt_room_mode.text = "노템전";          
        } else if(roomData.room_mode == 1) {
            txt_room_mode.text = "아이템전";
        }

        if(roomData.isPassword) {
            group_password.SetActive(true);
            txt_room_password.text = roomData.password;
        } else {
            group_password.SetActive(false);
        }

        // 방장 여부에 따라 시작버튼을 활성화 시켜준다.
        if(isHost) {
            btn_start.gameObject.SetActive(true);
        } else {
            btn_start.gameObject.SetActive(false);
        }

        // 본인의 아이디와 팀 숫자, 팀 색깔에 따른 값을 할당해준다.
         
        Transform panelForm = myPlayerUi.transform.Find("Panel");
        Transform userIdForm = myPlayerUi.transform.Find("txt_userId");
        Transform userNumForm = myPlayerUi.transform.Find("txt_userNum");

        userIdForm.GetComponent<Text>().text = GameManager.Instance.get_userId(); // 아이디 할당
        this.my_team_num = team_num;
        this.my_person_num = person_num;
        // userNumForm.GetComponent<Text>().text = person_num.ToString(); // 유저의 개인번호 할당. 본인의 개인번호는 필요없으므로 입력해주지 않는다.
        
        // 팀 색깔에 따라 패널 색깔 할당
        change_team_color(panelForm, team_num);

        // 본인의 게임 오버 여부는 false로 해준다.
         Transform gameOverForm = myPlayerUi.transform.Find("player_game_over");
         gameOverForm.gameObject.SetActive(false);
    

        // 다른 유저들에 대한 정보를 설정해준다.
        Debug.Log("otherPlayers 정보 : "+otherPlayerData_list.Count);
        for(int i = 0; i < otherPlayerData_list.Count; i++) {
            Debug.Log("otherPlayer 정보 id : "+otherPlayerData_list[i].player_id+", team 정보 : "+otherPlayerData_list[i].player_team_num);
            PlayerData otherPlayerData = otherPlayerData_list[i];
            set_other_player_info(otherPlayerData);
        }
    }

    public bool isItemMode() {
        // 아이템전은 1의 값
        if(this.roomData.room_mode == 1) {
            return true;
        }

        return false;
    }

    private void set_other_player_info(PlayerData playerData){

        // 할당할 ui를 가져와주고
        GameObject otherPlayerUi = null;
        int ui_num = -1;
        for(int i = 0; i < playerUi_list.Count; i++) {
        
            // 그에 해당하는 키 값이 없으면
            if(!isPlayerUiUsed.ContainsKey(playerUi_list[i])) {
                Debug.Log("값 할당 문제");
                return;            
            }

            bool isUsed = isPlayerUiUsed[playerUi_list[i]]; // 사용 여부를 가져오고
            if(!isUsed) {
                // 해당 ui를 가져오고, 반복문을 종료한다.
                otherPlayerUi = playerUi_list[i];
                isPlayerUiUsed[playerUi_list[i]] = true;
                ui_num = i; // ui의 숫자에 해당하는 값을 넣어준다.
                break;          
            }
        }

        // plyaer 데이터를 OtherPlayerData로 변환한 뒤 딕셔너리에 저장해주고
        OtherPlayerData otherPlayerData = new OtherPlayerData(playerData.player_id, playerData.player_team_num, 
                                                            playerData.player_person_num, ui_num, 
                                                            otherPlayerUi, otherPlayer_tilemap_list[ui_num]);
        other_player_list.Add(playerData.player_id, otherPlayerData);

        Debug.Log("player id : "+playerData.player_id+", player_team_num : "+playerData.player_team_num+" player_person_num : "+playerData.player_person_num);

        // 해당 ui에 값을 저장해준다.
        Transform panelForm = otherPlayerUi.transform.Find("Panel");
        Transform userIdForm = otherPlayerUi.transform.Find("txt_userId");
        // Transform userNumForm = otherPlayerUi.transform.Find("txt_userNum");

        otherPlayerUi.transform.Find("target_block").gameObject.SetActive(false);
        otherPlayerUi.transform.Find("target_item").gameObject.SetActive(false);


        Debug.Log("다른 사람의 id, team_num, person_num : "+playerData.player_id+ " , "+playerData.player_team_num+" , "+playerData.player_person_num);

        userIdForm.GetComponent<Text>().text = otherPlayerData.plyaer_id; // 아이디 할당
        // userNumForm.GetComponent<Text>().text = otherPlayerData.person_num.ToString(); // 개인 숫자 할당
        
        // 팀 색깔에 따라 패널 색깔 할당
        change_team_color(panelForm, otherPlayerData.team_num);
        
        otherPlayerUi.SetActive(true);
    }

    private void update_other_player_info(string player_id) {
        OtherPlayerData exitPlayerData = other_player_list[player_id];

        exitPlayerData.ui_player.SetActive(false);
        clear_other_tile(exitPlayerData.tilemap); // 다른 플레이어의 타일맵을 모두 지워준다.
        isPlayerUiUsed[exitPlayerData.ui_player] = false;
        other_player_list.Remove(player_id);
    }

    private void clear_other_tile(Tilemap tilemap) {
         BoundsInt bounds = tilemap.cellBounds;

        // Iterate through each cell in the Tilemap
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                tilemap.SetTile(cellPosition, null); // Set the tile at the cell position to null
            }
        }
    }

    // 방장 양도시 발생하는 메서드
    private void ack_assign_host() {
        Debug.Log("방장 할당 요청");
        if(!isMultiplayGameStart) {
            btn_start.gameObject.SetActive(true);          
        }
        
        this.isHost = true;
    }

    // 호스트가 나갔을 때 호출되는 메서드
    private void show_remove_screen() {
            
        group_afterRemove.SetActive(true);
        panel_afterRemove.SetActive(true);
        
    }


    // 자신이 팀 선택 버튼을 눌렀을 때 발생되는 메서드
    private void req_click_my_team_select_btn(int team_num) {
        Transform panelForm = myPlayerUi.transform.Find("Panel");
        Transform userNumForm = myPlayerUi.transform.Find("txt_userNum");
    
        // 팀 색깔에 따라 패널 색깔 할당
        change_team_color(panelForm, team_num);

        this.my_team_num = team_num;
        

        Debug.Log("my person num : "+this.my_person_num);
        GameManager.Instance.send_packet(
            PROTOCOL.CHANGE_TEAM_REQ , 
            new PlayerData(GameManager.Instance.get_userId(), team_num, this.my_person_num));

    }

    // 다른 사람이 팀 버튼을  눌렀을 때
    private void ack_team_select_btn(PlayerData playerData) {
        string changed_person_id = playerData.player_id;
        int change_team_num = playerData.player_team_num;

        OtherPlayerData changed_person_data = other_player_list[changed_person_id];
        GameObject changed_person_ui = changed_person_data.ui_player;
        changed_person_data.team_num = change_team_num;

        Transform panelForm = changed_person_ui.transform.Find("Panel");
        // 팀 색깔에 따라 패널 색깔 할당
        change_team_color(panelForm, change_team_num);
        
    }

    // 팀 숫자에 따라 패널의 색깔을 바꿔주는 메서드
    private void change_team_color(Transform panelForm, int change_team_num) {
        switch(change_team_num) {
            case 1:
                panelForm.GetComponent<Image>().color = new Color32(255,102,102,255); // 빨강
                break;
            case 2:
                panelForm.GetComponent<Image>().color = new Color32(255,178,102,255); // 주황
                break;
            case 3:
                panelForm.GetComponent<Image>().color = new Color32(255,255,102,255); // 노랑
                break;
            case 4:
                panelForm.GetComponent<Image>().color = new Color32(178,255,102,255); // 초록
                break;
            case 5:
                panelForm.GetComponent<Image>().color = new Color32(102,178,255,255); // 파랑
                break;
            case 6:
                panelForm.GetComponent<Image>().color = new Color32(178,102,255,255); // 보라
                break;
        }
    }

    // 게임 시작 시 호출되는 메서드
    private void start_game() {
                
        // 게임이 시작되었다고 다른 참가자들에게 알려준다.
        GameManager.Instance.send_packet(PROTOCOL.START_GAME_REQ , new UserIdData("게임 시작에 대한 요청"));
        
    }

    // 방장한테 게임이 시작되었다고 전달받은 상태. 
    private void ack_started_game(UserIdData userIdData) {
        if(userIdData.id.Equals("start_reject_by_own")) {
            // 게임 시작 거부

            panel_rejectStart.SetActive(true);
            group_rejectStart.SetActive(true);

            btn_start.gameObject.SetActive(true);            
          
          
        } else if(userIdData.id.Equals("start_approve_by_own")) {
            // 게임 시작 허용 (자기 자신)

            this.isMultiplayGameStart = true;
            this.multi_Board.isGameOver = false;
            this.multi_Board.isPlayerGameOver = false;

            // 모든 플레이어가 죽지 않았음을 나타내줌
            foreach (KeyValuePair<string, OtherPlayerData> entry in this.other_player_list) {
                entry.Value.isPlayerDead = false;
            }

            // 팀 선택 기능을 비활성화함
            group_teamSelect.SetActive(false);
            
            // 아이템 관련 객체 초기화
            if(isItemMode()) {
                this.item_list.Clear();
                this.tilemap_item.ClearAllTiles();              
            }

            panel_gameOver.SetActive(false);
            group_gameOver.SetActive(false);

            this.multi_Board.init_NextPiece();
            btn_start.gameObject.SetActive(false);

            // 타겟 설정 요청
            GameManager.Instance.send_packet(PROTOCOL.INIT_TARGET_BLOCK_REQ , new UserIdData("타겟 블록 요청"));

        } else if(userIdData.id.Equals("start_approve_by_other")) {
            // 게임 시작 허용 (방장)

            this.isMultiplayGameStart = true;
            this.multi_Board.isGameOver = false;
            this.multi_Board.isPlayerGameOver = false;

            // 모든 플레이어가 죽지 않았음을 나타내줌
            foreach (KeyValuePair<string, OtherPlayerData> entry in this.other_player_list) {
                entry.Value.isPlayerDead = false;
            }

            // 팀 선택 기능을 비활성화함
            group_teamSelect.SetActive(false);

            panel_gameOver.SetActive(false);
            group_gameOver.SetActive(false);

            replay_game();

            this.multi_Board.init_NextPiece();
            
            

            // 타겟 설정 요청
            GameManager.Instance.send_packet(PROTOCOL.INIT_TARGET_BLOCK_REQ , new UserIdData("타겟 블록 요청"));
        } 

        
    }



    // 게임 종료 시 이벤트

    // 게임이 종료되었다고 보내준다.
    public void req_end_game() {
        GameManager.Instance.send_packet(PROTOCOL.PLAYER_GAME_OVER_REQ, new UserIdData("게임 종료 관련 요청"));
    }

    /* 플레이어가 게임 오버되었을 때 */
    
    // 구경하기 옵션 선택 시 발생하는 메서드
    public void remain_room() {
        group_playerGameOver.SetActive(false);
        panel_playerGameOver.SetActive(false);
    }

    // 나가기 버튼을 눌렀을 때 발생하는 메서드
    private void exit_room() {
    
        // 서버 통신을 통해 나가고
        GameManager.Instance.send_packet(PROTOCOL.EXIT_GAME_ROOM_REQ , new UserIdData(GameManager.Instance.get_userId()));

        GameManager.Instance.isGameStart = false;
        // 씬을 이동해준다.
        GameManager.Instance.LoadScene("lobby_multi");

   }

    /* 게임 오버되었을 때 */
    
    // 다시하기 선택했을 때
    public void replay_game() {
        
        this.multi_Board.isGameOver = false;
        this.multi_Board.isPlayerGameOver = false;

        // 나의 tilemap 초기화
        this.multi_Board.tilemap.ClearAllTiles();
        this.multi_Board.ghost_tilemap.ClearAllTiles();
        myPlayerUi.transform.Find("player_game_over").gameObject.SetActive(false);

        // 다른 플레이어의 tilemap 초기화
        foreach (KeyValuePair<string, OtherPlayerData> entry in this.other_player_list)
        {
            entry.Value.tilemap.ClearAllTiles();
            entry.Value.ui_player.transform.Find("target_block").gameObject.SetActive(false);
            entry.Value.ui_player.transform.Find("target_item").gameObject.SetActive(false);
            entry.Value.ui_player.transform.Find("player_game_over").gameObject.SetActive(false);
        }

        // 아이템 관련 객체 초기화
        if(isItemMode()) {
            this.item_list.Clear();
            this.tilemap_item.ClearAllTiles();              
        }

        this.panel_gameOver.SetActive(false);
        this.group_gameOver.SetActive(false);

        if(isHost) {
            btn_start.gameObject.SetActive(true);
        }
    }


    
    // 구경하기 또는 나가기 옵션을 보여준다.
    public void show_gameOver_screen() {
        // 패널을 보여주고,
        group_gameOver.SetActive(true);
        panel_gameOver.SetActive(true);

        // 이후에 방안의 플레이어들에게 클리어된 보드를 전송하자.
    }

    public void show_playerGameOver_screen() {
        group_playerGameOver.SetActive(true);
        panel_playerGameOver.SetActive(true);
    }

    

    
    // 타일 좌표 값 보내는 메서드
    public void send_tileMap_info(TileMapInfo tileMapInfo) {

        GameManager.Instance.send_packet(PROTOCOL.SEND_TILE_MAP_INFO_REQ , tileMapInfo);
    }

    // 타일 좌표 좌표값을 받고 타일을 그려주는 메서드
    public void ack_tilemap_info(TileMapInfo tileMapInfo) {

        OtherPlayerData otherPlayerData = other_player_list[tileMapInfo.tilemap_user_id];
        Tilemap tilemap = otherPlayerData.tilemap;
        
        // 타일맵의 모든 타일을 초기화
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            tilemap.SetTile(pos, null);
        }
        
        // 보내준 좌표값에 따라서 순회해야함
        for(int i = 0; i < tileMapInfo.vector_tileColor.Count; i++) {
            int vector1 = tileMapInfo.vector_tileColor[i].vector[0];
            int vector2 = tileMapInfo.vector_tileColor[i].vector[1];
            int vector3 = tileMapInfo.vector_tileColor[i].vector[2];

            string color_name = tileMapInfo.vector_tileColor[i].color_name;
            int color_num = get_color_num(color_name);
            
            // Debug.Log("tetrominos 사이즈 : "+tetrominos.Length+" , color_num :"+color_num+" , "+color_name);

            tilemap.SetTile(new Vector3Int(vector1, vector2, vector3), tetrominos[color_num].tile);  
        }
        
    }

    public int get_color_num(string color_name) {
        int color_num = -1;
        
        switch (color_name)
            {
                case "Cyan":
                    color_num = 0;
                    break;
                
                case "Yellow":
                    color_num = 1;
                    break;
                
                case "Red":
                    color_num = 2;
                    break;
                
                case "Green":
                    color_num = 3;
                    break;

                case "Blue":
                    color_num = 4;
                    break;

                case "Orange":
                    color_num = 5;
                    break;

                case "Purple":
                    color_num = 6;
                    break;
                
                case "Ghost":
                    color_num = 7;
                    break;

                case "item_plus":
                    color_num = 8;
                    break;

                case "item_minus":
                    color_num = 9;
                    break;

                case "item_change":
                    color_num = 10;
                    break;

                case "item_clean":
                    color_num = 11;
                    break;

                case "item_dark":
                    color_num = 12;
                    break;
                
            }

            return color_num;
    }

    // 블록의 타겟 설정을 해주는 메서드
    private void set_target_block(TargetData targetData){

        // 타겟의 id를 설정해주고
        this.target_block_id = targetData.target_id;
        
        // id에 맞는 타겟의 이미지를 활성화해준다.
        GameObject otherPlayerUi = null;
        OtherPlayerData otherPlayerData = other_player_list[this.target_block_id];
        otherPlayerUi = otherPlayerData.ui_player;

        Transform blcokForm = otherPlayerUi.transform.Find("target_block");
        blcokForm.gameObject.SetActive(true); // 해당 오브젝트 활성화 

        foreach (KeyValuePair<string, OtherPlayerData> entry in this.other_player_list)
        {
            // 타겟의 오브젝트가 아니라면
            if (entry.Value.ui_player != otherPlayerUi)
            {
                GameObject notTargetUi = entry.Value.ui_player;
                notTargetUi.transform.Find("target_block").gameObject.SetActive(false);                
            }
        }
    }

    // 아이템의 타겟 설정을 해주는 메서드
    private void set_target_item(TargetData targetData) {
        
        if(!isItemMode()) {
            return;          
        }

        this.target_item_id = targetData.target_id;
         // id에 맞는 타겟의 이미지를 활성화해준다.
        GameObject otherPlayerUi = null;
        OtherPlayerData otherPlayerData = other_player_list[this.target_item_id];
        otherPlayerUi = otherPlayerData.ui_player;

        Transform itemForm = otherPlayerUi.transform.Find("target_item");
        itemForm.gameObject.SetActive(true); // 해당 오브젝트 활성화 
        

        foreach (KeyValuePair<string, OtherPlayerData> entry in this.other_player_list)
        {
            // 타겟의 오브젝트가 아니라면
            if (entry.Value.ui_player != otherPlayerUi)
            {
                GameObject notTargetUi = entry.Value.ui_player;
                notTargetUi.transform.Find("target_item").gameObject.SetActive(false);
            }
        }
    }

    // 가비지 라인을 요청하는 메서드
    public void send_garbage_line_req(int count_garbage_line) {
        
        GameManager.Instance.send_packet(
            PROTOCOL.SEND_GARBAGE_LINE_REQ , 
            new GarbageLineReqData(target_block_id, count_garbage_line));

        
    }

    // 가비지 라인 응답 메서드
    public void ack_garbage_line(GarbageLineAckData garbageLineAck) {
        
        // 가비지 라인의 수만큼 기존 타일을 올려주고 가비지 라인을 그려준다.
        multi_Board.LineUp(garbageLineAck);

        // 갱신된 자신의 tilemap을 새롭게 했다고 다른 사람들에게 알려준다.

    }

    // 플레이어의 게임 종료에 관한 요청
    public void send_player_game_over() {
        GameManager.Instance.send_packet(PROTOCOL.PLAYER_GAME_OVER_REQ , new UserIdData("게임 종료에 관한 요청"));
        
        // 게임 오버 이미지를 true로 해준다.
        Transform gameOverForm = myPlayerUi.transform.Find("player_game_over");
        gameOverForm.gameObject.SetActive(true);
    }


    // 다른 플레이어의 게임 종료에 관한 응답
    public void ack_other_player_game_over(UserIdData userIdData) {
        
        other_player_list[userIdData.id].isPlayerDead = true; // 플레이어가 죽었음을 표시 
        OtherPlayerData gameOverPlayerData = other_player_list[userIdData.id];
        Transform otherGameOverForm = gameOverPlayerData.ui_player.transform.Find("player_game_over");

        otherGameOverForm.gameObject.SetActive(true);
    }

    // 게임 종료에 관한 응답. 여기 자신의 팀의 승리와 패배에 관한 정보가 담겨야 한다.
    private void ack_game_over(GameOverData gameOverData) {
        
        this.multi_Board.isGameOver = true; // 게임 종료
        this.isMultiplayGameStart = false; // 게임 종료

        panel_gameOver.SetActive(true);
        group_gameOver.SetActive(true);
        
        panel_playerGameOver.SetActive(false);
        group_playerGameOver.SetActive(false);

        group_teamSelect.SetActive(true); // 팀 선택 활성화

        // ui 초기화
        // 다른 플레이어의 tilemap 초기화
        foreach (KeyValuePair<string, OtherPlayerData> entry in this.other_player_list)
        {
            entry.Value.ui_player.transform.Find("target_block").gameObject.SetActive(false);
            entry.Value.ui_player.transform.Find("target_item").gameObject.SetActive(false);
            entry.Value.ui_player.transform.Find("player_game_over").gameObject.SetActive(false);
        }


        // 아이템전일 시, 아이템 생성 메서드 중단
        if(isItemMode()) {
            multi_Board.stop_create_item();          
        }
        
        // 게임의 승리여부에 따라 ui의 메세지를 설정해줌.
        if(gameOverData.isWin) {
            // 게임에 승리했다면
            group_gameOver.transform.Find("txt_gameOver").GetComponent<Text>().text = "Win!";
            
        } else {
            // 패배했다면
            group_gameOver.transform.Find("txt_gameOver").GetComponent<Text>().text = "Lose!";
        }
        
    }


    /* 아이템전 관련 메서드 */
    
    // 아이템을 사용하기 위해 플레이어의 번호를 누를때 발생되는 메서드
    public void select_player_person_num(int ui_num) {

        // bool isValid = false;
        Debug.Log("아이템 타겟 설정- (선택한) person_num: "+ui_num+" , (내) team_num: "+my_team_num);

        foreach (KeyValuePair<string, OtherPlayerData> entry in other_player_list)
        {
            // 1) 선택한 플레이어의 번호와 상대의 번호가 같고 2) 팀 숫자가 다르고 3) 플레이어가 죽지 않았다면
            if(entry.Value.ui_num == ui_num 
                && entry.Value.team_num != this.my_team_num
                && !entry.Value.isPlayerDead) {
                // 유효한 선택이다.
                // isValid = true;
                Debug.Log("아이템 타겟 설정- person_num:"+ui_num+" , (상대) team_num"+entry.Value.team_num);
                this.target_item_id = entry.Key;

                // 이후 target_item_id를 요청해준다.
                GameManager.Instance.send_packet(PROTOCOL.SET_TARGET_ITEM_REQ , new TargetData(this.target_item_id));
            } 
        }
    }

    // 아이템 획득
    public void add_item(string item_name) {
        Tile item_tile = null;
        switch(item_name){
            
            case "item_plus":
                item_tile = this.tile_plus;
                break;

            case "item_minus":
                item_tile = this.tile_minus;
                break;
            
            case "item_change":
                item_tile = this.tile_change;
                break;

            case "item_clean":
                item_tile = this.tile_clean;
                break;

            case "item_dark":
                item_tile = this.tile_dark;
                break;
        }

        Tetris_item tetris_item = new Tetris_item(item_name, item_tile);
        if(item_list.Count < 11) {
            item_list.Add(tetris_item);

            // ui 업데이트
            tilemap_item.SetTile(new Vector3Int(-6+item_list.Count,0,0), item_tile);          
        }

    }
    
    // 아이템 사용
    public void use_item() {
        if(item_list.Count == 0) {
            return;          
        }
        Tetris_item tetris_Item = item_list[0];
        string item_name = tetris_Item.item_name;

        // 아이템에 따라서 패킷 안보내줘도 됨. (본인한테 쓰는거는 맵 갱신만, 상대에게 쓰는거는 패킷 보내기)
        switch(item_name){
            
            case "item_plus":
                use_item_plus();
                break;

            case "item_minus":
                use_item_minus();
                break;
            
            case "item_change":
                use_item_change();
                break;

            case "item_clean":
                use_item_clean();
                break;

            case "item_dark":
                use_item_dark();
                break;
        }


        // 아이템을 사용하면 리스트를 지워줌
        for(int i = 0; i < item_list.Count; i++) {
            tilemap_item.SetTile(new Vector3Int(-5+i,0,0), null);
        }


        item_list.RemoveAt(0);
        // ui 갱신
        for(int i = 0; i < item_list.Count; i++) {
            tilemap_item.SetTile(new Vector3Int(-5+i,0,0), item_list[i].item_tile);
        }
    }

    private void use_item_plus() {
        GameManager.Instance.send_packet(
            PROTOCOL.SEND_GARBAGE_LINE_REQ , 
            new GarbageLineReqData(this.target_item_id, 1));
    }

    private void use_item_minus() {
        this.multi_Board.LineClear(true, -10);
    }

    private void use_item_change() {
        
        // 내 타일맵의 정보를 가져온다.
        TileMapInfo myTileMapInfo = this.multi_Board.get_myTilemapInfo();
        myTileMapInfo.tilemap_user_id = target_item_id;

        // 상대에게 나의 타일맵 정보를 보내준다.
        GameManager.Instance.send_packet(PROTOCOL.ITEM_CHANGE_REQ , myTileMapInfo);

        Debug.Log("target_item_id : "+target_item_id);
        // 상대방의 타일맵 정보를 가져온다.
        TileMapInfo otherTilemapInfo = this.multi_Board.get_otherTilemapInfo(other_player_list[this.target_item_id].tilemap);

        // 내 타일맵에 상대방의 타일맵의 정보를 설정한다.
        this.multi_Board.set_myTileMap(otherTilemapInfo);


        // 갱신 정보를 알려준다.
        this.multi_Board.get_tile_info(true);               
    }

    private void ack_use_item_change(TileMapInfo otherTilemapInfo) {
        // 내 타일맵에 상대방의 타일맵의 정보를 설정한다.
        this.multi_Board.set_myTileMap(otherTilemapInfo);


        // 갱신 정보를 알려준다.
        this.multi_Board.get_tile_info(true);        
        
    }

   

    private void use_item_clean() {
        this.multi_Board.clean_allLine();         
    }

    private void use_item_dark() {
        GameManager.Instance.send_packet(PROTOCOL.ITEM_DARK_REQ , new UserIdData(this.target_item_id));        
    }

    private void ack_item_dark() {
        for(int i = 0; i < this.dark_image_list.Count; i++) {
            this.dark_image_list[i].gameObject.SetActive(true);            
        }
        Invoke("inactive_item_dark", 5f);
    }

    private void inactive_item_dark() {
        for(int i = 0; i < this.dark_image_list.Count; i++) {
            this.dark_image_list[i].gameObject.SetActive(false);            
        }
    }





   /*UI 관련 이벤트*/

   private void inactive_reject() {
        panel_rejectStart.SetActive(false);
        group_rejectStart.SetActive(false);
   }



    // 라인 클리어 이벤트를 띄어주는 메서드
   public void print_scoreEvent(int lineCount, int comboCount) {
        string lineText = "";
        string comboText = "";

        if(comboCount != 0) {
             comboText= " , "+comboCount.ToString()+" 콤보";
        }
        
        
        switch (lineCount) {
            case 1:
                lineText = "Single";
                break;
            case 2:
                lineText = "Double";
                break;
            case 3:
                lineText = "Triple";
                break;
            case 4:
                lineText = "Tetris";
                break;
        }

        string scoreEventText = lineText + comboText;
        txt_scoreEvent.text = scoreEventText;

     }
}
