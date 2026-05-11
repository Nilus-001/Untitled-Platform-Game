using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

using Global;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour{
    //~------------------------------------------------------------------ Variable --------------------------------------------------------------------

    [Header("--- Movements ---")]
    [SerializeField] private float movementSpeed;

    [Space(5)]
    [Header("  > Jump")]
    [SerializeField] private float jumpStrength;
    [SerializeField] private float jumpDeceleration;
    [SerializeField] private int bonusJumpNumber;

    [Space(5)]
    [Header("  > Dash")]
    [SerializeField] private float dashStrength;
    [SerializeField] private float dashTime;
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

    private List<ActionBuffer> InputBuffer =new List<ActionBuffer>();
    private Rigidbody2D playerRb;
    private Transform sprite;
    private float baseGravity;
    private int moveDirection;
    private int jump;

    private int dash ;
    private float dashCooldownTimer;
    
    private Transform grab;
    private Vector2 grabTargetDirection;
    private Vector2 grabTargetPos;
    private float grabCooldownTimer;
    private Vector2 LeftJoyVector;

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
        dashCooldownTimer = 0;
        grabCooldownTimer = 0;
        jump = bonusJumpNumber;
    }
    private void Update(){
        if (dashCooldownTimer > 0) 
            dashCooldownTimer -= Time.deltaTime;
        if (grabCooldownTimer > 0)
            grabCooldownTimer -= Time.deltaTime;

        ActionBufferExecute();

        TextureInteraction();

        // print("---------------------------");
        // foreach(ActionBuffer a in InputBuffer) {
        //     print(a.action + " :: "+ a.VerifyValidity() );
        // }

        // print(grabCooldownTimer + " |: grab | " + dashCooldownTimer + " |: dash"); //! prov
        // print("dash : "+_isDashing);
        // print("jump : "+_isJumping);
        // print("grab : "+_isGrabbing);
        
        
    }
    private void FixedUpdate(){
        ExecuteMove();
        ExecuteGrab();
    }

    private void KeepAndApplyPlayerGravity(float newVal = 0f) {
        float gravity = playerRb.gravityScale;
        if (gravity != 0) {
            baseGravity = gravity;
            playerRb.gravityScale = newVal;
        }
    }
    private void RestorePlayerGravity() {
        playerRb.gravityScale = baseGravity;
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
            if (jump > 0 || _grounded) {
                Jump();
                InputBuffer.Remove(action);
                return;
            }
        }

        if (action.action == ActionBuffer.ActionType.Dash) {
            if (dash > 0 && dashCooldownTimer <= 0) {
                Dash().Forget();
                InputBuffer.Remove(action);
                return;
            }
        }

        if (action.action == ActionBuffer.ActionType.Grab) {
            if (grabCooldownTimer <= 0) {
                TryGrab().Forget();
                InputBuffer.Remove(action);
                return;
            }
        }
        
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

        if (ctx.started ){
            InputBuffer.Add(new ActionBuffer(Time.time,ActionBuffer.ActionType.Jump));
        }

        if (ctx.canceled && playerRb.linearVelocityY > 5f){
            playerRb.linearVelocityY *=  jumpDeceleration;
            _isJumping = false;
        }
        
    }
    private void Jump(){
        _isJumping = true;
        if (!_grounded) {
            jump -= 1;
            if (_isDashing && _isJumping && UpDashRecovery) jump += 1; //& UPDASH RECOVERY
        }
        
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocityX , jumpStrength);
        
        
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
        KeepAndApplyPlayerGravity(0f);

        playerRb.linearVelocity = Vector2.zero;
        playerRb.linearVelocityX = moveDirection * dashStrength ;
         
        await UniTask.WaitForSeconds(dashTime);

        RestorePlayerGravity();
        if(!_grounded) dash -= 1;
        
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

        float a = Tools.GetAngleByVector(LeftJoyVector);
        grab.rotation =  Quaternion.Euler(0,0,a);
        grab.gameObject.SetActive(true);


        await UniTask.WaitForSeconds(0.2f);
        if (!_isGrabbing)
            grab.gameObject.SetActive(false);
            grabCooldownTimer = grabCooldown;

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
            grabCooldownTimer = grabCooldown;
            

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
        jump = bonusJumpNumber;
        dash = 1;
        _isJumping = false;
    }

}

//~--------------------------------------------------------------------------------------------------------- Action Buffer
public class ActionBuffer {
    
    public enum ActionType{
        Jump,
        Dash,
        Grab,
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
