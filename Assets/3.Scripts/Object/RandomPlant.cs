using UnityEngine;

public class RandomPlant : MonoBehaviour
{
    public GameObject[] plant;

    public int plantCount;
    public float generateRange;

    void Start()
    {
        for (int i = 0; i < plantCount; i++)
        {
            GameObject obj = Instantiate(plant[Random.Range(0, 2)], transform);

            obj.transform.position = transform.position + new Vector3(Random.Range(-generateRange, generateRange), 0, Random.Range(-generateRange, generateRange));
            obj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            obj.transform.localScale *= Random.Range(0.9f, 1.2f);
        }
    }

    void Update()
    {
        
    }
}
