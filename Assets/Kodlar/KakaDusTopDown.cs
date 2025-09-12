using UnityEngine;

public class KakaDusTopDown : MonoBehaviour
{
    [Header("Y�n ve H�z")]
    [Tooltip("Ekran�n 'a�a��' y�n�. D�ZG�N �ALI�MASI ���N normalize edilir.")]
    public Vector2 downDirection = new(0f, -1f);  // izometrik sahnede �rn: (1,-1)
    public float fallSpeed = 5f;
    [Tooltip("Kedinin x hizas�na hafif liderlik. Sadece yatay eksen gibi d���n.")]
    public float driftStrength = 2.0f;
    public float maxDrift = 2.5f;

    [Header("�ni� Ko�ulu")]
    [Tooltip("Ka� saniye sonra patlas�n. 0 ise mesafe e�i�i kullan.")]
    public float explodeAfterSeconds = 0f;
    [Tooltip("Spawn noktas�ndan �u kadar birim ilerleyince patla (explodeAfterSeconds=0 iken).")]
    public float explodeAfterDistance = 2.2f;

    [Header("Hasar ve Hedef")]
    public int tabanZarar = 10;
    public float tabanYaricap = 0.45f;
    public LayerMask hedefKatman;          // Kedi layer'�n� i�aretle
    public string hedefTag = "Kedi";       // Kedi tag

    [Header("G�rsel")]
    public SpriteRenderer govde;
    public SpriteRenderer golge;
    public float golgeScaleMin = 0.6f;
    public float golgeScaleMax = 1.1f;

    [Header("Par�a")]
    public GameObject kakaParcasiPrefab;
    public int parcacikSayisi = 8;
    public float parcacikHizi = 6f;
    public float parcacikRastgele = 10f;

    [Header("Ya�am")]
    public float maxYasam = 8f;

    // Dahili
    Vector3 startPos;
    Transform hedefKedi;  // opsiyonel, drift i�in

    void Awake()
    {
        if (!govde) govde = GetComponent<SpriteRenderer>();
        startPos = transform.position;

        // Kamera/sahne y�n�ne g�re "a�a��"y� normalize et
        if (downDirection.sqrMagnitude < 0.0001f) downDirection = Vector2.down;
        downDirection = downDirection.normalized;

        // RB varsa kavga ��kmas�n
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;
    }

    void Start()
    {
        if (maxYasam > 0f) Destroy(gameObject, maxYasam);

        // Drift i�in kediyi bulmak opsiyonel
        var k = GameObject.FindGameObjectWithTag(hedefTag);
        if (k) hedefKedi = k.transform;

        // Zemin g�lgesi ilk konum
        if (golge)
        {
            golge.transform.position = startPos;
            golge.transform.localScale = Vector3.one * golgeScaleMin;
            if (govde) golge.sortingOrder = govde.sortingOrder - 1;
        }
    }

    void Update()
    {
        // Drift (yaln�zca ekranda yatay eksen gibi d���nd���n y�nde)
        Vector2 drift = Vector2.zero;
        if (hedefKedi)
        {
            float dx = Mathf.Clamp(hedefKedi.position.x - transform.position.x, -1.2f, 1.2f);
            drift = new Vector2(dx * driftStrength, 0f);
            if (drift.magnitude > maxDrift) drift = drift.normalized * maxDrift;
        }

        // Hareket: a�a�� + drift
        Vector2 vel = downDirection * fallSpeed + drift;
        transform.position += (Vector3)(vel * Time.deltaTime);

        // G�lge b�y�me (opsiyonel)
        if (golge)
        {
            float t = Mathf.Clamp01(Vector3.Distance(startPos, transform.position) / Mathf.Max(0.01f, explodeAfterDistance));
            float s = Mathf.Lerp(golgeScaleMin, golgeScaleMax, t);
            golge.transform.position = startPos; // zemin noktas�
            golge.transform.localScale = new Vector3(s, s, 1f);
            golge.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.25f, 0.6f, t));
        }

        // �ni� tetikleyici: s�re ya da mesafe
        bool patla = false;
        if (explodeAfterSeconds > 0f)
        {
            explodeAfterSeconds -= Time.deltaTime;
            if (explodeAfterSeconds <= 0f) patla = true;
        }
        else
        {
            float dist = Vector3.Distance(startPos, transform.position);
            if (dist >= explodeAfterDistance) patla = true;
        }

        if (patla) Patla();
    }

    void Patla()
    {
        // �ni� �ember hasar�
        Collider2D[] hits = hedefKatman.value != 0
            ? Physics2D.OverlapCircleAll(transform.position, tabanYaricap, hedefKatman)
            : Physics2D.OverlapCircleAll(transform.position, tabanYaricap);

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (!c) continue;
            if (!string.IsNullOrEmpty(hedefTag) && !c.CompareTag(hedefTag)) continue;

            var can = c.GetComponentInParent<KedininCani>();
            if (can != null) can.HasarAl(tabanZarar);
        }

        // Par�ac�klar
        if (kakaParcasiPrefab && parcacikSayisi > 0)
        {
            float temel = 360f / parcacikSayisi;
            for (int i = 0; i < parcacikSayisi; i++)
            {
                float aci = temel * i + Random.Range(-parcacikRastgele, parcacikRastgele);
                Vector2 yon = new Vector2(Mathf.Cos(aci * Mathf.Deg2Rad), Mathf.Sin(aci * Mathf.Deg2Rad)).normalized;
                var p = Instantiate(kakaParcasiPrefab, transform.position, Quaternion.identity);
                var par = p.GetComponent<KakaParcasi>();
                if (par) par.hiz = yon * parcacikHizi;
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.6f, 0f, 0.4f);
        Gizmos.DrawWireSphere(Application.isPlaying ? transform.position : (Vector3)startPos, tabanYaricap);
        Gizmos.color = Color.cyan;
        Vector3 d = (Vector3)(downDirection.normalized) * 1.2f;
        Gizmos.DrawLine(transform.position, transform.position + d);
    }
}
