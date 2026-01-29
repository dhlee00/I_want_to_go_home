using UnityEngine;

public class Mgr_Sky : MonoBehaviour
{
    // ÇÏ·ç ½Ã°£
    [Range(0, 1)] [SerializeField] float dayTime;

    // ÇÏ·çÀÇ ±æÀÌ (ÃÊ)
    [SerializeField] float dayDuration;

    // ½ºÄ«ÀÌ¹Ú½º ¸ÞÅ×¸®¾ó
    [SerializeField] Material skyboxMat;
    
    // ºû (ÅÂ¾ç ¿ªÇÒ)
    [SerializeField] Light directionalLight;

    // ¹ãÇÏ´Ã È¸Àü ¼Óµµ
    [SerializeField] float nightRotationSpeed;

    // ¹ãÇÏ´Ã È¸Àü°ª
    float nightRot = 0f;

    void Update()
    {
        // ½Ã°£ Èå¸§ °è»ê
        dayTime += Time.deltaTime / dayDuration;
        if (dayTime > 1)
        {
            dayTime = 0;
        }

        SetSky();
    }

    // ÇÏ´Ã ¼³Á¤
    void SetSky()
    {
        // ÅÂ¾çÀÇ °¢µµ
        float sunAngle = dayTime * 360f;
        directionalLight.transform.localRotation = Quaternion.Euler(sunAngle, 0f, 0f);

        // ³·¹ã ÆÇÁ¤
        float rad = sunAngle * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);

        // ³·
        if (sin > 0)
        {
            // ¹ãÇÏ´Ã ¼¯´Â ºñÀ² 0
            skyboxMat.SetFloat("_Blend", 0);

            // ³·ÇÏ´Ã Exposure Á¶Àý (0.5 ~ 1)
            float exp = Mathf.Lerp(0.5f, 1.0f, sin);
            skyboxMat.SetFloat("_Exposure", exp);

            // ÅÂ¾çºû ¼¼±â Á¶Àý (0 ~ 1)
            directionalLight.intensity = sin;
            
            // ÁÖº¯±¤ Á¶Àý (0.6 ~ 1)
            RenderSettings.ambientIntensity = 0.6f + (0.4f * sin);
        }
        
        // ¹ã
        else
        {
            // ³·ÇÏ´Ã Exposure (0.5 °íÁ¤)
            skyboxMat.SetFloat("_Exposure", 0.5f);

            // ¹ãÇÏ´Ã ¼¯´Â ºñÀ² Á¶Àý (0 ~ 0.9)
            float blendAmount = Mathf.Lerp(0f, 0.9f, -sin);
            skyboxMat.SetFloat("_Blend", blendAmount);

            // ÅÂ¾çºû ¼¼±â Á¶Àý (0 °íÁ¤)
            directionalLight.intensity = 0;

            // ÁÖº¯±¤ Á¶Àý (0.6 °íÁ¤)
            RenderSettings.ambientIntensity = 0.6f;

            // ¹ãÇÏ´Ã¸¸ ¼¼·Î È¸Àü
            nightRot += Time.deltaTime * nightRotationSpeed;
            if (nightRot > 360f)
            {
                nightRot -= 360f;
            }
            skyboxMat.SetFloat("_Rotation", -nightRot);
        }
    }
}
