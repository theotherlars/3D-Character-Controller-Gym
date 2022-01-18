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
    [SerializeField]bool enableCoyoteJump;
    [SerializeField]float coyoteJumpTime;
    [SerializeField]float coyoteJumpMultiplier;

    
    [Header("Double Jump Stuff:")]
    [SerializeField]private bool allowDoubleJump;
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

    // PRIVATES    
    CharacterController cc;
    Vector3 movementDirection;
    int currentJumpCount;
    float coyoteJumpTimeCounter;

    // CONSTANTS
    const float gravityConst = -9.81f;

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
        
        // JUMPING
        HandleJump();

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

    void HandleJump(){
        cc.Move(velocity * Time.deltaTime); // Perform jump movement

        if(isGrounded){
            coyoteJumpTimeCounter = coyoteJumpTime;
        }
        else{
            coyoteJumpTimeCounter -= Time.deltaTime;
        }

        if(coyoteJumpTimeCounter > 0f  && Input.GetKeyDown(jumpKey)){ //is coyoteJumpTimeCounter greater than 0, it means the player is grounded
            Jump();
        }

        if(enableCoyoteJump){
            if(Input.GetKeyUp(jumpKey) && velocity.y > 0.0f){ // releasing jumpkey while on the way up, greater gravity down
                velocity.y *= coyoteJumpMultiplier * gravityScale;
                coyoteJumpTimeCounter = 0f;
            }
        }
            
        if(allowDoubleJump){
            if(Input.GetKeyDown(jumpKey) && !isGrounded && currentJumpCount < allowedExtraJumps){
                if(onlyAllowOnDecline && !isGrounded && cc.velocity.y < 0.0f){
                    Jump();
                }
                if(!onlyAllowOnDecline){
                    Jump();
                }
            }
        }
    }

    void Move(){
        Movement(movementSpeed);
    }

    void Run(){
        Movement(runSpeed);
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
