using UnityEngine;

public class Mgr_Ground : MonoBehaviour
{
    public Transform playerT;

    float offset;

    void Start()
    {
        offset = transform.position.x;
    }

    void Update()
    {
        transform.position = new Vector3(playerT.position.x + offset, transform.position.y, playerT.position.z + offset);
    }
}
