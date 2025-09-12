using UnityEngine;

[RequireComponent(typeof(Can))]
public class GanimetBirak : MonoBehaviour
{
    public GameObject kilcikPrefab;
    public int kilcikAdedi = 1;

    void Awake()
    {
        var can = GetComponent<Can>();
        can.Oldu += _ => Birak();
    }

    void Birak()
    {
        if (!kilcikPrefab) return;   
        for (int i = 0; i < kilcikAdedi; i++)
        {
            Vector2 rnd = Random.insideUnitCircle * 0.15f;
            Instantiate(kilcikPrefab, (Vector2)transform.position + rnd, Quaternion.identity);
        }
    }
}