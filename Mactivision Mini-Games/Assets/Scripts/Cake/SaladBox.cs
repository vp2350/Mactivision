using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaladBox : MonoBehaviour
{
    public GameObject managerObject;
    public CakeLevelManager levelManager;

    public GameObject screengreen;
    public GameObject screenred;

    bool correct;
    string boxName;
    int boxNumber;

    int inputObjectNumber;
    string inputObjectName;
    // Start is called before the first frame update
    void Start()
    {
        boxName = "SaladBox";
        boxNumber = 3;
        inputObjectNumber = -1;
        inputObjectName = "";
        levelManager = managerObject.GetComponent<CakeLevelManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Entered");
        inputObjectNumber = -1;
        inputObjectName = "";
        if (collision.tag == "Salad")
        {
            correct = true;
            inputObjectNumber = 3;

            screengreen.SetActive(true);
            StartCoroutine(DisableGreen(1f));
        }
        else
        {
            correct = false;
            if (collision.tag == "Entree")
            {
                inputObjectNumber = 2;
            }
            else if (collision.tag == "Dessert")
            {
                inputObjectNumber = 1;
            }
            screenred.SetActive(true);
            StartCoroutine(DisableRed(1f));
        }

        inputObjectName = collision.name;
        DateTime choiceTime = collision.GetComponent<MoveFood>().choiceStartTime;

        levelManager.RecordEvent(inputObjectNumber, inputObjectName, boxNumber, correct, choiceTime);

        collision.gameObject.transform.eulerAngles = Vector3.zero;
        Destroy(collision.gameObject);
    }

    IEnumerator DisableGreen(float delay)
    {
        yield return new WaitForSeconds(delay);
        screengreen.SetActive(false);
    }

    IEnumerator DisableRed(float delay)
    {
        yield return new WaitForSeconds(delay);
        screenred.SetActive(false);
    }
}
