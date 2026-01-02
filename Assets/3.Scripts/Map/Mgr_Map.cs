using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectData
{
    public float hp;
    public bool isDestroy;
}

public class Mgr_Map : MonoBehaviour
{
    // 플레이어 트랜스폼
    [SerializeField] Transform playerT;

    // 터레인
    [SerializeField] Terrain mapTerrain;

    // 청크 부모
    [SerializeField] Transform chunkParent;

    // 오브젝트 프리팹
    [SerializeField] GameObject objectPrefab;

    // 오브젝트 데이터
    [SerializeField] MapObjectData[] mapObjectData;

    // 그리드 크기
    [SerializeField] float gridSize;

    // 청크 크기
    [SerializeField] int chunkSize;

    // 플레이어 시야
    [SerializeField] int viewDistance;

    // 직전에 위치했던 청크
    Vector2Int preChunkPos;

    // 맵 데이터
    Dictionary<string, ObjectData> mapData = new Dictionary<string, ObjectData>();

    // 활성화 된 청크 목록
    Dictionary<Vector2Int, GameObject> activeChunkList = new Dictionary<Vector2Int, GameObject>();

    // 노이즈
    [SerializeField] float noiseScale;

    void Start()
    {
        preChunkPos = GridToChunk(WorldToGrid(playerT.position));
    }

    void Update()
    {
        // 터레인이 플레이어를 따라가게 설정
        mapTerrain.transform.position = new Vector3(playerT.position.x - 50, transform.position.y, playerT.position.z - 50);

        // 현재 플레이어의 청크 좌표 계산
        Vector2Int newChunkPos = GridToChunk(WorldToGrid(playerT.position));

        // 청크가 바뀌었을 때만 업데이트 실행
        if (newChunkPos != preChunkPos)
        {
            preChunkPos = newChunkPos;
            UpdateChunk();
        }
    }

