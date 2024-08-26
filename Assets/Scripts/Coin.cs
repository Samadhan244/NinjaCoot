using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour
{
    GameManager gameManager;
    bool isTaken;  // Tracks if a coin is collected
    string coinKey;  // Coin's unique key for PlayerPrefs

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        coinKey = gameManager.accountAndScene + gameObject.name;  // Account+Scene+Coin: Nikson_Level1_Coin_1
        if (PlayerPrefs.GetInt(coinKey) == 1) gameObject.SetActive(false);  // Hide the coin if it's already taken
    }

    public void Take()  // Marks the coin as taken and hides it(this method is activated by Player collision)
    {
        isTaken = true;
        gameObject.SetActive(false);
        gameManager.AddCoin();
    }

    public void SaveCoinState()
    {
        if (isTaken)
        {
            PlayerPrefs.SetInt(coinKey, 1);  // Save the state of the coin: Nikson_Level1_Coin_1 == 1(is taken)
            PlayerPrefs.SetInt(gameManager.accountAndScene + "TakenCoins", PlayerPrefs.GetInt(gameManager.accountAndScene + "TakenCoins", 0) + 1);
        }
    }
}