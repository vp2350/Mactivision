using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingLevelManager : LevelManager
{
    KeyCode leftKey = KeyCode.LeftArrow;   //Left monitor
    KeyCode upKey = KeyCode.UpArrow;       //Up monitor
    KeyCode rightKey = KeyCode.RightArrow; //Right monitor
    KeyCode downKey = KeyCode.DownArrow;   //Down monitor
    KeyCode noInput = KeyCode.X;           //Prompt not displayed on monitors

    //The monitors and their positions
    public GameObject[] monitors = new GameObject[4];
    Vector3[] spawnPoints = new Vector3[4];

    //The prompters for the right object and their positions
    public GameObject[] prompters = new GameObject[2];
    Vector3[] promptPoints = new Vector3[2];

    public int difficulty;
    public GameObject[] foods = new GameObject[10];

    enum GameState
    {
        Prompting,
        DisplayOptions,
        WaitingForPlayer,
        Response
    }
    GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
        Init();
    }   

    void Init()
    {
        for(int i = 0; i<monitors.Length; i++)
        {
            spawnPoints[i] = monitors[i].transform.position;
        }
        for (int i = 0; i < prompters.Length; i++)
        {
            promptPoints[i] = monitors[i].transform.position;
        }
        InitConfig();
    }

    void InitConfig()
    {
        LookingConfig tempConfig = new LookingConfig();

    }

    // Update is called once per frame
    void Update()
    {
        //monitors are in the order
        //up = 0
        //right = 1
        //down = 2
        //left = 3
    }
    // Handles GUI events (keyboard, mouse, etc events)
    void OnGUI()
    {
        Event e = Event.current;
        // navigate through the instructions before the game starts
        if (lvlState == 0 && e.type == EventType.KeyUp)
        {
            if (e.keyCode == KeyCode.B && instructionCount > 0)
            {
                ShowInstruction(--instructionCount);
            }
            else if (e.keyCode == KeyCode.N && instructionCount < instructions.Length)
            {
                ShowInstruction(++instructionCount);
            }
        }

        // game is over, go to next game/finish battery
        if (lvlState == 4 && e.type == EventType.KeyUp)
        {
            Battery.Instance.LoadNextScene();
        }
    }
}
