using UnityEngine;
public class Actor : Entity , IDamageSource{

    [SerializeField] private int baseDamage;
   

    void Start(){
        
    }

    void Update(){
        
    }

    public void DealDamage(Entity target, int damage = 0) {
        if (target == this) return;
        target.TakeDamage(this, baseDamage);
    }



    
}