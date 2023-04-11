using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// This class is responsible for managing the games foods, and dispensing of foods.
public class StageController : MonoBehaviour
{

    System.Random randomSeed;   // seed of the current game
    float avgUpdateFreq;        // average number of foods dispensed between each food update
    float updateFreqVariance;   // variance of `avgUpdateFreq`
    int lastUpdate = 0;         // number of foods dispensed since last food update
    int nextUpdate = 0;

    public List<GameObject> playerTypes = new List<GameObject>();
    public DateTime choiceStartTime { private set; get; }   // the time the current food is dispensed and the player can make a choice

    // Initializes the dispenser with the seed.
    // Randomly chooses which foods will be used in the game.
    // `gameFoods` has the list of `tf` food items that will
    // be used in the game, and initially, `badFoods` = `gameFoods`,
    // as all foods begin "unpreferred". `goodFoods` is an array of empty
    // strings the same length as `gameFoods` and `badFoods`.
    public void Init(string seed, int tf, float uf, float sd)
    {

        randomSeed = new System.Random(seed.GetHashCode());
        avgUpdateFreq = uf;
        updateFreqVariance = sd;

    }

    // Decides whether to update the list of liked foods.
    // Returns whether there is an update or not
    public bool WalkNext()
    {
        bool update = false;

       

        return update;
    }

    // Returns whether the choice to `feed` was correct or not
    public bool MakeChoice(bool feed)
    {
        return (Array.IndexOf(goodFoods, currentFood) >= 0 == feed);
    }

    // The actual function that randomly chooses a food and dispenses it.
    // Sets the `choiceStartTime` to the current time and activate the 
    // food GameObject and places it "in the pipe".
    // Physics does the rest to make it fall out of the pipe.
    void Walk()
    {
        int randIdx;
        randIdx = randomSeed.Next(gameFoods.Length);
        currentFood = gameFoods[randIdx];
        choiceStartTime = DateTime.Now;

        // find the current food GameObject and place it in the pipe
        foreach (GameObject obj in allFoods)
        {
            if (obj.name == currentFood)
            {
                obj.SetActive(true);
                obj.transform.position = new Vector3(0f, 4f, 0f);
                break;
            }
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
    void UpdateQueue()
    {
        // choose a food to update and swap it to the other list
        int randIdx = randomSeed.Next(gameFoods.Length);
        string food = badFoods[randIdx];
        if (goodFoodCount < 2)
        {
            do
            {
                randIdx = randomSeed.Next(gameFoods.Length);
                food = badFoods[randIdx];
            } while (food == "");
        }
        badFoods[randIdx] = goodFoods[randIdx];
        goodFoods[randIdx] = food;

        // show the food to be updated on the monitor, and activate either
        // the green or red flashing screen animation and sound
        if (food == "")
        {
            food = badFoods[randIdx];
            screenRed.SetActive(true);
            goodFoodCount--;
        }
        else
        {
            screenGreen.SetActive(true);
            goodFoodCount++;
        }
        food = food + " (screen)";
        screenFood = monitor.Find(food).gameObject;
        screenFood.SetActive(true);
        sound.PlayOneShot(screen_sound);
    }

    // Wait for the flashing screen animation and then dispense the next food.
    IEnumerator WaitForFoodUpdate(float wait)
    {
        yield return new WaitForSeconds(wait);
        screenGreen.SetActive(false);
        screenRed.SetActive(false);
        screenFood.SetActive(false);
        Dispense();
    }

    int FoodsBetweenNextUpdate(float avg, float sd)
    {
        float rand = (float)randomSeed.NextDouble();
        return (int)Mathf.Round((sd + 0.1333f) * 30f * Mathf.Pow(rand - 0.5f, 3f) + avg);
    }
}

