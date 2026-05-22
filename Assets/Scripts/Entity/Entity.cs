using Unity.VisualScripting;
using UnityEngine;

public class Entity : MonoBehaviour {   
    //~------------------------------------------------------------------ Variable --------------------------------------------------------------------

    [SerializeField] private int maxHp ;
    [SerializeField] private float invicibilityTime;
    [SerializeField] private int energyRelease;
    public int hp;
    private float invicibilityTimer;

    //~----------------------------------------------------------------------------------------------------------- Function 
    private void Update() {
        if (invicibilityTimer > 0) invicibilityTimer -= Time.deltaTime;
    }
    void Start(){
        hp = maxHp;
        if (maxHp <= 0){hp = 1;}
    }


    public void TakeDamage(IDamageSource attacker ,int damage = 1){
        if (invicibilityTimer > 0) return;
        invicibilityTimer = invicibilityTime;
        
        if (maxHp > 0){
            hp -= damage;
        }

        if (attacker is Player player) {
            player.RestoreEnergy(energyRelease);
        }

        if (hp <= 0){
            Kill();
        }
        print(name + "take " + damage + "damage ("+ hp+" remaining)"); //! prov
    }

    public void Kill(){
        print("killed : " + name); //ToDo : kill system
    }




}
