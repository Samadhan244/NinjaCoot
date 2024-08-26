using UnityEngine;

public class ShurikenProjectile : MonoBehaviour
{
    [SerializeField] Transform[] models;  // 4 models of the projectile
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip hitWall;
    public Transform target;
    float moveSpeed = 20f, rotationSpeed = 3f, lifespan = 20f, maxDistance = 25f;
    bool hasTouchedAnything, canRotate;
    Vector3 initialPosition, movementDirection;

    void Start()
    {
        // Randomly choose and activate 1 from 4 models and hide the others. If the chosen model is Kunai, it won't have a rotation animation
        int random = Random.Range(0, models.Length);
        for (int i = 0; i < models.Length; i++) models[i].gameObject.SetActive(random == i);
        canRotate = random != 3 ? true : false;

        movementDirection = transform.forward;  // Direction the player was looking when shooting
        initialPosition = transform.position;  // Save starting position to calculate how far it moved
        Destroy(gameObject, lifespan);  // Projectiles automatically despawn after some time
    }

    void Update()
    {
        if (!hasTouchedAnything)
        {
            if (target)
            {
                if (Vector3.Distance(transform.position, target.position) <= 2f)
                {
                    target.gameObject.GetComponent<Enemy>().DeathAndRespawn(2);
                    Destroy(gameObject);
                }
                else transform.position += (target.position - transform.position + Vector3.up).normalized * moveSpeed * Time.deltaTime;  // Move towards target, unless it touches anything
            }
            else transform.position += movementDirection * moveSpeed * Time.deltaTime;  // Move the projectile towards its forward direction, unless it touches anything
        }
        if (canRotate) transform.Rotate(0, rotationSpeed, 0);  // Rotate unless it's a Kunai or has touched anything
        if (Vector3.Distance(initialPosition, transform.position) > maxDistance) Destroy(gameObject);  // Destroy if it moves too far from the start position (prevents long-range kills)
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasTouchedAnything)
        {
            if (other.gameObject.CompareTag("Building") || other.gameObject.CompareTag("Untagged") || other.gameObject.CompareTag("Terrain") || other.gameObject.CompareTag("Damager"))
            {
                movementDirection = Vector3.zero;
                audioSource.PlayOneShot(hitWall);
                transform.SetParent(other.transform);
                hasTouchedAnything = true;
                canRotate = false;
            }
        }
    }
}