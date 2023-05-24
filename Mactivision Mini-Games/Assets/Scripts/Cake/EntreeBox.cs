using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntreeBox : MonoBehaviour
{
    bool correct;
    string boxName;
    int boxNumber;

    int inputObjectNumber;
    string inputObjectName;
    // Start is called before the first frame update
    void Start()
    {
        boxName = "EntreeBox";
        boxNumber = 2;
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
        if (collision.tag == "Entree")
        {
            correct = true;
            inputObjectNumber = 2;
        }
        else
        {
            correct = false;
            if (collision.tag == "Dessert")
            {
                inputObjectNumber = 1;
            }
            else if (collision.tag == "Salad")
            {
                inputObjectNumber = 3;
            }
        }

        inputObjectName = collision.name;
        collision.gameObject.transform.eulerAngles = Vector3.zero;
        Destroy(collision.gameObject);
    }
}
