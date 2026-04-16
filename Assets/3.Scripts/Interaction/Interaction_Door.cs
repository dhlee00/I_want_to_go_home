using System.Collections;
using TMPro;
using UnityEngine;

public class Interaction_Door : Interaction
{
    [SerializeField] float CloseDelay = 3.0f;
    [SerializeField] bool isOpen;

    
    [SerializeField] float OpenedY = 0.001948395f; // 닫힐때 높이
    [SerializeField] float ClosedY = -1.3f; // 열릴때 높이

    MeshCollider mc;
    Coroutine moveCoroutine; // 이동 코루틴 관리용
    Coroutine autoCloseCoroutine; // 자동 닫힘 코루틴 관리용

    void Awake()
    {
        mc = GetComponent<MeshCollider>();
    }

    public override void OnInteraction()
    {
        // 중복 실행 방지 이전 자동 닫힘 예약이 있다면 취소
        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);

        // 현재 움직임 취소
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);


        if (!isOpen) // 열기
        {
            Interaction_Name = "문 닫기";
            isOpen = true;

            moveCoroutine = StartCoroutine(OpenDoorDown(false));
        }
        else // 닫기
        {
            Interaction_Name = "문 열기";
            isOpen = false;

            moveCoroutine = StartCoroutine(OpenDoorDown(true));
        }
    }
   

    // 문 여닫기
    IEnumerator OpenDoorDown(bool inisOpen)
    {
        
        float duration = 0.5f;
        Vector3 startPosition = transform.localPosition;
        Vector3 targetPosition = new Vector3(transform.localPosition.x, (inisOpen ? OpenedY : ClosedY), transform.localPosition.z);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;

        if (isOpen)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseDoor());
        }
    }

    // 자동으로 닫기
    IEnumerator AutoCloseDoor()
    {
        yield return new WaitForSeconds(CloseDelay);

        if (isOpen)
            OnInteraction();
    }
}
