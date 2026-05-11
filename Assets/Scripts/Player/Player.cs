using UnityEngine;

public class Player : Entity{

    [SerializeField] private int energyMax;
    private int energy;

    void Start() {
        RestoreEnergy();
    }

    // Update is called once per frame
    void Update(){
        
    }

    private bool UseEnergy(int e = 1) {
        if (energy <= 0 || energy - e < 0) return false;
        energy -= e;
        return true;
    }

    private void RestoreEnergy(int e = -1) {
        if ( e == -1 || energy + e >= energyMax) {
            energy = energyMax;
            return;
        } 
        energy += e;

    }
}
