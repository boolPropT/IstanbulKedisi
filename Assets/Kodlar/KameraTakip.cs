
using UnityEngine;

public class KameraTakip : MonoBehaviour
{
    public Transform hedef; //"hedef" burada kedi oluyor. Inspector'da kedi buraya sürüklenecek.
    public BoxCollider2D dunyaSiniri; //Dünya sýnýrlarý bunun içine konulacak.

    Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate() //Sallanma azalmasý için late update...
    {
        if (hedef == null || dunyaSiniri == null) return; // Hedef ve sýnýr atanmadýysa NullReference verme, sus.
 
        Vector3 desired = new(hedef.position.x, hedef.position.y, transform.position.z);
        //kameranýn posizyonu hedefin x ve y'sine eþit olmalý ancak z'sine dokunulmamalý çünkü 2D kamerada z kameranýn derinliðidir.


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
