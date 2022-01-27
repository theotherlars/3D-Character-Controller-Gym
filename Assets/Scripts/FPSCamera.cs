using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCamera : MonoBehaviour{  

    public float mouseSensitivity;
    [Tooltip("Default: -75")][Range(-90,90)]
    public float maxLookUp = -75;
    [Tooltip("Default: 75")][Range(-90,90)]
    public float maxLookDown = 75;
    
    Camera cam;
    float xRotation;

    void Start(){
        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
    }

    void Update(){
        CameraMovement();
    }

    private void CameraMovement(){
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Handle rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, maxLookUp, maxLookDown);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }
}
