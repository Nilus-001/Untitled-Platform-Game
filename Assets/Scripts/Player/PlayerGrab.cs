using UnityEngine;

public class PlayerGrab : MonoBehaviour{

    private PlayerController playerC;    
    void Awake(){
        playerC = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("CanBeGrab")) {
            playerC.Grab(other);
        }
    }
}
