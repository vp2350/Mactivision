using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaladBox : MonoBehaviour
{
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        inputObjectNumber = -1;
        inputObjectName = "";
        if (collision.tag == "Salad")
        {
            correct = true;
            inputObjectNumber = 3;
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
        }

        inputObjectName = collision.name;
        collision.gameObject.transform.eulerAngles = Vector3.zero;
        Destroy(collision.gameObject);
    }
}
