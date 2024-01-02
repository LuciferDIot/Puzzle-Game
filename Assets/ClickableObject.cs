using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    public ObjectSpawner objectSpawner;

    void OnMouseDown()
    {
        if (!objectSpawner.ObjectMoving() 
        // && this.CompareTag("ClickableObject")
        )
        {
            // Destroy the clicked object
            Destroy(gameObject);

            // Move the objects above down
            objectSpawner.RemoveElementInDictionary(this.transform.position.x, this.transform.position.y);
            objectSpawner.MoveObjectsDown(this.transform.position.x, this.transform.position.y);
        }
    }
}
