using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyTetrisServer;

public class multiLobby : MonoBehaviour
{
    /* 상단 UI 관련 변수들 */

    public Text txt_id;
    public Button btn_myInfo;
    public Button btn_messanger;
    public Button btn_back;
    

     /* 멀티 로비 관련 변수들 */

     // 멀티 로비 상단 - 방만들기, 새로고침, 방 목록 필터링
     public GameObject group_lobby_top; // 방만들기, 새로고침, 방 목록 필터링을 해주는 버튼을 모아놓은 그룹
     public Button btn_go_create_room; // 방 만드는 버튼
     public Button btn_refresh; // 방의 목록들을 갱신해주는 버튼
     public Dropdown drop_mode; // 방의 목록들을 필터링 해주는 버튼(전체, 아이템전, 노템전)

     
     // 멀티 로비 중단 - 방 목록들을 보여줌
     public GameObject group_lobby_middle; // 방 목록들을 보여주는 그룹
     public List<GameObject> group_room; // 방 목록들의 리스트


     
     // 멀티 로비 하단 - 이전, 다음, 현재페이지와 전체페이지
     public GameObject group_lobby_bottom; // 이전, 다음, 현재페이지와 전체페이지를 보여주는 그룹
     public Button btn_previous; // 이전 버튼
     public Text txt_page; // 현재 페이지와 전체페이지를 보여주는 텍스트
     public Button btn_next; // 다음 버튼


     // 방 생성 관련 객체 
     public GameObject panel_create_room; // 방 생성이 생기는 패널
     public GameObject group_crate_room; // 방 생성시 띄어지는 그룹
     
     public InputField input_roomName; // 방 제목 입력창
     public Dropdown drop_create_mode; // 방 생성시 선택할 옵션에 대한 드랍다운
     
     public Toggle toggle_isPassword; // 비밀방 생성 여부 토글창
     public GameObject group_password; // 비밀번호 텍스트 및 입력 그룹
     public InputField input_password; // 비밀번호 입력창
     
     public Button btn_roomCancel; // 방생성 취소
     public Button btn_roomCreate; // 방생성 

    
    // 방 출입 금지 관련 객체
    public GameObject panel_ban_room; 
    public GameObject group_ban_room;
    public Text txt_ban_room;
    public Button btn_ban_room;

    // 비밀번호 관련 객체
    public GameObject panel_password_check;
    public GameObject group_password_check;
    public InputField input_password_check; // 비밀번호 입력
    public Text txt_password_check; // 비밀번호 옳고 그름 여부 관련 텍스트
    public Button btn_password_check_cancel; // 취소 버튼
    public Button btn_password_check_check; // 확인 버튼

    private int select_password_room_index; // 비밀번호 방을 눌렀을 시의 index




     // 기타 변수
     int current_page; // 현재 페이지를 나타내준다.
     int entry_page; // 전체 페이지를 나타내준다.
     List<RoomData> lobby_room; // 현재 로비에 보이는 룸들에 대한 정보다.
     private int request_room_mode; // 요청하는 방의 모드


    private void Awake() {

        lobby_room = new List<RoomData>();

        // 수신 시 콜백 메서드 설정
        GameManager.Instance.scene_callback = on_message;

        set_userId();      
        btn_back.onClick.AddListener(go_GameSelect);   // 뒤로가기(게임 선택 - 싱글 또는 멀티 창으로)

        /* 멀티 로비 상단 버튼 및 드랍다운 */
        btn_go_create_room.onClick.AddListener(go_create_room); // 방 생성 메서드
        btn_refresh.onClick.AddListener(refresh_room_list); // 방 새로고침 메서드
        drop_mode.onValueChanged.AddListener(change_room_list_by_mode); // 방 종류 필터링 메서드
        

        /* 멀티 로비 중단 버튼 */
        for(int i = 0; i < group_room.Count; i++) {
            GameObject room = group_room[i];
            int index = i;
            room.SetActive(false);
            room.GetComponent<Button>().onClick.AddListener(() => enter_room(index)); // 방 선택 후, 입장 메서드
        }


        /* 멀티 로비 하단 버튼 */
        btn_previous.onClick.AddListener(go_page_to_preivous); // 이전 방 목록 메서드
        btn_next.onClick.AddListener(go_page_to_next); // 다음 방 목록 메서드



        /* 방 생성 관련 버튼, 토글 */

        toggle_isPassword.onValueChanged.AddListener(showOrHidden_password);
        btn_roomCancel.onClick.AddListener(cancel_crate_room);
        btn_roomCreate.onClick.AddListener(create_room);

        
        /* 방 삭제 관련 버튼 및 해당 객체 비활성화 */
        btn_ban_room.onClick.AddListener(check_remove_room);

        panel_ban_room.SetActive(false);
        group_ban_room.SetActive(false);


        /* 비밀번호 입력 확인 창 관련 객체*/        
        
        panel_password_check.SetActive(false);
        group_password_check.SetActive(false);

        btn_password_check_cancel.onClick.AddListener(cancel_password_check);
        btn_password_check_check.onClick.AddListener(check_password_check);

        // 요청하는 방의 모드 (0 = 전체로 설정)
        request_room_mode = 0;
    }

