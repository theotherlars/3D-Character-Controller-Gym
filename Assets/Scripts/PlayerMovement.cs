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
    //test

    [Header("Movement Stuff:")]
    [SerializeField]float movementSpeed;
    [SerializeField]float crouchSpeed;
    [SerializeField]float wallRunSpeed;

    [Header("Running Stuff:")]
    [SerializeField]float runSpeed;
    [SerializeField]float runFOV;
    [SerializeField]float runFOVSpeed;

    [Header("Jump Stuff:")]
    [SerializeField]float jumpHeight;
    [SerializeField]bool enableAdjustableJump;
    [SerializeField]float adjustableJumpTime;
    [SerializeField]float adjustableJumpMultiplier;
    
    [Header("Double Jump Stuff:")]
    [SerializeField]bool allowDoubleJump;
    [SerializeField]int allowedExtraJumps;
    [SerializeField]bool onlyAllowOnDecline; 

    [Header("Jet Pack Stuff:")]
    [SerializeField]bool allowJetPack;
    [SerializeField]float thrustForce;
    [SerializeField]float jetPackTime;
    [Range(0,100)]
    [SerializeField]float jetPackFuel;
    [SerializeField]float jetPackFuelUsage;

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

    [Header("Wall Run Stuff:")]
    [SerializeField]bool allowWallRun;
    [SerializeField]float wallRunRotation;
    [SerializeField]float wallRunFOV;
    [SerializeField]float wallRunFOVSpeed;
    [SerializeField]float gravityScaleWallRunning;
    [SerializeField]float wallJumpForce;
    [SerializeField]LayerMask whatIsWallRun;
    [SerializeField]Transform wallRunRightCheck;
    [SerializeField]Transform wallRunLeftCheck;
    [SerializeField]float wallRunCheckRadius;
    public bool isWallRunning;

    [Header("Other Stuff:")]
    [SerializeField]float FOVBackToNormalSpeed;
    

    // PRIVATES    
    CharacterController cc;
    Animator anim;
    Vector3 movementDirection;
    float inputX;
    float inputY;
    int currentJumpCount;
    float adjustableJumpTimeCounter;
    float gravityStore;
    bool doOnceWallRunReset = false;
    float jetPackTimer;
    float orgFOV;
    Camera cam;
    bool isRunning;
    bool isWalking;
    

    // CONSTANTS
    const float gravityConst = -9.81f;

    void Start(){
        cc = GetComponent<CharacterController>();
        cam = Camera.main;
        currentJumpCount = 0;
        gravityStore = gravityScale;
        UIScript.Instance.playerFuel = jetPackFuel;
        orgFOV = cam.fieldOfView;
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

        // ENGAGE CLIMBING
        if(Input.GetKeyDown(jumpKey) && isFacingClimbableWall){
            isClimbing = true;
            gravityScale = 0f;
        }

        // ENGAGE WALLRUN
        if(!isGrounded && IsWallRunAvailable() && !isWallRunning && isRunning){
            isWallRunning = true;
            WallRun();
        }

        // DISENGAGE CLIMBING
        if(!isFacingClimbableWall){
            isClimbing = false;
            gravityScale = gravityStore;
        }

        // DISENGAGE WALLRUN
        if(!IsWallRunAvailable()){
            isWallRunning = false;
            if(doOnceWallRunReset){
                doOnceWallRunReset = false;
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            }
        }
        
        if(isGrounded){
            velocity.x = 0;
        }

        // ENAGE JETPACK
        if(Input.GetKey(jumpKey) && currentJumpCount > 0 && allowJetPack && jetPackFuel > 0){
            if(jetPackTimer >= jetPackTime){
                JetPackThrust();
            }
            else{
                jetPackTimer += Time.deltaTime;
            }
        }

        // ENGAGE MOVEMENT - EITHER NORMAL, CLIMBING OR WALLRUN
        if(!isClimbing && !isWallRunning){
            // gravityScale = gravityStore;

            // MOVEMENT
            movementDirection = (transform.right * inputX) + (transform.forward * inputY);
            HandleMovement();

            // JUMPING
            HandleJump();
        }
        else if(isClimbing){
            Climb();
        }
        else if(isWallRunning){
            WallRun();
            doOnceWallRunReset = true;
        }

        if(isWallRunning){
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRunFOV, wallRunFOVSpeed);
        }
        else{
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, orgFOV, FOVBackToNormalSpeed);
        }

        if(isRunning){
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, runFOV, runFOVSpeed);
        }
        else{
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, orgFOV, FOVBackToNormalSpeed);
        }
    }
    
    void HandleGravity(){
        if(isGrounded && velocity.y < 0.0f){
            velocity.y = -2;
        }
        else if(isWallRunning){
            velocity.y += gravityConst * gravityScaleWallRunning * Time.deltaTime;
        }
        else{
            velocity.y += gravityConst * gravityScale * Time.deltaTime;
        }
        cc.Move(velocity * Time.deltaTime);
    }

    void HandleMovement()
    {
        if (Input.GetKey(crouchKey) || isCrouching){
            CrouchMove();
        }
        else if (Input.GetKey(runKey)){
            isRunning = true;
            isWalking = false;
            Run();
        }
        else{
            isRunning = false;
            isWalking = true;
            Move();
        }

        if (Input.GetKeyDown(crouchKey) && !isCrouching){
            Crouch();
        }
        if (Input.GetKeyUp(crouchKey) && !IsRoofAbove()){
            UnCrouch();
        }
        if (isCrouching && !Input.GetKey(crouchKey) && !IsRoofAbove()){
            UnCrouch();
        }

    }

    void HandleJump(){
        cc.Move(velocity * Time.deltaTime); // Perform jump movement

        if(isGrounded){
            adjustableJumpTimeCounter = adjustableJumpTime;
        }
        else{
            adjustableJumpTimeCounter -= Time.deltaTime;
        }

        if(adjustableJumpTimeCounter > 0f  && Input.GetKeyDown(jumpKey)){ //is adjustableJumpTimeCounter greater than 0, it means the player is grounded
            Jump();
        }

        if(enableAdjustableJump){
            if(Input.GetKeyUp(jumpKey) && velocity.y > 0.0f){ // releasing jumpkey while on the way up, greater gravity down
                velocity.y *= adjustableJumpMultiplier;
                adjustableJumpTimeCounter = 0f;
            }
        }

        // DOUBLE JUMP             
        if(allowDoubleJump){
            if(Input.GetKeyDown(jumpKey) && !isGrounded && currentJumpCount < allowedExtraJumps){
                if(onlyAllowOnDecline && cc.velocity.y < 0.0f){
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
        jetPackTimer = 0;
        velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravityConst * gravityScale);
    }
    
    void WallJump(bool jumpRight){
        Debug.Log("Bool: " + jumpRight + " LocalEulerAngels.Y: " + transform.localEulerAngles.y);
        if(jumpRight){
            velocity.x = Mathf.Sqrt(wallJumpForce * -2 * gravityConst * gravityScaleWallRunning) * -transform.right.x;
            Jump();
        }
        else{
            velocity.x = Mathf.Sqrt(wallJumpForce * -2 * gravityConst * gravityScaleWallRunning) * transform.right.x;
            Jump();
        }
    }

    void JetPackThrust(){
        velocity.y = Mathf.Sqrt(thrustForce * -2 * gravityConst * gravityScale);
        jetPackFuel -= jetPackFuelUsage * Time.deltaTime;
        if(UIScript.Instance){
            UIScript.Instance.playerFuel = jetPackFuel;
        }
    }

    void Climb(){
        Vector3 climbMovement = (transform.right * inputX) + (transform.up * inputY); 
        cc.Move(climbMovement * climbSpeed * Time.deltaTime);
    }

    void WallRun(){        
        Vector3 wallRunMovement = Vector3.zero;
        
        bool rightWall = Physics.CheckSphere(wallRunRightCheck.position,wallRunCheckRadius,whatIsWallRun);
        bool leftWall = Physics.CheckSphere(wallRunLeftCheck.position,wallRunCheckRadius,whatIsWallRun);

        if(rightWall){
            wallRunMovement = (transform.forward * inputY); 
            cam.transform.localEulerAngles += new Vector3(0,0,wallRunRotation);
        }
        
        if(leftWall){
            wallRunMovement = (transform.forward * inputY); 
            cam.transform.localEulerAngles -= new Vector3(0,0,wallRunRotation);
        }

        if(Input.GetKeyDown(jumpKey)){
            WallJump(rightWall);
        }

        cc.Move(wallRunMovement * wallRunSpeed * Time.deltaTime);
    }

    void CrouchMove(){
        Movement(crouchSpeed);
    }

    void Crouch(){
        isCrouching = true;
        cc.height *= crouchMultiplier;
        Camera.main.transform.localPosition -= new Vector3(0, Camera.main.transform.localPosition.y * crouchMultiplier,0);
        roofCheck.localPosition *= 0.5f;
        groundCheck.localPosition *= 0.5f; 
    }

    void UnCrouch(){
        isCrouching = false;
        cc.height *= deCrouchMultiplier;
        Camera.main.transform.localPosition += new Vector3(0,Camera.main.transform.localPosition.y,0);
        roofCheck.localPosition *= 2;
        groundCheck.localPosition *= 2;
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
    }
    
    bool IsWallRunAvailable(){
        bool val = false;
        val = Physics.CheckSphere(wallRunRightCheck.position,wallRunCheckRadius,whatIsWallRun);
        if(!val){
            val = Physics.CheckSphere(wallRunLeftCheck.position,wallRunCheckRadius,whatIsWallRun);
        }
        return val;
    }

    bool IsFacingClimbableWall(){
        bool val = false;
        val = Physics.CheckSphere(climbableCheck.position,climbableCheckRadius,whatIsClimbable);
        return val;
    }
    
    private void OnDrawGizmosSelected(){
        if(groundCheck){Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);}
        if(climbableCheck){Gizmos.DrawWireSphere(climbableCheck.position,climbableCheckRadius);}
        if(wallRunLeftCheck){Gizmos.DrawSphere(wallRunLeftCheck.position,wallRunCheckRadius);}
        if(wallRunRightCheck){Gizmos.DrawSphere(wallRunRightCheck.position,wallRunCheckRadius);}
        // if(roofCheck){Gizmos.DrawRay(roofCheck.position,Vector3.up);}
    }
}
