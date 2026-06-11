using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

using Global;
using System.Collections.Generic;



public class PlayerController : MonoBehaviour{

    //~------------------------------------------------------------------ Variable --------------------------------------------------------------------
    //* ------------------------------------------------------- ELEMENTS  

    [Header("--- Elements ---")]
    [SerializeField] Transform sprite;
    [SerializeField] float spriteRotationSpeed;
    [SerializeField] float spriteRotationMaxAngle;
    //* ------------------------------------------------------- MOVEMENTS  

    [Space(10)]
    [Header("--- Movements ---")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float airAcceleration;
    [SerializeField] private float airDeceleration;
    [SerializeField] private float groundAcceleration;
    [SerializeField] private float groundDeceleration;
    //? -------------------------------------------------------- Launch
    [Space(5)]
    [Header("  > Launch")]
    [SerializeField] private float launchAirControlFactor;
    [SerializeField] private float apexThreshold;
    [SerializeField] private float apexGravityMultiplier;
    [SerializeField] private float apexAirControlBonus;


    //? -------------------------------------------------------- Jump  

    [Space(5)]
    [Header("  > Jump")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float timeToApex;
    [SerializeField] private float fallGravityMultiplier;
    
    [SerializeField] private float jumpDeceleration;
    [SerializeField] public int bonusJumpNumber;


    //? -------------------------------------------------------- Dash  

    [Space(5)]
    [Header("  > Dash")]
    [SerializeField] private int dashEnergyUsage;
    [SerializeField] private float dashStrength;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    //? -------------------------------------------------------- Grab  

    [Space(5)]
    [Header("  > Grab")]
    [SerializeField] private int grabEnergyUsage;
    [SerializeField] private float grabDistance;
    [SerializeField] private float garbStrength;
    [SerializeField] private float grabPropultionStrength;
    [SerializeField] private float grabPropultionDuration;
    [SerializeField] private float grabCooldown;
    //? -------------------------------------------------------- Attack  

    [Space(5)]
    [Header("  > Attack")]
    [SerializeField] private float attackDuration;
    [SerializeField] private float attackCooldown;
    //Todo : Idea -> combo counter 
    

    //* ------------------------------------------------------- EXTRA ABILITIES  

    [Space(10)]
    [Header("--- Capacities ---")]

    [SerializeField] private bool UpDash;
    [SerializeField] private bool UpDashRecovery;

    [Space(10)]
    [Header("------")]

    //* ------------------------------- ELEMENTS -------------------------------
    private Player playerLogic;
    private Rigidbody2D playerRb;
    private Transform damageBox;
    private Transform grab;
    private Vector2 leftJoyVector;
    private Animator spriteLegAnimator;

    //* ------------------------------- BASICS -------------------------------
    private int direction;
    private int permaDirection = 1;
    private float gravity;
    private float savedGravity;
    private bool _isGravityRestored;

    //* ------------------------------- ACTIONS -------------------------------
    private List<ActionBuffer> InputBuffer = new List<ActionBuffer>();
    private bool freezeMove;
    //? ------------------------------- Jump  
    public int jump;
    private float jumpForce;
    //? ------------------------------- Dash  
    public int dash ;
    private float dashCooldownTimer;
    //? ------------------------------- Grab  
    private Vector2 grabTargetDirection;
    private Vector2 grabTargetPos;
    private float grabCooldownTimer;
    //? ------------------------------- Attack  

    private float attackCooldownTimer;
    //* ------------------------------- GESTIONS -------------------------------

    //? ------------------------------- Energy  
    private bool energyRecovery;

    //? ------------------------------- Launch  
    private float launchTimer;
    private float launchTime;
    public float ApexBlend;
    

    //* ------------------------------- STATES -------------------------------
    private bool _isDashing;
    private bool _isGrounded;
    private bool _isJumping;
    private bool _isGrabbing;
    private bool _isAttacking;
    private bool _isLaunch;
    
    //~----------------------------------------------------------------------------------------------------------- Function 
    //? ----------------------------------------------------------------------------------------- Execute 
    private void Awake(){
        playerRb = GetComponent<Rigidbody2D>();
        playerLogic = GetComponent<Player>();
        spriteLegAnimator = sprite.GetComponentInChildren<Animator>();

        grab = transform.Find("GrabContainer"); //? Name Important
        grab.gameObject.SetActive(false);
        ResizedGrab();

        damageBox = transform.Find("DamageBox"); //? Name Important
        damageBox.gameObject.SetActive(false);

        //& -------- SetUp --------
        playerRb.gravityScale = 0f;

        gravity   = 2f * jumpHeight / (timeToApex * timeToApex);
        jumpForce = gravity * timeToApex;

        savedGravity = gravity;
        _isGravityRestored = true;

    }
    private void Start(){
        dashCooldownTimer = 0;
        grabCooldownTimer = 0;
        attackCooldownTimer = 0;

        jump = bonusJumpNumber;
    }
    private void Update(){
        if (dashCooldownTimer > 0) 
            dashCooldownTimer -= Time.deltaTime;
        if (grabCooldownTimer > 0)
            grabCooldownTimer -= Time.deltaTime;
        if (attackCooldownTimer > 0)
            attackCooldownTimer -= Time.deltaTime;        
        LaunchTimer();
        


        if (energyRecovery && playerLogic.energy < playerLogic.energyMax) {
            playerLogic.RestoreEnergy(1);
        }
        else {
            energyRecovery = false;
        }

        ActionBufferExecute();
        SpriteRotation();

        
        

        //? Jump Check :
        if (playerRb.linearVelocity.y <= 0f)
            _isJumping = false;

        
    }
    private void FixedUpdate(){
        ApplyGravity();
        ExecuteHorizontalMove();

        ExecuteGrab();
        sprite.GetChild(0).position = transform.position;
    }
    
    //? ----------------------------------------------------------------------------------------- Gravity

    public void ChangeAndStoreGravity(float g = 0f) {
        if (!_isGravityRestored) {
            print("gravity is unrestored ( you tried to change :" + g + " but savedGravity : "+ savedGravity + "wasn't restored) || Actual gravity : "+ gravity);
            return;
        }

        savedGravity = gravity;
        gravity = g;
        _isGravityRestored = false;
    }
    public void RestoreGravity() {
        gravity = savedGravity;
        _isGravityRestored = true;
    }
    
    //? ----------------------------------------------------------------------------------------- Propulsion
    private void Launch(Vector2 direction,float force,float duration) {
        if (_isLaunch) return;

        playerRb.linearVelocity  = Vector2.zero;
        playerRb.linearVelocity = direction * force;

        launchTime = duration;
        launchTimer = launchTime;

        _isLaunch = true;
        _isJumping = false; //? jump check

    }
    private void LaunchTimer() {
        if (launchTimer > 0) {
            launchTimer -= Time.deltaTime;
        }
        else {
            _isLaunch = false;
        }


        if (_isLaunch && !_isGrounded){
            float absVY = Mathf.Abs(playerRb.linearVelocityY);
            ApexBlend = Mathf.InverseLerp(apexThreshold, 0f, absVY);
            //? --------------------------------

            
        }
        else{
            ApexBlend = 0f;
        }
    }
   

    //* -------------------------------------------------------------------------------------------------- Action Buffer
    private void ActionBufferExecute() {
        if (InputBuffer.Count == 0) return;
        foreach(ActionBuffer action in InputBuffer.ToArray()) {
            if (action.VerifyValidity()) {
                ExecuteBufferAction(action);
                return;
            }
            else {
                InputBuffer.Remove(action);  
            }
        }
    }
    //? ----------------------------------------------------------------------------------------- Action Condition 
    private void ExecuteBufferAction(ActionBuffer action) {
        if (action.action == ActionBuffer.ActionType.Jump) {
            if ( jump > 0  && !_isAttacking && !_isGrabbing) {
                Jump();
                InputBuffer.Remove(action);
                return;
            }
        }

        if (action.action == ActionBuffer.ActionType.Dash) {
            if (dash > 0 && dashCooldownTimer <= 0 && !_isAttacking && !_isGrabbing && (_isGrounded || playerLogic.UseEnergy(dashEnergyUsage))) {
                Dash().Forget();
                InputBuffer.Remove(action);
                return;
            }
        }

        if (action.action == ActionBuffer.ActionType.Grab && playerLogic.HasEnergy(grabEnergyUsage)) {
            if (grabCooldownTimer <= 0 && !_isAttacking) {
                TryGrab().Forget();
                InputBuffer.Remove(action);
                return;
            }
        }

        if (action.action == ActionBuffer.ActionType.Attack) {
            if (attackCooldownTimer <= 0 && !_isDashing && !_isGrabbing && !_isAttacking) {
                Attack().Forget();
                InputBuffer.Remove(action);
                return;
            }
        }
        

        
    }


    //* -------------------------------------------------------------------------------------------------- Movement 
    //? ----------------------------------------------------------------------------------------- Gravity // Move
    private void ApplyGravity() {
        if (_isGrounded && playerRb.linearVelocityY <= 0) {
            //? On floor
            return;
        }
        float gravMultiplier = 1f;
        // if(_isLaunch && ApexBlend > 0f) {
        //     float baseMultiplier = playerRb.linearVelocityY < 0f ? fallGravityMultiplier : 1f;
        //     gravMultiplier = Mathf.Lerp(baseMultiplier, apexGravityMultiplier, ApexBlend);
        // }
        if (playerRb.linearVelocityY < 0) {
            //? Down
            gravMultiplier = fallGravityMultiplier;
        }
        else if (playerRb.linearVelocityY > 0 && !_isJumping) {
            //? Up (out Jump)
            gravMultiplier = fallGravityMultiplier;
        }
        playerRb.AddForce(Vector2.down * gravity * gravMultiplier, ForceMode2D.Force);
    }
    private void ExecuteHorizontalMove() {
        if (_isDashing || _isGrabbing) return;

        float Xspeed = direction * moveSpeed * leftJoyVector.magnitude;
        float currentXspeed = playerRb.linearVelocityX;

        if (freezeMove) Xspeed = 0; //? Freeze Mode
        
        float acceleration;
        if (_isLaunch) {
            if (Mathf.Abs(direction) > 0.01f) {
                float apexBoost = Mathf.Lerp(1f, apexAirControlBonus, ApexBlend);
                acceleration = airAcceleration * launchAirControlFactor * apexBoost;
            }
                
            else
                acceleration = 0f;
        }
        else if (_isGrounded) {
            acceleration = Mathf.Abs(direction) > 0.01f ? groundAcceleration : groundDeceleration;
        }
        else {
            acceleration = Mathf.Abs(direction) > 0.01f ? airAcceleration : airDeceleration;
        }
        float speedDelta = acceleration * Time.deltaTime;
        float movement = Mathf.Clamp(Xspeed-currentXspeed,-speedDelta,speedDelta);

        playerRb.AddForce(new Vector2(movement,0f),ForceMode2D.Impulse);
       
    }

    //? ----------------------------------------------------------------------------------------- Direction 
    public void OnMove(InputAction.CallbackContext ctx) {
        leftJoyVector = ctx.ReadValue<Vector2>();

        if (Mathf.Abs(leftJoyVector.x) > 0.05f) {
            permaDirection = (leftJoyVector.x > 0) ? 1 : -1;
        }
        if (_isDashing) return;
        
        direction = permaDirection;
        
        if (ctx.canceled) {
            leftJoyVector = Vector2.zero;
            direction = 0;
            return;
        }
        

   
    }
    
    //? ----------------------------------------------------------------------------------------- Jump 
    public void OnJump(InputAction.CallbackContext ctx) {
        if (_isDashing && !UpDash ) return;

        if (ctx.started ){
            InputBuffer.Add(new ActionBuffer(Time.time,ActionBuffer.ActionType.Jump));
        }

        if (ctx.canceled && playerRb.linearVelocityY > 0f){
            playerRb.linearVelocityY *=  jumpDeceleration;
            _isJumping = false;
        }
        
    }
    private void Jump(){
        
        
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocityX,0f);
        playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        if (!_isGrounded) {
            jump -= 1;
            if (_isDashing && _isJumping && UpDashRecovery) jump += 1; //& UPDASH RECOVERY
        }
        _isJumping = true;
        
    }
    
    //? ----------------------------------------------------------------------------------------- Dash 
    public void OnDash(InputAction.CallbackContext ctx) {
        if (_isDashing ) return;
        if (ctx.started){
            InputBuffer.Add(new ActionBuffer(Time.time,ActionBuffer.ActionType.Dash));
        }
    }
    private async UniTaskVoid Dash(){
        _isJumping = false;
        _isDashing = true;
        
        playerRb.linearVelocity = Vector2.zero;
        playerRb.linearVelocityX = permaDirection * dashStrength ;
         
        await UniTask.WaitForSeconds(dashTime);

        if(!_isGrounded) dash -= 1;
        
        dashCooldownTimer = dashCooldown;   
        _isDashing = false;
        
    }
   
    //? ----------------------------------------------------------------------------------------- Grab 
    public void OnGrab(InputAction.CallbackContext ctx) {
        if (ctx.started) {
            InputBuffer.Add(new ActionBuffer(Time.time,ActionBuffer.ActionType.Grab));
        }

    }
    private async UniTaskVoid TryGrab() {
        if (grabCooldownTimer > 0) return;

        float a = Tools.GetAngleByVector(leftJoyVector);
        grab.rotation =  Quaternion.Euler(0,0,a);
        grab.gameObject.SetActive(true);


        await UniTask.WaitForSeconds(0.2f);
        if (!_isGrabbing)
            grab.gameObject.SetActive(false);
            grabCooldownTimer = grabCooldown;

    }
    public void Grab(Collider2D collider) {

        playerLogic.UseEnergy(grabEnergyUsage);

        grabTargetPos = collider.transform.position;
        grabTargetDirection = (grabTargetPos - playerRb.position).normalized;
        
        playerRb.linearVelocity = Vector2.zero;

        _isGrabbing = true;
        grab.gameObject.SetActive(false);
        
    }
    private void ExecuteGrab() {
        if (!_isGrabbing ) return;
        
        Vector2 directionToTarget = (grabTargetPos - playerRb.position).normalized;

        if (Vector2.Dot(directionToTarget,grabTargetDirection) < 0) {
            Launch(grabTargetDirection ,grabPropultionStrength,grabPropultionDuration) ;
            _isGrabbing = false;
            grabTargetDirection = Vector2.zero;
            grabTargetPos = Vector2.zero;
            grabCooldownTimer = grabCooldown;
            

            return;
        }


        playerRb.linearVelocity = grabTargetDirection * garbStrength;
        
    }
    
    //? ----------------------------------------------------------------------------------------- Damage
    public void OnAttack(InputAction.CallbackContext ctx) {
        if (ctx.started) {
            InputBuffer.Add(new ActionBuffer(Time.time,ActionBuffer.ActionType.Attack));
        }
    }
    private async UniTaskVoid Attack() {

        _isAttacking = true;
        freezeMove = true;
        damageBox.gameObject.SetActive(true);
        ChangeAndStoreGravity(0f);
        //~ ---------------------------------------------------------------
        playerRb.linearVelocity = Vector2.zero;

        Vector3 scale = damageBox.localScale;
        damageBox.localScale = new Vector3(Mathf.Abs(scale.x) * permaDirection ,scale.y,scale.z);
        
        
        
        
        spriteLegAnimator.SetBool("isAttacking", true);
        //~ ---------------------------------------------------------------
        await UniTask.WaitForSeconds(attackDuration);

        RestoreGravity();
        freezeMove = false;
        _isAttacking = false;
        damageBox.gameObject.SetActive(false);
        spriteLegAnimator.SetBool("isAttacking", false);


        attackCooldownTimer = attackCooldown;


    }
    public void Damage(Entity e) {
        //Todo :  Extra Animation
        if (playerLogic.DealDamage(e)) {
            if(jump < bonusJumpNumber)
                jump += 1;
            dash = 1;
        }   

        
    }



    //* -------------------------------------------------------------------------------------------------- Texture Intertaction 
    private void SpriteRotation() {
        float angle = Tools.GetAngleByVector(playerRb.linearVelocity);
        angle = Mathf.Clamp(angle, -spriteRotationMaxAngle, spriteRotationMaxAngle);
        
        Quaternion targetRotation = Quaternion.Euler(0,0,0) ;
        if (!_isGrounded) {
            if (playerRb.linearVelocityX != 0f) {
                float sign = Mathf.Sign(playerRb.linearVelocityY);
                targetRotation = Quaternion.Euler(0, 0, sign * angle );
            }
        }
    
        sprite.rotation = Quaternion.RotateTowards(sprite.rotation, targetRotation, Time.deltaTime * spriteRotationSpeed);
       
    }
    private void ResizedGrab() {
       
        BoxCollider2D grabBox = grab.GetChild(0).GetComponent<BoxCollider2D>();
        // SpriteRenderer grabSprite = grab.GetChild(1).GetComponent<SpriteRenderer>();

        grabBox.size = new Vector2(2f,grabDistance);
        grabBox.offset = new Vector2(0f ,(grabDistance-12)/2);

    }
    //* -------------------------------------------------------------------------------------------------- Ground Detection 
    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Ground")){
            _isGrounded = true;
            GroundRecovery();
        }
    }
    void OnCollisionExit2D(Collision2D collision){
         if (collision.gameObject.CompareTag("Ground")){
            _isGrounded = false;
            GroundRecoveryCancel();
        }
    }
    private void GroundRecovery() {
        jump = bonusJumpNumber;
        dash = 1;
        _isJumping = false;
        energyRecovery = true;
        _isLaunch = false;
    }
    private void GroundRecoveryCancel() {
        energyRecovery = false;
    }

}

//~--------------------------------------------------------------------------------------------------------- Action Buffer
public class ActionBuffer {
    
    public enum ActionType{
        Jump,
        Dash,
        Grab,
        Attack,
    }
    public ActionType action;
    public float timeStamp;

    public static float expireTime = 0.2f; 

    public ActionBuffer(float time,ActionType a) {
        timeStamp = time;
        action = a;
    }

    public bool VerifyValidity() {
        if (timeStamp + expireTime >= Time.time) {
            return true;
        }
        return false;
    }


    
}
