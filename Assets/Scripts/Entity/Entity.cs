using Unity.VisualScripting;
using UnityEngine;

public class Entity : MonoBehaviour {   
    //~------------------------------------------------------------------ Variable --------------------------------------------------------------------

    [SerializeField] private int maxHp ;
    [SerializeField] private float invicibilityTime;
    [SerializeField] private int energyRelease;
    public int hp;
    protected float invicibilityTimer;
    public bool _isInvincible = false;

    //~----------------------------------------------------------------------------------------------------------- Function 
    protected void Update() {
        
        if (invicibilityTimer > 0){
            invicibilityTimer -= Time.deltaTime;
            _isInvincible = true;
        }
        else {
            _isInvincible = false;
        }
    }
    protected void Start(){
        hp = maxHp;
        if (maxHp <= 0){hp = 1;}
    }


    public virtual bool TakeDamage(IDamageSource attacker ,int damage = 1){
        if (_isInvincible) return false;
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
        
        return true;
    }

    public void Kill(){
        print("killed : " + name); //ToDo : kill system
    }




}
