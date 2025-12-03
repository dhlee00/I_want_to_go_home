using System.Collections.Generic;
using UnityEngine;

public class Mgr_Map : MonoBehaviour
{
    // 플레이어 위치
    [SerializeField] Transform playerT;
    Vector3 prePlayerPos;
    
    [Header("")]

    // 맵 오브젝트 프리팹들
    [SerializeField] GameObject mountainPrefab;  // 산
    [SerializeField] GameObject plantPrefabs;    // 식물
    [SerializeField] GameObject[] treePrefabs;   // 나무
    [SerializeField] GameObject[] rockPrefabs;   // 돌

    // 맵 터레인
    Terrain mapTerrain;

    // 생성 범위
    float spawnRange;

    [Header("")]

    // 생성될 오브젝트 개수
    [SerializeField] int totalObjectCount;

    // 노이즈 스케일
    [SerializeField] float noiseScale;

    // 노이즈 오프셋
    [SerializeField] Vector2 noiseOffset;

    // 오브젝트들 넣을 부모
    [SerializeField] Transform objectsParent;

    // 오브젝트 위치, 객체 저장 변수
    Dictionary<Vector3, GameObject> mapObjects = new Dictionary<Vector3, GameObject>();

    // 오브젝트 풀
    Queue<GameObject> mountainPool = new Queue<GameObject>();  // 산
    Queue<GameObject> plantPool = new Queue<GameObject>();     // 식물
    Queue<GameObject> treePool = new Queue<GameObject>();      // 나무
    Queue<GameObject> rockPool = new Queue<GameObject>();      // 돌

    // 새로운 위치, 노이즈 저장 변수
    Dictionary<Vector3, float> newObjectPos = new Dictionary<Vector3, float>();

    // 삭제할 거 있을 때 넣어두는 변수
    List<Vector3> trashList = new List<Vector3>();

    void Start()
    {
        // 현재 생성된 터레인 불러오기
        if (mapTerrain == null)
        {
            mapTerrain = Terrain.activeTerrain;
        }

        // 터레인 크기만큼 생성 범위 지정
        spawnRange = mapTerrain.terrainData.size.x / 2;

        // 오프셋 랜덤 설정
        //noiseOffset = new Vector2(Random.Range(0f, 100f), Random.Range(0f, 100f));

        // 지정한 개수만큼 오브젝트 생성
        for (int i = 0; i < totalObjectCount; i++)
        {
            GenerateObject();
        }
    }

    void Update()
    {
        // 터레인이 플레이어를 따라가게 설정
        mapTerrain.transform.position = new Vector3(playerT.position.x - spawnRange, transform.position.y, playerT.position.z - spawnRange);

        // 플레이어가 10만큼 이동할 때 마다 실행
        if (10.0f < Vector3.Distance(playerT.position, prePlayerPos))
        {
            // 오브젝트 위치 확인
            CheckObjectsPos();

            // 새로운 위치 계산
            GetNewPos();

            // 오브젝트 재배치
            RelocatObject();
        }

        foreach (var obj in mapObjects.Values)
        {

        }
    }

    // 초기 오브젝트 생성
    void GenerateObject()
    {
        // 생성 범위 중 랜덤 위치
        float posX = Random.Range(-spawnRange, spawnRange);
        float posZ = Random.Range(-spawnRange, spawnRange);

        // 노이즈 값 설정
        float noiseX = (posX * noiseScale) + noiseOffset.x;
        float noiseZ = (posZ * noiseScale) + noiseOffset.y;
        float noiseValue = Mathf.PerlinNoise(noiseX, noiseZ);

        GameObject obj;

        // 노이즈 값에 따라 생성 오브젝트 결정
        if (noiseValue < 0.45f) { obj = plantPrefabs; }
        else if (noiseValue < 0.60f) { obj = treePrefabs[Random.Range(0, treePrefabs.Length)]; }
        else if (noiseValue < 0.80f) { obj = rockPrefabs[Random.Range(0, rockPrefabs.Length)]; }
        else { obj = mountainPrefab; }

        // 오브젝트 생성
        GameObject geneObj = Instantiate(obj, objectsParent);

        // 생성 위치
        geneObj.transform.position = new Vector3(posX, 0.0f, posZ);

        // 랜덤 회전 부여
        geneObj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // 랜덤 크기 부여
        geneObj.transform.localScale *= Random.Range(0.9f, 1.1f);

        // 오브젝트 저장
        mapObjects.Add(geneObj.transform.position, geneObj);
    }

    // 오브젝트 위치 확인
    void CheckObjectsPos()
    {
        // 생성된 오브젝트들의 위치 체크
        foreach (var pos in mapObjects.Keys)
        {
            // 오브젝트가 터레인 범위를 벗어난 경우
            if (pos.x < (playerT.position.x - spawnRange) ||
                pos.x > (playerT.position.x + spawnRange) ||
                pos.z < (playerT.position.z - spawnRange) ||
                pos.z > (playerT.position.z + spawnRange))
            {
                // 비활성화
                mapObjects[pos].SetActive(false);

                // 오브젝트의 종류에 따라 풀에 저장
                switch (mapObjects[pos].GetComponent<ObjectData>().objectType)
                {
                    case ObjectType.Mountain: mountainPool.Enqueue(mapObjects[pos]); break;
                    case ObjectType.Plant: plantPool.Enqueue(mapObjects[pos]); break;
                    case ObjectType.Tree: treePool.Enqueue(mapObjects[pos]); break;
                    case ObjectType.Rock: rockPool.Enqueue(mapObjects[pos]); break;
                }

                // 제거 할 오브젝트 모으기
                trashList.Add(pos);
            }
        }

        // 제거
        foreach (var trash in trashList)
        {
            mapObjects.Remove(trash);
        }
        trashList.Clear();
    }

