using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Mermi Ayarları")]
    public float speed = 15f;
    public float lifeTime = 2f; // Ekranda kalma süresi

    private float currentLifeTime;

    void OnEnable()
    {
        // Mermi havuzdan her çağrıldığında ömrünü sıfırla
        currentLifeTime = lifeTime;
    }

    void Update()
    {
        // 2D'de sağ yön (transform.right) merminin baktığı yöndür. 
        // İleri doğru sabit hızda hareket ettir.
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Ömrü dolunca mermiyi kapat (havuza geri dönmüş olur)
        currentLifeTime -= Time.deltaTime;
        if (currentLifeTime <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // İlerisi için Enemy veya Obstacle etiketli objelere çarpma kontrolü
        if (hitInfo.CompareTag("Enemy") || hitInfo.CompareTag("Obstacle"))
        {
            // TODO: Düşmana hasar verme kodunu buraya ekleyebilirsin
            // hitInfo.GetComponent<Enemy>().TakeDamage(10);

            // Çarptığı an mermiyi kapat
            gameObject.SetActive(false);
        }
    }
}