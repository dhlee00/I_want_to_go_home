using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;




public class Weapon : MonoBehaviour
{
    public EWeaponType WeaponType;
    public int AttackPower; // 데미지
    public bool isAttacking = false;

    // 무기를 소유할 오너
    protected GameObject Owner;

    // 다단히트 및 중복처리를 막을 리스트 변수
    public List<ITakeDamage> TakeDamageList;

    public void SapwnWeapon(GameObject owner)
    {
        Owner = owner;
        isAttacking = false;

        TakeDamageList = new List<ITakeDamage>();
        TakeDamageList.Clear();
    }

    public virtual void Attack()
    {
        Player_Ctrl player = Owner.GetComponent<Player_Ctrl>();

        if (player != null)
            player.Anim_Attack();
    }

    public void Attacking(bool isStart)
    {
        isAttacking = isStart;
        TakeDamageList.Clear();
    }


    private void OnTriggerEnter(Collider other)
    {
        // 무기를 가진 오너와 같을 시, 공격 중이 아닌 경우 리턴
        if (Owner.gameObject == other.gameObject || isAttacking == false) return;

        // ITakeDamage를 상속 받지 않았다면 리턴
        ITakeDamage takeDamage = other.GetComponentInParent<ITakeDamage>(); // 메쉬의 부모에서 받아오기
        if (takeDamage == null) return;

        // 중복 체크
        foreach(ITakeDamage damage in TakeDamageList)
        {
            if (damage == takeDamage) return;
        }

        takeDamage.TakeDamage(this.gameObject, this.gameObject, AttackPower, WeaponType);
        TakeDamageList.Add(takeDamage);
    }



}
