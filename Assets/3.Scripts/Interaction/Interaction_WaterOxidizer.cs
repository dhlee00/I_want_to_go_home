using UnityEngine;

public class Interaction_WaterOxidizer : Interaction
{
    public override void OnInteraction()
    {
        // 제작대 UI 출력
        Mgr_Game.Inst.OpenWaterOxidizerUI(true);
    }
}
