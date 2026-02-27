using System.Collections;
using UnityEngine;

public class Interaction_Door : Interaction
{
    [SerializeField] bool isOpen;

    void Awake()
    {
        InteractionType = EInteractionType.door;
    }

    void Start()
    {

    }

    public override void OnInteraction()
    {
        // 문 여닫기
        if (!isOpen)
        {
            StartCoroutine(OpenDoor(Quaternion.Euler(0, -100.0f, 0)));
            Interaction_Name = "문 닫기";
            isOpen = true;
        }

        else
        {
            StartCoroutine(OpenDoor(Quaternion.Euler(0, 0, 0)));
            Interaction_Name = "문 열기";
            isOpen = false;
        }
    }

    IEnumerator OpenDoor(Quaternion targetRotation)
    {
        float duration = 0.5f;
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // elapsedTime / duration 비율에 따라 0에서 1까지 증가
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
