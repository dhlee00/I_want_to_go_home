using UnityEngine;

public class Inven_Slot : MonoBehaviour, ISlot
{
    [SerializeField] int SlotNum;

    // ½½·Ô ÀÎµ¦½º ºÎ¿©
    void ISlot.Set_SlotNum(int num)
    {
        SlotNum = num;
    }
}
