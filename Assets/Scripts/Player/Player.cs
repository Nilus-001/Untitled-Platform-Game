using UnityEngine;

public class Player : Actor {

    [SerializeField] public int energyMax;
    public int energy;

    void Awake(){
        GameManager.instance.RegisterPlayer(this);
    }

    protected new void Start() {
        base.Start();
        
        RestoreEnergy();
    }

    // Update is called once per frame

    
    public bool UseEnergy(int e = 1) {
        if (energy <= 0 || energy - e < 0) return false;
        energy -= e;
        return true;
    }
    public bool HasEnergy(int e) {
        return energy - e >= 0 ;
    }

    public void RestoreEnergy(int e = -1) {
        if ( e < 0 || energy + e >= energyMax) {
            energy = energyMax;
            return;
        } 
        energy += e;

    }
}
