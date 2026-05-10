
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

using Global;
using UnityEditor.Experimental.GraphView;

public class PlayerController : MonoBehaviour{
    //~------------------------------------------------------------------ Variable --------------------------------------------------------------------

    [Header("--- Movements ---")]
    [SerializeField] public float movementSpeed;

    [Space(5)]
    [Header("  > Jump")]
    [SerializeField] public float jumpStrength;
    [SerializeField] public float jumpDeceleration;
    
    [Space(5)]
    [Header("  > Dash")]
    [SerializeField] public float dashStrength;
    [SerializeField] public float dashTime;
    [SerializeField] private float dashCooldown;

    [Space(5)]
    [Header("  > Grab")]
    [SerializeField] private float grabDistance;
    [SerializeField] private float garbStrength;
    [SerializeField] private float garbExitStrength;
    [SerializeField] private float grabCooldown;


    [Space(10)]
    [Header("--- Capacities ---")]

    [SerializeField] private bool UpDash;
    [SerializeField] private bool UpDashRecovery;

    
    private Rigidbody2D playerRb;
    private Transform sprite;
    private Transform grab;
    private int moveDirection;
    private Vector2 LeftJoyVector;
    private bool doubleJump;
    private int dash ;
    private float dashCooldownValue;
    private float baseGravity;
    private Vector2 grabTargetDirection;
    private Vector2 grabTargetPos;
    private float grabCooldownValue;

    //* -------------------------------------------------------------------------------------------------- Detection // Status 
    private bool _isDashing;
    private bool _grounded;
    private bool _isJumping;
    private bool _isGrabbing;

    
    //~----------------------------------------------------------------------------------------------------------- Function 

    private void Awake(){
        playerRb = GetComponent<Rigidbody2D>();
        sprite = transform.GetChild(0);

        grab = transform.GetChild(1);
        grab.gameObject.SetActive(false);
        ResizedGrab();
    }
    private void Start(){
        dashCooldownValue = 0;
    }
    private void Update(){
        if (dashCooldownValue > 0) 
            dashCooldownValue -= Time.deltaTime;
        if (grabCooldownValue > 0)
            grabCooldownValue -= Time.deltaTime;

        TextureInteraction();
        UpDashRecoveryVerifier();
        
    }
    private void FixedUpdate(){
        ExecuteMove();
        ExecuteGrab();
    }

    private void KeepAndApplyPlayerGravity(float newVal = 0) {
        float gravity = playerRb.gravityScale;
        if (gravity != 0) {
            baseGravity = gravity;
            playerRb.gravityScale = newVal;
        }
    }
    private void RestorePlayerGravity() {
        playerRb.gravityScale = baseGravity;
    }


