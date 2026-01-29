using UnityEngine;

// 오브젝트 종류
public enum EHarvestObjectType
{
    Plant,    // 풀
    Tree,     // 나무
    Rock,     // 돌
    Mountain  // 산
}

[CreateAssetMenu(fileName = "New Object", menuName = "MapObject")]
public class MapObjectData : ScriptableObject
{
    // 오브젝트 종류
    public EHarvestObjectType objectType;

    // 오브젝트 프리팹
    public GameObject[] prefab;
}
