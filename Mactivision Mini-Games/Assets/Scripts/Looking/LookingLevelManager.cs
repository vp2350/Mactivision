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

    int uniqueObjects;                         // number of foods to be used in the current game
    float avgUpdateFreq;                    // average number of foods dispensed between each food update
    float updateFreqVariance;               // variance of `avgUpdateFreq`

    int maxFoodDisplayed;                   // maximum foods dispensed before game ends
    public int foodDisplayed;
    bool rightDecision;

    LookingChoiceMetric lcMetric;            // records choice data during the game
    MetricJSONWriter metricWriter;          // outputs recording metric (lcMetric) as a json file
    string recordKey;

    public LookingDisplays displayController;
    //The monitors and their positions
    public GameObject[] monitors = new GameObject[4];
    Vector3[] spawnPoints = new Vector3[4];
    public GameObject[] arrows = new GameObject[4];

    //The prompters for the right object and their positions
    public GameObject[] prompters = new GameObject[2];
    Vector3[] promptPoints = new Vector3[2];

    public int difficulty;
    public GameObject[] foods = new GameObject[10];
    public GameObject greenCheck;
    public GameObject redCross;

    public bool feedbackCalled;
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

        displayController.Init(seed, uniqueObjects, avgUpdateFreq, updateFreqVariance, spawnPoints, promptPoints, foods); // initialize the display controller
        foodDisplayed = 0;
    }

    void Init()
    {
        for(int i = 0; i<monitors.Length; i++)
        {
            spawnPoints[i] = monitors[i].transform.position;
        }
        for (int i = 0; i < prompters.Length; i++)
        {
            promptPoints[i] = prompters[i].transform.position;
        }
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
        maxFoodDisplayed = lookingConfig.MaxFoodDisplayed > 0 ? lookingConfig.MaxFoodDisplayed : Default(15, "MaxFoodDisplayed");
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
            if (!lcMetric.isRecording) StartGame();

            // game automatically ends after maxGameTime seconds
            if (Time.time - gameStartTime >= maxGameTime || foodDisplayed >= maxFoodDisplayed)
            {
                Debug.Log("Loading Next Scene");
                EndGame();
                return;
            }

            // The game cycle
            switch (gameState)
            {
                case GameState.Prompting:
                    Prompt();
                    feedbackCalled = false;
                    break;
                case GameState.DisplayOptions:
                    break;
                case GameState.WaitingForPlayer:
                    WaitForPlayer();
                    break;
                case GameState.Response:
                    if (!feedbackCalled)
                    {
                        GiveFeedback();
                        feedbackCalled = true;
                    }
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
        rightDecision = false;
        
        if (Input.GetKeyDown(upKey) || Input.GetKeyDown(leftKey)
            || Input.GetKeyDown(rightKey) || Input.GetKeyDown(downKey) || Input.GetKeyDown(noInput))
        {
            if(Input.GetKeyDown(upKey))
            {
                rightDecision = displayController.MakeChoice(0);
                recordKey = "Up";
                StartCoroutine(DarkenArrow(0.1f, arrows[0]));
            }
            else if(Input.GetKeyDown(rightKey))
            {
                rightDecision = displayController.MakeChoice(1);
                recordKey = "Right";
                StartCoroutine(DarkenArrow(0.1f, arrows[1]));
            }
            else if(Input.GetKeyDown(downKey))
            {
                rightDecision = displayController.MakeChoice(2);
                recordKey = "Down";
                StartCoroutine(DarkenArrow(0.1f, arrows[2]));
            }
            else if(Input.GetKeyDown(leftKey))
            {
                rightDecision = displayController.MakeChoice(3);
                recordKey = "Left";
                StartCoroutine(DarkenArrow(0.1f, arrows[3]));
            }
            else
            {
                rightDecision = displayController.MakeChoice(-1);
                recordKey = "X";
            }

            Debug.Log(displayController.GetObjectsShown().Count);
            // record the choice made
            lcMetric.recordEvent(new LookingChoiceEvent(
                displayController.choiceStartTime,
                rightDecision.ToString(),
                displayController.goodFood,
                displayController.GetObjectsShown(),
                recordKey,
                DateTime.Now
            )); 

            gameState = GameState.Response;
        }
    }

    void GiveFeedback()
    {
        StartCoroutine(WaitForFeedback(0.5f));
    }

    void Prompt()
    {
        if (displayController.DisplayNext())
        {
            StartCoroutine(WaitForFoodDisplay(1.75f));
        }
        else
        {
            StartCoroutine(WaitForFoodDisplay(0.2f));
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
        if(rightDecision)
        {
            greenCheck.SetActive(true);
        }
        else
        {
            redCross.SetActive(true);
        }
        yield return new WaitForSeconds(wait);
        if (rightDecision)
        {
            greenCheck.SetActive(false);
        }
        else
        {
            redCross.SetActive(false);
        }
        foodDisplayed++;
        
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
        displayController.StopAllCoroutines();

        foreach (AudioSource aud in FindObjectsOfType(typeof(AudioSource)) as AudioSource[])
        {
            aud.Stop();
        }
        EndLevel(0f);
    }
}
