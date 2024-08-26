using UnityEngine;

public class XPText : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshPro xpText;
    Transform cameraTransform;
    float elapsedTime, displayDuration = 1f, moveSpeed = 1f;

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    public void ShowXPText(int xpAmount)
    {
        xpText.text = "XP +" + xpAmount.ToString();
    }

    void Update()
    {
        if (elapsedTime < displayDuration) elapsedTime += Time.deltaTime;
        else Destroy(gameObject);

        transform.LookAt(cameraTransform);
        transform.Rotate(0, 180, 0);
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }
}