using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStageMovement : MonoBehaviour
{
    public Vector2 target;
    public float startTime;
    public float speed;

    public Animator actor;   // rockstar gameobject, used to animate red guy

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        speed = 5f;
    }

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime; // calculate distance to move
        if (Vector2.Distance(transform.position, target) > 0.01f)
        {
            actor.SetFloat("Velocity", 5);
            transform.position = Vector3.MoveTowards(transform.position, target, step);
            if(target.x <= transform.position.x)
            {
                transform.localScale = Vector3.Reflect(Vector3.one, Vector3.right) * 2f;
            }
        }
        else
        {
            actor.SetFloat("Velocity", 0);
        }
    }

    public void SetTarget(Vector2 newTarget)
    {
        target = newTarget;
    }
}
