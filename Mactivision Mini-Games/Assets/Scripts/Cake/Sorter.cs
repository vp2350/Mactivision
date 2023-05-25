using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sorter : MonoBehaviour
{
    float velocity;         // just x velocity because y doesn't change
    float minPos = -2.75f;   // the minimum value for position (left)
    float maxPos = -0.125f;    // the maximum value for position (right)

    public GameObject redUp;
    public GameObject redDown;
    public GameObject orangeUp;
    public GameObject orangeDown;

    // Initializes the spotlight
    public void Init(float v)
    {
        velocity = 3.5f;
    }

    private void Start()
    {
        Init(2f);
    }
    private void Update()
    {
        if(Input.GetKey(KeyCode.DownArrow))
        {
            Move(false);
            redDown.SetActive(false);
            orangeDown.SetActive(true);
            redUp.SetActive(true);
            orangeUp.SetActive(false);
        }
        else if(Input.GetKey(KeyCode.UpArrow))
        {
            Move(true);
            redDown.SetActive(true);
            orangeDown.SetActive(false);
            redUp.SetActive(false);
            orangeUp.SetActive(true);
        }
        else
        {
            redUp.SetActive(true);
            redDown.SetActive(true);

            orangeUp.SetActive(false);
            orangeDown.SetActive(false);
        }
    }

    // Move the spotlight left and right
    // Parameter `right` is true to move right, false to move left
    public void Move(bool right)
    {

        if (right && gameObject.transform.position.y <= maxPos)
            gameObject.transform.Translate(Vector3.up * velocity * Time.deltaTime);

        else if (!right && gameObject.transform.position.y >= minPos)
            gameObject.transform.Translate(Vector3.down * velocity * Time.deltaTime);
    }

    // Returns the spotlight's position
    public Vector2 GetPosition()
    {
        Vector2 pos = gameObject.transform.position;
        return pos;
    }
}
