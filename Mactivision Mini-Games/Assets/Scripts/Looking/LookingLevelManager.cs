using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingLevelManager : LevelManager
{
    KeyCode leftKey = KeyCode.LeftArrow;
    KeyCode upKey = KeyCode.UpArrow;
    KeyCode rightKey = KeyCode.RightArrow;   // press to feed monster
    KeyCode downKey = KeyCode.DownArrow;   // press to throw away

    public GameObject[] monitors = new GameObject[4];
    Vector3[] spawnPoints = new Vector3[4];

    public GameObject[] prompters = new GameObject[4];
    Vector3[] promptPoints = new Vector3[2];

    public GameObject[] foods = new GameObject[10];

    enum GameState
    {
        Prompting,
        WaitingForPlayer,
        Response
    }
    GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }   

    void Init()
    {
        for(int i = 0; i<monitors.Length; i++)
        {
            spawnPoints[i] = monitors[i].transform.position;
        }
        for (int i = 0; i < prompters.Length; i++)
        {
            promptPoints[i] = monitors[i].transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
