using UnityEngine;
public class Actor : Entity , IDamageSource{

    [SerializeField] private int baseDamage;
   





    public bool DealDamage(Entity target, int damage = 0) {
        if (target == this) return false;
        return target.TakeDamage(this, baseDamage);
    }



    
}