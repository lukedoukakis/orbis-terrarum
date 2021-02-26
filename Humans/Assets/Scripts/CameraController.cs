using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Camera MainCamera;

    Rigidbody rb;
    [SerializeField] float moveSpeed;
    [SerializeField] float sensitivity;
    float acceleration;

    float hor;
    float ver;



    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 1;

        rb = GetComponent<Rigidbody>();


        acceleration = moveSpeed / 4f;

        MainCamera.transform.position = new Vector3(Random.Range(-1000f, 1000f), 0f, Random.Range(-1000f, 1000f)) + Vector3.up * 100f;
        MainCamera.transform.rotation = Quaternion.Euler(25f, 45f, 0f);
    }

    void FixedUpdate()
    {


        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce((transform.forward + transform.up*25f/45f).normalized * acceleration, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce((transform.forward + transform.up * 25f / 45f).normalized * -1f * acceleration, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(transform.right * -1f * acceleration, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(transform.right * acceleration, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * acceleration, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            rb.AddForce(Vector3.up * -1f * acceleration, ForceMode.Acceleration);
        }

        /*
        float rotX = Input.GetAxis("Mouse X");
        if(Mathf.Abs(rotX) > 0f)
        {
            mainCamera.transform.Rotate(Vector3.up * rotX * sensitivity * 50f*Time.deltaTime);
        }
        
        float rotY = Input.GetAxis("Mouse Y");
        if (Mathf.Abs(rotY) > 0f)
        {
            mainCamera.transform.Rotate(Vector3.forward * rotY * sensitivity * 50f * Time.deltaTime);
        }
        */
        float h = 1f * Input.GetAxis("Mouse X");
        float v = -1f * Input.GetAxis("Mouse Y");

        MainCamera.transform.Rotate(v, h, 0);

        Vector3 eulers = transform.rotation.eulerAngles;
        Vector3 pos = MainCamera.transform.position;
        //mainCamera.transform.rotation = Quaternion.Euler(30f, eulers.y, 0f);
        MainCamera.transform.rotation = Quaternion.Euler(eulers.x, eulers.y, 0f);
        MainCamera.transform.position = new Vector3(pos.x, pos.y, pos.z);

        float maxSpeed = moveSpeed;
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        

    }
}
