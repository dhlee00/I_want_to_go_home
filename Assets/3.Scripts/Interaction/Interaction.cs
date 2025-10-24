using UnityEngine;

// 상호작용 타입
public enum EInteractionType
{
    Itme,
}

public class Interaction : MonoBehaviour
{
    public EInteractionType InteractionType;

    // 상호작용시 업데이트 자식에서 재정의
    public virtual void OnInteraction()
    {

    }
}
