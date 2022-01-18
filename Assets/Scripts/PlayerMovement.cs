using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
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

    [Header("Climbing Stuff:")]
    [SerializeField]float climbSpeed;
    public bool isClimbing;

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

    [Header("Climbing Check Stuff:")]
    [SerializeField]LayerMask whatIsClimbable;
    [SerializeField]float climbableCheckRadius;
    [SerializeField]Transform climbableCheck;
    public bool isFacingClimbableWall;
    

    // PRIVATES    
    CharacterController cc;
    Vector3 movementDirection;
    int currentJumpCount;
    float coyoteJumpTimeCounter;
    float inputX;
    float inputY;
    float gravityStore;

    // CONSTANTS
    const float gravityConst = -9.81f;

    void Start(){
        cc = GetComponent<CharacterController>();
        currentJumpCount = 0;
        gravityStore = gravityScale;
    }

    void Update(){
        isGrounded = IsGrounded();
        isRoofAbove = IsRoofAbove();
        isFacingClimbableWall = IsFacingClimbableWall();
        HandleGravity();
        HandleInput();
    }

    void HandleInput(){
        // INPUT
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");

        if(Input.GetKeyDown(jumpKey) && isFacingClimbableWall){
            isClimbing = true;
            gravityScale = -0.1f;
        }

        if(!isFacingClimbableWall){
            isClimbing = false;
        }

        if(!isClimbing){
            // MOVEMENT
            movementDirection = (transform.right * inputX) + (transform.forward * inputY);
            HandleMovement();

            // JUMPING
            HandleJump();
            gravityScale = gravityStore;
        }
        else{
            Climb();
        }
    }

    

    void HandleGravity(){
        if(isGrounded && velocity.y < 0.0f){
            velocity.y = -2;
        }
        else{
            velocity.y += gravityConst * gravityScale * Time.deltaTime;
        }
    }

    void HandleMovement()
    {
        if (Input.GetKey(crouchKey) || isCrouching)
        {
            CrouchMove();
        }
        else if (Input.GetKey(runKey))
        {
            Run();
        }
        else
        {
            Move();
        }

        if (Input.GetKeyDown(crouchKey) && !isCrouching)
        {
            Crouch();
        }
        if (Input.GetKeyUp(crouchKey) && !IsRoofAbove())
        {
            UnCrouch();
        }
        if (isCrouching && !Input.GetKey(crouchKey) && !IsRoofAbove())
        {
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
                velocity.y *= coyoteJumpMultiplier;
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

    void Movement(float speed){
        cc.Move(movementDirection * speed * Time.deltaTime);
    }
    
    void Move(){
        Movement(movementSpeed);
    }

    void Run(){
        Movement(runSpeed);
    }

    void Jump(){
        currentJumpCount += 1;
        velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravityConst * gravityScale);
    }
    
    void Climb(){
        isClimbing = true;
        Vector3 climbMovement = (transform.right * inputX) + (transform.up * inputY); 

        if(Input.GetKey(jumpKey)){
            cc.Move(climbMovement * climbSpeed * Time.deltaTime);
        }
    }
    
    void CrouchMove(){
        Movement(crouchSpeed);
    }

    void Crouch(){
        isCrouching = true;
        
        cc.height *= crouchMultiplier;
        
        Vector3 roofCheckPos = roofCheck.localPosition;
        roofCheck.localPosition -= new Vector3(roofCheckPos.x, roofCheckPos.y * crouchMultiplier, roofCheckPos.z);

        Vector3 camPos = Camera.main.transform.localPosition;
        Camera.main.transform.localPosition -= new Vector3(camPos.x, camPos.y * crouchMultiplier,camPos.z);
    }

    void UnCrouch(){
        isCrouching = false;
        
        cc.height *= deCrouchMultiplier;
        
        Vector3 roofCheckPos = roofCheck.localPosition;
        roofCheck.localPosition -= new Vector3(roofCheckPos.x, roofCheckPos.y * deCrouchMultiplier, roofCheckPos.z);

        Vector3 camPos = Camera.main.transform.localPosition;
        Camera.main.transform.localPosition = new Vector3(camPos.x,camPos.y * deCrouchMultiplier,camPos.z);
    }

    bool IsGrounded(){
        bool val = false;
        val = Physics.CheckSphere(groundCheck.position,groundCheckRadius,whatIsGround);
        if(val){currentJumpCount = 0;}
        return val;
    }
    
    bool IsRoofAbove(){
        bool val = false;
        Vector3 endPos = roofCheck.position;
        endPos.y += roofCheckRadius;
        if(Physics.Linecast(roofCheck.position,endPos,out RaycastHit hit, whatIsRoof)){
            val = true;
        }
        return val;
        // Physics.Raycast(roofCheck.position,Vector3.up,roofCheckRadius,whatIsRoof);
        // return Physics.CheckSphere(roofCheck.position,roofCheckRadius,whatIsRoof);
    }
    
    bool IsFacingClimbableWall(){
        bool val = false;
        val = Physics.CheckSphere(climbableCheck.position,climbableCheckRadius,whatIsClimbable);
        return val;
    }
    
    private void OnDrawGizmosSelected(){
        if(groundCheck){Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);}
        if(climbableCheck){Gizmos.DrawWireSphere(climbableCheck.position,climbableCheckRadius);}
        // if(roofCheck){Gizmos.DrawRay(roofCheck.position,Vector3.up);}
    }
}
