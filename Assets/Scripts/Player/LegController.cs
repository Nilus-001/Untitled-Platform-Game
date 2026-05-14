using Global;
using UnityEngine;

public class LegController : MonoBehaviour{

    [SerializeField] private Transform linkedLegTarget;
    [SerializeField] private LegController oppositeLeg;
    [SerializeField] private float legMovementSpeed;
    [SerializeField] private float stepDistance;
    [SerializeField] private float liftDistance;
    [SerializeField] private Transform linkedRayCast;
    [SerializeField] private float rayCastHover;
    [SerializeField] private float checkGroundDistance;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float startDelay = 0f; 
    private float delayTimer = 0f;

    private bool _isgrounded;
    private int stepIndex;
    private Vector2 startPoint;
    private Vector2 liftPoint;
    private Vector2 targetPoint;
    private float T;
    private MainLegsController globalConroller;



    void Awake() {
        stepIndex = 0;
        globalConroller = GetComponentInParent<MainLegsController>();
        //*---------------------------- Scale Adaptor ----------------------------
        float scale = PlayerSpriteMech.playerScale;

        stepDistance *= scale;
        liftDistance *= scale;
        rayCastHover *= scale;
        // legMovementSpeed *= scale;
        checkGroundDistance *= scale;


    }

    void Update(){
        
        
        if (delayTimer < startDelay) {
            delayTimer += Time.deltaTime;
            return;
        }
        

        if (globalConroller._isgrounded ) {

            CheckGround();
            targetPoint = transform.position;

            if (Vector2.Distance(transform.position,linkedLegTarget.position) > stepDistance && stepIndex == 0 && oppositeLeg._isgrounded) {
                startPoint = linkedLegTarget.position;
                liftPoint = Vector2.Lerp(startPoint,targetPoint,1f/3f);
                liftPoint.y += liftDistance;
                stepIndex = 1;
                T = 0f;
            }
            if (stepIndex == 1) {
                T += legMovementSpeed * Time.deltaTime;
                linkedLegTarget.position = Tools.BezierQuadratique(startPoint, liftPoint, targetPoint, T);
                if (T >= 1f) stepIndex = 0;
            }
            

            //? --------------------------------------------- Ground Check
            _isgrounded = stepIndex == 0;
        }
        else {
            linkedLegTarget.position = transform.position;
        }
        
        
    }


    private void CheckGround() {
        
        
        RaycastHit2D ray = Physics2D.Raycast(linkedRayCast.gameObject.transform.position,Vector2.down,checkGroundDistance,groundLayer);
        if (ray.collider != null) {
            Vector2 v = ray.point;
            v.y += rayCastHover;
            transform.position = ray.point;
        }
        else {
            
        }
    }

}
