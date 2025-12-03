using Unity.VisualScripting;
using UnityEngine;

public enum EHarvestObjectType
{
    Stone


}


public class HarvestObject : MonoBehaviour, ITakeDamage
{
    public EHarvestObjectType HarvestObjectType;

    public float MaxHp = 100f;   // 최대 체력
    public float Hp = 100f;      // 현재 체력

    public int Harvest_Item_Index;  // 스폰될 아이템 인덱스
    public int MaxCount = 20;    // 채집 가능한 최대 개수
    public int Count = 0;           // 떨어뜨린 갯수


    public bool bIsSapwn = true;

    public float ReSpawnTime = 5.0f;
    public float ReSpawn = 0;

    MeshRenderer m_MeshRenderer;
    Collider m_Collider;

    private void Awake()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();
        m_Collider = GetComponent<Collider>();
    }

    void Start()
    {
        
    }


    void Update()
    {
        if(bIsSapwn == false)
        {
            ReSpawn += Time.deltaTime;

            if(ReSpawn > ReSpawnTime)
            {
                SetSpawn(true);
            }

        }
    }

    public void TakeDamage(GameObject DamageOwner, GameObject DamageObj, float Damage, EWeaponType DamageType)
    {
        if (bIsSapwn == false) return;

        // 곡괭이 도끼등 특정 무기가 들어왔을 경우 추가 데미지
        switch (HarvestObjectType)
        {
            case EHarvestObjectType.Stone:
                {
                    if (DamageType == EWeaponType.pickax)
                        Damage *= 2;
                }
                break;


        }

        Hp -= Damage;


        // 데미지 연출
        {
            Vector3 start = DamageObj.transform.position;
            Vector3 dir = (this.transform.position - start).normalized;

            Vector3 hitPos = start;
            Vector3 hitDir = dir;
            if (Physics.Raycast(start, dir, out RaycastHit hitRay, 3f))
            {
                hitPos = hitRay.point + hitRay.normal * 0.3f;
                //hitDir = start - hitPos;
            }

            Mgr_Game.Inst.SpawnDamageText(hitPos, Damage);      // 데미지 텍스트
            Mgr_Game.Inst.SpawnHitParticle(hitPos, -hitDir);    // 임팩트
        }


        // 체력 비율에 따라 Count갱신
        {
            float newCount = Mathf.Ceil((Hp / MaxHp) * MaxCount);
            newCount = Mathf.Max(0, newCount);
            

            // count가 실제로 감소할 때만 처리
            int dropped = (MaxCount - Count) - (int)newCount;

            if(dropped > 0)
            {
                Count += dropped;

                DropItems(DamageOwner, dropped);
            }
            
        }

        if(Hp <= 0f)
        {
            SetSpawn(false);
        }
        else
        {

        }


    }

    void DropItems(GameObject DamageOwner, int num)
    {
        // 스폰될 위치
        Vector3 pos = Vector3.zero;
        {
            // MeshRenderer의 경계 정보 가져오기
            Bounds bounds = m_MeshRenderer.bounds;

            // 방향 벡터 계산 (스폰 오브젝트 → 플레이어)
            Vector3 dir = (DamageOwner.gameObject.transform.position - bounds.center).normalized;

            // 방향에 따라 메쉬 끝점 계산
            // Bounds의 extents는 각 축 방향의 절반 크기
            Vector3 halfSize = bounds.extents;
            pos = bounds.center + Vector3.Scale(dir, halfSize);

            pos += dir * 0.1f;
        }

        // 날라갈 방향
        Vector3 forceDir = (DamageOwner.gameObject.transform.position - this.gameObject.transform.position).normalized;

        Interaction_Item item = Mgr_Game.Inst.SpawnItme();

        item.SetInteractionItem(Harvest_Item_Index, num, pos, forceDir);
    }


    void SetSpawn(bool isSpawn)
    {
        bIsSapwn = isSpawn;

        m_MeshRenderer.enabled = bIsSapwn;
        m_Collider.enabled = bIsSapwn;

        if(bIsSapwn)
        {
            Hp = MaxHp;
            Count = 0;
        }
    }
}
