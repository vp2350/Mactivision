using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

// This class is responsible for managing the games foods, and dispensing of foods.
public class LookingDisplays : MonoBehaviour
{
    private System.Random rng;
    public Animator pipe;               // this class will play the pipe dispensing animation
    //public Transform monitor;           // the monitor that shows food updates
    public GameObject screenGreen1;      // monitor flashes green when food is updated to preferred
    public GameObject screenGreen2;        // monitor flashes red when food is updated to unpreferred
    public GameObject[] allFoods;       // array of all possible foods
    public GameObject goodObject;
    public List<GameObject> badObjects;
    public AudioClip display_sound;    // sound played when a food is dispensed from the pipe
    public AudioClip prompt_sound;      // sound played when there is a food update
    AudioSource sound;

    bool goodObjectShown;
    public List<string> objectsDisplayed;

    System.Random randomSeed;   // seed of the current game
    float avgUpdateFreq;        // average number of foods dispensed between each food update
    float updateFreqVariance;   // variance of `avgUpdateFreq`
    int lastUpdate = 0;         // number of foods dispensed since last food update
    int nextUpdate = 0;

    string[] gameFoods;                                 // foods being used in the current game
    public string goodFood { private set; get; }     // foods the monster will eat
    public int goodFoodNumber { private set; get; }
    string[] badFoods;                                  // foods the monster will spit out
    int goodFoodCount = 0;                              // number of foods the monster likes

    GameObject[] objectsUsed;
    int correctMonitor;

    Vector3[] spawns;
    Vector3[] promptPoints;

    public int cash;
    public GameObject cashCounter;
    public TextMeshPro cashCounterText;

    //GameObject screenFood;                                  // the food shown on the screen during a food update
    //public string currentFood { private set; get; }         // the current food the player has to decide on
    public DateTime choiceStartTime { private set; get; }   // the time the current food is dispensed and the player can make a choice

    // Initializes the dispenser with the seed.
    // Randomly chooses which foods will be used in the game.
    // `gameFoods` has the list of `tf` food items that will
    // be used in the game, and initially, `badFoods` = `gameFoods`,
    // as all foods begin "unpreferred". `goodFoods` is an array of empty
    // strings the same length as `gameFoods` and `badFoods`.
    public void Init(string seed, int tf, float uf, float sd, Vector3[] spawnPoints, Vector3[] promptLocations, GameObject[] foods)
    {
        rng = new System.Random();

        cash = 0;
        cashCounterText = cashCounter.GetComponent<TextMeshPro>();
        cashCounterText.text = "$" + cash.ToString();

        allFoods = new GameObject[foods.Length];
        for(int i = 0; i< foods.Length; i++)
        {
            allFoods[i] = foods[i];
        }

        goodObjectShown = false;

        screenGreen1.SetActive(false);
        screenGreen2.SetActive(false);
        foreach (GameObject obj in allFoods) obj.SetActive(false);

        sound = gameObject.GetComponent<AudioSource>();

        randomSeed = new System.Random(seed.GetHashCode());
        avgUpdateFreq = uf;
        updateFreqVariance = sd;

        gameFoods = new string[tf];

        goodFood = allFoods[randomSeed.Next(allFoods.Length)].name;

        spawns = spawnPoints;
        promptPoints = promptLocations;

        objectsUsed = new GameObject[4];
    }

    // Decides whether to update the list of liked foods.
    // Returns whether there is an update or not
    public bool DisplayNext()
    {
        bool update = false;
        foreach(GameObject obj in allFoods)
        {
            obj.SetActive(false);
        }

        if (++lastUpdate >= nextUpdate || goodFoodCount == 0)
        {
            UpdateObjects();
            objectsDisplayed.Clear();

            update = true;
            lastUpdate = 0;
            nextUpdate = ObjectsBetweenNextUpdate(avgUpdateFreq, updateFreqVariance);
            StartCoroutine(WaitForObjectUpdate(1.75f)); // wait for a food update and then dispense next
        }
        else
        {
            StartCoroutine(WaitForObjectUpdate(0f)); // instantly dispense next food
        }

        return update;
    }

    //Returns whether the input matches correct item
    public bool MakeChoice(int inputNumber)
    {
        bool result = false;
        if (inputNumber > -1 && inputNumber < 4)
        {
            result = (objectsUsed[inputNumber].name == goodObject.name) ? true : false;
        }
        else
        {
            if(!goodObjectShown)
            {
                result = true;
            }
        }

        if(result)
        {
            cash += 100;
            cashCounterText.text = cash.ToString();
        }

        return result;
    }

