using UnityEngine;

public class GrabbingPole : Entity{

    private Animator animator;
    private BoxCollider2D col;

    void Awake() {
        animator = GetComponentInChildren<Animator>();
        col = GetComponent<BoxCollider2D>();
    }

    protected new void Update() {
        base.Update();

        bool isInvincible = animator.GetBool("isInvincible");
        if (invicibilityTimer - 0.4 < 0 && isInvincible) {
            animator.SetBool("isInvincible", false); 
           
        }
        if (!_isInvincible ){
            col.enabled = true;
        }


    }

    public override bool TakeDamage(IDamageSource attacker ,int damage = 1) {
        bool hasdDealDamage = base.TakeDamage(attacker,damage);
        if (!hasdDealDamage) return false;

        animator.SetBool("isInvincible", true);
        if (col.enabled) col.enabled = false;
            
        
        return true;
        
    }
}
