
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

using Global;

public class PlayerController : MonoBehaviour{
    //~------------------------------------------------------------------ Variable --------------------------------------------------------------------

    [Header("--- Movements ---")]
    [SerializeField] public float movementSpeed;
    [SerializeField] public float jumpStrength;
    [SerializeField] public float jumpDeceleration;
    [SerializeField] public float dashStrength;
    [SerializeField] public float dashTime;
    [SerializeField] private float dashCooldown;
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
    }
    private void Start(){
        dashCooldownValue = 0;
    }
    private void Update(){
        if (dashCooldownValue > 0) 
            dashCooldownValue -= Time.deltaTime;

        TextureInteraction();
        UpDashRecoveryVerifier();
        
    }
    private void FixedUpdate(){
        if (!_isDashing)
            playerRb.linearVelocity = new Vector2(moveDirection * movementSpeed, playerRb.linearVelocityY);
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
    //? ----------------------------------------------------------------------------------------- Jump 
    public void OnJump(InputAction.CallbackContext ctx) {
        if (_isDashing && !UpDash) return;

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

        if (ctx.started){
            _isJumping = false;
            Dash().Forget();
        }
    }
    private async UniTaskVoid Dash(){

        if (_isDashing) return;
        if (dash < 1 || dashCooldownValue > 0 ) return;

        var baseGravity = playerRb.gravityScale;
        _isDashing = true;


        playerRb.gravityScale = 0;
        playerRb.linearVelocity = Vector2.zero;
        playerRb.linearVelocityX = moveDirection * dashStrength ;
         
        await UniTask.WaitForSeconds(dashTime);

        playerRb.gravityScale = baseGravity;
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
            _isGrabbing = true;
            Grab();
        }
    }
    private void Grab() {
        float a = Tools.GetAngleByVector(LeftJoyVector);
        grab.rotation =  Quaternion.Euler(0,0,a);

        _isGrabbing = false;
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

