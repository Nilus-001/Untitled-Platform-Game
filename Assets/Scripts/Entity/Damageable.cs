using Unity.VisualScripting;
using UnityEngine;

public abstract class Damageable : MonoBehaviour {   
    //~------------------------------------------------------------------ Variable --------------------------------------------------------------------

    [SerializeField] private int maxHp ;
    [SerializeField] private float invicibilityTime;
    private int energyRelease = 1;
    private int hp;
    private float invicibilityTimer;

    //~----------------------------------------------------------------------------------------------------------- Function 
    private void Update() {
        if (invicibilityTimer > 0) invicibilityTimer -= Time.deltaTime;
    }
    void Start(){
        hp = maxHp;
        if (maxHp == -1){hp = 1;}
    }


    public void TakeDamage(Entity attacker ,int damage = 1){
        if (invicibilityTimer <= 0){
            invicibilityTimer = invicibilityTime;
            
            if (maxHp != -1){
                hp -= damage;
            }

            if (attacker is Player player) {
                
            }

            if (hp <= 0){
                Kill();
            }
        }

        
    }

    public void Kill(){
        print("killed : " + name); //ToDo : kill system
    }




}
