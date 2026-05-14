using UnityEngine;

public class Monster_Anim : MonoBehaviour
{
    Monster monster;

    void Start()
    {
        monster = transform.parent.GetComponent<Monster>();
    }

    public void AnimEnd()
    {
        monster.animEnd = true;
    }
}
