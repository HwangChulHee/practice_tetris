using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO.Compression;
using System.IO;

namespace MyTetrisServer
{
    public static class DataForJson<T>
    {
        public static T get_fromJson(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        public static string get_toJson(T data)
        {
            return JsonUtility.ToJson(data);
        }
    }

    [Serializable]
    public class UserIdData
    {
        public string id;

        public UserIdData(string id)
        {
            this.id = id;
        }
    }

    [Serializable]
    public class ChatData
    {
        public string id;
        public string msg;
        public ChatData(string id, string msg)
        {
            this.id = id;
            this.msg = msg;
        }
    }

    [Serializable]
    public class RoomData
    {
        public string room_id;  // 방의 고유한 id 클라이언트에서 생성해준다.
        public string room_name;  // room_name
        public int room_mode;  // room_mode (0: 노템전, 1: 아이템전)
        public bool isPassword;  // 비밀방 여부
        public string password; // 비밀번호
        public int room_people;

        public RoomData() {
            //
        }

        public RoomData(string room_name, int room_mode, bool isPassword, string password)
        {
            set_roomId();
            this.room_name = room_name;
            this.room_mode = room_mode;
            this.isPassword = isPassword;
            this.password = password;
        }

        public void set_roomId(){
            // 방장 이름 + 시간으로  room의 고유한 id 설정
            this.room_id = DateTime.Now.ToString("yyyyMMdd_hhmmss_"+GameManager.Instance.get_userId());
        }        
    }

    [Serializable]
    public class RoomRefreshReqData
    {
        public int request_page; // 요청한 페이지
        public int mode; // 0 - 전체, 1 - 아이템전, 2 - 노템전

        public RoomRefreshReqData(int request_page, int mode)
        {
            this.request_page = request_page;
            this.mode = mode;
        }
    }

    [Serializable]
    public class RoomRefreshAckData
    {
        public List<RoomData> roomDatas = new List<RoomData>();
        public int current_page; // 현재 페이지
        public int entry_page; // 전체 페이지
        public bool isValid; // 해당 요청이 타당한지의 여부

        public RoomRefreshAckData()
        {
        }

        public RoomRefreshAckData(List<RoomData> roomDatas, int current_page, int entry_page)
        {
            this.roomDatas = roomDatas;
            this.current_page = current_page;
            this.entry_page = entry_page;
        }

    }
    
    [Serializable]
    public class OfferRoomData
    {
        public RoomData roomData; // 룸 데이터
        public bool isHost; // 방장 여부
        public int player_team_num; // 플레이어의 팀 숫자. (1 ~ 6)
        public int player_person_num; // 플레이어의 개인 숫자. (1 ~ 6)
        public List<PlayerData> otherPlayers = new List<PlayerData>(); // 다른 플레이어의 정보

        public OfferRoomData(RoomData roomData, bool isHost, int player_team_num, int plyaer_person_num, List<PlayerData> otherPlayers)
        {
            this.roomData = roomData;
            this.isHost = isHost;
            this.player_team_num = player_team_num;
            this.player_person_num = plyaer_person_num;
            this.otherPlayers = otherPlayers;
        }
    }

    [Serializable]
    public class PlayerData
    {
        public string player_id; // 플레이어의 id
        public int player_team_num; // 플레이어의 팀 숫자. (1 ~ 6)
        public int player_person_num; // 플레이어의 개인 숫자. (1 ~ 6)

        public PlayerData(string player_id, int player_team_num, int plyaer_person_num)
        {
            this.player_id = player_id;
            this.player_team_num = player_team_num;
            this.player_person_num = plyaer_person_num;
        }
    }

    [Serializable]
    public class CoordinateColorData
    {
        public int[] vector;
        public string color_name;

        public CoordinateColorData(int[] vector, string color_name)
        {
            this.vector = vector;
            this.color_name = color_name;
        }

    }

    public class TileMapInfo
    {
        public string tilemap_user_id;
        // 좌표인 vector 값과 해당 vector의 tile의 색깔(스프라이트 이름)
        public List<CoordinateColorData> vector_tileColor = new List<CoordinateColorData>();

        public TileMapInfo()
        {
        }

        public TileMapInfo(string tilemap_user_id, List<CoordinateColorData> vector_tileColor)
        {
            this.tilemap_user_id = tilemap_user_id;
            this.vector_tileColor = vector_tileColor;   
            
        }


    }


    public class TargetData
    {
        public string target_id;
        

        public TargetData(string target_id)
        {
            this.target_id = target_id;
        }
    }

    public class GarbageLineReqData
    {
        public string target_id; // 타겟의 id
        public int count_garbage_line; // 보낼 가비지 라인 수

        public GarbageLineReqData(string target_id, int count_garbage_line)
        {
            this.target_id = target_id;
            this.count_garbage_line = count_garbage_line;
        }
    }


     public class GarbageLineAckData
    {
        public string target_id; // 타겟의 id
        public int count_garbage_line; // 가비지 라인 수
        public List<int[]> vector_list; // int 배열인 벡터 리스트

        public GarbageLineAckData(string target_id, int count_garbage_line, List<int[]> vector_list)
        {
            this.target_id = target_id;
            this.count_garbage_line = count_garbage_line;
            this.vector_list = vector_list;
        }
    }

    public class GameOverData
    {
        public bool isWin; // 승리 여부

        public GameOverData(bool isWin)
        {
            this.isWin = isWin;
        }
    }
}