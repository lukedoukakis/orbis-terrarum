using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Camera mainCamera;
    [SerializeField] float moveSpeed;
    [SerializeField] float sensitivity;

    float hor;
    float ver;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        hor = Mathf.Lerp(hor, Input.GetAxis("Mouse X"), sensitivity*Time.deltaTime);
        ver = Mathf.Lerp(ver, Input.GetAxis("Mouse Y"), sensitivity * Time.deltaTime);
        transform.Rotate(new Vector3(ver*-1f, hor, 0f));

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * (moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += transform.forward*-1f * (moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += transform.right*-1f * (moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * (moveSpeed * Time.deltaTime);
        }
    }
}
