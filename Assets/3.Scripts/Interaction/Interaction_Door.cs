using System.Collections;
using UnityEngine;

public class Interaction_Door : Interaction
{
    [SerializeField] bool isOpen;

    MeshCollider mc;

    void Awake()
    {
        mc = GetComponent<MeshCollider>();
    }

    public override void OnInteraction()
    {
        // 문 여닫기
        if (!isOpen)
        {
            //StartCoroutine(OpenDoor(Quaternion.Euler(0, -100.0f, 0)));
            StartCoroutine(OpenDoorDown(new Vector3(transform.localPosition.x, -1.3f, transform.localPosition.z)));
            Interaction_Name = "문 닫기";
            isOpen = true;
        }

        else
        {
            //StartCoroutine(OpenDoor(Quaternion.Euler(0, 0, 0)));
            StartCoroutine(OpenDoorDown(new Vector3(transform.localPosition.x, 0.001948395f, transform.localPosition.z)));
            Interaction_Name = "문 열기";
            isOpen = false;
        }
    }

    // 문 여닫기
    IEnumerator OpenDoor(Quaternion targetRotation)
    {
        float duration = 0.5f;
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;
        mc.enabled = false;

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mc.enabled = true;
        transform.rotation = targetRotation;
    }

    // 문 여닫기
    IEnumerator OpenDoorDown(Vector3 targetPosition)
    {
        float duration = 0.5f;
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;
    }
}
