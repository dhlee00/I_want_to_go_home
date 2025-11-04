using Unity.VisualScripting;
using UnityEngine;


public class HarvestObject : MonoBehaviour, ITakeDamage
{
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
        else if(Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(Player_Ctrl.LocalInst.gameObject, 10f);
        }
    }

    public void TakeDamage(GameObject DamageOwner, float Damage)
    {
        if (bIsSapwn == false) return;

        Hp -= Damage;

        // 체력 비율에 따라 Count갱신
        {
            float newCount = Mathf.Ceil((Hp / MaxHp) * MaxCount);
            newCount = Mathf.Max(0, newCount);
            

            // count가 실제로 감소할 때만 처리
            int dropped = (MaxCount - Count) - (int)newCount;
            Count += dropped;

            DropItems(DamageOwner, dropped);
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
        Interaction_Item ItemPrefab = Resources.Load<Interaction_Item>("Prefab/Interaction_Prefab");

        Interaction_Item item = Instantiate(ItemPrefab.gameObject).GetComponent<Interaction_Item>();
        

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
