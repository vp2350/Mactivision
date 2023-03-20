using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    float velocity;
    Rigidbody2D rb;
    bool pickupAllowed;
    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        velocity = 2f;
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if(this.transform.position.x > 50f)
        {
            Destroy(this);
        }

        
        if (pickupAllowed && Input.GetKeyDown(KeyCode.E))
            PickUp();
        
    }

    void Init(float vel)
    {
        rb.velocity = new Vector3(vel, 0, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("Player") && player.GetComponent<Baker>().GetIsHolding())
        {
            pickupAllowed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("Player"))
        {
            pickupAllowed = false;
        }
    }
    private void PickUp()
    {
        this.transform.SetParent(player.transform);
        player.GetComponent<Baker>().SetIsHolding(true);
    }
}
