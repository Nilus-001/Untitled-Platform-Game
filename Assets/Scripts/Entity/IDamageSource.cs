using UnityEngine;

public interface IDamageSource {

    public bool DealDamage(Entity target, int damage = 0);
}
