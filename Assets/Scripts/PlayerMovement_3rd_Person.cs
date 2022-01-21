using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement_3rd_Person : MonoBehaviour {
    
    [Header("Input Stuff:")]
    [SerializeField]KeyCode runKey;
    [SerializeField]KeyCode jumpKey;
    [SerializeField]KeyCode crouchKey;
    //test

    [Header("Movement Stuff:")]
    [SerializeField]private float movementSpeed;
    [SerializeField]private float runSpeed;
    [SerializeField]private float crouchSpeed;

    [Header("Jump Stuff:")]
    [SerializeField]float jumpHeight;
    [SerializeField]bool enableAdjustableJump;
    [SerializeField]float adjustableJumpTime;
    [SerializeField]float adjustableJumpMultiplier;
    
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
    
    [Header("Camera Stuff:")]
    [SerializeField]bool invertY;
    [SerializeField]float turnSmoothTime;
    [SerializeField]Transform followTarget;
    [SerializeField]float rotationPower;

    // PRIVATES    
    CharacterController cc;
    [SerializeField]CinemachineVirtualCamera cineCam;
    float cameraDistance;
    [SerializeField] Animator anim;
    [SerializeField]Transform cam;

    public Vector3 movementDirection;
    int currentJumpCount;
    float adjustableJumpTimeCounter;
    float inputX;
    float inputY;
    float gravityStore;
    float turnSmoothVelocity;

    // CONSTANTS
    const float gravityConst = -9.81f;

    void Start(){
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        // anim = GetComponent<Animator>();
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
            gravityScale = 0f;
        }

        if(!isFacingClimbableWall){
            isClimbing = false;
        }

        if(!isClimbing){
            gravityScale = gravityStore;

            // MOVEMENT
            movementDirection = (transform.right * inputX) + (transform.forward * inputY);
            HandleMovement();

            // JUMPING
            HandleJump();
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
        if(movementDirection.magnitude > 0){
            movementDirection.Normalize();
            cc.Move(movementDirection * speed * Time.deltaTime);
        }
        
        // Animations
        HandleMovementAnimations();
        
        // Rotation handling
        transform.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotationPower, Vector3.up);
        followTarget.transform.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotationPower, Vector3.up);
        if(invertY){
            followTarget.transform.rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * rotationPower, Vector3.right);
        }
        else{
            followTarget.transform.rotation *= Quaternion.AngleAxis(-Input.GetAxis("Mouse Y") * rotationPower, Vector3.right);
        }
        var angles = followTarget.transform.localEulerAngles;
        angles.z = 0;
        var angle = followTarget.transform.localEulerAngles.x;
        if(angle > 180 && angle < 340){
            angles.x = 340;
        }
        else if(angle < 180 && angle > 40){
            angles.x = 40;
        }
        followTarget.transform.localEulerAngles = angles;
        transform.rotation = Quaternion.Euler(0,followTarget.transform.rotation.eulerAngles.y, 0);
        followTarget.transform.localEulerAngles = new Vector3(angles.x, 0,0);
    }
    
    void HandleMovementAnimations(){
        Vector3 direction = new Vector3(inputX,0,inputY).normalized;
        float velocityZ = Vector3.Dot(direction.normalized, transform.forward);
        float velocityX = Vector3.Dot(direction.normalized, transform.right);

        anim.SetFloat("velocityZ",direction.normalized.z);//,0.1f,Time.deltaTime);
        anim.SetFloat("velocityX",direction.normalized.x);//,0.1f,Time.deltaTime);
    }

    void Move(){
        Movement(movementSpeed);
    }

    void Run(){
        Movement(runSpeed);
    }

    void Jump(){
        anim.SetTrigger("jump");
        currentJumpCount += 1;
        velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravityConst * gravityScale);
    }
    
    void Climb(){
        Vector3 climbMovement = (transform.right * inputX) + (transform.up * inputY); 
        
            cc.Move(climbMovement * climbSpeed * Time.deltaTime);
        
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
