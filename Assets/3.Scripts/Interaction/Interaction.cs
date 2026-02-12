using UnityEngine;
using UnityEngine.UI;

// 상호작용 타입
public enum EInteractionType
{
    item,
    craftingStation
}

public class Interaction : MonoBehaviour
{
    [Header("타입")]
    public EInteractionType InteractionType;

    [Header("아이템이 아닌경우 설정")]
    public string Interaction_Name;
    public Sprite Interaction_Icon;

    

    // 상호작용시 업데이트 자식에서 재정의
    public virtual void OnInteraction()
    {

    }
}
