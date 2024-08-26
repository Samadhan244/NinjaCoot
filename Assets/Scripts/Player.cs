using UnityEngine;
using UnityEngine.UI;
using TMPro;

[SelectionBase]
public class Player : MonoBehaviour
{
    [SerializeField] float life, currentHealth, maxHealth = 3, currentEnergy = 3f, maxEnergy, speed = 10f, currentSpeed, jumpHeight = 1f, verticalVelocity;
    [SerializeField] int currentXP, maxXP = 50, level = 1;
    [SerializeField] float spinTime, spinCD, spinReady = 3f, throwCD, throwReady = 3f;
    public bool hasKey, isThrowing, isSpinning, isSprinting, isTouchingWater, canSprint = true, isAlive = true;

    Vector3 movementDirection;
    Vector3 respawnPosition;
    [SerializeField] Transform cam;
    [SerializeField] GameManager gameManager; [SerializeField] Quest quest;
    [SerializeField] GameObject shurikenPrefab;
    [SerializeField] Image energyFill, healthFill, cooldownFill1, cooldownFill2, XPFill;
    [SerializeField] TextMeshProUGUI lifeText, healthText, XPText, levelText;
    [SerializeField] CharacterController characterController;
    [SerializeField] Animator animator;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] footstepSounds, painSounds, deathSounds;
    [SerializeField] AudioClip shoot, jump, spin, arrowImpact, mageImpact, fall, coin, levelUp;

    void Start()
    {
        LoadPlayerStats();
        currentHealth = currentHealth == 0 ? maxHealth : currentHealth;  //  Prevent starting with zero health if the player died during a scene transition
        maxEnergy = currentEnergy;
        UpdateHud(1);
        UpdateHud(2);
    }

    void Update()
    {
        Control();
        UpdateHud(3);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 10f);
    }

    void Control()
    {
        if (!isAlive) return;
        movementDirection = (cam.forward * Input.GetAxis("Vertical") + cam.right * Input.GetAxis("Horizontal")).normalized;
        // Jump & Gravity
        if (characterController.isGrounded && Input.GetButtonDown("Jump")) { verticalVelocity = isSpinning ? jumpHeight + 0.2f : jumpHeight; PlaySound("Jump"); }  // Jumpes a bit further when spinning
        else if (characterController.isGrounded) verticalVelocity = -1;  // Stays on the ground
        else verticalVelocity -= 3 * Time.deltaTime;  // Vertical velocity decreases/falling strengthens while in air
        movementDirection.y = verticalVelocity;

        // Movements & Rotation
        currentSpeed = isTouchingWater ? speed * 0.7f : isSprinting ? speed * 1.5f : speed;  // Moves faster when sprinting
        characterController.Move(movementDirection * currentSpeed * Time.deltaTime);
        if (movementDirection.x != 0 || movementDirection.z != 0) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(movementDirection.x, 0f, movementDirection.z)), Time.deltaTime * 10f);

        // Animations
        animator.SetInteger(Animator.StringToHash("MovingState"), Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0 ? 1 : 0);  // 1: Moving, 0: Idle animations
        animator.SetFloat(Animator.StringToHash("MovingSpeedAnimator"), currentSpeed / 10f);
        animator.SetBool(Animator.StringToHash("IsGrounded"), characterController.isGrounded);  // False: Jump/Fly animations

        // Spin | Throw
        throwCD = Mathf.Min(throwReady, throwCD + Time.deltaTime);  // Increases shootCD until it's ready
        spinCD = Mathf.Min(spinReady, spinCD + Time.deltaTime);
        if (Input.GetButton("Spin") && !isThrowing && !isSpinning && spinCD == spinReady)
        {
            spinCD = 0;
            isSpinning = true;
            animator.Play(Animator.StringToHash("Spin" + Random.Range(1, 3)));
            PlaySound("Spin");

            this.Wait(spinTime, () =>  // Break from the animation after "spinTime" seconds
            {
                isSpinning = false;
                if (isAlive) animator.Play(Animator.StringToHash("Idle"));
            });
        }
        else if (Input.GetButton("Throw") && !isSpinning && !isThrowing && throwCD == throwReady)
        {
            throwCD = 0;
            isThrowing = true;
            animator.Play(Animator.StringToHash("Throw"));
            PlaySound("Throw");
            FindAndShootNearestEnemyIfAlive();
            this.Wait(0.6f, () => isThrowing = false);  // Break from the animation after it's done
        }

        // Sprint
        isSprinting = canSprint && Input.GetButton("Sprint") && currentEnergy > 0 && characterController.isGrounded && (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) ? true : false;
        if (!isSprinting) currentEnergy += Time.deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        if (canSprint)
        {
            if (currentEnergy <= 0)
            {
                canSprint = false;
                energyFill.color = Color.grey;
            }
            if (isSprinting) currentEnergy -= Time.deltaTime;
        }
        else if (currentEnergy == maxEnergy)
        {
            canSprint = true;
            energyFill.color = Color.white;
        }
    }

    void FindAndShootNearestEnemyIfAlive()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 10f, LayerMask.GetMask("Enemy"));
        Transform nearestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy && enemy.isAlive)
            {
                Vector3 directionToEnemy = hit.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, directionToEnemy);

                if (angle < 60f)  // We can adjust the angle to your preference
                {
                    float distance = directionToEnemy.sqrMagnitude;

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestEnemy = hit.transform;
                    }
                }
                // Draw a line to each detected enemy
                Debug.DrawLine(transform.position, hit.transform.position, Color.yellow, 0.5f);
            }
        }
        GameObject shuriken = Instantiate(shurikenPrefab, transform.position + transform.forward + transform.up, transform.rotation);
        ShurikenProjectile shurikenProjectile = shuriken.GetComponent<ShurikenProjectile>();
        shurikenProjectile.target = nearestEnemy;
    }

    void OnCollisionExit(Collision collision) { transform.parent = null; }
    void OnCollisionStay(Collision collision) { if (collision.gameObject.CompareTag("MovingPlatform")) transform.SetParent(collision.transform); }
    void OnCollisionEnter(Collision collision)
    {
        if (isAlive)
        {
            if (collision.gameObject.CompareTag("Lava")) Death();
            else if (collision.gameObject.CompareTag("Checkpoint"))
            {
                ActivatedOrNot checkPoint = collision.gameObject.GetComponent<ActivatedOrNot>();
                if (!checkPoint.isActivated)
                {
                    checkPoint.isActivated = true;
                    PlaySound("Checkpoint");
                    respawnPosition = new Vector3(collision.transform.position.x, collision.transform.position.y + 1f, collision.transform.position.z);
                    gameManager.EventText(2);
                }
            }
        }
    }
    void OnTriggerExit(Collider collision) {if (collision.gameObject.CompareTag("Water")) isTouchingWater = false;}
    void OnTriggerEnter(Collider collision)
    {
        if (isAlive)
        {
            if (collision.gameObject.CompareTag("Damager")) TakeDamage();
            else if (collision.gameObject.CompareTag("Door")) quest.QuestIt();
            else if (collision.gameObject.CompareTag("Water")) isTouchingWater = true;
            else if (collision.gameObject.CompareTag("Lava")) Death();
            else if (collision.gameObject.CompareTag("Arrow"))
            {
                PlaySound("ArrowImpact");
                collision.gameObject.SetActive(false);  // Projectile dissapears after it touches the Player
                TakeDamage();  // Player can't resist Arrow, no matter what
            }
            else if (collision.gameObject.CompareTag("Spell"))
            {
                PlaySound("MageImpact");
                collision.gameObject.SetActive(false);  // Projectile dissapears after it touches the Player
                if (!isSpinning) TakeDamage();  // If the Player touches Mage's projectile without spinning, the Player will take damage
            }
            else if (collision.gameObject.CompareTag("Coin"))
            {
                PlaySound("Coin");
                Coin coin = collision.gameObject.GetComponent<Coin>();
                coin.Take();
            }
            else if (collision.gameObject.CompareTag("Endpoint"))
            {
                ActivatedOrNot endingPoint = collision.gameObject.GetComponent<ActivatedOrNot>();
                if (!endingPoint.isActivated)
                {
                    endingPoint.isActivated = true;
                    animator.Play(Animator.StringToHash("Victory"));
                    isAlive = false;
                    PlaySound("Victory");
                    transform.position = new Vector3(collision.transform.position.x, collision.transform.position.y + 1f, collision.transform.position.z);
                    gameManager.EventText(3);
                }
            }
        }
    }

    void UpdateHud(byte index)
    {
        if (index == 1)
        {
            lifeText.text = $"{life}";
            healthText.text = $"{currentHealth} / {maxHealth}";
            healthFill.fillAmount = currentHealth / maxHealth;
        }
        else if (index == 2)
        {
            levelText.text = level.ToString();
            XPText.text = currentXP + " / " + maxXP;
            XPFill.fillAmount = (float)currentXP / (float)maxXP;
        }
        else if (index == 3)
        {
            energyFill.fillAmount = currentEnergy / maxEnergy;
            cooldownFill1.fillAmount = spinCD / spinReady;
            cooldownFill2.fillAmount = throwCD / throwReady;
        }
    }

    public void GainXP(int XP)
    {
        currentXP += XP;
        if (currentXP >= maxXP)
        {
            currentXP = currentXP - maxXP;
            maxXP = (int)(maxXP * 1.5 + level * 20);
            level += 1;
            this.Wait(0.5f, () => { PlaySound("LevelUp"); gameManager.EventText(2); });
        }
        UpdateHud(2);
    }

    public void TakeDamage()  // This method is called by Enemy's animation
    {
        if (isAlive)
        {
            currentHealth -= 1;
            PlaySound("Pain");
            if (!isSpinning) animator.Play(Animator.StringToHash("GetHit"));
            if (currentHealth <= 0) Death();
            UpdateHud(1);
        }
    }

    void Death()
    {
        isAlive = false; isSpinning = false;
        animator.Play(Animator.StringToHash("Death"));
        // Prevent bug where Player respawns with jumping/moving animation after it is killen in air/when moving
        verticalVelocity = -1; animator.SetBool(Animator.StringToHash("IsGrounded"), true); animator.SetInteger(Animator.StringToHash("MovingState"), 0);
        PlaySound("Death");
        PlaySound("Fall");
        currentHealth = 0;
        gameManager.EventText(1);
        if (life > 0)
        {
            life -= 1;
            UpdateHud(1);
            this.Wait(2f, () => { animator.Play(Animator.StringToHash("Revive")); transform.position = respawnPosition; });
            this.Wait(4f, () => { isAlive = true; currentHealth = maxHealth; spinCD = spinReady; throwCD = throwReady; currentEnergy = maxEnergy; UpdateHud(1); });
        }
        else UpdateHud(1);
    }

    void PlaySound(string soundName)
    {
        switch (soundName)
        {
            case "Footsteps": audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]); break;
            case "Throw": audioSource.PlayOneShot(shoot); break;
            case "Spin": audioSource.PlayOneShot(spin); break;
            case "Jump": audioSource.PlayOneShot(jump); break;
            case "Pain": audioSource.PlayOneShot(painSounds[Random.Range(0, painSounds.Length)]); break;
            case "Death": audioSource.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)]); break;
            case "Fall": audioSource.PlayOneShot(fall); break;
            case "ArrowImpact": audioSource.PlayOneShot(arrowImpact); break;
            case "MageImpact": audioSource.PlayOneShot(mageImpact); break;
            case "Coin": audioSource.PlayOneShot(coin); break;
            case "LevelUp": audioSource.PlayOneShot(levelUp); break;
        }
    }

    void LoadPlayerStats()
    {
        life = 5;
        currentHealth = 3;
        maxHealth = 3;
        spinReady = 1.5f;
        throwReady = 3f;
        spinTime = 0.8f;
        spinCD = spinReady;
        throwCD = throwReady;

        if (PlayerPrefs.GetInt(gameManager.accountAndScene + "SavedPositon") == 1)
        {
            respawnPosition = new Vector3(
                PlayerPrefs.GetFloat(gameManager.accountAndScene + "PosX"),
                PlayerPrefs.GetFloat(gameManager.accountAndScene + "PosY"),
                PlayerPrefs.GetFloat(gameManager.accountAndScene + "PosZ"));
            transform.position = respawnPosition;
        }
        else respawnPosition = transform.position;
    }

    public void SavePlayerData()
    {
        PlayerPrefs.SetFloat(PlayerPrefs.GetString("Account") + "_" + "Life", life);
        PlayerPrefs.SetFloat(PlayerPrefs.GetString("Account") + "_" + "CurrentHealth", currentHealth);
        PlayerPrefs.SetFloat(PlayerPrefs.GetString("Account") + "_" + "MaxHealth", maxHealth);

        PlayerPrefs.SetInt(gameManager.accountAndScene + "SavedPositon", 1);
        PlayerPrefs.SetFloat(gameManager.accountAndScene + "PosX", transform.position.x);
        PlayerPrefs.SetFloat(gameManager.accountAndScene + "PosY", transform.position.y);
        PlayerPrefs.SetFloat(gameManager.accountAndScene + "PosZ", transform.position.z);
    }
}