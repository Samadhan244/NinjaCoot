using UnityEngine;
using TMPro;

public class Quest : MonoBehaviour
{
    static public int questIndex = 1;
    [SerializeField] TextMeshProUGUI questText;
    [SerializeField] AudioSource audioSource; [SerializeField] AudioClip questDone;
    [SerializeField] Animator doorAnimator;

    public void QuestIt()
    {
        if (questIndex == 1)
        {
            if (Random.Range(0, 100) < 50)
            {
                audioSource.PlayOneShot(questDone);
                questText.text = "Quest: You've found a key! Now you can enter the village";
                Player player = GameObject.Find("Player").GetComponent<Player>();
                player.hasKey = true;
                questIndex = 2;
            }
        }
        else if (questIndex == 2)
        {
            audioSource.PlayOneShot(questDone);
            questText.text = "Quest: Kill them all";
            doorAnimator.Play("Open");
            questIndex = 0;
        }
    }
}