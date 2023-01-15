using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    public GameObject ballPrefab;
    public GameObject playerPrefab;
    public GameObject brick1Prefab;
    public GameObject brick2Prefab;
    public GameObject brick3Prefab;
    public GameObject level;
    public int zeroBlock = 70;
    public int oneBlock = 24;
    public int twoBlock = 5;
    public int threeBlock = 1;
    public int ballsStart = 3;
    
    static int fieldColumns = 16; // stepX = 2
    static int fieldRows = 4; // stepY = 1
    

    public GameObject[] brickTypes = new GameObject[3];
    
    
    int[] brickProbability = new int[100];
    Random rnd = new Random();

    Vector3 brickPosition = new Vector3();
    Quaternion brickRotation = new Quaternion();

    public Text scoreText;
    public Text ballsText;
    public Text levelText;
    public Text highscoreText;

    public GameObject panelMenu;
    public GameObject panelPlay;
    public GameObject panelLevelCompleted;
    public GameObject panelGameOver;
    
    public GameObject[] levels;

    GameObject _currentBall;
    GameObject _currentLevel;
    bool _isSwitchingState;

    public static GameManager Instance { get; private set; }


    public enum State { MENU, INIT, PLAY, LEVELCOMPLETED, LOADLEVEL, GAMEOVER }
    State _state;

    private int _score;

    public int Score
    {
        get { return _score; }
        set { _score = value;
            scoreText.text = "SCORE: " + _score;
        }
    }

    private int _level;

    public int Level
    {
        get { return _level; }
        set { _level = value;
            levelText.text = "LEVEL: " + _level;
        }
    }


    private int _balls;

    public int Balls
    {
        get { return _balls; }
        set { _balls = value;
            ballsText.text = "BALLS: " + _balls;
        }
    }

    public void PlayClicked()
    {
        SwitchState(State.INIT);
    }

    void Start()
    {
        brickTypes[0] = brick1Prefab;
        brickTypes[1] = brick2Prefab;
        brickTypes[2] = brick3Prefab;
        Instance = this;
        SwitchState(State.MENU);
    }

    public void SwitchState(State newState, float delay = 0)
    {
       StartCoroutine(SwitchDelay(newState, delay));
    }

    IEnumerator SwitchDelay(State newState, float delay)
    {
        _isSwitchingState = true;
        yield return new WaitForSeconds(delay);
        EndState();
        _state = newState;
        BeginState(newState);
        _isSwitchingState = false;
    }

    void BeginState(State newState)
    {
        switch (newState)
        {
            case State.MENU:
                Destroy(_currentBall);
                Cursor.visible = true;
                highscoreText.text = "HIGHSCORE: " + PlayerPrefs.GetInt("highscore");
                panelMenu.SetActive(true);
                break;
            case State.INIT:

                zeroBlock = 0;
                oneBlock = 100;
                twoBlock = 0;
                threeBlock = 0;

                
                Cursor.visible = false;
                panelPlay.SetActive(true);
                Score = 0;
                Level = 1;
                Balls = ballsStart;
                if( _currentLevel != null)
                {
                    Destroy(_currentLevel );
                }
                Instantiate(playerPrefab);
                SwitchState(State.LOADLEVEL);
                break;
            case State.PLAY:
                break;
            case State.LEVELCOMPLETED:
                Destroy(_currentBall);
                Destroy(_currentLevel);
                Level++;
                panelLevelCompleted.SetActive(true);
                SwitchState(State.LOADLEVEL, 2);
                break;
            case State.LOADLEVEL:
                if (fieldColumns == 64 && fieldRows == 16)
                {
                    if (oneBlock > 4)
                    {
                        zeroBlock = 30;
                        oneBlock -= 3;
                        twoBlock += 2;
                        threeBlock += 1;
                    } else
                    {
                        if(twoBlock > 5)
                        {
                            zeroBlock = 0;
                            twoBlock -= 5;
                            threeBlock += 5;
                        }
                    }
                }
                else
                {
                    fieldColumns += 16;
                    fieldRows += 4;

                    zeroBlock += 10;
                    oneBlock -= 15;
                    twoBlock += 5;
                }
                this.brickProbability = createProbabilityArray(brickProbability);

                GameObject level = new GameObject();
                _currentLevel = level;
                createLevelData(this.brickProbability, level);
                SwitchState(State.PLAY);
                break;
            case State.GAMEOVER:
                if(Score > PlayerPrefs.GetInt("highscore"))
                {
                    PlayerPrefs.SetInt("highscore", Score);
                }
                panelGameOver.SetActive(true);
                break;
        }

    }

    void Update()
    {
        switch (_state)
        {
            case State.MENU:
                break;
            case State.INIT:
                break;
            case State.PLAY:
                if (_currentBall == null)
                {
                    if (Balls > 0)
                    {
                        _currentBall = Instantiate(ballPrefab);
                    }
                    else
                    {
                        SwitchState(State.GAMEOVER);
                    }
                }
                if (_currentLevel != null && _currentLevel.transform.childCount == 0 && !_isSwitchingState)
                {
                    SwitchState(State.LEVELCOMPLETED);
                }
                if(Input.GetKeyDown(KeyCode.Escape)) {
                    SwitchState(State.MENU);
                }
                //DEBUG
                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    SwitchState(State.LEVELCOMPLETED);
                }
                break;
            case State.LEVELCOMPLETED:
                break;
            case State.LOADLEVEL:
                break;
            case State.GAMEOVER:
                if(Input.anyKeyDown)
                {
                    SwitchState(State.MENU);
                }
                break;
        }
    }

    void EndState()
    {
        switch (_state)
        {
            case State.MENU:
                panelMenu.SetActive(false);
                break;
            case State.INIT:
                break;
            case State.PLAY:
                break;
            case State.LEVELCOMPLETED:
                panelLevelCompleted.SetActive(false);
                break;
            case State.LOADLEVEL:
                break;
            case State.GAMEOVER:
                panelPlay.SetActive(false);
                panelGameOver.SetActive(false);
                break;
        }
    }

    private void createLevelData(int[] _brickProbability, GameObject _level)
    {
        int[,] LevelData = new int[fieldRows, fieldColumns];

        for (int r = 0; r < fieldRows - 2; r++)
        {
            for (int c = 0; c <= fieldColumns/2; c++)
            {
                if (c >= (fieldColumns / 4))
                {
                    LevelData[r, c] = LevelData[r, (c - 2*(c-(fieldColumns / 4)))];
                    addBrickToLayout(r, c, LevelData[r, (c - 2 * (c - (fieldColumns / 4)))], _level);
                }
                else
                {
                    int ix = rnd.Next(0,99);
                    LevelData[r, c] = brickProbability[ix];
                    addBrickToLayout(r, c, brickProbability[ix], _level);
                }
            }
        }

    }

    private void addBrickToLayout(int r, int c, int t, GameObject _level)
    {
        if (t != 0 && r < 15)
        {
            GameObject autoBrick = new GameObject();
            int xCoord = 0;
            /*if (c % 2 != 0)
            {
                xCoord = c+1 - (fieldColumns / 2);
            } else
            {
                xCoord = c - (fieldColumns / 2);
            }*/
            xCoord = c * 2 - (fieldColumns / 2);
            

            brickPosition = new Vector3(xCoord, r, 1);
            brickRotation = new Quaternion(0, 0, 0, 1);
            autoBrick = Instantiate(brickTypes[t-1], brickPosition, brickRotation);
            autoBrick.transform.parent = _level.transform;
        }
    }

    // empty - 25%, 1 hit - 35%, 2 hits - 25%, 3 hits - 15%
    int[] createProbabilityArray(int[] brickProbability)
    {
        for (int i = 0; i < zeroBlock; i++)
        {
            brickProbability[i] = 0;
        }
        for (int i = zeroBlock; i < (zeroBlock+oneBlock); i++)
        {
            brickProbability[i] = 1;
        }
        for (int i = (zeroBlock+oneBlock); i < (zeroBlock+oneBlock+twoBlock); i++)
        {
            brickProbability[i] = 2;
        }
        for (int i = (zeroBlock+oneBlock+twoBlock); i < 100; i++)
        {
            brickProbability[i] = 3;
        }

        return brickProbability;
    }
}
