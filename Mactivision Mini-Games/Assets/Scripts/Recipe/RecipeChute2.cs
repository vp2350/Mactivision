using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// This class animates the trashchute and resets food GameObjects entering it
public class RecipeChute2 : MonoBehaviour
{
    public RecipeDispenser dispenser;         // will tell this class the correctness of the decision
    public SpriteRenderer recycleIcon;  // icon changes colour based on correctness
    public Color correct = Color.green;
    public Color incorrect = Color.red;
    Color defaultCol = Color.white;
    AudioSource sound;

    Animator m_Animator;
    AnimatorClipInfo[] m_CurrentClipInfo;


    bool correctChoice;
    void Start()
    {
        sound = gameObject.GetComponent<AudioSource>();
        target = this.transform.position;
        dispenser = GameObject.Find("pipe").GetComponent<RecipeDispenser>();
        m_Animator = GetComponent<Animator>();
        m_Animator.GetComponent<Animator>().enabled = false;

    }

    public Vector2 target;
    public Vector2 newTarget;
    public float startTime;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime; // calculate distance to move
        if (Vector2.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, step);
        }
    }

    // When the trashchute detects a food GameObject, it will determine
    // whether the food was correctly discarded. The `recycleIcon` will
    // change to green or red based on the correct or incorrect decision.
    // Food GameObject gets deactivated and rotation reset.
    void OnTriggerEnter2D(Collider2D other)
    {
        correctChoice = dispenser.MakeChoice(true) ? true : false ;
        other.attachedRigidbody.velocity = Vector2.zero;
        other.gameObject.transform.eulerAngles = Vector3.zero;
        other.gameObject.SetActive(false);
        //sound.PlayDelayed(0f);

        if(correctChoice)
        {
            newTarget = new Vector2(this.transform.position.x + 20, this.transform.position.y);
        }
        else
        {
            newTarget = new Vector2(this.transform.position.x, this.transform.position.y - 20);
        }

        StartCoroutine(WaitForTarget());
        StartCoroutine(DestroyThis(10f));
    }

    // Wait a bit before returning the icon back to gray
    IEnumerator WaitForTarget()
    {
        yield return new WaitForSeconds(1.5f);
        m_Animator.GetComponent<Animator>().enabled = true;
        m_CurrentClipInfo = this.m_Animator.GetCurrentAnimatorClipInfo(0);
        m_CurrentClipInfo[0].clip.wrapMode = WrapMode.ClampForever;

        target = newTarget;
    }

    // Wait a bit before returning the icon back to gray
    IEnumerator DestroyThis(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopAllCoroutines();
        Destroy(this);
    }
}

