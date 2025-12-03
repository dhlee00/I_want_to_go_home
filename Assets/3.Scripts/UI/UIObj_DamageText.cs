using TMPro;
using UnityEngine;

public class UIObj_DamageText : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI m_Text;
    [SerializeField] float LifeTime;

    public void SpawnDamageText(Vector3 SpawnPos, float Deamge)
    {
        transform.position = SpawnPos;
        m_Text.text = Deamge.ToString();

        Destroy(this.gameObject, LifeTime);
    }

    void Update()
    {
        transform.forward = (Camera.main.transform.forward);
    }
}
