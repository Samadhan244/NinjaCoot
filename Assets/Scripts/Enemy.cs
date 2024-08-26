using UnityEngine;
using UnityEngine.AI;

[SelectionBase]
public class Enemy : MonoBehaviour
{
    [SerializeField] byte enemyType; 
    [SerializeField] GameObject[] projectilesToLoop;
    [SerializeField] Vector2 xp = new Vector2();
    [SerializeField] float speed, attackRange, attackDelay, maxMovementDistance = 25f, patrolInterval, patrolRadius = 12f, respawnInterval = 30f;
    [SerializeField] bool isDoingAttackAnimation, isGoingBack, isKillenOnce; public bool isAlive = true;
    [SerializeField] Transform target; [SerializeField] Player targetScript; [SerializeField] GameManager gameManager; [SerializeField] Quest quest;
    [SerializeField] GameObject xpPrefab;

    [SerializeField] Animator animator; int[] animatorHashes;
    [SerializeField] NavMeshAgent agent; NavMeshHit hit; Vector3 startingPosition;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip getHitByShurikenSound, attackSound;
    [SerializeField] AudioClip[] footstepSounds, getHitByPlayerSounds, deathSounds;

    void Start()
    {
        startingPosition = transform.position;
        transform.Rotate(0, Random.Range(0, 361), 0);  // Starts the game with a random rotation
        animatorHashes = new int[]{
            Animator.StringToHash("MovingState"),
            Animator.StringToHash("Attack1"),
            Animator.StringToHash("Attack2"),
            Animator.StringToHash("Death1"),
            Animator.StringToHash("Death2"),
            Animator.StringToHash("Respawn") };
        if (enemyType == 1) { attackRange = 1.5f; attackDelay = 1f; speed = 7.5f; }
        else { attackRange = 8f; attackDelay = 1.5f; speed = 6f; }
        agent.speed = speed; agent.angularSpeed = 1000f; agent.acceleration = 1000f; agent.radius = 0.4f;
    }

    void Update()
    {
        PatrolDetectChase();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            target = other.transform;
            agent.ResetPath();
        }
    }

    void PatrolDetectChase()
    {
        if (isAlive)
        {
            animator.SetInteger(animatorHashes[0], agent.velocity.magnitude == 0 ? 0 : (target || isGoingBack) ? 2 : 1);  // We need this check for NPC's animations(Idle, Running, Walking)
            patrolInterval = Mathf.Max(0, patrolInterval - Time.deltaTime);
            // NPC patrols nearby his "patrolRadius" in every random interval
            if (!target && patrolInterval == 0)
            {
                patrolInterval = Random.Range(5f, 10f);
                if (NavMesh.SamplePosition(Random.insideUnitSphere * patrolRadius + startingPosition, out hit, patrolRadius, NavMesh.AllAreas))
                {
                    agent.speed = speed * 0.8f;  // Moves a bit slow when patrolling
                    agent.stoppingDistance = 1.1f;
                    agent.SetDestination(hit.position);
                }
            }
            // If Player is dead, NPC only does patrol
            if (target && !isGoingBack)
            {
                if (!targetScript.isAlive)  // If Target dies, reset target
                {
                    target = null;
                    return;
                }  

                // In agressive stance, moves with its normal speed and always rotating towards the Target
                agent.speed = speed;
                transform.rotation = Quaternion.LookRotation(new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z));  // Npc always looks(rotates) at Player
                // When the Player touches NPC while the Player is spinning, NPC dies
                if (Vector3.Distance(transform.position, target.position) <= 1.5f && targetScript.isSpinning) DeathAndRespawn(1);
                // If NPC moves too far away from his "startingPosition", it will go back and nullify target
                if (Vector3.Distance(transform.position, startingPosition) > maxMovementDistance)
                {
                    isGoingBack = true;
                    target = null;
                    agent.speed = speed * 1.5f;
                    this.Wait(0.1f, () => agent.SetDestination(startingPosition));
                    this.Wait(3f, () => { if (isGoingBack) isGoingBack = false; });  // We need "isGoingBack" check to avoid bug when Player kills the NPC while it's going back
                }
                // Attack the Player if he is in NPC's "attackRange" and the NPC doesn't already do the attack animation
                else if (Vector3.Distance(transform.position, target.position) <= attackRange && !isDoingAttackAnimation)
                {
                    if (isAlive)  // We need to check again if "isAlive" to avoid a bug where the NPC does attack animation when dead
                    {
                        isDoingAttackAnimation = true;
                        animator.Play(animatorHashes[Random.Range(1, 3)]);  // Does random of 2 attack animations for: Swordsman, Archer, Mage
                        this.Wait(attackDelay, () => isDoingAttackAnimation = false);
                    }
                }
                // Else chase the Target
                else
                {
                    agent.stoppingDistance = attackRange;
                    agent.SetDestination(target.transform.position);
                }
            }
        }
    }

    public void DeathAndRespawn(byte deathStyle)  // Death styles: 1. By Projectile, 2. By Player
    {
        animator.Play(deathStyle == 1 ? animatorHashes[3] : animatorHashes[4]);
        if (deathStyle == 2) PlaySound("GetHitByShruiken");
        else PlaySound("GetHitByPlayer");
        this.Wait(0.2f, () => PlaySound("Death"));
        agent.ResetPath();
        isAlive = false;

        if(!isKillenOnce) {isKillenOnce = true; gameManager.AddKill();}
        if(Quest.questIndex == 1) quest.QuestIt();

        int randomXp = Random.Range((int)xp.x, (int)xp.y + 1);
        GameObject XP = Instantiate(xpPrefab, transform.position + Vector3.up, transform.rotation);
        xpPrefab.GetComponent<XPText>().ShowXPText(randomXp);
        targetScript.GainXP(randomXp);

        this.Wait(3f, () => gameObject.SetActive(false));
        this.Wait(respawnInterval, () =>
        {
            gameObject.SetActive(true);
            agent.Warp(startingPosition);
            animator.Play(animatorHashes[5]);  // Respanw animation
            this.Wait(1f, () => { isAlive = true; target = null; });
        });
    }

    void DealDamage()  // Enemy's attack animation calls for this method when NPC is attacking
    {
        if (enemyType == 1) targetScript.TakeDamage();
        else
            foreach (GameObject x in projectilesToLoop)
            {
                if (!x.activeInHierarchy)
                {
                    x.transform.position = transform.position + transform.forward + transform.up;  // Projectile teleports near the Enemy which shot it
                    x.transform.rotation = transform.rotation;  // Projectile goes wherever the Enemy was looking when shooting
                    x.SetActive(true);
                    return;
                }
            }
    }

    void PlaySound(string soundName)  // Enemy's attack animation calls for this method's "Attack" sound
    {
        switch (soundName)
        {
            case "Footsteps": audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]); break;
            case "GetHitByPlayer": audioSource.PlayOneShot(getHitByPlayerSounds[Random.Range(0, getHitByPlayerSounds.Length)]); break;
            case "GetHitByShruiken": audioSource.PlayOneShot(getHitByShurikenSound); break;
            case "Death": audioSource.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)]); break;
            case "Attack": audioSource.PlayOneShot(attackSound); break;
        }
    }
}