using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class sign : MonoBehaviour
{
    /* UI 관련 변수들 */
    public GameObject sign_group; // 이메일, 인증, 비밀번호를 묶어놓은 객체

    public InputField input_email; // 이메일 입력 창
    public Button btn_send_auth; // 인증메인 전송 버튼

    public InputField input_auth_num; // 인증번호 입력 창
    public Button btn_check_auth; // 인증확인 버튼

    public InputField input_id; // 아이디 입력 창
    public Button btn_check_id; // 아이디 중복검사 버튼

    public InputField input_password; // 비밀번호 입력 창
    public InputField input_check_password; // 비밀번호 확인 입력 창

    public Button btn_cancel; // 취소 버튼
    public Button btn_sign; // 회원가입 버튼

    public GameObject afterSign_panel; // 이메일 유효성, 중복, 인증번호 발송과 비밀번호 일치여부와 회원가입을 띄어주는 panel
    public GameObject afterSign_group; // 이메일 유효성, 중복, 인증번호 발송과 비밀번호 일치여부와 회원가입을 띄어주는 객체
    public Text txt_afterSign; // 여기에 여러가지 문구가 적힌다.
    public Button btn_afterSign; // 알림에 대한 확인 버튼


    /* 예외처리 관련 변수들 */
    private bool isIdDuplicate; // 아이디 중복 여부
    private bool isSentAuth; // 이메일 인증번호 발송 여부
    private bool isAuthAgree; // 인증번호 일치 여부
    private bool isSign; // 회원가입 여부




    private void Awake() {

        isIdDuplicate = false;
        isSentAuth = false;
        isAuthAgree = false;
        isSign = false;

        btn_send_auth.onClick.AddListener(send_auth); //이메일 인증번호 발송 메서드(그 전 유효성과 중복검사부터 진행함)
        btn_check_auth.onClick.AddListener(check_auth_num); // 인증번호 확인 메서드
        btn_check_id.onClick.AddListener(check_id); // 아이디 중복검사 메서드 
        btn_cancel.onClick.AddListener(do_cancel); // 취소시 발동하는 메서드 (뒤로가기)
        btn_sign.onClick.AddListener(do_sign); // 회원 가입 입력시 발동하는 메서드(1. 인증메일 및 2. 인증확인 절차 확인, 3. 비밀번호 입력, 4. 비밀번호 일치 확인)
        btn_afterSign.onClick.AddListener(do_afterSign); // afterSign 창을 닫아주고 회원가입 성공 시, 씬 이동시켜주는 메서드        
    }


    //이메일 인증번호 발송 메서드(그 전 유효성과 중복검사부터 진행함)
    public void send_auth(){
        
        string email = input_email.text;

        // 우선 이메일 유효성 검사.
        if(IsValidEmail(email)) {
            
            send_emailAuth(email); // 해당 이메일을 서버로 보내고, 이미 중복되있는지 검사한 후, 인증번호를 이메일로 보낸다.       

        } else {

            open_afterSign("이메일이 유효하지 않습니다.");

        }
        
    }

    // 이메일의 유효성 검사해주는 메서드
     private bool IsValidEmail(string email)
    {
        // 이메일 주소의 유효성 검사를 위한 정규식
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        // 이메일 주소가 정규식에 맞는지 검사
        return Regex.IsMatch(email, emailPattern);
    }

    // 해당 이메일을 서버로 보내고, 이미 중복되있는지 검사한 후, 인증번호를 이메일로 보낸다.
    private void send_emailAuth(string email) {
        StartCoroutine(HTTP_emailAuth(email)); // 사용할 곳에 적절히 배치        
    }

    IEnumerator HTTP_emailAuth(string email) {
        WWWForm form = new WWWForm();
        form.AddField("email", email); // 전송할 key와 data값 작성
    
        UnityWebRequest www = UnityWebRequest.Post("https://booksns.tk/tetris/loginSign/send_emailAuth.php", form);
        yield return www.SendWebRequest();
    
        if(www.result == UnityWebRequest.Result.ProtocolError) {
            Debug.Log("HTTP 통신 에러 : "+www.error);
        }
        else {
            Debug.Log("Form upload complete!");
            Debug.Log("파싱 후 데이터 : "+www.downloadHandler.text);
            jsonParsing_emailAuth(www.downloadHandler.text);
            www.Dispose();
        }
    }
    
    [System.Serializable]

    //받아올 데이터의 형식을 작성
    public class AuthData {
        
        public bool isSend;
        public string phrases;
    }

    void jsonParsing_emailAuth(string json){
        AuthData data = JsonUtility.FromJson<AuthData>(json);

        if (data == null) {
            Debug.Log("JsonUtility.FromJson 메서드 오류일 확률 높음. json 값 : "+json);
            return;
        }
            
        // 배열이 아닌 값 파싱
        bool isSend = data.isSend; 
        string phrases = data.phrases;


        this.isSentAuth = isSend; // 이메일 발송여부 확인
        open_afterSign(phrases);
    }


    // 인증번호 확인 메서드
    public void check_auth_num() {
        if(string.IsNullOrEmpty(input_auth_num.text.Trim())) {
            
            open_afterSign("인증번호를 입력해주세요.");                      

        } else if(isSentAuth == false) {
            // 아직 인증메일을 발송하지 않았다면
            open_afterSign("인증 메일을 발송해주세요.");
        
        } else {
            // 인증번호가 null이 아니고 인증메일을 발송햇다면

            StartCoroutine(HTTP_check_authNum(input_email.text, input_auth_num.text)); // 사용할 곳에 적절히 배치
        }
           
    }

    IEnumerator HTTP_check_authNum(string email, string authNum) {
        WWWForm form = new WWWForm();
        form.AddField("email", email); // 전송할 key와 data값 작성
        form.AddField("auth_num", authNum); // 전송할 key와 data값 작성
    
        UnityWebRequest www = UnityWebRequest.Post("https://booksns.tk/tetris/loginSign/check_authNum.php", form);
        yield return www.SendWebRequest();
    
        if(www.result == UnityWebRequest.Result.ProtocolError) {
            Debug.Log("HTTP 통신 에러 : "+www.error);
        }
        else {
            Debug.Log("Form upload complete!");
            Debug.Log("파싱 후 데이터 : "+www.downloadHandler.text);
            jsonParsing_checkAuthNum(www.downloadHandler.text);
            www.Dispose(); // 메모리 해제
        }
    }
    
    [System.Serializable]

    //받아올 데이터의 형식을 작성
    public class checkAuthNum {
        
        public bool isAgree;
        public string msg;
    }

    void jsonParsing_checkAuthNum(string json){
        checkAuthNum data = JsonUtility.FromJson<checkAuthNum>(json);

        if (data == null) {
            Debug.Log("JsonUtility.FromJson 메서드 오류일 확률 높음. json 값 : "+json);
            return;
        }
            
        // 배열이 아닌 값 파싱
        bool isAgree = data.isAgree;
        string msg = data.msg;

        if(isAgree) {
            this.isAuthAgree = true; 
        } else {
            this.isAuthAgree = false; 
        }

        open_afterSign(msg);
    }

    // 아이디 중복검사 메서드
    public void check_id() {
        if(string.IsNullOrEmpty(input_id.text.Trim())) {
            open_afterSign("아이디를 입력해주세요.");
        } else {
            StartCoroutine(HTTP_check_id(input_id.text.Trim())); // 사용할 곳에 적절히 배치
        }
    }

    IEnumerator HTTP_check_id(string id) {
        WWWForm form = new WWWForm();
        form.AddField("id", id); // 전송할 key와 data값 작성
    
        UnityWebRequest www = UnityWebRequest.Post("https://booksns.tk/tetris/loginSign/check_id.php", form);
        yield return www.SendWebRequest();
    
        if(www.result == UnityWebRequest.Result.ProtocolError) {
            Debug.Log("HTTP 통신 에러 : "+www.error);
        }
        else {
            Debug.Log("Form upload complete!");
            Debug.Log("파싱 후 데이터 : "+www.downloadHandler.text);
            jsonParsing_check_id(www.downloadHandler.text);
            www.Dispose(); // 메모리 해제
        }
    }
    
    [System.Serializable]

    //받아올 데이터의 형식을 작성
    public class checkIdData {
        
        public bool isIdDuplicate;
        public string msg;
    }

    void jsonParsing_check_id(string json){
        checkIdData data = JsonUtility.FromJson<checkIdData>(json);

        if (data == null) {
            Debug.Log("JsonUtility.FromJson 메서드 오류일 확률 높음. json 값 : "+json);
            return;
        }
            
        // 배열이 아닌 값 파싱

        this.isIdDuplicate = data.isIdDuplicate;
        Debug.Log(data.isIdDuplicate);
        open_afterSign(data.msg);
        
    }

    // 취소시 발동하는 메서드 (뒤로가기)
    public void do_cancel(){
        GameManager.Instance.LoadScene("login");
    }

    // 회원 가입 입력시 발동하는 메서드(1. 인증메일 및 2. 인증확인 절차 확인, 3. 비밀번호 입력, 4. 비밀번호 일치 확인)
    public void do_sign() {
        
        if(this.isAuthAgree == false) {
            open_afterSign("메일 인증을 해주세요.");
        
        } else if(this.isIdDuplicate == false) {
            open_afterSign("아이디 중복검사를 해주세요.");


        } else if(string.IsNullOrEmpty(input_password.text.Trim())) {
            open_afterSign("비밀번호를 입력해주세요.");
            

        } else if(!input_password.text.Equals(input_check_password.text)) {
            open_afterSign("비밀번호가 다릅니다.");
            
            
        } else if(input_password.text.Equals(input_check_password.text)) {

            StartCoroutine(
                HTTP_sign(input_email.text.Trim(), input_id.text.Trim(), input_password.text.Trim())
            );
            
        } else {
            Debug.Log("코딩 실수");
        }

    }

    

    IEnumerator HTTP_sign(string email, string id, string password) {
        WWWForm form = new WWWForm();
        form.AddField("email", email); // 전송할 key와 data값 작성
        form.AddField("id", id); // 전송할 key와 data값 작성
        form.AddField("password", password); // 전송할 key와 data값 작성
    
        UnityWebRequest www = UnityWebRequest.Post("https://booksns.tk/tetris/loginSign/sign.php", form);
        yield return www.SendWebRequest();
    
        if(www.result == UnityWebRequest.Result.ProtocolError) {
            Debug.Log("HTTP 통신 에러 : "+www.error);
        }
        else {
            Debug.Log("Form upload complete!");
            Debug.Log("파싱 후 데이터 : "+www.downloadHandler.text);
            jsonParsing_sign(www.downloadHandler.text);
            www.Dispose(); // 메모리 해제
        }
    }
    
    [System.Serializable]

    //받아올 데이터의 형식을 작성
    public class SignData {
        
        public bool isSign;
        public string msg;
    }

    void jsonParsing_sign(string json){
        SignData data = JsonUtility.FromJson<SignData>(json);

        if (data == null) {
            Debug.Log("JsonUtility.FromJson 메서드 오류일 확률 높음. json 값 : "+json);
            return;
        }
            
        // 배열이 아닌 값 파싱
        this.isSign = data.isSign; 
        Debug.Log(isSign); 

        // http 통신한 뒤 두 줄 실행.
        open_afterSign(data.msg);
        
    }

    public void open_afterSign(string msg) {
        afterSign_panel.SetActive(true);
        afterSign_group.SetActive(true);
        sign_group.SetActive(false);

        txt_afterSign.text = msg;
    }

    // afterSign 창을 닫아주고 회원가입 성공 시, 씬 이동시켜주는 메서드
    public void do_afterSign() {
        afterSign_panel.SetActive(false);
        afterSign_group.SetActive(false);
        sign_group.SetActive(true);

        if(isSign) {
            GameManager.Instance.LoadScene("login");
        }
    }
}