    private void Start() {

        current_page = 1; // 현재 페이지를 1페이지로 설정한다.
        entry_page = 1; // 전체 페이지를 1페이지로 설정한다.
        txt_page.text = current_page.ToString() +" / "+ entry_page.ToString();

        //멀티 로비 입장
        GameManager.Instance.send_packet(
            PROTOCOL.ENTER_MULTI_LOBBY_REQ, new UserIdData(GameManager.Instance.get_userId())
        );      
        
        InvokeRepeating("refresh_room_list", 0f, 7f);

        // +a 페이징도 해줘야한다.
            
    }



    /// <summary>
    //멀티 로비 상단, 하단, 중단 관련 메서드
    /// </summary>
    

    // 방 생성 메서드
    private void go_create_room() {
        group_crate_room.SetActive(true);
        panel_create_room.SetActive(true);
    }

    // 방 새로고침 메서드
    private void refresh_room_list() {
        
        GameManager.Instance.send_packet(
            PROTOCOL.REFRESH_GAME_ROOM_REQ, 
            new RoomRefreshReqData(this.current_page, drop_mode.value));

    }

   

    // 방 종류 필터링 메서드
    private void change_room_list_by_mode(int value) {
        Debug.Log("drop 다운 선택된 값 : "+value);
        refresh_room_list();
    }

    // 방 선택 후, 입장 메서드
    private void enter_room(int room_index) {

        RoomData roomData = lobby_room[room_index];

        if(roomData.isPassword) {
          
          panel_password_check.SetActive(true);
          group_password_check.SetActive(true);

          select_password_room_index = room_index; 

          return;

        } else {
            GameManager.Instance.send_packet(PROTOCOL.ENTER_GAME_ROOM_REQ , lobby_room[room_index]);   
        }


        
    }

    // 이전 방 목록 메서드
    private void go_page_to_preivous() {
        if(current_page <= 1) {
        
          return;
        
        } else {
            GameManager.Instance.send_packet(
                PROTOCOL.REFRESH_GAME_ROOM_REQ, 
                new RoomRefreshReqData(this.current_page-1, drop_mode.value));
        }
    }

    // 다음 방 목록 메서드
    private void go_page_to_next() {
        GameManager.Instance.send_packet(
            PROTOCOL.REFRESH_GAME_ROOM_REQ, 
            new RoomRefreshReqData(this.current_page+1, drop_mode.value));
    }





    /// <summary>
    //방 생성시 생기는 메서드
    /// </summary>


    // 방 생성 시 비밀방 여부를 할건지 체크하는 토글
    private void showOrHidden_password(bool isPassword){
        
        // 비밀방을 만들것이라는 토글을 누르면 비밀번호 입력창을 활성화해준다.
        if(isPassword) {
            group_password.SetActive(true);
        } else {
            input_password.text = null;
            group_password.SetActive(false);
        }
    }    



    // 방 생성하는 메서드 (서버 통신). 씬 전환도 이루어져야함.
    private void create_room() {
        
        // 입력 정보 가져오기
        string roomName = input_roomName.text.Trim();
        int roomMode = drop_create_mode.value;
        bool isPassword = toggle_isPassword.isOn;
        string password = input_password.text.Trim();

        // 방 제목을 입력안하면 리턴
        if(string.IsNullOrEmpty(roomName)){
            return;          
        }

        // 서버 통신 (방 생성)
        GameManager.Instance.send_packet(
            PROTOCOL.CREATE_GAME_ROOM_REQ , 
            new RoomData(roomName, roomMode, isPassword, password));
        
        // 이후 서버의 RoomData도 수정해줘야한다.


        // 씬 전환
        
        // Debug.Log("roomName : "+roomName+", roomMode : "+roomMode+", isPassword : "+isPassword+", password : "+password);
        GameManager.Instance.LoadScene("tetris_multi");
    }


    // 방 만드는거 취소하는 메서드.
    private void cancel_crate_room() {
        input_roomName.text = null;
        drop_create_mode.value = 0;
        toggle_isPassword.isOn = false;
        input_password.text = null;

        group_crate_room.SetActive(false);
        panel_create_room.SetActive(false);
    }

 




    /// <summary>
    //서버를 통해 방의 정보를 갱신해주는 메서드
    /// </summary>
    /// 
    