    // The actual function that randomly chooses a food and dispenses it.
    // Sets the `choiceStartTime` to the current time and activate the 
    // food GameObject and places it "in the pipe".
    // Physics does the rest to make it fall out of the pipe.
    void Display()
    {
        int randIdx;
        randIdx = randomSeed.Next(2);
        correctMonitor = randomSeed.Next(4);
        choiceStartTime = DateTime.Now;
        objectsUsed = new GameObject[4];
        objectsDisplayed = new List<string>();
        goodObjectShown = false;

        int n = badObjects.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            GameObject value = badObjects[k];
            badObjects[k] = badObjects[n];
            badObjects[n] = value;
        }

        if (randIdx == 0)
        {
            for(int i = 0; i < 4; i++)
            {
                if(i == correctMonitor)
                {
                    goodObject.SetActive(true);
                    goodObjectShown = true;
                    goodObject.transform.position = new Vector3(spawns[correctMonitor].x, spawns[correctMonitor].y, -1);
                    objectsUsed[i] = goodObject;
                    objectsDisplayed.Add(goodObject.name);
                }
                else
                {
                    badObjects[i].SetActive(true);
                    badObjects[i].transform.position = new Vector3(spawns[i].x, spawns[i].y, -1);
                    objectsUsed[i] = badObjects[i];
                    objectsDisplayed.Add(badObjects[i].name);
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {    
                badObjects[i].SetActive(true);
                badObjects[i].transform.position = new Vector3(spawns[i].x, spawns[i].y, -1);
                objectsUsed[i] = badObjects[i];
            }
        }
        // find the current food GameObject and place it in the pipe
        //foreach (GameObject obj in allFoods)
        //{
        //    if (obj.name == currentFood)
        //    {
        //        obj.SetActive(true);
        //        obj.transform.position = new Vector3(-1f, 4f, 0f);
        //        break;
        //    }
        //}

        // dispensing animation and sound
        //pipe.Play("Base Layer.pipe_dispense");
        //sound.clip = dispense_sound;
        //sound.PlayDelayed(0f);
    }

    // Update the list of good and bad foods. Essentially swaps items between
    // the good and bad lists. If there are less than two foods in the good list, 
    // it will move a food from the bad list to the good list. 
    // On update, this function will activate the flashing green or red screen
    // depending on if a food was moved to the good or bad list, respectively.
    void UpdateObjects()
    {
        int rand = randomSeed.Next(allFoods.Length);
        goodFood = allFoods[rand].name;
        goodObject = allFoods[rand];
        goodFoodCount++;
        goodObject.SetActive(true);
        goodObject.transform.position = new Vector3(promptPoints[0].x, promptPoints[0].y, -1);

        badObjects.Clear();
        for(int i = 0; i < allFoods.Length; i++)
        {
            if (allFoods[i].name != goodObject.name)
            {
                badObjects.Add(allFoods[i]);
            }    
        }

        screenGreen1.SetActive(true);
        screenGreen2.SetActive(true);
        // choose a food to update and swap it to the other list
        //int randIdx = randomSeed.Next(gameFoods.Length);
        //string food = badFoods[randIdx];
        //if (goodFoodCount < 2)
        //{
        //    do
        //    {
        //        randIdx = randomSeed.Next(gameFoods.Length);
        //        food = badFoods[randIdx];
        //    } while (food == "");
        //}
        //badFoods[randIdx] = goodFoods[randIdx];
        //goodFoods[randIdx] = food;
        //
        //// show the food to be updated on the monitor, and activate either
        //// the green or red flashing screen animation and sound
        //if (food == "")
        //{
        //    food = badFoods[randIdx];
        //    screenRed.SetActive(true);
        //    goodFoodCount--;
        //}
        //else
        //{
        //    screenGreen.SetActive(true);
        //    goodFoodCount++;
        //}
        //food = food + " (screen)";
        //screenFood = monitor.Find(food).gameObject;
        //screenFood.SetActive(true);
        //sound.PlayOneShot(screen_sound);
    }

    // Wait for the flashing screen animation and then dispense the next food.
    IEnumerator WaitForObjectUpdate(float wait)
    {
        yield return new WaitForSeconds(wait);
        screenGreen1.SetActive(false);
        screenGreen2.SetActive(false);
        goodObject.SetActive(false);
        foreach(GameObject obj in badObjects)
        {
            obj.SetActive(false);
        }
        Display();
    }

    int ObjectsBetweenNextUpdate(float avg, float sd)
    {
        float rand = (float)randomSeed.NextDouble();
        return (int)Mathf.Round((sd + 0.1333f) * 30f * Mathf.Pow(rand - 0.5f, 3f) + avg);
    }

    public List<string> GetObjectsShown()
    {
        return objectsDisplayed;
    }
    
}
