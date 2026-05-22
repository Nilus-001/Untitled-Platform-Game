using UnityEngine;

public class GameManager : MonoBehaviour{

    //*---------------------------------------------------------------------------------------------------------------
    public static GameManager instance;
    void Awake(){
        if ( instance != null) {
            Debug.LogWarning("More than 1 instance of GameManager in the scene");
            // Destroy(gameObject);
            return;
        }
        instance = this;
        // DontDestroyOnLoad(gameObject);
    }
    //*---------------------------------------------------------------------------------------------------------------

    public Player Player;
    public void RegisterPlayer(Player player) => Player = player;
   
}
