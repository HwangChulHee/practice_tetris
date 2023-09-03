using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class singleLobby : MonoBehaviour
{
    /* 상단 UI 관련 변수들 */

    public Text txt_id;
    public Button btn_myInfo;
    public Button btn_messanger;
    public Button btn_back;

    private void Awake() {
        findButton();
        set_userId(); // 아이디를 설정해줌.

        btn_back.onClick.AddListener(go_GameSelect);
    }
   
   private void findButton() {
        Transform img_group = GameObject.Find("Canvas").transform.GetChild(1); 
        int count = img_group.childCount;
        // Debug.Log(count);
        for(int i = 0; i < count; i++) {
            Transform btn = img_group.GetChild(i);
            int index = i;
            btn.GetComponent<Button>().onClick.AddListener(()=>OnButtonClick(index));
        }
   }

   public void OnButtonClick(int level) {
        switch(level) {
            case 0:
                GameManager.Instance.LoadScene("sprint");
                break;
            case 1:
                GameManager.Instance.LoadScene("timeAttack");
                break;
            case 2:
                GameManager.Instance.LoadScene("maraton");
                break;
            case 3:
                // GameManager.Instance.LoadScene("practice");
                break;
        }
    
    }

    public void set_userId() {
        Debug.Log("id : "+GameManager.Instance.get_userId());
        txt_id.text = GameManager.Instance.get_userId();
    }

    public void go_GameSelect() {
        GameManager.Instance.LoadScene("GameSelect");
    }


}
