using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Camera MainCamera;

    Rigidbody rb;
    [SerializeField] float moveSpeed;
    [SerializeField] float sensitivity;
    [SerializeField] float featureCullDistance;
    [SerializeField] float smallFeatureCullDistance;
    float acceleration;

    float hor;
    float ver;

    static Vector3 flat = new Vector3(1f, 0f, 1f);



    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 1;
        float[] cullDistances = new float[32];
        cullDistances[10] = featureCullDistance;
        cullDistances[11] = smallFeatureCullDistance;

        MainCamera.layerCullDistances = cullDistances;

        rb = GetComponent<Rigidbody>();


        acceleration = moveSpeed / 4f;

        RandomSpawn();
    }
    

    void RandomSpawn(){
        bool landHit = false;
        Vector3 randomPos = Vector3.zero;
        int i = 0;
        while(!landHit){
            randomPos = new Vector3(Random.Range(-1000f, 1000f), 0f, Random.Range(-1000f, 1000f)) + Vector3.up * (ChunkGenerator.ElevationAmplitude*.82f);
            landHit = Mathf.PerlinNoise((randomPos.x - ChunkGenerator.Seed + .01f) / ChunkGenerator.ElevationMapScale, (randomPos.z - ChunkGenerator.Seed + .01f) / ChunkGenerator.ElevationMapScale) >= .5f;
            i++;

            if(i > 1000){
                Debug.Log(":(");
                break;
            }
        } 
        MainCamera.transform.position = randomPos;
        MainCamera.transform.rotation = Quaternion.Euler(15f, 45f, 0f);
    }

    void Update(){
        float h = Input.GetAxis("Mouse X") * 60f * Time.deltaTime;
        float v = Input.GetAxis("Mouse Y") * -60f * Time.deltaTime;
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


        if (Input.GetKeyUp(KeyCode.P))
        {
            moveSpeed += 5;
            acceleration = moveSpeed / 4f;
        }
        if (Input.GetKeyUp(KeyCode.O))
        {
            moveSpeed -= 5;
            acceleration = moveSpeed / 4f;
        }
        acceleration = Mathf.Clamp(acceleration, 0f, 200f);

    }

    void FixedUpdate()
    {


        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(Vector3.Scale(transform.forward, flat).normalized * acceleration, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.AddForce(Vector3.Scale(transform.forward, flat).normalized * acceleration * -1f, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(transform.right * -1f * acceleration*.6f, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(transform.right * acceleration*.6f, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * acceleration*.2f, ForceMode.Acceleration);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            rb.AddForce(Vector3.up * -1f * acceleration*.2f, ForceMode.Acceleration);
        }


    }
}
