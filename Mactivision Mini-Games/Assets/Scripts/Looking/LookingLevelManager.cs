using System;
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
        if (lvlState == 2)
        {
            StartGame();

            // game automatically ends after maxGameTime seconds
            if (Time.time - gameStartTime >= maxGameTime)
            {
                EndGame();
                return;
            }

            // The game cycle
            switch (gameState)
            {
                case GameState.Prompting:
                    break;
                case GameState.DisplayOptions:
                    DisplayOptions();
                    break;
                case GameState.WaitingForPlayer:
                    WaitForPlayer();
                    break;
                case GameState.Response:
                    GiveFeedback();
                    break;
            }
        }
    }

    // Begin the actual game, start recording metrics
    void StartGame()
    {
        //mcMetric.startRecording();
        //metricWriter = new MetricJSONWriter("Feeder", DateTime.Now, seed); // initialize metric data writer
        gameStartTime = Time.time;
        //sound.clip = bite_sound;
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

    void DisplayOptions()
    {

    }

    void WaitForPlayer()
    {

    }

    void GiveFeedback()
    {

    }

    // End game, stop animations, sounds, physics. Finish recording metrics
    void EndGame()
    {
        //mcMetric.finishRecording();
        //var str = metricWriter.GetLogMetrics(
        //            DateTime.Now,
        //            new List<AbstractMetric>() { mcMetric }
        //        );
        //StartCoroutine(Post("feeder_" + DateTime.Now.ToFileTime() + ".json", str));
        //
        //dispenser.StopAllCoroutines();
        //dispenser.screenRed.SetActive(false);
        //dispenser.screenGreen.SetActive(false);
        //dispenser.enabled = false;
        //monster.speed = 0f;
        //foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Food"))
        //{
        //    obj.GetComponent<Rigidbody2D>().isKinematic = true;
        //    obj.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        //    obj.GetComponent<Rigidbody2D>().angularVelocity = 0f;
        //}
        foreach (AudioSource aud in FindObjectsOfType(typeof(AudioSource)) as AudioSource[])
        {
            aud.Stop();
        }
        EndLevel(0f);
    }
}
