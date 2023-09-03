using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FreeNet;
using FreeNetUnity;
using MyTetrisServer;

public class GameManager : MonoBehaviour
{
        // 싱글톤 패턴을 사용하기 위한 인스턴스 변수
    private static GameManager _instance;
    // 인스턴스에 접근하기 위한 프로퍼티
    public static string userId = null; // 유저의 id
    public delegate void on_scene_message(PROTOCOL protocol, string jsonData);
    public on_scene_message scene_callback;

    public CFreeNetUnityService gameserver{get; private set;}
	string received_msg;
    public bool isGameStart;

    public static GameManager Instance
    {
        get {
            // 인스턴스가 없는 경우에 접근하려 하면 인스턴스를 할당해준다.
            if(!_instance)
            {
                _instance = FindObjectOfType(typeof(GameManager)) as GameManager;

                if (_instance == null)
                    Debug.Log("no Singleton obj");
            }
            // Debug.Log("instance 호출");
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        // 인스턴스가 존재하는 경우 새로생기는 인스턴스를 삭제한다.
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        // 아래의 함수를 사용하여 씬이 전환되더라도 선언되었던 인스턴스가 파괴되지 않는다.
        DontDestroyOnLoad(gameObject);

        init_network();
    }


    void init_network() {
        this.received_msg = "";

		// 네트워크 통신을 위해 CFreeNetUnityService객체를 추가합니다.
		this.gameserver = gameObject.AddComponent<CFreeNetUnityService>();

		// 상태 변화(접속, 끊김등)를 통보 받을 델리게이트 설정.
		this.gameserver.appcallback_on_status_changed += on_status_changed;

		// 패킷 수신 델리게이트 설정.
		this.gameserver.appcallback_on_message += on_message;

    }

    // Use this for initialization
	void Start()
	{
		connect();
	}

	void connect()
	{
		this.gameserver.connect("127.0.0.1", 7979);
	}

	/// <summary>
	/// 네트워크 상태 변경시 호출될 콜백 매소드.
	/// </summary>
	/// <param name="server_token"></param>
	void on_status_changed(NETWORK_EVENT status)
	{
		switch (status)
		{
				// 접속 성공.
			case NETWORK_EVENT.connected:
				{
					CLogManager.log("on connected");
					this.received_msg += "on connected\n";

					
				}
				break;

				// 연결 끊김.
			case NETWORK_EVENT.disconnected:
				CLogManager.log("disconnected");
				this.received_msg += "disconnected\n";
				break;
		}
	}

	void on_message(CPacket msg)
	{
		// 제일 먼저 프로토콜 아이디를 꺼내온다.
		PROTOCOL protocol_id = (PROTOCOL)msg.pop_int32();
        string jsonData = msg.pop_string_many();
        
        if (protocol_id != PROTOCOL.SEND_TILE_MAP_INFO_ACK && protocol_id != PROTOCOL.REFRESH_GAME_ROOM_ACK)
        {
            Debug.Log("수신 유형 " + protocol_id);
            Debug.Log("json 데이터 " + jsonData);
        }

        scene_callback(protocol_id, jsonData);

	}

    public void send_packet<T> (PROTOCOL protocol, T data){

        if (protocol != PROTOCOL.SEND_TILE_MAP_INFO_REQ && protocol != PROTOCOL.REFRESH_GAME_ROOM_REQ)
        {
            Debug.Log("발신 유형 " + protocol);
            Debug.Log("json 데이터 " + data.ToString());
        }

        // Debug.Log("send packet. protocol : "+protocol+" , data : "+DataForJson<T>.get_toJson(data));

        // 헤더사이트 4바이트..
        CPacket cPacket = CPacket.create((int) protocol);
        
        // Debug.Log("buffer에 넣을 데이터 사이즈 : "+Encoding.UTF8.GetBytes(DataForJson<T>.get_toJson(data)).Length);

        cPacket.push_many(DataForJson<T>.get_toJson(data));
        this.gameserver.send(cPacket);
    }

	public void send(CPacket msg)
	{
		this.gameserver.send(msg);
	}





    // 씬을 로드해주는 메서드
    public void LoadScene(string sceneName)
    {
        // Debug.Log("씬 이름 : "+sceneName);
        SceneManager.LoadScene(sceneName);
        // StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    // 씬을 종료하는 메서드
    public void UnloadScene(string sceneName)
    {
        // Debug.Log("씬 종료");
        SceneManager.UnloadSceneAsync(sceneName);
    }

    // 게임을 종료하는 메서드
    public void ExitGame()
    {
        Application.Quit();
    }

    public void set_userId(string userId) {
        GameManager.userId = userId;
    }

    public string get_userId() {
        return GameManager.userId;
    }
}
