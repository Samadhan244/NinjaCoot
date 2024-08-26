using UnityEngine;

public class HideObjectsWhichCoverPlayer : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Check if the collider that entered is tagged as "Building"
        if (other.CompareTag("Building"))
        {
            Renderer buildingRenderer = other.GetComponent<Renderer>();
            buildingRenderer.material.color = new Color(buildingRenderer.material.color.r, buildingRenderer.material.color.g, buildingRenderer.material.color.b, 100f / 255f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the collider that exited is tagged as "Building"
        if (other.CompareTag("Building"))
        {
            Renderer buildingRenderer = other.GetComponent<Renderer>();
            buildingRenderer.material.color = new Color(buildingRenderer.material.color.r, buildingRenderer.material.color.g, buildingRenderer.material.color.b, 255f / 255f);
        }
    }
}
