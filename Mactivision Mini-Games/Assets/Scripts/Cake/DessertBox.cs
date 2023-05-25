using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DessertBox : MonoBehaviour
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
        boxName = "DessertBox";
        boxNumber = 1;
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
        inputObjectNumber = -1;
        inputObjectName = "";
        if(collision.tag == "Dessert")
        {
            correct = true;
            inputObjectNumber = 1;
            screengreen.SetActive(true);
            StartCoroutine(DisableGreen(1f));
        }
        else
        {
            correct = false;
            if(collision.tag == "Entree")
            {
                inputObjectNumber = 2;
            }
            else if(collision.tag == "Salad")
            {
                inputObjectNumber = 3;
            }
            screenred.SetActive(true);
            StartCoroutine(DisableRed(1f));
        }

        inputObjectName = collision.name;

        Debug.Log(inputObjectNumber);
        Debug.Log(correct);
        Debug.Log(boxNumber);
        //levelManager.RecordEvent(inputObjectNumber, boxNumber, correct);
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
