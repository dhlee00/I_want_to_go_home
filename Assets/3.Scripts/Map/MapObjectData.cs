using UnityEngine;

[CreateAssetMenu(fileName = "New Object", menuName = "MapObject")]
public class MapObjectData : ScriptableObject
{
    // 오브젝트 종류
    public EHarvestObjectType objectType;

    // 오브젝트 프리팹
    public GameObject[] prefab;
}