    //* -------------------------------------------------------------------------------------------------- Movement 
    //? ----------------------------------------------------------------------------------------- Direction 
    public void OnMove(InputAction.CallbackContext ctx) {

        
        if (ctx.canceled) {
            moveDirection = 0;
            return;
        }
        if (_isDashing) return;

        LeftJoyVector = ctx.ReadValue<Vector2>();
        moveDirection = (LeftJoyVector.x > 0) ? 1 : (LeftJoyVector.x < 0) ? -1 : 0;
    }
    private void ExecuteMove() {
        if (!_isDashing && !_isGrabbing)
            playerRb.linearVelocity = new Vector2(moveDirection * movementSpeed, playerRb.linearVelocityY);
    }
    //? ----------------------------------------------------------------------------------------- Jump 
    public void OnJump(InputAction.CallbackContext ctx) {
        if (_isDashing && !UpDash ) return;

        if (ctx.started && doubleJump){
            Jump();
        }

        if (ctx.canceled && playerRb.linearVelocityY > 5f){
            playerRb.linearVelocityY *=  jumpDeceleration;
            _isJumping = false;
        }
        
    }
    private void Jump(){
        _isJumping = true;
        if (!_grounded) {
            doubleJump = false;
        }
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocityX , jumpStrength);
        
    }
    //? ----------------------------------------------------------------------------------------- Dash 
    public void OnDash(InputAction.CallbackContext ctx) {
        if (_isDashing ) return;
        if (ctx.started){
            _isJumping = false;
            Dash().Forget();
        }
    }
    private async UniTaskVoid Dash(){

        if (dash < 1 || dashCooldownValue > 0 ) return;

        _isDashing = true;
        KeepAndApplyPlayerGravity(0f);

        playerRb.linearVelocity = Vector2.zero;
        playerRb.linearVelocityX = moveDirection * dashStrength ;
         
        await UniTask.WaitForSeconds(dashTime);

        RestorePlayerGravity();
        if(!_grounded) dash -= 1;
        
        dashCooldownValue = dashCooldown;   
        _isDashing = false;
        
    }
    private void UpDashRecoveryVerifier() {
        if (_isDashing && _isJumping && !_grounded && UpDashRecovery) doubleJump = true;
    }
    //? ----------------------------------------------------------------------------------------- Grab 
    public void OnGrab(InputAction.CallbackContext ctx) {
        if (ctx.started) {
            TryGrab().Forget();
        }
        // if (ctx.canceled) {
        //     _isGrabbing = false;
        // }
    }
    private async UniTaskVoid TryGrab() {
        if (grabCooldownValue > 0) return;

        float a = Tools.GetAngleByVector(LeftJoyVector);
        grab.rotation =  Quaternion.Euler(0,0,a);
        grab.gameObject.SetActive(true);


        await UniTask.WaitForSeconds(0.2f);
        if (!_isGrabbing)
            grab.gameObject.SetActive(false);

    }
    
    public void Grab(Collider2D collider) {

        grabTargetPos = collider.transform.position;
        grabTargetDirection = (grabTargetPos - playerRb.position).normalized;
        
        playerRb.linearVelocity = Vector2.zero;

        _isGrabbing = true;
        grab.gameObject.SetActive(false);
        
        


    }

    private void ExecuteGrab() {
        if (!_isGrabbing ) return;
        
        Vector2 directionToTarget =( grabTargetPos - playerRb.position).normalized;

        if (Vector2.Dot(directionToTarget,grabTargetDirection) < 0) {
            playerRb.linearVelocity = grabTargetDirection * garbExitStrength;
            _isGrabbing = false;
            grabTargetDirection = Vector2.zero;
            grabTargetPos = Vector2.zero;
            grabCooldownValue = grabCooldown;
            

            return;
        }


        playerRb.linearVelocity = grabTargetDirection * garbStrength;
        
    }




    //* -------------------------------------------------------------------------------------------------- Texture Intertaction 
    private float stabilizeAngle = 0;
    private void TextureInteraction(){
    //? ----------------------------------------------------------------------------------------- Sprite Rotation 

        float angle = Tools.GetAngleByVector( playerRb.linearVelocity);
        if (_grounded){
            
            stabilizeAngle = stabilizeAngle >= -10f && stabilizeAngle <= 10f ? 0f : stabilizeAngle;
            float addDegrees = -Mathf.Sign(stabilizeAngle);

            if ( moveDirection != 0 && Mathf.Sign(moveDirection) == Mathf.Sign(addDegrees)){
                stabilizeAngle += addDegrees + moveDirection;
            }
            else{
                stabilizeAngle += moveDirection != 0 ? -moveDirection : addDegrees;
            }
             
            angle = stabilizeAngle;
        }
    
        sprite.rotation =  Quaternion.Euler(0,0,angle);
        stabilizeAngle = angle;
        


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
            _grounded = true;
            GroundRecovery();
        }
    }
    void OnCollisionExit2D(Collision2D collision){
         if (collision.gameObject.CompareTag("Ground")){
            _grounded = false;
        }
    }
    private void GroundRecovery() {
        doubleJump = true;
        dash = 1;
    }

}

