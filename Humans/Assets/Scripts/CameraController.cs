using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Camera mainCamera;
    public Transform gyro;

    Rigidbody rb;
    [SerializeField] float moveSpeed;
    [SerializeField] float sensitivity;

    float hor;
    float ver;



    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = -1;

        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {

        hor = Mathf.Lerp(hor, Input.GetAxis("Mouse X"), 6f*sensitivity);
        ver = Mathf.Lerp(ver, Input.GetAxis("Mouse Y"), 6f*sensitivity);

        gyro.transform.Rotate(new Vector3(ver*-1f, hor, 0f));
        transform.rotation = Quaternion.Slerp(transform.rotation, gyro.rotation, .5f);

        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(transform.forward * moveSpeed, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce(transform.forward * -1f * moveSpeed, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(transform.right * -1f * moveSpeed, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(transform.right * moveSpeed, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(Vector3.up* moveSpeed, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            rb.AddForce(Vector3.up * -1f * moveSpeed, ForceMode.Acceleration);
        }

        if (rb.velocity.magnitude > 10f)
        {
            rb.velocity = rb.velocity.normalized * 10f;
        }

        
    }
}
