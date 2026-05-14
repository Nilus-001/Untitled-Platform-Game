using UnityEngine;

public class PlayerSpriteMech : MonoBehaviour{

    public static float playerScale ;

    void Awake() {
        playerScale = transform.root.localScale.x;
    }
    void Update(){
        playerScale = transform.root.localScale.x;
        
    }
}
