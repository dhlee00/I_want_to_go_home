using UnityEngine;

public enum ObjectType
{
    Mountain,
    Plant,
    Tree,
    Rock
}

public class ObjectData : MonoBehaviour
{
    [SerializeField] ObjectType ObjectType;

    public ObjectType objectType { get => ObjectType; set => ObjectType = value; }
}
