using System.Collections.Generic;
using UnityEngine;

public class UI_ObjPool : MonoBehaviour
{
    [Header("Interact_UI")]
    [SerializeField] Interaction_UI Interact_UI_Prefab;
    [SerializeField] Transform Interact_UI_Tr;

    public static UI_ObjPool Inst = null;

    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }
    }


    #region 상호작용 UI
    public Interaction_UI Spawn_Interaction_UI(Interaction interaction)
    {
        GameObject obj = Instantiate(Interact_UI_Prefab.gameObject);
        Interaction_UI interaction_UI = obj.GetComponent<Interaction_UI>();
        obj.transform.SetParent(Interact_UI_Tr, false);


        switch (interaction.InteractionType)
        {
            case EInteractionType.Itme:
                {

                    interaction_UI.Item_Obj_List.Add(interaction.GetComponent<Interaction_Item>());
                    
                    break;
                }


        }

        interaction_UI.InteractionType = interaction.InteractionType;
        interaction_UI.UI_Update();

        return interaction_UI;
    }
    #endregion
}
