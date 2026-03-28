using UnityEngine;

public class Storage : Interaction
{
    // 창고 슬롯 수량
    [SerializeField] int SlotCount = 0;

    void Start()
    {

    }

    public override void OnInteraction()
    {
        // 제작대 UI 출력
        Mgr_Game.Inst.OpenStorageUI(true, SlotCount, this);
    }
}
