using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float speed = 2f;

    private Rigidbody rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h != 0 || v != 0) {
            Move(h, v);
        }
    }

    void Move(float horizontalMove, float verticalMove) {
        //
        //rigidbody.AddForce(speed * horizontalMove.normalized * Time.deltaTime, speed * verticalMove.normalized * Time.deltaTime);
        rigidbody.AddForce(new Vector3(horizontalMove, 0f ,verticalMove).normalized * speed * Time.deltaTime);
        //transform.Translate(new Vector3(horizontalMove, 0f ,verticalMove).normalized * speed * Time.deltaTime);
    }
}
