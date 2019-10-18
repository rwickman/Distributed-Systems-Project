using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public Transform hand;

    private Vector3 offset;
    private Camera m_cam;
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    public float speedH = 2.0f;
    public float speedV = 2.0f;
    public float handZOffset = 0.4f;
    public float handViewportPosX = 0.88f;
    public float handViewportPosY = -0.08f;
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - player.transform.position;
        m_cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position + offset;
        //print("pos: " + Input.mousePosition.y);
        //print("INPUT: " + Input.GetAxis("Mouse Y"));
        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

       // yaw = Mathf.Clamp(yaw, -90f, 90f);
        //the rotation range
        pitch = Mathf.Clamp(pitch, -60f, 55f);
        //the rotation range

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        //print( m_cam.ViewportToWorldPoint(new Vector3(0.95f, 0.1f, 0.0f)));
        Vector3 updatedHandPos = m_cam.ViewportToWorldPoint(new Vector3(handViewportPosX, handViewportPosY, m_cam.nearClipPlane + handZOffset));
        
        hand.position = updatedHandPos; 
        hand.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        //player.transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        //transform.LookAt(m_cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_cam.nearClipPlane)), Vector3.up);
    }
}
