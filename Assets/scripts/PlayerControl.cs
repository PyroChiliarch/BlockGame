using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerControl : MonoBehaviour
{
    public GameObject terrain;
    TerrainController terrainController;

    float movementSpeed = 30f;
    float rotationSpeed = 15f;
    new Rigidbody rigidbody;
    Vector3 oldMousePos;


    // Start is called before the first frame update
    void Start()
    {
        rigidbody = this.gameObject.GetComponent<Rigidbody>();
        oldMousePos = Vector3.zero;

        Resolution res = Screen.currentResolution;
        oldMousePos = new Vector3(res.width/2, res.height/2, 0);

        terrainController = terrain.GetComponent<TerrainController>();
        terrainController.RegisterChunkLoader(this.gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        

        if (Input.GetKey(KeyCode.W))
        {
            rigidbody.AddForce(transform.forward * movementSpeed);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rigidbody.AddForce((transform.right * -1) * movementSpeed);
        }

        if (Input.GetKey(KeyCode.D))
        {
            rigidbody.AddForce(transform.right * movementSpeed);
        }

        if (Input.GetKey(KeyCode.S))
        {
            rigidbody.AddForce((transform.forward * -1) * movementSpeed);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            rigidbody.AddForce(transform.up * movementSpeed);
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            rigidbody.AddForce((transform.up * -1) * movementSpeed);
        }


        float rotY = Input.GetAxis("Mouse X") * rotationSpeed;
        transform.localEulerAngles += new Vector3(0, rotY, 0);

        
    }
}
