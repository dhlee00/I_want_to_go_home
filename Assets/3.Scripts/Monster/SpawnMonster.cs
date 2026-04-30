using System.Collections.Generic;
using UnityEngine;

public class SpawnMonster : MonoBehaviour
{
    // 몬스터 프리팹
    [SerializeField] List<GameObject> monsterPrefab;

    [SerializeField] Player_Ctrl player;

    [SerializeField] float respawnTime;
    [SerializeField] float respawnTimer;

    [SerializeField] float spawnRange;

    public List<Monster> spawnedMon;

    void Start()
    {

    }

    void Update()
    {
        // 기지 안이 아닌 경우
        if (player.nowChunk != new Vector2Int(0, 0))
        {
            if (spawnedMon.Count < 1)
            {
                respawnTimer -= Time.deltaTime;

                if (respawnTimer < 0)
                {
                    Spawn();
                    respawnTimer = respawnTime;
                }
            }
        }

        else
        {
            respawnTimer = 0;
        }
    }

    // 몬스터 생성
    public void Spawn()
    {
        Vector2 randomCirclePos = Random.onUnitSphere * spawnRange;

        Vector3 spawnOffset = new Vector3(randomCirclePos.x, 0, randomCirclePos.y);
        Vector3 spawnPosition = player.transform.position + spawnOffset;

        GameObject monsterObj = Instantiate(monsterPrefab[Random.Range(0, monsterPrefab.Count)], spawnPosition, Quaternion.identity);
        Monster mon = monsterObj.GetComponent<Monster>();

        mon.spawnMonster = this;
        mon.target = player;

        spawnedMon.Add(mon);
    }
}
