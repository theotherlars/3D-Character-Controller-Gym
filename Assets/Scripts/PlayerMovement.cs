using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    
    
    [Header("Input Stuff:")]
    [SerializeField]KeyCode runKey;
    [SerializeField]KeyCode jumpKey;
    [SerializeField]KeyCode crouchKey;

    
    [Header("Movement Stuff:")]
    [SerializeField]private float movementSpeed;
    [SerializeField]private float runSpeed;
    [SerializeField]private float crouchSpeed;

    [Header("Jump Stuff:")]
    [SerializeField]float jumpHeight;

    
    [Header("Double Jump Stuff:")]
    [SerializeField]private bool allowDoubleJump;
    // [SerializeField]float doubleJumpTime;
    [SerializeField]int allowedExtraJumps;
    [SerializeField]private bool onlyAllowOnDecline; 


    [Header("Crouch Stuff:")]
    [SerializeField]float crouchMultiplier;
    [SerializeField]float deCrouchMultiplier;
    public bool isCrouching;

    [Header("Gravity stuff:")]
    [SerializeField]float gravityScale;
    public Vector3 velocity;

    [Header("Ground Check Stuff:")]
    [SerializeField]Transform groundCheck;
    [SerializeField]float groundCheckRadius;
    [SerializeField]LayerMask whatIsGround;
    public bool isGrounded;

    [Header("Roof Check Stuff:")]
    [SerializeField]LayerMask whatIsRoof;
    [SerializeField]float roofCheckRadius;
    [SerializeField]Transform roofCheck;
    public bool isRoofAbove;
    
    CharacterController cc;
    Vector3 movementDirection;
    const float gravityConst = -9.81f;
    int currentJumpCount;

    void Start(){
        cc = GetComponent<CharacterController>();
        currentJumpCount = 0;
    }

    void Update(){
        isGrounded = IsGrounded();
        isRoofAbove = IsRoofAbove();
        HandleGravity();
        HandleInput();
    }

    void HandleInput(){
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        movementDirection = (transform.right * x) + (transform.forward * z);
        
        cc.Move(velocity * Time.deltaTime); // Jump movement
         
        
        if(allowDoubleJump){
            if(Input.GetKeyDown(jumpKey) && isGrounded || Input.GetKeyDown(jumpKey) && currentJumpCount < allowedExtraJumps){
                if(!onlyAllowOnDecline){
                    Jump();
                }
                else if(onlyAllowOnDecline && cc.velocity.y < 0.0f){
                    Jump();
                }
                else{
                    Jump();
                }
            }
        }
        else{
            if(Input.GetKeyDown(jumpKey) && isGrounded){
                Jump();
            }
        }

        if(Input.GetKey(crouchKey)){    
            CrouchMove();
        }
        else if(Input.GetKey(runKey)){
            Run();
        }
        else{
            Move();
        }

        if(Input.GetKeyDown(crouchKey)){
            Crouch();
        }
        if(Input.GetKeyUp(crouchKey)){
            UnCrouch();
        }
    }

    void Move(){
        Movement(movementSpeed);
        // Walking Animations
    }

    void Run(){
        Movement(runSpeed);
        // Run Animations
    }

    void CrouchMove(){
        Movement(crouchSpeed);
    }

    void Movement(float speed){
        cc.Move(movementDirection * speed * Time.deltaTime);
    }

    void Crouch(){
        isCrouching = true;
        cc.height *= crouchMultiplier;
        Vector3 camPos = Camera.main.transform.localPosition;
        Camera.main.transform.localPosition -= new Vector3(camPos.x, camPos.y * crouchMultiplier,camPos.z);
    }

    void UnCrouch(){
        isCrouching = false;
        cc.height *= deCrouchMultiplier;
        Vector3 camPos = Camera.main.transform.localPosition;
        Camera.main.transform.localPosition = new Vector3(camPos.x,camPos.y * deCrouchMultiplier,camPos.z);
    }

    void Jump(){
        currentJumpCount += 1;
        velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravityConst * gravityScale);
    }

    bool IsGrounded(){
        bool val = false;
        val = Physics.CheckSphere(groundCheck.position,groundCheckRadius,whatIsGround);
        if(val){currentJumpCount = 0;}
        return val;
    }
    bool IsRoofAbove(){
        return Physics.CheckSphere(roofCheck.position,roofCheckRadius,whatIsRoof);
    }

    void HandleGravity(){
        if(isGrounded && velocity.y < 0.0f){
            velocity.y = -2;
        }
        else{
            velocity.y += gravityConst * gravityScale * Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected() {
        if(groundCheck){Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);}
        if(roofCheck){Gizmos.DrawSphere(roofCheck.position, roofCheckRadius);}
    }
}