    // 청크 최신화
    void UpdateChunk()
    {
        // 내 주변 청크 좌표 리스트
        List<Vector2Int> neighborChunk = new List<Vector2Int>();

        // 시야 범위만큼 탐색
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Vector2Int chunkCoord = new Vector2Int(preChunkPos.x + x, preChunkPos.y + y);
                neighborChunk.Add(chunkCoord);
            }
        }

        // 멀어진 청크 삭제
        List<Vector2Int> removeChunkList = new List<Vector2Int>();
        foreach (var activeChunk in activeChunkList.Keys)
        {
            // 켜져있는 청크가 내 주변 리스트에 없는 경우
            if (!neighborChunk.Contains(activeChunk))
            {
                // 삭제 목록에 추가
                removeChunkList.Add(activeChunk);
            }
        }

        // 실제 게임 오브젝트 삭제
        foreach (var pos in removeChunkList)
        {
            // 삭제될 청크 오브젝트
            GameObject chunkToDestroy = activeChunkList[pos];

            // 청크 안에 있는 모든 MapObject 데이터 최신화
            MapObject[] objectsInChunk = chunkToDestroy.GetComponentsInChildren<MapObject>();
            foreach (var obj in objectsInChunk)
            {
                // 딕셔너리에 해당 시드가 있다면 현재 HP와 파괴 상태를 기록
                if (mapData.ContainsKey(obj.uniqueKey))
                {
                    mapData[obj.uniqueKey].hp = obj.hp;

                    // 만약 체력이 0 이하라면 파괴된 것으로 간주
                    if (obj.hp <= 0)
                    {
                        mapData[obj.uniqueKey].isDestroy = true;
                    }
                }
            }

            Destroy(chunkToDestroy);
            activeChunkList.Remove(pos);
        }

        // 새로 들어온 청크 생성
        foreach (var pos in neighborChunk)
        {
            // 내 주변 리스트에 있는데 비활성 상태인 경우
            if (!activeChunkList.ContainsKey(pos))
            {
                // 빈 게임 오브젝트를 하나 만들어서 해당 청크의 부모로 지정
                GameObject chunkObj = new GameObject($"Chunk ({pos.x}, {pos.y})");
                
                // 아이콘 지정
                Texture2D icon = UnityEditor.EditorGUIUtility.IconContent("sv_label_3").image as Texture2D;
                UnityEditor.EditorGUIUtility.SetIconForObject(chunkObj, icon);

                // 부모 지정
                chunkObj.transform.SetParent(chunkParent);

                // 청크의 실제 월드 위치 계산
                float worldX = pos.x * chunkSize * gridSize;
                float worldZ = pos.y * chunkSize * gridSize;
                chunkObj.transform.position = new Vector3(worldX, 0, worldZ);

                // 활성화 된 청크 목록에 추가
                activeChunkList.Add(pos, chunkObj);

                // 청크에 오브젝트 심기
                GenerateObjectInChunk(chunkObj);
            }
        }
    }

    // 청크에 오브젝트 심기
    void GenerateObjectInChunk(GameObject parent)
    {
        // 청크 좌표를 기반으로 노이즈 값 추출
        float noiseX = parent.transform.position.x;
        float noiseZ = parent.transform.position.z;
        float noise = Mathf.PerlinNoise(noiseX * noiseScale, noiseZ * noiseScale);

        // 산인 경우
        if (0.7f <= noise)
        {
            // 랜덤 시드 고정
            string uniqueKey = $"{parent.name}_Mountain";
            int seed = uniqueKey.GetHashCode();
            Random.InitState(seed);

            // 생성된 적 있는 오브젝트인 경우
            if (mapData.ContainsKey(uniqueKey))
            {
                // 파괴된 오브젝트인지 확인
                if (mapData[uniqueKey].isDestroy)
                {
                    return;
                }
            }

            MapObjectData mountainData = mapObjectData[3];

            // 산 생성 및 기본 설정
            GameObject mountainObj = Instantiate(objectPrefab);
            mountainObj.name = mountainData.name;
            mountainObj.transform.SetParent(parent.transform);

            // 위치를 중앙으로 고정
            mountainObj.transform.localPosition = Vector3.zero;

            // 외형 적용
            GameObject mesh = Instantiate(mountainData.prefab[Random.Range(0, mountainData.prefab.Length)]);
            mesh.name = "Mesh";
            mesh.transform.SetParent(mountainObj.transform);
            mesh.transform.localPosition = Vector3.zero;
            mesh.transform.localRotation = Quaternion.identity;

            // 무작위 크기 부여 (Y축만 변경)
            float randomY = Random.Range(1.0f, 1.5f);
            mountainObj.transform.localScale = new Vector3(1f, randomY, 1f);

            // 정보 전달
            MapObject objInfo = mountainObj.GetComponent<MapObject>();
            objInfo.mesh = mesh.transform;

            // 오브젝트에 시드 전달
            objInfo.uniqueKey = uniqueKey;

            // 생성된 적 있는 오브젝트인 경우
            if (mapData.ContainsKey(uniqueKey))
            {
                // 파괴된 오브젝트가 아니면
                if (!mapData[uniqueKey].isDestroy)
                {
                    // 저장된 체력 복사
                    objInfo.hp = mapData[uniqueKey].hp;
                }
            }

            // 아닌 경우
            else
            {
                // 새로 추가
                mapData.Add
                (
                    uniqueKey,
                    new ObjectData
                    {
                        hp = objInfo.hp,
                        isDestroy = false
                    }
                );
            }

            // 함수 종료
            return;
        }

        // 산이 아닐 경우
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                // 랜덤 시드 고정
                string uniqueKey = $"{parent.name}_Grid ({x}, {z})";
                int seed = uniqueKey.GetHashCode();
                Random.InitState(seed);

                // 생성된 적 있는 오브젝트인 경우
                if (mapData.ContainsKey(uniqueKey))
                {
                    // 파괴된 오브젝트인지 확인
                    if (mapData[uniqueKey].isDestroy)
                    {
                        continue;
                    }
                }

                // 청크 좌표를 기반으로 노이즈 값 추출
                noiseX = parent.transform.position.x + (x * gridSize);
                noiseZ = parent.transform.position.z + (z * gridSize);
                noise = Mathf.PerlinNoise(noiseX * noiseScale, noiseZ * noiseScale);

                // 노이즈 값에 따라 생성할 데이터 설정
                MapObjectData objData = null;

                // 돌
                if (0.55f <= noise)
                {
                    objData = mapObjectData[2];
                }

                // 나무
                else if (0.4f <= noise)
                {
                    objData = mapObjectData[1];
                }

                // 풀
                else
                {
                    objData = mapObjectData[0];
                }

                // 오브젝트 생성
                GameObject obj = Instantiate(objectPrefab);
                obj.name = objData.name;

                // 청크를 부모로 설정
                obj.transform.SetParent(parent.transform);

                // 오브젝트 정보 불러오기
                MapObject objInfo = obj.GetComponent<MapObject>();

                // 오브젝트에 시드 전달
                objInfo.uniqueKey = uniqueKey;

                // 외형 생성
                GameObject mesh = Instantiate(objData.prefab[Random.Range(0, objData.prefab.Length)]);
                mesh.name = "Mesh";
                mesh.transform.SetParent(obj.transform);
                mesh.transform.localPosition = Vector3.zero;
                mesh.transform.localRotation = Quaternion.identity;
                objInfo.mesh = mesh.transform;

                // 그리드의 중심 구하기
                float totalSize = chunkSize * gridSize;
                float posX = (x * gridSize) - totalSize / 2f + (gridSize / 2f);
                float posZ = (z * gridSize) - totalSize / 2f + (gridSize / 2f);

                // 그리드의 중심을 기준으로 무작위 위치에 배치
                float offsetRange = gridSize * 0.3f;
                float offsetX = Random.Range(-offsetRange, offsetRange);
                float offsetZ = Random.Range(-offsetRange, offsetRange);
                obj.transform.localPosition = new Vector3(posX + offsetX, 0, posZ + offsetZ);

                // 랜덤 회전값 적용
                obj.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);

                // 랜덤 크기값 적용
                float randomScale = Random.Range(0.7f, 1.3f);
                obj.transform.localScale *= randomScale;

                // 생성된 적 있는 오브젝트인 경우
                if (mapData.ContainsKey(uniqueKey))
                {
                    // 파괴된 오브젝트가 아니면
                    if (!mapData[uniqueKey].isDestroy)
                    {
                        // 저장된 체력 복사
                        objInfo.hp = mapData[uniqueKey].hp;
                    }
                }

                // 아닌 경우
                else
                {
                    // 새로 추가
                    mapData.Add
                    (
                        uniqueKey,
                        new ObjectData
                        {
                            hp = objInfo.hp,
                            isDestroy = false
                        }
                    );
                }
            }
        }

        // 시드 고정 풀기
        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    // 월드 좌표를 그리드 좌표로 변환
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / gridSize);
        int z = Mathf.FloorToInt(worldPos.z / gridSize);
        return new Vector2Int(x, z);
    }

    // 그리드 좌표를 해당 그리드가 속한 청크 좌표로 변환
    public Vector2Int GridToChunk(Vector2Int gridPos)
    {
        int x = Mathf.FloorToInt((float)gridPos.x / chunkSize);
        int y = Mathf.FloorToInt((float)gridPos.y / chunkSize);
        return new Vector2Int(x, y);
    }
}
