using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FreeNet;
using FreeNetUnity;
using MyTetrisServer;

public class login : MonoBehaviour
{
    /* UI 관련 변수들 */
    public GameObject login_group; // 로그인에 필요한 기능을 묶어놓은 객체

    public InputField input_id; // 아이디 입력 창
    public InputField input_password; // 비밀번호 입력 창

    public Button btn_login; // 로그인 버튼
    public Button btn_searchPassword; // 비밀번호 찾기 버튼
    public Button btn_sign; // 회원가입 버튼

    public GameObject afterLogin_panel; // 이메일 유효성, 중복, 인증번호 발송과 비밀번호 일치여부와 회원가입을 띄어주는 panel
    public GameObject afterLogin_group; // 이메일 유효성, 중복, 인증번호 발송과 비밀번호 일치여부와 회원가입을 띄어주는 객체
    public Text txt_afterLogin; // 여기에 여러가지 문구가 적힌다.
    public Button btn_afterLogin; // 알림에 대한 확인 버튼

    private void Awake() {
        
        btn_login.onClick.AddListener(do_login);
        btn_searchPassword.onClick.AddListener(go_searchPassword);
        btn_sign.onClick.AddListener(go_sign);

        btn_afterLogin.onClick.AddListener(do_afterLogin);
    }

    // 로그인 시도하는 버튼
    private void do_login() {
        string id = input_id.text.Trim();
        string password = input_password.text.Trim();

        if(string.IsNullOrEmpty(id)) {
            open_afterLogin("아이디를 입력해주세요.");                      

        } else if(string.IsNullOrEmpty(password)) {
            open_afterLogin("비밀번호를 입력해주세요.");

        } else {
            StartCoroutine(HTTP_login(id, password)); // 사용할 곳에 적절히 배치
        }
        
    }

    

    IEnumerator HTTP_login(string id, string password) {
        WWWForm form = new WWWForm();
        form.AddField("id", id); // 전송할 key와 data값 작성
        form.AddField("password", password); // 전송할 key와 data값 작성
    
        UnityWebRequest www = UnityWebRequest.Post("https://booksns.tk/tetris/loginSign/login.php", form);
        yield return www.SendWebRequest();
    
        if(www.result == UnityWebRequest.Result.ProtocolError) {
            Debug.Log("HTTP 통신 에러 : "+www.error);
        }
        else {
            Debug.Log("Form upload complete!");
            Debug.Log("파싱 후 데이터 : "+www.downloadHandler.text);
            jsonParsing_login(www.downloadHandler.text);
            www.Dispose(); // 메모리 해제
        }
    }
    
    [System.Serializable]

    //받아올 데이터의 형식을 작성
    public class LoginData {
        
        public string msg;
        public int isLogin;
    }

    void jsonParsing_login(string json){
        LoginData data = JsonUtility.FromJson<LoginData>(json);

        if (data == null) {
            Debug.Log("JsonUtility.FromJson 메서드 오류일 확률 높음. json 값 : "+json);
            return;
        }
            
        // 배열이 아닌 값 파싱
        string msg = data.msg; 
        int isLogin = data.isLogin;
        // Debug.Log("isLogin : "+isLogin);

        if(isLogin == 1) {
          GameManager.Instance.set_userId(input_id.text.Trim());
          GameManager.Instance.LoadScene("GameSelect");

          GameManager.Instance.send_packet(PROTOCOL.SEND_USER_ID, new UserIdData(input_id.text.Trim()));

        } else {
            open_afterLogin(msg);
        }
        
    }

    
    // 비밀번호 찾기로 이동하는 버튼
    private void go_searchPassword() {
        GameManager.Instance.LoadScene("password");
    }

    // 회원가입으로 이동하는 버튼
    private void go_sign() {
        GameManager.Instance.LoadScene("sign");
    }

     public void open_afterLogin(string msg) {
        afterLogin_panel.SetActive(true);
        afterLogin_group.SetActive(true);
        login_group.SetActive(false);

        txt_afterLogin.text = msg;
    }

    // afterSign 창을 닫아주는 메서드
    public void do_afterLogin() {
        afterLogin_panel.SetActive(false);
        afterLogin_group.SetActive(false);
        login_group.SetActive(true);
    }
}
