﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class animates the chest and coin rising
public class ChestAnimator : MonoBehaviour
{
    public bool opened;
    public GameObject player;
    public GameObject coin;
    public Vector3 destination = new Vector3(0f, 0.83f, -1.5f);
    public float coinspeed = 1.0f;
    Animator anim;
    AudioSource sound;

    // Start is called before the first frame update
    void Start()
    {
        opened = false;
        anim = gameObject.GetComponent<Animator>();
        sound = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (opened) { // make the coin rise up from the chest when it is opened
            coin.transform.localPosition = Vector3.MoveTowards(coin.transform.localPosition, destination, coinspeed * Time.deltaTime);
        }
    }

    // When the player collides with the chest, it opens
    void OnTriggerEnter2D(Collider2D c)
    {
        if (c.gameObject == player) {
            opened = true;
            anim.Play("Base Layer.chest");
            sound.PlayDelayed(0.75f);
        }
    }
}
