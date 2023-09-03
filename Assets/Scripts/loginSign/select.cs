using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyTetrisServer;

public class select : MonoBehaviour
{
    /* 상단 UI 관련 변수들 */

    public Text txt_id;
    public Button btn_myInfo;
    public Button btn_messanger;


    /* select 씬의 UI 관련 변수들 */

    public GameObject group_select;
    public Button btn_single;
    public Button btn_multi;



    private void Awake() {
        set_userId(); // 아이디를 설정해줌.

        btn_myInfo.onClick.AddListener(show_myInfo);
        // btn_messanger.onClick.AddListener(show_messanger);
        
        btn_single.onClick.AddListener(go_single);
        btn_multi.onClick.AddListener(go_multi);
           
    }

    public void set_userId() {
        txt_id.text = GameManager.Instance.get_userId();
    }

    public void show_myInfo() {

    }

    public void show_messanger() {
        
    }

    // 싱글 플레이로 이동
    public void go_single() {
        GameManager.Instance.LoadScene("lobby_single");        
    }

    // 멀티 플레이로 이동
    public void go_multi() {
        GameManager.Instance.LoadScene("lobby_multi");        
    }


}
