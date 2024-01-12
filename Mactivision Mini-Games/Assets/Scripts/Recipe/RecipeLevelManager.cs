using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// This class manages the majority of the game functionality
public class RecipeLevelManager : LevelManager
{
    public Animator monster;                // monster that eats food
    public Transform plate;                 // plate that the food GameObject falls onto, will be tilted
    public Transform lever;                 // purely for animation purposes
    public RecipeDispenser dispenser;             // responsible for choosing and dispensing foods
    public AudioClip plate_up;              // BRRRRRR
    public AudioClip plate_down;            // brrrrrr

    public AudioSource bgmPlayer;
    public AudioClip bgm;

    public GameObject chest;

    int uniqueFoods;                         // number of foods to be used in the current game
    float avgUpdateFreq;                    // average number of foods dispensed between each food update
    float updateFreqVariance;               // variance of `avgUpdateFreq`

    int maxFoodDispensed;                   // maximum foods dispensed before game ends
    int foodDispensed;

    KeyCode feedKey = KeyCode.RightArrow;   // press to feed monster
    KeyCode trashKey = KeyCode.LeftArrow;   // press to throw away

    RecipeChoiceMetric rcMetric;            // records choice data during the game
    MetricJSONWriter metricWriter;          // outputs recording metric (mcMetric) as a json file

    float tiltPlateTo;                      // angle to tilt the plate (food will slide into trash or monster's mouth)
    bool correct;

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

        countDoneText = "Feed!";
        correct = false;

        rcMetric = new RecipeChoiceMetric(); // initialize metric recorder

