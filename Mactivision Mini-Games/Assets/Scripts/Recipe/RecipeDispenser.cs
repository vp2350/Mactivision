using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// This class is responsible for managing the games foods, and dispensing of foods.
public class RecipeDispenser : MonoBehaviour
{
    public Animator pipe;               // this class will play the pipe dispensing animation
    public Transform monitor;           // the monitor that shows food updates
    public GameObject screenGreen;      // monitor flashes green when food is updated to preferred
    public GameObject screenRed;        // monitor flashes red when food is updated to unpreferred
    public GameObject[] allFoods;       // array of all possible foods
    public AudioClip dispense_sound;    // sound played when a food is dispensed from the pipe
    public AudioClip screen_sound;      // sound played when there is a food update
    AudioSource sound;

    System.Random randomSeed;   // seed of the current game
    float avgUpdateFreq;        // average number of foods dispensed between each food update
    float updateFreqVariance;   // variance of `avgUpdateFreq`
    int lastUpdate = 0;         // number of foods dispensed since last food update
    int nextUpdate = 0;

    string[] gameFoods;                                 // foods being used in the current game
    GameObject[] gameFoodObjs;
    public string[] goodFoods { private set; get; }     // foods the monster will eat
    public GameObject[] goodFoodObjs;
    string[] badFoods;                                  // foods the monster will spit out
    GameObject[] badFoodObjs;
    int goodFoodCount = 0;                              // number of foods the monster likes

    bool wasCorrectDispensed;

    GameObject screenFood1;                                  // the food shown on the screen during a food update
    GameObject screenFood2;                                  // the food shown on the screen during a food update

    public string currentFood { private set; get; }         // the current food the player has to decide on
    public DateTime choiceStartTime { private set; get; }   // the time the current food is dispensed and the player can make a choice

    // Initializes the dispenser with the seed.
    // Randomly chooses which foods will be used in the game.
    // `gameFoods` has the list of `tf` food items that will
    // be used in the game, and initially, `badFoods` = `gameFoods`,
    // as all foods begin "unpreferred". `goodFoods` is an array of empty
    // strings the same length as `gameFoods` and `badFoods`.
    public void Init(string seed, int tf, float uf, float sd)
    {
        screenGreen.SetActive(false);
        screenRed.SetActive(false);
        foreach (GameObject obj in allFoods) obj.SetActive(false);

        sound = gameObject.GetComponent<AudioSource>();

        randomSeed = new System.Random(seed.GetHashCode());
        avgUpdateFreq = uf;
        updateFreqVariance = sd;
        
        gameFoods = new string[tf];
        gameFoodObjs = new GameObject[tf];
        goodFoods = new string[tf];
        goodFoodObjs = new GameObject[2];
        badFoods = new string[tf - 2];
        badFoodObjs = new GameObject[tf - 2];

        wasCorrectDispensed = false;
        // randomly select the foods that will be used in the game 
        int i = 0;
        while (i<tf) {
            int rand = randomSeed.Next(allFoods.Length);
            string food = allFoods[rand].name;
            if (Array.IndexOf(gameFoods, food)<0) {
                gameFoods[i] = food;
                gameFoodObjs[i] = allFoods[i];
                goodFoods[i] = "";
                i++;
            }
        }
    }

    // Decides whether to update the list of liked foods.
    // Returns whether there is an update or not
    public bool DispenseNext()
    {
        bool update = false;

        if (++lastUpdate >= nextUpdate || goodFoodCount==0) {
            UpdateFoods();
            update = true;
            lastUpdate = 0;
            nextUpdate = FoodsBetweenNextUpdate(avgUpdateFreq, updateFreqVariance);
            StartCoroutine(WaitForFoodUpdate(1.75f)); // wait for a food update and then dispense next
        } else {
            StartCoroutine(WaitForFoodUpdate(0f)); // instantly dispense next food
        }

        return update;
    }

    // Returns whether the choice to `feed` was correct or not
    public bool MakeChoice(bool feed)
    {
        return (wasCorrectDispensed == feed);
    }

