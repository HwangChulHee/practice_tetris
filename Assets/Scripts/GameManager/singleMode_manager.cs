using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class singleMode_manager : MonoBehaviour
{
    public string gameType; // 게임의 종류를 나타내는 변수 (sprint, timeAttack, maraton)
    public GameObject gameOverPanel; // game 종료시 띄어지는 panael을 의미
    
    /*스프린트 관련 UI*/

    /* 게임 진행 중 뜨는 UI*/
    public Text sprint_currenttimeText; // 경과 시간
    public Text sprint_remainText; // 남아 있는 줄의 개수

    /* 게임 종료 시 뜨는 UI*/
    public Text sprint_finalTimeText;
    public Text sprint_finalLineClearText;

    
    /*타임어택 관련 UI*/

    /* 게임 진행 중 뜨는 UI*/
    public Text timeAttack_remainTimeTxt; // 남은 시간
    public Text timeAttack_scoreTxt; // 점수
    public Text timeAttack_speedTxt; // 속도
    public Text timeAttack_experTxt; // 경험치

    /* 게임 종료 시 뜨는 UI*/

    public Text timeAttack_finalTimeTxt; // 경과 시간
    public Text timeAttack_finalScoreTxt; // 최종 점수

    
    /*마라톤 관련 UI*/

    /* 게임 진행 중 뜨는 UI*/
    public Text maraton_timeTxt; // 경과 시간
    public Text maraton_scoreTxt; // 점수
    public Text maraton_speedTxt; // 속도
    public Text maraton_experTxt; // 경험치

    /* 게임 종료 시 뜨는 UI*/

    public Text maraton_finalTimeTxt; // 경과 시간
    public Text maraton_finalScoreTxt; // 최종 점수
    

    
    public Text scoreEventUI;
    
    private float elapsedTime; // 경과 시간
    int remainLine;

    string niceTime; 
    string remainTime;

    int totalScore; // 점수

    public Board board;

    private void Start() {
        elapsedTime = 0;
        totalScore = 0;
    }

    private void Update() {


         if(!board.isGameOver) {
            elapsedTime += Time.deltaTime;
            gameInfo_GUI(elapsedTime);
        }

        // 스프린트 모드일 땐, 40줄이 클리어 되었을 때, 타임어택일 땐 시간이 00:00일때 게임을 종료한다.
        if(gameType =="sprint" && board.lineClearCount > 0
            || gameType == "timeAttack" && remainTime == "00:00") {
            modeGameOver();          
        }
        
    }

    private void gameInfo_GUI (float timer) {
        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer - minutes * 60);
        niceTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        int remainSeconds =  Mathf.FloorToInt(121 - timer);
        int screenMinutes = (int)((remainSeconds / 60F));
        int screenSeconds = remainSeconds - screenMinutes * 60;
        remainTime = string.Format("{0:00}:{1:00}", screenMinutes , screenSeconds);


        if(gameType == "sprint") {
            remainLine = 40 - board.lineClearCount;
            sprint_remainText.text = "남은 줄 : "+remainLine.ToString();
            sprint_currenttimeText.text = "경과 시간 : "+niceTime;

        } else if(gameType == "timeAttack") {
            
            timeAttack_remainTimeTxt.text = "남은 시간 : "+remainTime;
            timeAttack_scoreTxt.text = "점수 : "+totalScore;
            timeAttack_speedTxt.text = "속도 : "+board.block_speed;
            timeAttack_experTxt.text= "경험치 : "+board.exper+" / "+"10";

        } else if(gameType == "maraton") {

            maraton_timeTxt.text = "경과 시간 : "+niceTime;
            maraton_scoreTxt.text = "점수 : "+totalScore;
            maraton_speedTxt.text = "속도 : "+board.block_speed;
            maraton_experTxt.text= "경험치 : "+board.exper+" / "+"10";
        }
        
        
     }

     public void modeGameOver() {
        board.isGameOver = true;
        gameOverPanel.SetActive(true);

        
        if(gameType == "sprint") {
            sprint_finalTimeText.text = "경과 시간 : "+niceTime;
            sprint_finalLineClearText.text = "라인 클리어 : "+board.lineClearCount.ToString();

        } else if(gameType == "timeAttack") {
            timeAttack_finalTimeTxt.text = "경과 시간 : "+niceTime;
            timeAttack_finalScoreTxt.text = "최종 점수 : "+totalScore;

        } else if(gameType == "maraton") {
            maraton_finalTimeTxt.text = "경과 시간 : "+niceTime;
            maraton_finalScoreTxt.text = "최종 점수 : "+totalScore;
        }
        
        
     }

     public void printScoreEvent(int lineCount, int comboCount) {
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
        scoreEventUI.text = scoreEventText;

     }

     public void print_score(int gainScore) {
        this.totalScore += gainScore;
        // Debug.Log("totalScore : "+totalScore);
        // Debug.Log("gainScore : "+gainScore);

        if(gameType == "sprint") {

        } else if(gameType == "timeAttack") {
            timeAttack_scoreTxt.text = "점수 : "+totalScore;
            timeAttack_speedTxt.text = "속도 : "+board.block_speed;
            timeAttack_experTxt.text= "경험치 : "+board.exper+" / "+"10";

        } else if(gameType == "maraton") {
            maraton_scoreTxt.text = "점수 : "+totalScore;
            maraton_speedTxt.text = "속도 : "+board.block_speed;
            maraton_experTxt.text= "경험치 : "+board.exper+" / "+"10";

        }
     }

     public void print_exper() {
        // Debug.Log("Exper : "+board.exper);
        // Debug.Log("speed : "+board.block_speed);

        if(gameType == "sprint") {

        } else if(gameType == "timeAttack") {
            timeAttack_speedTxt.text = "속도 : "+board.block_speed;
            timeAttack_experTxt.text= "경험치 : "+board.exper+" / "+"10";

        } else if(gameType == "maraton") {
            maraton_speedTxt.text = "속도 : "+board.block_speed;
            maraton_experTxt.text= "경험치 : "+board.exper+" / "+"10";

        }
     }

     



    public void btn_exit(string sceneName) {
        GameManager.Instance.LoadScene(sceneName);
    }

    public void btn_replay() {

        if(gameType == "sprint") {
            GameManager.Instance.LoadScene("sprint");
        } else if(gameType == "timeAttack") {
            GameManager.Instance.LoadScene("timeAttack");
        } else if(gameType == "maraton") {
            GameManager.Instance.LoadScene("maraton");
        }
        
        
        gameOverPanel.SetActive(false);
    }

}
