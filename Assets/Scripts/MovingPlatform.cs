using UnityEngine;

[SelectionBase]
public class MovingPlatform : MonoBehaviour
{
    public float moveX = 5f;
    public float moveY = 0f;
    public float touchDistance = 3f;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, new Vector3(moveX, 0, moveY), out hit, touchDistance))  // თუკი რაიმე ობიექტის კოლაიდერს ეხება
        {
            if (!hit.collider.CompareTag("Player"))  // თუკი შეხებულის კოლაიდერი არ არის "Player", მაშინ სიარული დაიწყოს საპირისპირო მიმართულებით
            {
                moveX = -moveX;
                moveY = -moveY;
            }
        }
        transform.position += new Vector3(moveX, 0, moveY) * Time.deltaTime;  // გადაადგილება
    }
}