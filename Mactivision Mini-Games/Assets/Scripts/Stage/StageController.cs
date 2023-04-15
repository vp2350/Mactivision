using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.UI.Image;
using UnityEngine.UIElements;
using UnityEditor.PackageManager;

// This class is responsible for managing the games foods, and dispensing of foods.
public class StageController : MonoBehaviour
{

    System.Random randomSeed;   // seed of the current game

    int difficulty;
    int playerCount;
    int rowMax;
    float maxDistance;

    int falseShephard;
    bool faked;

    public GameObject[] spawns = new GameObject[3];

    int lastUpdate = 0;         // number of foods dispensed since last food update
    int nextUpdate = 0;

    public List<GameObject> spawnedPlayers = new List<GameObject>();
    public List<Color> playerColors = new List<Color>();

    public GameObject playerPrefab;

    public List<GameObject> playerTypes = new List<GameObject>();
    public DateTime choiceStartTime { private set; get; }   // the time the current food is dispensed and the player can make a choice

    // Initializes the dispenser with the seed.
    // Randomly chooses which foods will be used in the game.
    // `gameFoods` has the list of `tf` food items that will
    // be used in the game, and initially, `badFoods` = `gameFoods`,
    // as all foods begin "unpreferred". `goodFoods` is an array of empty
    // strings the same length as `gameFoods` and `badFoods`.
    public void Init(string seed, int diff)
    {

        playerCount = 0;
        randomSeed = new System.Random(seed.GetHashCode());
        difficulty = diff;
        UpdatePlayerCount();
        maxDistance = 10f;
    }

    public void UpdateDiff(int diff)
    {
        difficulty = diff;
        UpdatePlayerCount();
    }

    void UpdatePlayerCount()
    {
        switch (difficulty)
        {
            case 1:
                playerCount = 6;
                rowMax = 3;
                break;
            case 2:
                playerCount = 9;
                rowMax = 4;
                break;
            case 3:
                playerCount = 12;
                rowMax = 5;
                break;
        }
    }

    // Decides whether to update the list of liked foods.
    // Returns whether there is an update or not
    public void SpawnNext(bool secondDisplay)
    {
        faked = false;

        for (int i = 0; i < spawnedPlayers.Count; i++)
        {
            Destroy(spawnedPlayers[i]);
        }
        spawnedPlayers.Clear();
        playerColors.Clear();

        Spawn(secondDisplay);
        //Walk();
    }

    void Spawn(bool secondDisplay)
    {
        int maxForThis = rowMax;
        int left = -1;

        for (int i = 0; i < 3; i++)
        {
            for (int j = maxForThis; j >= maxForThis - 2; j--)
            {
                GameObject tempPlayer = Instantiate(playerPrefab);
                tempPlayer.transform.position = new Vector3(tempPlayer.transform.position.x + (maxForThis * left), tempPlayer.transform.position.y, tempPlayer.transform.position.z);

                SpriteRenderer temp = tempPlayer.GetComponent<SpriteRenderer>();
                float r = randomSeed.Next(255);
                float g = randomSeed.Next(255);
                float b = randomSeed.Next(255);
                Debug.Log(r + " " + g + " " + b);
                Color tempColor = new Color(0, 0, 0, 1f);
                temp.color = tempColor;

                spawnedPlayers.Add(tempPlayer);
                playerColors.Add(tempColor); 
            }
            maxForThis -= 1;
            left = -left;
        }

        for(int i = 0; i < spawnedPlayers.Count; i++)
        {
            SpriteRenderer temp = spawnedPlayers[i].GetComponent<SpriteRenderer>();
            float r = randomSeed.Next(255);
            float g = randomSeed.Next(255);
            float b = randomSeed.Next(255);
            Color tempColor = new Color(r, g, b);
            temp.color = tempColor;
        }

        if(secondDisplay)
        {
            int randNew = randomSeed.Next(5);
            if(randNew == 0 || randNew == 1)
            {
                randNew = randomSeed.Next(spawnedPlayers.Count);
                GameObject objectToChange = spawnedPlayers[randNew];

                SpriteRenderer temp = objectToChange.GetComponent<SpriteRenderer>();
                Color tempColor = new Color(randomSeed.Next(255), randomSeed.Next(255), randomSeed.Next(255), 1f);
                temp.color = tempColor;

                playerColors[randNew] = tempColor;
                faked = true;
                falseShephard = randNew;
            }
        }
    }

    // Returns whether the choice to `feed` was correct or not
    public bool MakeChoice(bool feed)
    {
        return faked == feed ? true : false;
    }

    // The actual function that randomly chooses a food and dispenses it.
    // Sets the `choiceStartTime` to the current time and activate the 
    // food GameObject and places it "in the pipe".
    // Physics does the rest to make it fall out of the pipe.
    void Walk()
    {
        Debug.Log("Called");
        int maxForThis = rowMax;
        int left = -1;
        int playerNumber = 0;
        float speed = 2f;
        for(int i = 0; i < 3 ; i++)
        {
            for (int j = maxForThis; j >= maxForThis - 2 && playerNumber < spawnedPlayers.Count; j--)
            {
                spawnedPlayers[playerNumber].GetComponent<Rigidbody2D>().MovePosition(new Vector2(spawnedPlayers[playerNumber].transform.position.x + ((maxDistance - maxDistance/maxForThis) * left),
                    spawnedPlayers[playerNumber].transform.position.y));
                playerNumber++;
            }
            maxForThis -= 1;
            left = -left;
        }
        //int randIdx;
        //randIdx = randomSeed.Next(gameFoods.Length);
    }

    // Wait for the flashing screen animation and then dispense the next food.
    IEnumerator WaitForFoodUpdate(float wait)
    {
        yield return new WaitForSeconds(wait);
        //screenGreen.SetActive(false);
        //screenRed.SetActive(false);
        //screenFood.SetActive(false);
        //Dispense();
    }

    int FoodsBetweenNextUpdate(float avg, float sd)
    {
        float rand = (float)randomSeed.NextDouble();
        return (int)Mathf.Round((sd + 0.1333f) * 30f * Mathf.Pow(rand - 0.5f, 3f) + avg);
    }
}

