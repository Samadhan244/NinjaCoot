using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    int currentCoins, maxCoins, currentKills, maxKills;
    static bool isHudOn = true;
    public string accountAndScene, difficulty;
    float updateFpsCd;

    [SerializeField] Player player;
    [SerializeField] GameObject coinsParent, enemiesParent;
    [SerializeField] TextMeshProUGUI eventText, mainText, coinText, killText;

    void Awake()
    {
        accountAndScene = PlayerPrefs.GetString("Account") + "_" + SceneManager.GetActiveScene().name;
        difficulty = PlayerPrefs.GetString(PlayerPrefs.GetString("Account") + "_Difficulty");

        currentCoins = PlayerPrefs.GetInt(accountAndScene + "TakenCoins", 0);
        maxCoins = coinsParent.transform.childCount;  // Count of "Coin" inside "Coins"
        maxKills = enemiesParent.transform.childCount;

        //hudAnimator.Play(Animator.StringToHash(isHudOn ? "HudOn" : "HudOff"));
        //mainBackground.SetActive(isHudOn);
        coinText.text = $"{currentCoins:00}/{maxCoins:00}";
        killText.text = $"{currentKills:00}/{maxKills:00}";
    }

    void Update()
    {
        UpdateMainHud();
        // Toggle Hud
        if (Input.GetButtonDown("ToggleHud"))
        {
            isHudOn = !isHudOn;
            //hudAnimator.Play(Animator.StringToHash(isHudOn ? "HudOn" : "HudOff"));
            //mainBackground.SetActive(isHudOn);
        }

        if (Input.GetKeyDown(KeyCode.G)) SaveGame();
    }

    void UpdateMainHud()  // Hud updates every second
    {
        updateFpsCd += Time.deltaTime;
        if (updateFpsCd >= 1)
        {
            updateFpsCd = 0;
            mainText.text = $"FPS: {1f / Time.deltaTime:F0}   Progress: {0}%";
        }
    }

    public void AddCoin()
    {
        currentCoins += 1;
        coinText.text = $"{currentCoins:00}/{maxCoins:00}";
    }

    public void AddKill()
    {
        currentKills += 1;
        killText.text = $"{currentKills:00}/{maxKills:00}";
    }

    public void EventText(int Event)
    {
        eventText.text = Event == 1 ? "You are dead" : Event == 2 ? "Level Up!" : Event == 3 ? "You won!" : null;
        eventText.color = Event == 1 ? Color.red : Event == 2 ? Color.green : Event == 3 ? Color.green : Color.white;
        this.Wait(3f, () => eventText.text = null);
    }

    void SaveGame()
    {
        player.SavePlayerData();
        foreach (Transform x in coinsParent.transform)
        {
            Coin coin = x.GetComponent<Coin>();
            coin.SaveCoinState();
        }
    }
}