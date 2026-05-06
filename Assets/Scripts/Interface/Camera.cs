using UnityEngine;

public class Camera : MonoBehaviour
{

    [SerializeField] private float damping;
    
    public Transform target;

    private Vector3 vel = Vector3.zero ;

    void FixedUpdate()
    {
        Vector3 targetPosition = target.position;
        Vector3 camPosition = transform.position;

        targetPosition.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(camPosition,targetPosition, ref vel ,damping);

        
    }
}
