
using UnityEngine;

public class KameraTakip : MonoBehaviour
{
    public Transform hedef; //"hedef" burada kedi oluyor. Inspector'da kedi buraya s�r�klenecek.
    public BoxCollider2D dunyaSiniri; //D�nya s�n�rlar� bunun i�ine konulacak.

    Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate() //Sallanma azalmas� i�in late update...
    {
        if (hedef == null || dunyaSiniri == null) return; // Hedef ve s�n�r atanmad�ysa NullReference verme, sus.
 
        Vector3 desired = new(hedef.position.x, hedef.position.y, transform.position.z);
        //kameran�n posizyonu hedefin x ve y'sine e�it olmal� ancak z'sine dokunulmamal� ��nk� 2D kamerada z kameran�n derinli�idir.


        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Bounds b = dunyaSiniri.bounds;
        float minX = b.min.x + halfW;
        float maxX = b.max.x - halfW;
        float minY = b.min.y + halfH;
        float maxY = b.max.y - halfH;

        if (minX > maxX) { minX = maxX = b.center.x; }
        if (minY > maxY) { minY = maxY = b.center.y; }

        float clampedX = Mathf.Clamp(desired.x, minX, maxX);
        float clampedY = Mathf.Clamp(desired.y, minY, maxY);

        transform.position = new Vector3(clampedX, clampedY, desired.z);
    }
}
