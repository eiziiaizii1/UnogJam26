using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Ateşleme Ayarları")]
    public Transform firePoint;      // Merminin çıkacağı nokta
    public GameObject bulletPrefab;  // Mermi prefabi
    public float fireRate = 0.15f;   // İki mermi arasındaki bekleme süresi

    [Header("Havuz Ayarları")]
    public int poolSize = 20;        // Aynı anda ekranda olabilecek maksimum mermi sayısı

    [Tooltip("Sahnede mermilerin toplanacağı boş objeyi buraya sürükle")]
    public Transform bulletContainer; // <-- Yeni eklediğimiz referans

    private GameObject[] bulletPool;
    private int currentBulletIndex = 0;
    private float nextFireTime = 0f;

    void Start()
    {
        bulletPool = new GameObject[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            // Unity'nin pratik Instantiate özelliği ile objeyi üretirken direkt Container'ın içine atıyoruz
            GameObject obj = Instantiate(bulletPrefab, bulletContainer);

            obj.SetActive(false); // Başlangıçta gizli
            bulletPool[i] = obj;
        }
    }

    void Update()
    {
        // Sol tık ve ateş etme hızı kontrolü
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // Havuzdaki sıradaki mermiyi al
        GameObject bullet = bulletPool[currentBulletIndex];

        // Merminin pozisyonunu ve rotasyonunu FirePoint'e eşitle
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;

        // Mermiyi aktif et
        bullet.SetActive(true);

        // İndeksi bir artır (Loopable / Dairesel Sistem)
        currentBulletIndex = (currentBulletIndex + 1) % poolSize;
    }
}