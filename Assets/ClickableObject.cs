using System.Collections.Generic;
using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    public ObjectSpawner objectSpawner;

    void OnMouseDown()
    {
        if (!objectSpawner.ObjectMoving())
        {
            StartCoroutine(objectSpawner.RemoveElementInDictionary(this.transform.position.x, this.transform.position.y));
            StartCoroutine(objectSpawner.MoveObjectsDown(this.transform.position.x, this.transform.position.y));
        }
        
    }

    
}
