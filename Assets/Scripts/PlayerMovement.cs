using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    
    
    [Header("Input Stuff:")]
    [SerializeField]KeyCode runKey;
    [SerializeField]KeyCode jumpKey;
    
    [Header("Movement Stuff:")]
    [SerializeField]private float movementSpeed;
    [SerializeField]private float runSpeed;
    [SerializeField]float jumpForce;

    [Header("Gravity")]
    [SerializeField]float gravity;

    [Header("Ground Check Stuff:")]
    [SerializeField]Transform groundCheck;
    [SerializeField]float checkDistance;
    [SerializeField]LayerMask whatIsFloor;
    
    CharacterController cc;
    Vector3 movementDirection;
    
    void Start(){
        cc = GetComponent<CharacterController>();
    }

    void Update(){
        HandleInput();
        HandleGravity();
        print(IsGrounded());
    }

    void HandleInput(){
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        movementDirection = (transform.right * x) + (transform.forward * z);
        
        if(Input.GetKeyDown(jumpKey)){
            Jump();
        }
        
        if(Input.GetKey(runKey)){
            Run();
        }
        else{
            Move();
        }

    }
    void Move(){
        cc.Move(movementDirection * movementSpeed * Time.deltaTime);
    }
    void Run(){
        cc.Move(movementDirection * runSpeed * Time.deltaTime);
    }
    void Jump(){
        if(!IsGrounded()){return;}
        print("tried to jump");
        movementDirection.y = jumpForce;
    }
    bool IsGrounded(){
        bool val = false;
        bool hit = Physics.Raycast(groundCheck.position,Vector3.down,checkDistance,whatIsFloor.value);
        if(hit){
            val = true;
        }
        return val;
    }

    void HandleGravity(){
        if(IsGrounded()){
            movementDirection.y = 0f;
        }
        movementDirection.y -= gravity * Time.deltaTime;
    }
}