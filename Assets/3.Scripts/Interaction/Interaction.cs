using UnityEngine;

public class Interaction : MonoBehaviour
{
    // 상호작용 오브젝트 이름
    public Item ItemData;

    // 상호작용시 업데이트 자식에서 재정의
    public virtual void OnInteraction()
    {

    }
}
