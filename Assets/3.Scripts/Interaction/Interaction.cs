using UnityEngine;

// ��ȣ�ۿ� Ÿ��
public enum EInteractionType
{
    Itme,
}

public class Interaction : MonoBehaviour
{
    public EInteractionType InteractionType;

    // ��ȣ�ۿ�� ������Ʈ �ڽĿ��� ������
    public virtual void OnInteraction()
    {

    }
}
