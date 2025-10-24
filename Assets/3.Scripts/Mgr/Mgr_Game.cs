using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Mgr_Game : MonoBehaviour
{



    public static Mgr_Game Inst;

    void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        // Å×½ºÆ®
        Cursor.lockState = CursorLockMode.Locked;
    }

    async void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            await Mgr_Data.Inst.TestSave();
        }
    }
}
