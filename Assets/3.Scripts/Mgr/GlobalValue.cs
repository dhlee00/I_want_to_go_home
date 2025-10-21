using System.Collections.Generic;
using UnityEngine;

public static class GlobalValue
{
    public static Dictionary<int, Item> User_Inventory = new Dictionary<int, Item>();

    public static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}