    public void set_room_list(RoomRefreshAckData roomRefreshAckData) {

        // 페이지에 대한 요청이 타당하지 않으면 리턴.
        if(!roomRefreshAckData.isValid) {
            return;          
        }
        

        set_inactive_room_list(); // 룸들 비활성화
        
        Debug.Log("set room list 메서드 호출");
        lobby_room.Clear();
        for(int i = 0; i < roomRefreshAckData.roomDatas.Count; i++) {
            RoomData roomData = roomRefreshAckData.roomDatas[i];
            lobby_room.Add(roomData);
            set_room_info(i, roomData);            
        }

        // 현재 페이지, 전체 페이지 갱신
        this.current_page = roomRefreshAckData.current_page;
        this.entry_page = roomRefreshAckData.entry_page;

        string ack_current_page = roomRefreshAckData.current_page.ToString();
        string ack_entry_page = roomRefreshAckData.entry_page.ToString();

        txt_page.text = ack_current_page+" / "+ack_entry_page;      
    }

     private void set_inactive_room_list() {
        
        for(int i = 0; i < group_room.Count; i++) {
            GameObject room = group_room[i]; 
            room.SetActive(false);                        
        }

    }

     public void set_room_info(int room_index, RoomData roomData) {
        
        // 해당 방 가져오기
        GameObject room = group_room[room_index]; 
        room.SetActive(true);


        // 방 제목 
        GameObject txt_room_name = room.transform.Find("txt_room_name").gameObject;
        txt_room_name.GetComponent<Text>().text = "방 제목 : "+roomData.room_name;


        // 방 모드 (아이템전, 노템전)
        GameObject txt_mode = room.transform.Find("txt_mode").gameObject;
        if(roomData.room_mode == 0) {
            txt_mode.GetComponent<Text>().text = "모드 : 노템전";          
        } else if(roomData.room_mode == 1) {
            txt_mode.GetComponent<Text>().text = "모드 : 아이템전";
        }
        

        
        // 방 인원 수
        GameObject txt_people = room.transform.Find("txt_people").gameObject;
        txt_people.GetComponent<Text>().text = "인원 : "+roomData.room_people.ToString();

        
        // 비밀번호 방 여부에 따라 해당 이미지 활성화 / 비활성화
        if(!roomData.isPassword) {
            GameObject image = room.transform.Find("Image").gameObject;
            image.SetActive(false);          
        }
        
    }



    public void on_message(PROTOCOL protocol, string jsonData) {
        Debug.Log("유형 " + protocol);
        Debug.Log("json 데이터 " + jsonData);
            

		// 프로토콜에 따른 분기 처리.
		switch (protocol)
		{   
            // 방 갱신 응답
			case PROTOCOL.REFRESH_GAME_ROOM_ACK:
                RoomRefreshAckData roomRefreshAckData = DataForJson<RoomRefreshAckData>.get_fromJson(jsonData);
                set_room_list(roomRefreshAckData);
				break;

            // 방 입장 응답
            case PROTOCOL.ENTER_GAME_ROOM_ACK:
                UserIdData userIdData = DataForJson<UserIdData>.get_fromJson(jsonData);
                enter_game_room(userIdData);
                break;

		}
    }

    private void enter_game_room(UserIdData userIdData) {
        
        if(userIdData.id.Equals("approve")) {
            
            GameManager.Instance.LoadScene("tetris_multi");          
            return;
        
        } else if(userIdData.id.Equals("started")) {
            
            txt_ban_room.text = "이미 시작되었습니다.";

        } else if(userIdData.id.Equals("full")) {
            
            txt_ban_room.text = "가득찼습니다.";

        } else if(userIdData.id.Equals("reject")) {

            txt_ban_room.text = "이미 삭제되었습니다.";            

        } else {
            Debug.Log("방 금지 오류");
            return;
        }

        panel_ban_room.SetActive(true);
        group_ban_room.SetActive(true);
        refresh_room_list();
    }

    private void check_remove_room() {
        panel_ban_room.SetActive(false);
        group_ban_room.SetActive(false);        
    }

    /* 비밀번호 관련 메서드 */

    private void cancel_password_check() {
        
        // placeholder 초기화
        input_password_check.placeholder.GetComponent<Text>().text = null;

        panel_password_check.SetActive(false);
        group_password_check.SetActive(false);
        txt_password_check.gameObject.SetActive(false);

    }

    private void check_password_check() {
        RoomData roomData = lobby_room[select_password_room_index];

        // inputfield의 값 가져오고
        if(roomData.password.Equals(input_password_check.text)) {
            GameManager.Instance.send_packet(PROTOCOL.ENTER_GAME_ROOM_REQ , roomData);          
        } else {
            txt_password_check.gameObject.SetActive(true);
        }

    }




    /// <summary>
    /// 게임창 상단 관련 메서드
    /// </summary>
    public void set_userId() {
        txt_id.text = GameManager.Instance.get_userId();
    }

    public void go_GameSelect() {
        GameManager.Instance.LoadScene("GameSelect");
        GameManager.Instance.send_packet(PROTOCOL.EXIT_MULTI_LOBBY_REQ , new UserIdData(GameManager.Instance.get_userId()));
    }
}
