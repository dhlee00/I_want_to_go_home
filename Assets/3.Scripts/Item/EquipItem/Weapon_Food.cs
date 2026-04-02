using UnityEngine;

public class Weapon_Food : Weapon
{
    [SerializeField] float Add_Hunger = 10;
    [SerializeField] float Add_Thirst = 20;


    public override void Attack()
    {
        Player_Ctrl player = Owner.GetComponent<Player_Ctrl>();

        if (player != null)
        {
            // 배고픔 수분 회복
            player.Current_Hunger += Add_Hunger;
            player.Current_Thirst += Add_Thirst;

            // 아이템 삭제
            Mgr_Inventory.Inst.UseEquipInventoryItem(1);
        }
    }

}
