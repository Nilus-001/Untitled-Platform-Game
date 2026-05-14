using System.Collections.Generic;
using UnityEngine;

public class MainLegsController : MonoBehaviour{

    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private List<LegController> legTargetList;
    [SerializeField] private List<Transform> legAirPointBaseTargetList;
    [SerializeField] private List<Transform> legAirPointUpTargetList;
    [SerializeField] private float jumpRetractSpeed;
    [SerializeField] private float rayCastDistance;
    [SerializeField] private LayerMask groundLayer;
    private List<Vector2> jumpPreviousPosList = new();
    public bool _isgrounded;


    private int jumpIndex = 0;
    private float T;

    void Awake() {
        float scale = PlayerSpriteMech.playerScale;
        
        jumpRetractSpeed *= scale;
        rayCastDistance *= scale;

        
    }

    void Update(){
        CheckGround();
        if (!_isgrounded) {
            SetJumpPosition();
        }
        else {
            jumpIndex = 0;
        }
    }


    private void SetJumpPosition() {

        if (jumpIndex == 0) {
            jumpPreviousPosList.Clear();
            for (int i = 0 ; i < legTargetList.Count; i++) {
                jumpPreviousPosList.Add(legTargetList[i].transform.localPosition);
            }
            jumpIndex = 1;
            T = 0;
        }
        if (jumpIndex == 1) {
            T += jumpRetractSpeed * Time.deltaTime;
             for (int i = 0 ; i < legTargetList.Count; i++) {
                float tVelocity = 1 - Mathf.Clamp(Mathf.Abs(playerRb.linearVelocityY)/8, 0, 1);

                Vector2 vFirst = legAirPointBaseTargetList[i].localPosition;
                Vector2 vSecond = legAirPointUpTargetList[i].localPosition;

                Vector2 v = Vector2.Lerp(vFirst, vSecond, tVelocity);
                Vector2 vStart = jumpPreviousPosList[i];

                legTargetList[i].transform.localPosition = Vector2.Lerp(vStart,v,T);
            }
        }
       
    }


    private void CheckGround() {
        RaycastHit2D ray = Physics2D.Raycast(transform.position,Vector2.down,rayCastDistance,groundLayer);
        if (ray.collider != null) {
            _isgrounded = true;
        }
        else {
            _isgrounded = false;
        }
    }
}
