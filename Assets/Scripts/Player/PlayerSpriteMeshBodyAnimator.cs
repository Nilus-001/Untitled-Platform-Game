using UnityEngine;

public class PlayerSpriteMeshBodyAnimator : MonoBehaviour{
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobSpeed = 8f;
    [SerializeField] private float tiltAmount = 5f;




    private Vector3 baseLocalPos;
    private Quaternion baseLocalRotation;


    void Start() {
        baseLocalPos = transform.localPosition;
        baseLocalRotation = transform.localRotation;
    }

    void LateUpdate() {

        if (playerRb == null) return; // Si pas de Rigidbody, on ne fait rien

        float speed = playerRb.linearVelocity.magnitude;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude * Mathf.Clamp01(speed);

        float tilt = -playerRb.linearVelocityX * tiltAmount;
        
        transform.localPosition = baseLocalPos + new Vector3(0, bob, 0);
        transform.localRotation = baseLocalRotation * Quaternion.Euler(0, 0, tilt);
    }

}
