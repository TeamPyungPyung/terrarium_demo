using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCamera : MonoBehaviour
{
    public float mouseSensitivityX = 5.0f;
    public float mouseSensitivityY = 5.0f;
    float rotY = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // mouse click.
        if (Input.GetMouseButtonDown(1))
        {
            float rotX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivityX;
            rotY += Input.GetAxis("Mouse Y") * mouseSensitivityY;
            rotY = Mathf.Clamp(rotY, -89.5f, 89.5f);
            transform.localEulerAngles = new Vector3(-rotY, rotX, 0.0f);
        }

        //W, up or S, down to control camera
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * .1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * .1f;
        }

        //Camera move to some point fixed
        if (Input.GetKey(KeyCode.U))
        {
            gameObject.transform.localPosition = new Vector3(0.0f, 50.0f, 0.0f);
            transform.localEulerAngles = new Vector3(90.0f, 0.0f, 0.0f);
        }

    }
}
