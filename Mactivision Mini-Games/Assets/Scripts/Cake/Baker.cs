using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baker : MonoBehaviour
{
    public Animator rockstar;   // rockstar gameobject, used to animate red guy

    float velocity;         // just x velocity because y doesn't change
    float minPos = -2f;   // the minimum value for position (left)
    float maxPos = 2f;    // the maximum value for position (right)

    bool right = true;
    bool isHolding;

    // Initializes the spotlight
    void Start()
    {
        isHolding = false;
        velocity = 0f;
    }

    private void Update()
    {
        Move();
    }
    // Move the spotlight left and right
    // Parameter `right` is true to move right, false to move left
    public void Move()
    {
        rockstar.SetFloat("Velocity", (0f));
        if (Input.GetKey(KeyCode.RightArrow) && gameObject.transform.position.x <= maxPos)
        {
            velocity = 5f;
            gameObject.transform.Translate(Vector3.right * velocity * Time.deltaTime);
            rockstar.SetFloat("Velocity", (velocity));
            if (!right)
            {
                transform.localScale = Vector3.one * 3f;
                right = true;
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && gameObject.transform.position.x >= minPos)
        {
            velocity = 5f;
            Debug.Log("!234");
            gameObject.transform.Translate(Vector3.left * velocity * Time.deltaTime);
            rockstar.SetFloat("Velocity", (velocity));
            if (right)
            {
                transform.localScale = Vector3.Reflect(Vector3.one, Vector3.right) * 3f;
                right = false;
            }
        }
    }

    // Returns the spotlight's position
    public Vector2 GetPosition()
    {
        Vector2 pos = gameObject.transform.position;
        return pos;
    }

    public bool GetIsHolding()
    {
        return isHolding;
    }
    public void SetIsHolding(bool holding)
    {
        isHolding = false;
    }
}