    // The actual function that randomly chooses a food and dispenses it.
    // Sets the `choiceStartTime` to the current time and activate the 
    // food GameObject and places it "in the pipe".
    // Physics does the rest to make it fall out of the pipe.
    void Dispense()
    {
        int n = gameFoods.Length;
        GameObject[] tempFoods = new GameObject[n];
        for (int i = 0; i < n; i++)
        {
            tempFoods[i] = gameFoodObjs[i];
            tempFoods[i].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            tempFoods[i].transform.eulerAngles = Vector3.zero;
        }
        while (n > 1)
        {
            n--;
            int k = randomSeed.Next(n + 1);
            GameObject value = tempFoods[k];
            tempFoods[k] = tempFoods[n];
            tempFoods[n] = value;
        }

        // find the current food GameObject and place it in the pipe
        foreach (GameObject obj in allFoods) {
            if (obj.name==currentFood) {
                obj.SetActive(true);
                obj.transform.position = new Vector3(1f, 4f, 0f);
                break;
            }
        }

        int rand = randomSeed.Next(5);
        if (rand == 0)
        {
            wasCorrectDispensed = true;
            goodFoodObjs[0].SetActive(true);
            goodFoodObjs[0].transform.position = new Vector3(-1f, 4f, 0f);
            
            goodFoodObjs[1].SetActive(true);
            goodFoodObjs[1].transform.position = new Vector3(1f, 4f, 0f);
        }
        else
        {
            wasCorrectDispensed = false;
            tempFoods[0].SetActive(true);
            tempFoods[0].transform.position = new Vector3(-1f, 4f, 0f);

            tempFoods[1].SetActive(true);
            tempFoods[1].transform.position = new Vector3(1f, 4f, 0f);
        }

        // dispensing animation and sound
        pipe.Play("Base Layer.pipe_dispense");
        sound.clip = dispense_sound;
        sound.PlayDelayed(0f);
    }
 
    // Update the list of good and bad foods. Essentially swaps items between
    // the good and bad lists. If there are less than two foods in the good list, 
    // it will move a food from the bad list to the good list. 
    // On update, this function will activate the flashing green or red screen
    // depending on if a food was moved to the good or bad list, respectively.
    void UpdateFoods()
    {
        // choose a food to update and swap it to the other list
        int randIdx = randomSeed.Next(gameFoods.Length);
        int n = gameFoods.Length;
        GameObject[] tempFoods = new GameObject[n];
        for(int i = 0; i < n; i++)
        {
            tempFoods[i] = gameFoodObjs[i];
        }
        while (n > 1)
        {
            n--;
            int k = randomSeed.Next(n + 1);
            GameObject value = tempFoods[k];
            tempFoods[k] = tempFoods[n];
            tempFoods[n] = value;
        }

        string foodLeft = tempFoods[0].name;
        string foodRight = tempFoods[1].name;

        goodFoodObjs[0] = tempFoods[0];
        goodFoodObjs[1] = tempFoods[1];
        goodFoods[0] = tempFoods[0].name;
        goodFoods[1] = tempFoods[1].name;
        goodFoodCount = 2;

        for (int i = 2; i < n; i++)
        {
            badFoodObjs[i - 2] = tempFoods[i];
            badFoods[i - 2] = tempFoods[i].name;
        }

        //badFoods[randIdx] = goodFoods[randIdx];
        //goodFoods[randIdx] = food;
        //
        //// show the food to be updated on the monitor, and activate either
        //// the green or red flashing screen animation and sound
        //if (food=="") {
        //    food = badFoods[randIdx];
        //    screenRed.SetActive(true);
        //    goodFoodCount--;
        //} else {
        screenGreen.SetActive(true);
        //    goodFoodCount++;
        //}
        foodLeft = foodLeft + " (screenLeft)";
        screenFood1 = monitor.Find(foodLeft).gameObject;
        screenFood1.SetActive(true);

        foodRight = foodRight + " (screenRight)";
        screenFood2 = monitor.Find(foodRight).gameObject;
        screenFood2.SetActive(true);
        sound.PlayOneShot(screen_sound);
    }

    // Wait for the flashing screen animation and then dispense the next food.
    IEnumerator WaitForFoodUpdate(float wait)
    {
        yield return new WaitForSeconds(wait);
        screenGreen.SetActive(false);
        screenRed.SetActive(false);
        screenFood1.SetActive(false);
        screenFood2.SetActive(false);

        Dispense();
    }

    int FoodsBetweenNextUpdate(float avg, float sd) {
        float rand = (float)randomSeed.NextDouble();
        return (int)Mathf.Round((sd+0.1333f)*30f*Mathf.Pow(rand-0.5f, 3f)+avg);
    }
}
