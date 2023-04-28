using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStageMovement : MonoBehaviour
{
    public Vector2 target;
    public float startTime;
    public float speed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        var step = speed * Time.deltaTime; // calculate distance to move
        if (Vector2.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, step);
        }
    }

    public void SetTarget(Vector2 newTarget)
    {
        target = newTarget;
    }
}