    // 새로운 위치 계산
    void GetNewPos()
    {
        // 이동 후 범위
        float nowX = playerT.position.x - spawnRange;
        float nowZ = playerT.position.z - spawnRange;
        Rect nowRange = new Rect(nowX, nowZ, spawnRange * 2, spawnRange * 2);

        // 이동 전 범위
        float preX = prePlayerPos.x - spawnRange;
        float preZ = prePlayerPos.z - spawnRange;
        Rect preRange = new Rect(preX, preZ, spawnRange * 2, spawnRange * 2);

        // 겹치는 영역 계산
        float overlapXMin = Mathf.Max(nowRange.xMin, preRange.xMin);
        float overlapXMax = Mathf.Min(nowRange.xMax, preRange.xMax);
        float overlapYMin = Mathf.Max(nowRange.yMin, preRange.yMin);
        float overlapYMax = Mathf.Min(nowRange.yMax, preRange.yMax);

        // 겹치지 않는 영역 계산
        List<Rect> diffRange = new List<Rect>();

        // 상단 영역
        if (nowRange.yMax > overlapYMax)
        {
            diffRange.Add(new Rect(nowRange.xMin, overlapYMax, nowRange.width, nowRange.yMax - overlapYMax));
        }

        // 하단 영역
        if (nowRange.yMin < overlapYMin)
        {
            diffRange.Add(new Rect(nowRange.xMin, nowRange.yMin, nowRange.width, overlapYMin - nowRange.yMin));
        }

        // 좌측 영역
        if (nowRange.xMin < overlapXMin)
        {
            diffRange.Add(new Rect(nowRange.xMin, overlapYMin, overlapXMin - nowRange.xMin, overlapYMax - overlapYMin));
        }

        // 우측 영역
        if (nowRange.xMax > overlapXMax)
        {
            diffRange.Add(new Rect(overlapXMax, overlapYMin, nowRange.xMax - overlapXMax, overlapYMax - overlapYMin));
        }

        // 비활성화된 오브젝트 개수만큼 반복
        for (int i = 0; i < 300 - mapObjects.Count; i++)
        {
            // 영역 중 하나를 무작위로 선택
            Rect range = diffRange[Random.Range(0, diffRange.Count)];

            // 선택된 영역 내에서 랜덤한 위치를 생성
            float randomX = Random.Range(range.xMin, range.xMax);
            float randomZ = Random.Range(range.yMin, range.yMax);

            // 노이즈 계산
            float noiseX = (randomX * noiseScale) + noiseOffset.x;
            float noiseZ = (randomZ * noiseScale) + noiseOffset.y;
            float noiseValue = Mathf.PerlinNoise(noiseX, noiseZ);

            Vector3 newPos = new Vector3(randomX, 0.0f, randomZ);

            // 새로운 위치 저장
            if (!newObjectPos.ContainsKey(newPos))
            {
                newObjectPos.Add(newPos, noiseValue);
            }
        }

        // 이전 위치 갱신
        prePlayerPos = playerT.position;
    }

    // 오브젝트 재배치
    void RelocatObject()
    {
        // 새로 생성한 위치 개수만큼 반복
        foreach (var pos in newObjectPos)
        {
            GameObject obj = null;

            // 노이즈 값에 따라 해당하는 오브젝트 풀에서 꺼내기 없으면 생성
            if (pos.Value < 0.45f)
            {
                if (0 < plantPool.Count) { obj = plantPool.Dequeue(); }
                else { obj = Instantiate(plantPrefabs, objectsParent); }
            }

            else if (pos.Value < 0.60f)
            {
                if (0 < treePool.Count) { obj = treePool.Dequeue(); }
                else { obj = Instantiate(treePrefabs[Random.Range(0, treePrefabs.Length)], objectsParent); }
            }

            else if (pos.Value < 0.80f)
            {
                if (0 < rockPool.Count) { obj = rockPool.Dequeue(); }
                else { obj = Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Length)], objectsParent); }
            }

            else
            {
                if (0 < mountainPool.Count) { obj = mountainPool.Dequeue(); }
                else { obj = Instantiate(mountainPrefab, objectsParent); }
            }

            // 오브젝트 활성화 + 재배치
            obj.SetActive(true);
            obj.transform.position = pos.Key;

            // 현재 생성되어 있는 딕셔너리에 추가
            mapObjects.Add(obj.transform.position, obj);

            // 제거할 위치 모으기
            trashList.Add(obj.transform.position);
        }

        // 제거
        foreach (var trash in trashList)
        {
            newObjectPos.Remove(trash);
        }
        trashList.Clear();
    }
}
