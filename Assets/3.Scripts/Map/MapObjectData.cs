using UnityEngine;

// 오브젝트 종류
public enum ObjectType
{
    Mountain,  // 산
    Plant,     // 식물
    Tree,      // 나무
    Rock       // 돌
}

[CreateAssetMenu(fileName = "New Object", menuName = "MapObject")]
public class MapObjectData : ScriptableObject
{
    // 오브젝트 종류
    public ObjectType objectType;

    // 오브젝트 프리팹
    public GameObject[] prefab;
}
