using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float speed = 500f;
    public bool isMoveable = true;
    public float maxSpeed = 5;
    public float stoppingLerpTerm = 0.8f;
    public float dirMoveFactor = 0.8f;

    private float minVelocityMag = 0.005f;
    private Rigidbody rigidbody;
    private GameObject sword;
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
        if ((h != 0 || v != 0) && isMoveable) {
            Move(h, v);
        } else if (rigidbody.velocity.magnitude > minVelocityMag)
        {
            rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, Vector3.zero, stoppingLerpTerm);
        }
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, transform.localEulerAngles.z);
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, maxSpeed);
        print(rigidbody.velocity.magnitude);
    }

    void Move(float horizontalMove, float verticalMove) {
        //rigidbody.AddForce(new Vector3(horizontalMove, 0f ,verticalMove).normalized * speed * Time.deltaTime);
        //Vector3 moveDir = Camera.main.transform.TransformDirection(new Vector3(horizontalMove, 0f, verticalMove));
        rigidbody.AddRelativeForce(new Vector3(horizontalMove * dirMoveFactor, 0f, verticalMove * dirMoveFactor).normalized * speed * Time.deltaTime);
    }
}
