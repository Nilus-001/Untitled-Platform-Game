using UnityEngine;

public interface IDamageSource {

    public void DealDamage(Entity target, int damage = 0);
}
