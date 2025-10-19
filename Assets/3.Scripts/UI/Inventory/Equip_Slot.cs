using UnityEngine;

public class Equip_Slot : MonoBehaviour, ISlot
{
    [SerializeField] int SlotNum;
    [SerializeField] Item ItemData;

    // ½½·Ô ÀÎµ¦½º ºÎ¿©
    void ISlot.Set_SlotNum(int num)
    {
        SlotNum = num;
    }
}
