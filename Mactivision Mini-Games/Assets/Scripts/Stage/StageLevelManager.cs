using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageLevelManager : LevelManager
{
    KeyCode leftKey = KeyCode.LeftArrow;   //Left monitor
    KeyCode upKey = KeyCode.UpArrow;       //Up monitor
    KeyCode rightKey = KeyCode.RightArrow; //Right monitor
    KeyCode downKey = KeyCode.DownArrow;   //Down monitor
    KeyCode noInput = KeyCode.X;           //Prompt not displayed on monitors

    StageController controller;

    int uniqueObjects;                         // number of foods to be used in the current game
    float avgUpdateFreq;                    // average number of foods dispensed between each food update
    float updateFreqVariance;               // variance of `avgUpdateFreq`

    int maxFoodDisplayed;                   // maximum foods dispensed before game ends
    int foodDisplayed;

    LookingChoiceMetric lcMetric;            // records choice data during the game
    MetricJSONWriter metricWriter;          // outputs recording metric (lcMetric) as a json file
    string recordKey;

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

        lcMetric = new LookingChoiceMetric(); // initialize metric recorder

        randomSeed = new System.Random(seed.GetHashCode());
        gameState = GameState.Prompting;


    }

    void Init()
    {
        InitConfig();
    }

    void InitConfig()
    {
        LookingConfig lookingConfig = new LookingConfig();

        // if running the game from the battery, override `lookingConfig` with the config class from Battery
        LookingConfig tempConfig = (LookingConfig)Battery.Instance.GetCurrentConfig();
        if (tempConfig != null)
        {
            lookingConfig = tempConfig;
        }
        else
        {
            Debug.Log("Battery not found, using default values");
        }

        // use battery's config values, or default values if running game by itself
        seed = !String.IsNullOrEmpty(lookingConfig.Seed) ? lookingConfig.Seed : DateTime.Now.ToString(); // if no seed provided, use current DateTime
        maxGameTime = lookingConfig.MaxGameTime > 0 ? lookingConfig.MaxGameTime : Default(90f, "MaxGameTime");
        maxFoodDisplayed = lookingConfig.MaxFoodDisplayed > 0 ? lookingConfig.MaxFoodDisplayed : Default(20, "MaxFoodDisplayed");
        uniqueObjects = lookingConfig.UniqueObjects >= 2 && lookingConfig.UniqueObjects <= displayController.allFoods.Length ? lookingConfig.UniqueObjects : Default(6, "UniqueObjects");
        avgUpdateFreq = lookingConfig.AverageUpdateFrequency > 0 ? lookingConfig.AverageUpdateFrequency : Default(3f, "AverageUpdateFrequency");
        updateFreqVariance = lookingConfig.UpdateFreqVariance >= 0 && lookingConfig.UpdateFreqVariance <= 1 ? lookingConfig.UpdateFreqVariance : Default(0.3f, "UpdateFreqVariance");

        // udpate battery config with actual/final values being used
        lookingConfig.Seed = seed;
        lookingConfig.MaxGameTime = maxGameTime;
        lookingConfig.MaxFoodDisplayed = maxFoodDisplayed;
        lookingConfig.UniqueObjects = uniqueObjects;
        lookingConfig.AverageUpdateFrequency = avgUpdateFreq;
        lookingConfig.UpdateFreqVariance = updateFreqVariance;
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
                    Prompt();
                    break;
                case GameState.DisplayOptions:
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
        lcMetric.startRecording();
        metricWriter = new MetricJSONWriter("Looking", DateTime.Now, seed); // initialize metric data writer
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
        bool rightDecision = false;

        if (Input.GetKeyDown(upKey) || Input.GetKeyDown(leftKey)
            || Input.GetKeyDown(rightKey) || Input.GetKeyDown(downKey) || Input.GetKeyDown(noInput))
        {
            if (Input.GetKeyDown(upKey))
            {
                
            }
            else if (Input.GetKeyDown(leftKey))
            {
                
            }
            else
            {
                
            }
            // set the angle the plate should tilt to. Play monster eating animation & sound if applicable
            //if (Input.GetKeyDown(feedKey))
            //{
            //    monster.Play("Base Layer.monster_eat");
            //    sound.PlayDelayed(0.85f);
            //    tiltPlateTo = -33f;
            //}
            //else
            //{
            //    tiltPlateTo = 33f;
            //}
            //
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
        if (displayController.DisplayNext())
        {
            StartCoroutine(WaitForFoodDisplay(1.75f));
        }
        else
        {
            StartCoroutine(WaitForFoodDisplay(1f));
        }
        gameState = GameState.DisplayOptions;
    }

    // Wait for the food dispensing animation
    IEnumerator WaitForFoodDisplay(float wait)
    {
        yield return new WaitForSeconds(wait);
        gameState = GameState.WaitingForPlayer;
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
        lcMetric.finishRecording();
        var str = metricWriter.GetLogMetrics(
                    DateTime.Now,
                    new List<AbstractMetric>() { lcMetric }
                );
        StartCoroutine(Post("looking_" + DateTime.Now.ToFileTime() + ".json", str));
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
