using System.Collections;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public enum State
{
    Idle,
    Move,
    Attack,
    TakeDamage,
    Die
}

public class Monster : MonoBehaviour, ITakeDamage
{
    // 몬스터 생성기
    [SerializeField] public SpawnMonster spawnMonster;

    // 애니메이션
    Animation anim;

    // 스킨 메쉬 랜더러
    [SerializeField] SkinnedMeshRenderer m_SkinnedMeshRenderer;

    Rigidbody rig;

    // 추격 대상
    public Player_Ctrl target;

    // 상태
    [SerializeField] State state;

    // 타겟과의 거리
    [SerializeField] float disOfTarget;

    // 애니메이션 종료
    public bool animEnd;

    // 피격 당함
    public bool takeDamage;

    public Vector3 rayOrigin;
    public float rayDis;
    bool isAvoiding = false;

    [Header("")]

    // 체력
    public float hp;

    // 공격력
    public float attackPower;

    // 속도
    public float speed;

    public int Harvest_Item_Index;  // 스폰될 아이템 인덱스

    // 아이템 개수
    public int Harvest_Item_Amount;

    void Start()
    {
        anim = GetComponent<Animation>();
        rig = GetComponent<Rigidbody>();
    }

    void Update()
    {
        TestKill();

        disOfTarget = Vector3.Distance(target.transformHandle.position, transform.position);

        if (!takeDamage)
        {
            switch (state)
            {
                case State.Idle: Idle(); break;
                case State.Move: Move(); break;
                case State.Attack: Attack(); break;
                case State.Die: Die(); break;
            }

            PlayAnim(state);
        }

        else
        {
            PlayAnim(State.TakeDamage);

            if (animEnd)
            {
                takeDamage = false;
                animEnd = false;
            }
        }
    }

    void Idle()
    {
        if (disOfTarget <= 30.0f)
        {
            state = State.Move;
        }
    }

    void Move()
    {
        if (disOfTarget > 30.0f)
        {
            state = State.Idle;
        }

        if (disOfTarget <= 1.3f)
        {
            state = State.Attack;
        }

        if (!isAvoiding)
        {
            DetectObstacle();
            NormalMove();
        }
    }

    void DetectObstacle()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + rayOrigin, transform.forward, out hit, rayDis))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                StartCoroutine(AvoidRoutine());
            }
        }
        Debug.DrawRay(transform.position + rayOrigin, transform.forward * rayDis, Color.green);
    }

    void NormalMove()
    {
        Vector3 targetPosFlat = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
        Vector3 direction = (targetPosFlat - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            rig.MoveRotation(Quaternion.Slerp(rig.rotation, targetRot, Time.deltaTime * 5f));

            Vector3 moveVelocity = transform.forward * speed;
            moveVelocity.y = rig.linearVelocity.y;
            rig.linearVelocity = moveVelocity;
        }
    }

    IEnumerator AvoidRoutine()
    {
        isAvoiding = true;

        Vector3 targetPosFlat = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
        Vector3 relativePoint = transform.InverseTransformPoint(targetPosFlat);

        float turnAngle = relativePoint.x > 0 ? 60f : -60f;

        Quaternion startRotation = rig.rotation;
        Quaternion targetRotation = rig.rotation * Quaternion.Euler(0, turnAngle, 0);

        float rotateDuration = 0.4f;
        float timer = 0f;
        while (timer < rotateDuration)
        {
            if (IsPlayerFarAngle()) break;

            timer += Time.deltaTime;
            float t = timer / rotateDuration;

            rig.MoveRotation(Quaternion.Slerp(startRotation, targetRotation, t));
            rig.linearVelocity = transform.forward * speed + Vector3.up * rig.linearVelocity.y;

            yield return null;
        }

        float moveTimer = 0f;
        while (moveTimer < 0.7f)
        {
            if (IsPlayerFarAngle()) break;

            moveTimer += Time.deltaTime;
            rig.linearVelocity = transform.forward * speed + Vector3.up * rig.linearVelocity.y;

            yield return null;
        }

        // 3단계: 종료 및 초기화
        rig.linearVelocity = Vector3.up * rig.linearVelocity.y;

        isAvoiding = false;
    }

    bool IsPlayerFarAngle()
    {
        Vector3 targetDir = (target.transform.position - transform.position);
        targetDir.y = 0;

        Vector3 monsterForward = transform.forward;
        monsterForward.y = 0;

        float angle = Vector3.Angle(monsterForward, targetDir.normalized);

        if (angle > 90f)
        {
            return true;
        }

        return false;
    }

    void Attack()
    {
        if (disOfTarget > 1.3f)
        {
            state = State.Move;
        }

        if (animEnd)
        {
            transform.LookAt(target.transform.position);

            target.TakeDamage(attackPower);

            animEnd = false;
        }
    }

    public void TakeDamage(GameObject DamageOwner, GameObject DamageObj, float Damage, EWeaponType DamageType)
    {
        hp -= Damage;

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

        if (hp <= 0)
        {
            state = State.Die;
        }

        else
        {
            takeDamage = true;
        }
    }

    void Die()
    {
        if (animEnd)
        {
            DropItems(target.gameObject, Harvest_Item_Amount);

            spawnMonster.spawnedMon.Remove(this);
            Destroy(gameObject);
        }
    }

    public void PlayAnim(State state)
    {
        string animName = "";

        switch (state)
        {
            case State.Idle: animName = "Idle"; break;
            case State.Move: animName = "Walk"; break;
            case State.Attack: animName = "Attack"; break;
            case State.TakeDamage: animName = "TakeDamage"; break;
            case State.Die: animName = "Die"; break;
        }

        anim.Play(animName);
    }

    void DropItems(GameObject DamageOwner, int num)
    {
        // 스폰될 위치
        Vector3 pos = Vector3.zero;
        {
            // MeshRenderer의 경계 정보 가져오기
            Bounds bounds = m_SkinnedMeshRenderer.bounds;

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

    public void AnimEnd()
    {
        animEnd = true;
    }

    void TestKill()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            state = State.Die;
        }
    }
}
