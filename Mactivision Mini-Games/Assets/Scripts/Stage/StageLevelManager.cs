using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StageLevelManager : LevelManager
{
    KeyCode yesKey = KeyCode.Y;   //Left monitor
    KeyCode noKey = KeyCode.N;       //Up monitor
    KeyCode rowOne = KeyCode.Alpha1; //Right monitor
    KeyCode rowTwo = KeyCode.Alpha2;   //Down monitor
    KeyCode rowThree = KeyCode.Alpha3;           //Prompt not displayed on monitors

    public StageController stageController;

    int uniqueTypes;                         // number of foods to be used in the current game
    int uniqueTokens;

    bool prompting;
    bool displayingOptions;

    int maxPlayersDisplayed;                   // maximum foods dispensed before game ends
    int playerTypeDisplayed;

    //LookingChoiceMetric lcMetric;            // records choice data during the game
    //MetricJSONWriter metricWriter;          // outputs recording metric (lcMetric) as a json file
    //string recordKey;

    public int difficulty;
    public GameObject player;
    public int maxPrompts;

    List<GameObject> playersSpawned = new List<GameObject>();

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

        //lcMetric = new LookingChoiceMetric(); // initialize metric recorder

        randomSeed = new System.Random(seed.GetHashCode());
        gameState = GameState.Prompting;

    }

    void Init()
    {
        prompting = false;
        displayingOptions = false;

        InitConfig();

        stageController.Init(seed, difficulty);
    }

    void InitConfig()
    {
        StageConfig stageConfig = new StageConfig();

        // if running the game from the battery, override `lookingConfig` with the config class from Battery
        StageConfig tempConfig = (StageConfig)Battery.Instance.GetCurrentConfig();
        if (tempConfig != null)
        {
            stageConfig = tempConfig;
        }
        else
        {
            Debug.Log("Battery not found, using default values");
        }

        // use battery's config values, or default values if running game by itself
        seed = !String.IsNullOrEmpty(stageConfig.Seed) ? stageConfig.Seed : DateTime.Now.ToString(); // if no seed provided, use current DateTime
        maxGameTime = stageConfig.MaxGameTime > 0 ? stageConfig.MaxGameTime : Default(90f, "MaxGameTime");
        maxPlayersDisplayed = stageConfig.MaxPlayersDisplayed > 0 ? stageConfig.MaxPlayersDisplayed : Default(10, "MaxFoodDisplayed");
        uniqueTypes = stageConfig.UniqueTypes >= 2 && stageConfig.UniqueTypes <= stageController.playerTypes.Count ? stageConfig.UniqueTypes : Default(1, "UniqueTypes");
        difficulty = stageConfig.DiffLevel > 0 ? stageConfig.DiffLevel : Default(2, "DiffLevel");
        maxPrompts = stageConfig.MaxPrompts > 0 ? stageConfig.MaxPrompts : Default(5, "MaxPrompts");

        // udpate battery config with actual/final values being used
        stageConfig.Seed = seed;
        stageConfig.MaxGameTime = maxGameTime;
        stageConfig.MaxPlayersDisplayed = maxPlayersDisplayed;
        stageConfig.UniqueTypes = uniqueTypes;
        stageConfig.MaxPrompts = maxPrompts;
        stageController.UpdateDiff(difficulty);
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
                    if (!prompting)
                    {
                        Prompt();
                        prompting = true;
                    }
                    break;
                case GameState.DisplayOptions:
                    if (!displayingOptions)
                    {
                        ShowChoices();
                        displayingOptions = true;
                    }
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
        //lcMetric.startRecording();
        //metricWriter = new MetricJSONWriter("Looking", DateTime.Now, seed); // initialize metric data writer
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

    void WaitForPlayer()
    {
        displayingOptions = false;
        bool rightDecision = false;

        if (Input.GetKeyDown(yesKey) || Input.GetKeyDown(noKey)
            || Input.GetKeyDown(rowOne) || Input.GetKeyDown(rowTwo) || Input.GetKeyDown(rowThree))
        {
            if (Input.GetKeyDown(yesKey))
            {
                
            }
            else if (Input.GetKeyDown(noKey))
            {
                
            }
            else
            {
                
            }
            
            // record the choice made
            //lcMetric.recordEvent(new LookingChoiceEvent(
            //    displayController.choiceStartTime,
            //    displayController.goodFood,
            //    displayController.GetObjectsShown(),
            //    recordKey,
            //    DateTime.Now
            //));

            //
            //// animate choice and play plate sound
            //sound.PlayOneShot(plate_up);
            //StartCoroutine(AnimateChoice(Input.GetKeyDown(feedKey) && !dispenser.MakeChoice(Input.GetKeyDown(feedKey))));
            gameState = GameState.Response;
        }
    }

    void GiveFeedback()
    {
        WaitForFeedback(2f);
        gameState = GameState.Prompting;
    }

    void Prompt()
    {
        stageController.SpawnNext(false);
        StartCoroutine(WaitForCharacters(12f, false));
    }

    void ShowChoices()
    {
        prompting = false;
        stageController.SpawnNext(true);
        StartCoroutine(WaitForCharacters(15f, true));
    }
    // Wait for the food dispensing animation
    IEnumerator WaitForCharacters(float wait, bool secondDisplay)
    {
        yield return new WaitForSeconds(wait);
        Debug.Log("Second");
        if(!secondDisplay)
        {
            gameState = GameState.DisplayOptions;
        }
        else
        {
            gameState = GameState.WaitingForPlayer;
        }
    }

    // Wait for the food dispensing animation
    IEnumerator WaitForFeedback(float wait)
    {
        yield return new WaitForSeconds(wait);
        gameState = GameState.Prompting;
    }

    // Wait for the food dispensing animation
    IEnumerator DarkenArrow(float wait, GameObject arrow)
    {
        SpriteRenderer temp = arrow.GetComponent<SpriteRenderer>();
        temp.color = new Color(0f, 0f, 0f, 1f);
        yield return new WaitForSeconds(wait);
        temp.color = new Color(251f, 233f, 0f, 1f);

    }
    // End game, stop animations, sounds, physics. Finish recording metrics
    void EndGame()
    {
        //lcMetric.finishRecording();
        //var str = metricWriter.GetLogMetrics(
        //            DateTime.Now,
        //            new List<AbstractMetric>() { lcMetric }
        //        );
        //StartCoroutine(Post("stage_" + DateTime.Now.ToFileTime() + ".json", str));
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

