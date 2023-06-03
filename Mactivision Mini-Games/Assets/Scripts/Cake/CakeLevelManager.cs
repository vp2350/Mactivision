using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// This class manages the majority of the game functionality
public class CakeLevelManager : LevelManager
{
    int uniqueFoods;                         // number of foods to be used in the current game
    float avgDispenseFrequency;                    // average number of foods dispensed between each food update
    float foodVelocity;               // variance of `avgUpdateFreq`

    bool dispenseFirst;

    int maxFoodDispensed;                   // maximum foods dispensed before game ends
    int foodDispensed;
    int foodRegistered;

    public GameObject[] allFoods;
   
    CakeChoiceMetric ccMetric;            // records choice data during the game
    MetricJSONWriter metricWriter;          // outputs recording metric (mcMetric) as a json file

    // Represents the state of the game cycle
    enum GameState
    {
        DispensingFood,
        WaitingForPlayer,
        TiltingPlate,
        FoodExpended
    }
    GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        Setup(); // run initial setup, inherited from parent class

        InitConfigurable(); // initialize configurable values

        randomSeed = new System.Random(seed.GetHashCode());
        foodDispensed = 0;
        gameState = GameState.FoodExpended;

        foodRegistered = 0;
        countDoneText = "Feed!";

        ccMetric = new CakeChoiceMetric(); // initialize metric recorder

        //dispenser.Init(seed, uniqueFoods, avgDispenseFrequency, foodVelocity); // initialize the dispenser
        dispenseFirst = false;
    }

    // Initialize values using config file, or default values if config values not specified
    void InitConfigurable()
    {
        CakeConfig cakeConfig = new CakeConfig();

        // if running the game from the battery, override `feederConfig` with the config class from Battery
        CakeConfig tempConfig = (CakeConfig)Battery.Instance.GetCurrentConfig();
        if (tempConfig != null)
        {
            cakeConfig = tempConfig;
        }
        else
        {
            Debug.Log("Battery not found, using default values");
        }

        // use battery's config values, or default values if running game by itself
        seed = !String.IsNullOrEmpty(cakeConfig.Seed) ? cakeConfig.Seed : DateTime.Now.ToString(); // if no seed provided, use current DateTime
        maxGameTime = cakeConfig.MaxGameTime > 0 ? cakeConfig.MaxGameTime : Default(90f, "MaxGameTime");
        maxFoodDispensed = cakeConfig.MaxFoodDispensed > 0 ? cakeConfig.MaxFoodDispensed : Default(20, "MaxFoodDispensed");
        uniqueFoods = cakeConfig.UniqueFoods >= 2 && cakeConfig.UniqueFoods <= allFoods.Length ? cakeConfig.UniqueFoods : Default(9, "UniqueFoods");
        avgDispenseFrequency = cakeConfig.AverageDispenseFrequency > 0 ? cakeConfig.AverageDispenseFrequency : Default(3f, "AverageDispenseFrequency");
        foodVelocity = cakeConfig.FoodVelocity >= 0 && cakeConfig.FoodVelocity <= 10 ? cakeConfig.FoodVelocity : Default(2.25f, "UpdateFreqVariance");

        // udpate battery config with actual/final values being used
        cakeConfig.Seed = seed;
        cakeConfig.MaxGameTime = maxGameTime;
        cakeConfig.MaxFoodDispensed = maxFoodDispensed;
        cakeConfig.UniqueFoods = uniqueFoods;
        cakeConfig.AverageDispenseFrequency = avgDispenseFrequency;
        cakeConfig.FoodVelocity = foodVelocity;
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

    // Update is called once per frame
    void Update()
    {
        if (lvlState == 2)
        {
            // begin game, begin recording 
            if (!ccMetric.isRecording) StartGame();

            // game automatically ends after maxGameTime seconds
            if (Time.time - gameStartTime >= maxGameTime || foodRegistered >= maxFoodDispensed)
            {
                EndGame();
                return;
            }

            if (!dispenseFirst)
            {
                Dispense();
                dispenseFirst = true;
            }
        }
    }

    // Begin the actual game, start recording metrics
    void StartGame()
    {
        ccMetric.startRecording();
        metricWriter = new MetricJSONWriter("Feeder", DateTime.Now, seed); // initialize metric data writer
        gameStartTime = Time.time;
    }

    // End game, stop animations, sounds, physics. Finish recording metrics
    void EndGame()
    {
        ccMetric.finishRecording();
        var str = metricWriter.GetLogMetrics(
                    DateTime.Now,
                    new List<AbstractMetric>() { ccMetric }
                );
        StartCoroutine(Post("cake_" + DateTime.Now.ToFileTime() + ".json", str));

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Food"))
        {
            obj.GetComponent<Rigidbody2D>().isKinematic = true;
            obj.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            obj.GetComponent<Rigidbody2D>().angularVelocity = 0f;
        }
        foreach (AudioSource aud in FindObjectsOfType(typeof(AudioSource)) as AudioSource[])
        {
            aud.Stop();
        }
        EndLevel(0f);
    }

    void Dispense()
    {
        int rand = randomSeed.Next(uniqueFoods);
        GameObject tempFood = Instantiate(allFoods[rand], new Vector3(-4f, -2.35f, -2f), Quaternion.identity);
        tempFood.GetComponent<MoveFood>().Init(2f);
        foodDispensed++;
	if (Time.time - gameStartTime <= maxGameTime && foodDispensed < maxFoodDispensed)
        {
           StartCoroutine(DispenseNext(avgDispenseFrequency));
        }
    }

    public void RecordEvent(int objectNumber, string objectName, int boxNumber, bool correct, DateTime choiceStartTime)
    {
        ccMetric.recordEvent(new CakeChoiceEvent(
                choiceStartTime,
                objectNumber,
                objectName,
                boxNumber,
                correct,
                DateTime.Now
            ));
        foodRegistered++;
    }

    IEnumerator DispenseNext(float frequency)
    {
        yield return new WaitForSeconds(frequency);
        Dispense();
    }
}