        dispenser.Init(seed, uniqueFoods, avgUpdateFreq, updateFreqVariance); // initialize the dispenser
        MakeNewChest();
    }

    // Initialize values using config file, or default values if config values not specified
    void InitConfigurable()
    {
        RecipeConfig recipeConfig = new RecipeConfig();

        // if running the game from the battery, override `feederConfig` with the config class from Battery
        RecipeConfig tempConfig = (RecipeConfig)Battery.Instance.GetCurrentConfig();
        if (tempConfig != null)
        {
            recipeConfig = tempConfig;
        }
        else
        {
            Debug.Log("Battery not found, using default values");
        }

        // use battery's config values, or default values if running game by itself
        seed = !String.IsNullOrEmpty(recipeConfig.Seed) ? recipeConfig.Seed : DateTime.Now.ToString(); // if no seed provided, use current DateTime
        maxGameTime = recipeConfig.MaxGameTime > 0 ? recipeConfig.MaxGameTime : Default(90f, "MaxGameTime");
        maxFoodDispensed = recipeConfig.MaxFoodDispensed > 0 ? recipeConfig.MaxFoodDispensed : Default(20, "MaxFoodDispensed");
        uniqueFoods = recipeConfig.UniqueFoods >= 2 && recipeConfig.UniqueFoods <= dispenser.allFoods.Length ? recipeConfig.UniqueFoods : Default(11, "UniqueFoods");
        avgUpdateFreq = recipeConfig.AverageUpdateFrequency > 0 ? recipeConfig.AverageUpdateFrequency : Default(3f, "AverageUpdateFrequency");
        updateFreqVariance = recipeConfig.UpdateFreqVariance >= 0 && recipeConfig.UpdateFreqVariance <= 1 ? recipeConfig.UpdateFreqVariance : Default(0.3f, "UpdateFreqVariance");

        // udpate battery config with actual/final values being used
        recipeConfig.Seed = seed;
        recipeConfig.MaxGameTime = maxGameTime;
        recipeConfig.MaxFoodDispensed = maxFoodDispensed;
        recipeConfig.UniqueFoods = uniqueFoods;
        recipeConfig.AverageUpdateFrequency = avgUpdateFreq;
        recipeConfig.UpdateFreqVariance = updateFreqVariance;
    }

    // Handles GUI events (keyboard, mouse, etc events)
    void OnGUI()
    {
        Event e = Event.current;
        // navigate through the instructions before the game starts
        if (lvlState == 0 && e.type == EventType.KeyUp)
        {
            if (e.keyCode == KeyCode.LeftArrow && instructionCount > 0)
            {
                ShowInstruction(--instructionCount);
            }
            else if (e.keyCode == KeyCode.RightArrow && instructionCount < instructions.Length)
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
            if (!rcMetric.isRecording) StartGame();

            // game automatically ends after maxGameTime seconds
            if (Time.time - gameStartTime >= maxGameTime || foodDispensed >= maxFoodDispensed)
            {
                EndGame();
                return;
            }

            // The game cycle
            switch (gameState)
            {
                case GameState.DispensingFood:
                    break;
                case GameState.WaitingForPlayer:
                    WaitForPlayer();
                    break;
                case GameState.TiltingPlate:
                    TiltPlate();
                    break;
                case GameState.FoodExpended:
                    FoodExpended();
                    break;
            }
        }
    }

    // Begin the actual game, start recording metrics
    void StartGame()
    {
        rcMetric.startRecording();
        metricWriter = new MetricJSONWriter("Recipe", DateTime.Now, seed); // initialize metric data writer
        gameStartTime = Time.time;
        bgmPlayer.loop = true;
        bgmPlayer.clip = bgm;
        bgmPlayer.Play();
    }

    // End game, stop animations, sounds, physics. Finish recording metrics
    void EndGame()
    {
        rcMetric.finishRecording();
        var str = metricWriter.GetLogMetrics(
                    DateTime.Now,
                    new List<AbstractMetric>() { rcMetric }
                );
        StartCoroutine(Post("recipe_" + DateTime.Now.ToFileTime() + ".json", str));

        dispenser.StopAllCoroutines();
        dispenser.screenRed.SetActive(false);
        dispenser.screenGreen.SetActive(false);
        dispenser.enabled = false;
        //monster.speed = 0f;
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

    // This function is called each frame the game is waiting for input from the player.
    // When the player makes a choice, it plays appropriate animations and  
    // records the metric event, and starts the choice wait coroutine.
    void WaitForPlayer()
    {
        if (Input.GetKeyDown(feedKey) || Input.GetKeyDown(trashKey))
        {
            // set the angle the plate should tilt to. Play monster eating animation & sound if applicable
            if (Input.GetKeyDown(feedKey))
            {
                //monster.Play("Base Layer.monster_eat");
                //sound.PlayDelayed(0.85f);
                tiltPlateTo = -33f;
                StartCoroutine(WaitForNewChest());
            }
            else
            {
                tiltPlateTo = 33f;
            }

            correct = dispenser.MakeChoice(Input.GetKeyDown(feedKey));

            // record the choice made
            rcMetric.recordEvent(new RecipeChoiceEvent(
                dispenser.choiceStartTime,
                new List<String>(dispenser.goodFoods),
                dispenser.dispensed,
                Input.GetKeyDown(feedKey),
                correct,
                DateTime.Now
            ));

            // animate choice and play plate sound
            sound.PlayOneShot(plate_up);
            StartCoroutine(AnimateChoice(Input.GetKeyDown(feedKey) && !dispenser.MakeChoice(Input.GetKeyDown(feedKey))));
            gameState = GameState.TiltingPlate;
        }
    }

    // This function tilts the plate by a small increment. When called over multiple
    // successive frames, it animates smoothly. If the plate reaches the intended
    // tilt amount, it stays there. 
    void TiltPlate()
    {
        float tiltSpeed = 100f;
        Vector3 rot = lever.localEulerAngles;
        rot.z = rot.z < 180f ? rot.z : rot.z - 360f;
        float oldZ = rot.z;
        rot.z = Mathf.MoveTowards(rot.z, tiltPlateTo, tiltSpeed * Time.deltaTime);
        if (tiltPlateTo < 0f || oldZ < 0f)
        {
            plate.GetChild(0).localEulerAngles = rot;
        }
        else
        {
            plate.localEulerAngles = rot;
        }
        lever.localEulerAngles = rot;
    }

    // This function is called when the food has been dealt with (trashed or feed).
    // Dispenses the next food and starts dispensing wait coroutine.
    void FoodExpended()
    {
        if (dispenser.DispenseNext())
        {
            StartCoroutine(WaitForFoodDispense(2.55f));
        }
        else
        {
            StartCoroutine(WaitForFoodDispense(0.8f));
        }
        gameState = GameState.DispensingFood;
    }

    void MakeNewChest()
    {
        Instantiate(chest, new Vector3(3.65f, -2.01f, 0), Quaternion.identity);
    }
    // Wait for the choice animation to finish. If the player feeds the monster
    // incorrectly, wait longer for the monster spit animation.
    IEnumerator AnimateChoice(bool spit)
    {
        // return the plate to the original resting position
        yield return new WaitForSeconds(1.3f);
        tiltPlateTo = 0f;
        sound.PlayOneShot(plate_down);

        // Make sure angles are zeroed, and finish animation
        yield return new WaitForSeconds(spit ? 2.5f : 1.33f);
        lever.localEulerAngles = Vector3.zero;
        plate.localEulerAngles = Vector3.zero;
        plate.GetChild(0).localEulerAngles = Vector3.zero;
        // animatingChoice = false;
        foodDispensed++;
        gameState = GameState.FoodExpended;
    }

    // Wait for the food dispensing animation
    IEnumerator WaitForFoodDispense(float wait)
    {
        yield return new WaitForSeconds(wait);
        gameState = GameState.WaitingForPlayer;
    }

    IEnumerator WaitForNewChest()
    {
        yield return new WaitForSeconds(3.5f);
        MakeNewChest();
    }
}
