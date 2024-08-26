using UnityEngine;

public class HidablePlatforms : MonoBehaviour
{
    public GameObject[] objects1;
    public GameObject[] objects2;

    void Start()
    {
        InvokeRepeating(nameof(ToggleVisibility), 0f, 1f);
    }

    void ToggleVisibility()
    {
        foreach (GameObject x in objects1) x.SetActive(!x.activeSelf);
        foreach (GameObject x in objects2) x.SetActive(!x.activeSelf);
    }
}