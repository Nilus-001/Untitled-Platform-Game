using UnityEngine;

public class EnergyBar : ProgressBarToCenter {
    private Player player;

    new void Start() {
        base.Start();
        player = GameManager.instance.Player;

    }

    new void Update(){
        
        value = (float)player.energy / (float)player.energyMax;
        base.Update();
        
    }
}

