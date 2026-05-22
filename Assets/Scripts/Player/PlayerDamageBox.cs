using UnityEngine;

public class PlayerDamageBox : MonoBehaviour
{
    private PlayerController playerC;    
    void Awake(){
        playerC = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent<Entity>(out Entity entity)) {
            playerC.Damage(entity);
        }
    }
}
