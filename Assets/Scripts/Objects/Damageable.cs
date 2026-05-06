using UnityEngine;

public abstract class Damageable : MonoBehaviour
{   

    [SerializeField] public int MaxHp ;
    private int Hp;
    [SerializeField] public float InvicibilityTime; // ToDo 


    public void TakeDamage(Transform attacker ,int damage = 1)
    {
        if (InvicibilityTime <= 0)
        {
            if (MaxHp != -1)
            {
                Hp -= damage;
            }

            //Todo : implement energy regen 


            if (Hp <= 0)
            {
                kill();
            }
        }

        
    }

    public void kill()
    {
        print("killed : " + name); //ToDo : kill system
    }






    void Start()
    {
        Hp = MaxHp;
        if (MaxHp == -1){Hp = 1;}
    }

    void Update()
    {
        
    }
}
