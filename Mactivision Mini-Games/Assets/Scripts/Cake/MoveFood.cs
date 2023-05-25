using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFood : MonoBehaviour
{
    float velocity;
    public DateTime choiceStartTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(float velocity)
    {
        this.velocity = velocity;
        choiceStartTime = DateTime.Now;
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x + (velocity * Time.deltaTime), transform.position.y, transform.position.z);
    }
}
