using UnityEngine;

public class UI_Active : MonoBehaviour
{
    // 애니메이션 이벤트 함수
    #region UI_Active/Deactive
    public void Active_UI()
    {
        this.gameObject.SetActive(true);
    }

    public void Deactive_UI()
    {
        this.gameObject.SetActive(false);
    }
    #endregion
}
