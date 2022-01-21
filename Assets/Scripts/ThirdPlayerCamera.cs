using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPlayerCamera : MonoBehaviour
{  
    public float mouseSensitivity;
    [SerializeField]float turnSmoothTime;
    
    Transform cam;
    Vector3 direction;
    PlayerMovement_3rd_Person playerMovement;
    float xRotation;
    float turnSmoothVelocity;

    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main.transform;
        playerMovement = GetComponent<PlayerMovement_3rd_Person>();
    }

    void Update(){
        // HandleMovement();
    }

    // public void HandleMovement(){
    //     if(playerMovement.movementDirection.magnitude >= 0.1f){
    //         float targetAngle = Mathf.Atan2(playerMovement.movementDirection.x, playerMovement.movementDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
    //         float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle,ref turnSmoothVelocity,turnSmoothTime);
    //         transform.rotation = Quaternion.Euler(0f,angle,0f);
    //         Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
    //         controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
    //         transform.position = new Vector3(transform.position.x,1,transform.position.z);
    //         // animator.SetBool("Moving",true);
    //     }
    //     else{
    //         // animator.SetBool("Moving",false);
    //     }
    // }
}
