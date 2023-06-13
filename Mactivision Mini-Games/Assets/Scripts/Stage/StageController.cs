using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

// This class is responsible for managing the games foods, and dispensing of foods.
public class StageController : MonoBehaviour
{

    System.Random randomSeed;   // seed of the current game

    int difficulty;
    int playerCount;
    int rowMax;
    float maxDistance;

    Color[] colorList;
    List<int> colorsShown;

    public int falseShephard;
    public bool faked;
    public Color ogColor;
    public int ogColorNumber;
    public int changedColorNumber;

    public float startTime;
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
        startTime = Time.time;
        playerCount = 0;
        randomSeed = new System.Random(seed.GetHashCode());
        Debug.Log(seed.GetHashCode());
        difficulty = diff;
        UpdatePlayerCount();
        maxDistance = 20f;
        falseShephard = -1;
        ogColor = new Color(0, 0, 0);
        colorsShown = new List<int>();

        colorList = new Color[13] {  
            new Color(173/255f, 216/255f, 230/255f, 1),    //light blue
            new Color(0/255f, 0/255f, 255/255f, 1),    //dark blue
            Color.gray,
            Color.green,
            Color.magenta,
            Color.red,
            Color.white,
            Color.yellow,
            new Color(1f, 105/255f, 180/255f, 1),  //hot pink
            new Color(251/255f, 142/255f, 147/255f, 1), //orange
            new Color(128/255f, 0/255f, 0/255f, 1), //maroon
            new Color(0, 0/255f, 139/255f, 1), //dark blue
            new Color(102/255f, 51/255f, 0/255f, 1) //brown
        };
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
                playerCount = 3;
                rowMax = 2;
                break;
            case 2:
                playerCount = 6;
                rowMax = 3;
                break;
            case 3:
                playerCount = 9;
                rowMax = 4;
                break;
        }
    }

    // Decides whether to update the list of liked foods.
    // Returns whether there is an update or not
    public void SpawnNext(bool secondDisplay)
    {
        if (!secondDisplay)
        {

            for (int i = 0; i < spawnedPlayers.Count; i++)
            {
                Destroy(spawnedPlayers[i]);
            }

            Spawn();
            Walk();
            StartCoroutine(WaitForWalk(10f));
        }
        else
        {
            SpawnOptions();
            choiceStartTime = DateTime.Now;
        }
    }

    void Spawn()
    {
        faked = false;
        ogColor = new Color(0, 0, 0);
        falseShephard = -1;

        spawnedPlayers.Clear();
        playerColors.Clear();
        colorsShown.Clear();

        int maxForThis = rowMax;
        int left = 1;

        int colourCount = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = maxForThis; j > 0; j--)
            {
                GameObject tempPlayer = Instantiate(playerPrefab);
                tempPlayer.transform.position = new Vector3(spawns[i].transform.position.x + (j * maxDistance / (maxForThis + 1) * left), spawns[i].transform.position.y, spawns[i].transform.position.z);

                SpriteRenderer temp = tempPlayer.GetComponent<SpriteRenderer>();
                int colorNext = randomSeed.Next(colorList.Length);
                colorsShown.Add(colorNext);

                Color tempColor = colorList[colorNext];
                temp.color = tempColor;
                playerColors.Add(tempColor);
                
                spawnedPlayers.Add(tempPlayer);
                colourCount++;
                
            }
            maxForThis -= 1;
            left = -left;
        }

    }

    void SpawnOptions()
    {
        Debug.Log("Spawning 2");
        int randNew = randomSeed.Next(5);
        if (randNew == 0 || randNew == 1)
        {
            randNew = randomSeed.Next(spawnedPlayers.Count);
            GameObject objectToChange = spawnedPlayers[randNew];

            int colorNext;

            do
            {
                colorNext = randomSeed.Next(colorList.Length);
            } while (playerColors[randNew].r != colorList[colorNext].r
                && playerColors[randNew].g != colorList[colorNext].g
                && playerColors[randNew].b != colorList[colorNext].b);

            ogColor = playerColors[randNew]; 
            for(int i = 0; i < colorList.Length; i++)
            {
                if(ogColor.r  == colorList[i].r 
                    && ogColor.g == colorList[i].g
                    && ogColor.b == colorList[i].b)
                {
                    ogColorNumber = i; 
                    break;
                }
            }

            //float r = randomSeed.Next(50, 255);
            //float g = randomSeed.Next(50, 255);
            //float b = randomSeed.Next(50, 255);
            SpriteRenderer temp = objectToChange.GetComponent<SpriteRenderer>();
            Color tempColor = (colorList[colorNext]);
            temp.color = tempColor;

            changedColorNumber = colorNext;
            playerColors[randNew] = tempColor;
            faked = true;
            falseShephard = randNew;
        }
        WalkOpp();


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
        int maxForThis = rowMax;
        int left = -1;
        int playerNumber = 0;
        for(int i = 0; i < 3 ; i++)
        {
            for (int j = maxForThis; j > 0 && playerNumber < spawnedPlayers.Count; j--)
            {
                Vector2 endPos = new Vector2(spawnedPlayers[playerNumber].transform.position.x + (maxDistance * left),
                    spawnedPlayers[playerNumber].transform.position.y);
                
                spawnedPlayers[playerNumber].GetComponent<PlayerStageMovement>().SetTarget(endPos);
                playerNumber++;
            }
            maxForThis -= 1;
            left = -left;
        }
        //int randIdx;
        //randIdx = randomSeed.Next(gameFoods.Length);
    }
    void WalkOpp()
    {
        int maxForThis = rowMax;
        int left = 1;
        int playerNumber = 0;
        float speed = 0.2f;
        for (int i = 0; i < 3; i++)
        {
            for (int j = maxForThis; j > 0 && playerNumber < spawnedPlayers.Count; j--)
            {
                Vector2 endPos = new Vector2(spawnedPlayers[playerNumber].transform.position.x + (maxDistance * left),
                    spawnedPlayers[playerNumber].transform.position.y);

                spawnedPlayers[playerNumber].GetComponent<PlayerStageMovement>().SetTarget(endPos);
                playerNumber++;
            }
            maxForThis -= 1;
            left = -left;
        }
        //int randIdx;
        //randIdx = randomSeed.Next(gameFoods.Length);
    }

    // Wait for the flashing screen animation and then dispense the next food.
    IEnumerator WaitForWalk(float wait)
    {
        Debug.Log("Waiting");
        yield return new WaitForSeconds(wait);
        Debug.Log("walking off");
        Walk();
        
        //screenGreen.SetActive(false);
        //screenRed.SetActive(false);
        //screenFood.SetActive(false);
        //Dispense();
    }

    public Color GetOriginalColor()
    {
        if (faked)
            return ogColor;
        else
            return new Color(0, 0, 0);
    }

    public int GetOriginalColorNumber()
    {
        if (faked)
            return ogColorNumber;
        else
            return -1;
    }
    public Color GetChangedColor()
    {
        if (faked)
            return playerColors[falseShephard];
        else
            return new Color(0, 0, 0);
    }
    public int GetChangedColorNumber()
    {
        if (faked)
            return changedColorNumber;
        else
            return -1;
    }
    public List<int> GetColors()
    {
        return colorsShown;
        //return playerColors;
    }
    int FoodsBetweenNextUpdate(float avg, float sd)
    {
        float rand = (float)randomSeed.NextDouble();
        return (int)Mathf.Round((sd + 0.1333f) * 30f * Mathf.Pow(rand - 0.5f, 3f) + avg);
    }
}

