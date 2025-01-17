using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StageLevelManager : LevelManager
{
    KeyCode yesKey = KeyCode.RightShift;   //Left monitor
    KeyCode noKey = KeyCode.LeftShift;       //Up monitor
    KeyCode rowOne = KeyCode.Alpha1; //Right monitor
    KeyCode rowTwo = KeyCode.Alpha2;   //Down monitor
    KeyCode rowThree = KeyCode.Alpha3;           //Prompt not displayed on monitors

    public AudioSource bgmPlayer;
    public AudioClip bgm;

    public StageController stageController;

    int uniqueTypes;                         // number of foods to be used in the current game
    int uniqueTokens;

    bool rightDecision;
    bool prompting;
    bool displayingOptions;
    bool feedbackGiven;

    int maxPlayersDisplayed;                   // maximum foods dispensed before game ends
    int playerTypeDisplayed;

    StageChoiceMetric scMetric;            // records choice data during the game
    MetricJSONWriter metricWriter;          // outputs recording metric (lcMetric) as a json file
    string recordKey;

    public int difficulty;
    public GameObject player;
    public int maxPrompts;
    int prompts;

    List<GameObject> playersSpawned = new List<GameObject>();
    public GameObject greenSquare;
    public GameObject redSquare;
    public GameObject greenCheck;
    public GameObject redCross;
    public GameObject prompt;

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

        scMetric = new StageChoiceMetric(); // initialize metric recorder

        randomSeed = new System.Random(seed.GetHashCode());
        gameState = GameState.Prompting;
        prompts = 0;

    }

    void Init()
    {
        prompting = false;
        displayingOptions = false;
        feedbackGiven = false;

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
        maxPlayersDisplayed = stageConfig.MaxPlayersDisplayed > 0 ? stageConfig.MaxPlayersDisplayed : Default(10, "MaxPlayersDisplayed");
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
            if (!scMetric.isRecording) StartGame();


            // game automatically ends after maxGameTime seconds
            if (Time.time - gameStartTime >= maxGameTime || prompts>=maxPrompts)
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
                    if(!feedbackGiven)
                        GiveFeedback();
                    break;
            }
        }
    }

    // Begin the actual game, start recording metrics
    void StartGame()
    {
        scMetric.startRecording();
        metricWriter = new MetricJSONWriter("Stage", DateTime.Now, seed); // initialize metric data writer
        gameStartTime = Time.time;
        bgmPlayer.loop = true;
        bgmPlayer.clip = bgm;
        bgmPlayer.Play();
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
        rightDecision = false;
        prompt.SetActive(true);

        if (Input.GetKeyDown(yesKey) || Input.GetKeyDown(noKey)
            || Input.GetKeyDown(rowOne) || Input.GetKeyDown(rowTwo) || Input.GetKeyDown(rowThree))
        {
            if (Input.GetKeyDown(yesKey))
            {
                rightDecision = stageController.MakeChoice(true);
                recordKey = yesKey.ToString();
            }
            else if (Input.GetKeyDown(noKey))
            {
                rightDecision = stageController.MakeChoice(false);
                recordKey = noKey.ToString();
            }

            int ogColor = stageController.GetOriginalColorNumber();
            int changedColor = stageController.GetChangedColorNumber();
            List<int> colors = stageController.GetColors();

            StageChoiceEvent tempEvent;
            if (colors.Count == 3)
            {
                tempEvent = new StageChoiceEvent(
                stageController.choiceStartTime,
                stageController.faked,
                ogColor,
                changedColor,
                rightDecision,
                recordKey,
                DateTime.Now,
                colors[0],
                colors[1],
                colors[2]
                );
            }
            else if (colors.Count == 6)
            {
                tempEvent = new StageChoiceEvent(
                stageController.choiceStartTime,
                stageController.faked,
                ogColor,
                changedColor,
                rightDecision,
                recordKey,
                DateTime.Now,
                colors[0],
                colors[1],
                colors[2],
                colors[3],
                colors[4],
                colors[5]
                );
            }
            else
            {
                tempEvent = new StageChoiceEvent(
                stageController.choiceStartTime,
                stageController.faked,
                ogColor,
                changedColor,
                rightDecision,
                recordKey,
                DateTime.Now,
                colors[0],
                colors[1],
                colors[2],
                colors[3],
                colors[4],
                colors[5],
                colors[6],
                colors[7],
                colors[8]
                );
            }

            // record the choice made
            scMetric.recordEvent(tempEvent); 

            //
            //// animate choice and play plate sound
            //sound.PlayOneShot(plate_up);
            //StartCoroutine(AnimateChoice(Input.GetKeyDown(feedKey) && !dispenser.MakeChoice(Input.GetKeyDown(feedKey))));
            gameState = GameState.Response;
        }
    }

    void GiveFeedback()
    {      
        StartCoroutine(WaitForFeedback(2f));
    }

    void Prompt()
    {
        feedbackGiven = false;
        stageController.SpawnNext(false);
        StartCoroutine(WaitForCharacters(8.25f, false));
    }

    void ShowChoices()
    {
        prompting = false;
        stageController.SpawnNext(true);
        StartCoroutine(WaitForCharacters(2.5f, true));
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
        feedbackGiven = true;
        prompt.SetActive(false);
        if(stageController.faked)
        {
            if(rightDecision && stageController.falseShephard != -1)
            {
                greenSquare.transform.position = stageController.spawnedPlayers[stageController.falseShephard].transform.position;
            }
            else if(stageController.falseShephard != -1)
            {
                redSquare.transform.position = stageController.spawnedPlayers[stageController.falseShephard].transform.position;
            }
        }
        else
        {
            if (rightDecision)
            {
                greenCheck.transform.position = new Vector3(0, 0, -1);
            }
            else
            {
                redCross.transform.position = new Vector3(0, 0, -1);
            }
        }
        yield return new WaitForSeconds(wait);
        redSquare.transform.position = new Vector3(10, 10, 10);
        greenSquare.transform.position = new Vector3(10, 10, 10);
        redCross.transform.position = new Vector3(10, 10, 10);
        greenCheck.transform.position = new Vector3(10, 10, 10);
        Debug.Log("Adding prompts");
        prompts++;

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
        scMetric.finishRecording();
        var str = metricWriter.GetLogMetrics(
                    DateTime.Now,
                    new List<AbstractMetric>() { scMetric }
                );
        StartCoroutine(Post("stage_" + DateTime.Now.ToFileTime() + ".json", str));
        
        stageController.StopAllCoroutines();
        //dispenser.screenRed.SetActive(false);
        //dispenser.screenGreen.SetActive(false);
        //dispenser.enabled = false;
        //monster.speed = 0f;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            obj.SetActive(false);
        }
        foreach (AudioSource aud in FindObjectsOfType(typeof(AudioSource)) as AudioSource[])
        {
            aud.Stop();
        }
        EndLevel(0f);
    }
}

