using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Mgr_Game : MonoBehaviour
{
    public bool bCanMove = true;

    public static Mgr_Game Inst;

    void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        // 테스트
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            Mgr_Data.Inst.SaveInven();
        }

        // 인벤토리 열기
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (Mgr_UI.Inst.OnInventory())
            {// 인벤토리가 열릴 때
                Mgr_UI.Inst.EquipSlot_On(false);
                bCanMove = false;
                Mgr_Camera.Inst.SetCameraLock(true);    // 카메라 잠금

                Cursor.lockState = CursorLockMode.Confined; 
            }
            else
            {// 인벤토리가 닫힐 때
                Mgr_UI.Inst.EquipSlot_On(true);
                bCanMove = true;
                Mgr_Camera.Inst.SetCameraLock(false);   // 카메라 잠금 풀기
                
                Cursor.lockState = CursorLockMode.Locked;
            }

            
        }
    }


    public Interaction_Item DropItem(Item ItemData)
    {
        Interaction_Item item = Mgr_Game.Inst.SpawnItme();
        Vector3 dropPos = Player_Ctrl.LocalPlayer.transform.position;
        dropPos.y += 1;


        item.SetInteractionItem(ItemData.Get_Item_Index, ItemData.Get_Item_Amount, dropPos, Player_Ctrl.LocalPlayer.transform.forward.normalized);

        return item;
    }


    #region 아이템 오브젝트 스폰
    Interaction_Item ItemPrefab;
    public Interaction_Item SpawnItme()
    {
        if(ItemPrefab == null)
        {
            ItemPrefab = Resources.Load<Interaction_Item>("Prefab/Interaction_Prefab");
        }
        
        return Instantiate(ItemPrefab.gameObject).GetComponent<Interaction_Item>();
    }
    #endregion


    #region 데미지 텍스트
    UIObj_DamageText DamageTextPrefab;
    public UIObj_DamageText SpawnDamageText(Vector3 SpawnPos, float Deamge)
    {
        if(DamageTextPrefab == null)
            DamageTextPrefab = Resources.Load<UIObj_DamageText>("Prefab/UIObj/DamageText_Prefab");


        UIObj_DamageText obj = Instantiate(DamageTextPrefab.gameObject).GetComponent<UIObj_DamageText>();
        obj.SpawnDamageText(SpawnPos, Deamge);

        return obj;
    }
    #endregion


    #region 히트 임팩트
    ParticleSystem HitParticleSystem;
    public ParticleSystem SpawnHitParticle(Vector3 SpawnPos, Vector3 Dir)
    {
        if(HitParticleSystem == null)
            HitParticleSystem = Resources.Load<ParticleSystem>("Prefab/Effects/FX_Shoot_01_muzzle");

        ParticleSystem obj = Instantiate(HitParticleSystem.gameObject).GetComponent<ParticleSystem>();
        obj.gameObject.transform.position = SpawnPos;
        obj.transform.LookAt(Dir);
        obj.Play();

        float duration = obj.main.duration + obj.main.startLifetime.constantMax;
        Destroy(obj.gameObject, duration);

        return obj;
    }

    #endregion
}
