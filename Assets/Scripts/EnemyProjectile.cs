using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    enum Type { Arrow, Spell }
    [SerializeField] Type type;
    [SerializeField] Transform player;
    [SerializeField] Player playerScript;
    float lifespan = 4f, arrowSpeed = 35f, spellSpeed = 7f, startingLifespan;

    void OnEnable() { startingLifespan = lifespan; }
    void OnDisable() { lifespan = startingLifespan; }

    void Update()
    {
        // If the projectile is an Arrow, it will move in the direction it is facing; if it's a Spell, it will move towards the Player. Arrow moves faster
        if (type == Type.Arrow) transform.position += transform.forward.normalized * arrowSpeed * Time.deltaTime;
        else transform.position += (player.position - transform.position + Vector3.up).normalized * spellSpeed * Time.deltaTime;

        // When its lifespan reaches 0 or when the Player dies, reset the lifespan and disable the projectile to be used again later
        if (lifespan <= 0 || !playerScript.isAlive)
        {
            lifespan = startingLifespan;
            gameObject.SetActive(false);
        }
        else lifespan -= Time.deltaTime;
    }
}