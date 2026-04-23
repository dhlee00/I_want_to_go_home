using UnityEngine;

public class Mgr_BaseCamp : MonoBehaviour
{


    public float Max_OxygenAmount = 100f;  // 최대 산소량
    public float _OxygenAmount = 100f;     // 현재 산소량

    public float OxygenAmount
    {
        get { return _OxygenAmount; }
        set
        {
            if (value <= 0)
            {
                _OxygenAmount = 0;
                return;
            }
            _OxygenAmount = (value >= Max_OxygenAmount) ? (Max_OxygenAmount) : (value);
        }
    }

    public static Mgr_BaseCamp Inst;

    private void Awake()
    {
        Inst = this;
    }


}
