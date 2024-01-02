using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] objectPrefabs; // Array of object prefabs to spawn
    public int numRows = 10;
    public int numColumns = 10;
    private bool isObjectMoving = false;

    // Dictionary to store objects based on their column number
    public Dictionary<int, Dictionary<float, GameObject>> columnObjectsDictionary = new Dictionary<int, Dictionary<float, GameObject>>();

    void Start()
    {
        SpawnObjectsOnTable();
        SortObjectsInColumns();
        // PrintObjectsInDictionary();
    }

    void SpawnObjectsOnTable()
    {
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numColumns; col++)
            {
                int columnKey = numColumns / 2 - col;
                int rowKey = numRows / 2 - row;

                // Randomly select an object prefab from the array
                GameObject selectedPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];

                // Calculate the position for the new object
                Vector3 spawnPosition = new Vector3(columnKey, rowKey, 0f);

                // Instantiate the object at the calculated position
                GameObject spawnedObject = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

                // Attach a script to the spawned object to handle clicks
                ClickableObject clickableObject = spawnedObject.AddComponent<ClickableObject>();
                clickableObject.objectSpawner = this;

                StoreObjectInDictionary(columnKey, rowKey, spawnedObject);
            }
        }
    }

    public void MoveObjectsDown(float column, float row)
    {
        if (!isObjectMoving)
        {
            isObjectMoving = true;
                // Get the column dictionary
            Dictionary<float, GameObject> columnObjects = columnObjectsDictionary[Mathf.RoundToInt(column)];

            // Iterate through rows above the specified row
            for (float currentRow = row + 1; currentRow <= numRows / 2; currentRow++)
            {
                // Check if there is an object in the current row
                if (columnObjects.ContainsKey(currentRow))
                {
                    // Get the object in the current row
                    GameObject currentObject = columnObjects[currentRow];

                    if (currentObject)
                    {
                        // Calculate the new position for the object
                        Vector3 newPosition = new Vector3(column, currentRow - 1, 0f);

                        // Move the object gradually (you can adjust the speed as needed)
                        StartCoroutine(MoveObjectGradually(currentObject, newPosition, 0.5f, column, row));

                        // Update the dictionary with the new row value
                        columnObjectsDictionary[Mathf.RoundToInt(column)].Remove(currentRow);
                        columnObjectsDictionary[Mathf.RoundToInt(column)][currentRow - 1] = currentObject;

                    }
                }
            }
        }
    }

    // Coroutine to move the object gradually
    private IEnumerator MoveObjectGradually(GameObject obj, Vector3 targetPosition, float duration, float column, float row)
    {
        float elapsedTime = 0f;
        Vector3 startingPos = obj.transform.position;

        while (elapsedTime < duration)
        {
            obj.transform.position = Vector3.Lerp(startingPos, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = targetPosition;
        isObjectMoving = false;


        CheckAndDestroy(column, row);
    }


    public void RemoveElementInDictionary(float column, float row)
    {
        Dictionary<float, GameObject> rowDictionary = columnObjectsDictionary[Mathf.RoundToInt(column)];
        rowDictionary.Remove(row);
    }

    private void CheckAndDestroy(float column, float row)
    {
        var limit = 3;
        Dictionary<float, GameObject> rowDictionary = new();

        if (columnObjectsDictionary[Mathf.RoundToInt(column)].ContainsKey(row))
        {
            foreach (var columnVar in columnObjectsDictionary)
            {
                rowDictionary[columnVar.Key] = columnVar.Value[row];
            }

            List<float> sameTypeIndex = GetRowNumberOfSameType(rowDictionary);
            for (int i = 0; i < sameTypeIndex.Count; i++)
            {
                Destroy(rowDictionary[sameTypeIndex[i]]);
            }
        }
    }


    private List<float> GetRowNumberOfSameType(Dictionary<float, GameObject> rowDictionary)
    {
        List<float> rowIndexes = new();

        var previousIndex = 100;
        var typesCount = 0;

        for (int rowIndex = -4; rowIndex < 6; rowIndex++)
        {
            if (rowDictionary.ContainsKey(rowIndex) && previousIndex != 100)
            {
                GameObject previousObject = rowDictionary[previousIndex];
                GameObject currentObject = rowDictionary[rowIndex];

                if (previousObject != null && currentObject != null && currentObject.CompareTag(previousObject.tag))
                {
                    typesCount++;
                }
                else
                {
                    if (typesCount > 1)
                    {
                        for (int i = rowIndex - 1; typesCount > 0; i--)
                        {
                            rowIndexes.Add(i);
                            typesCount--;
                        }
                    }

                    // Reset typesCount when a mismatch occurs
                    typesCount = 0;
                }
            }

            // Move this outside the loop to properly update previousIndex
            if (rowDictionary.ContainsKey(rowIndex)) 
                previousIndex = rowIndex;
        }

        return rowIndexes;
    }


    private void SortObjectsInColumns()
    {
        // Create a copy of the dictionary values to avoid modification during iteration
        var columnValuesCopy = new List<Dictionary<float, GameObject>>(columnObjectsDictionary.Values);

        // Iterate through each column in the copied list
        foreach (var columnObjects in columnValuesCopy)
        {
            // Sort objects in the column by their row positions (from top to bottom)
            var sortedRowDictionary = columnObjects.OrderBy(entry => entry.Key)
                                                .ToDictionary(entry => entry.Key, entry => entry.Value);
            columnObjectsDictionary[columnObjectsDictionary.First(x => x.Value == columnObjects).Key] = sortedRowDictionary;
        }
    }


    private void StoreObjectInDictionary(int column, float row, GameObject obj)
    {
        // Check if the column key exists in the dictionary
        if (!columnObjectsDictionary.ContainsKey(column))
        {
            // If not, add a new dictionary for the column
            columnObjectsDictionary[column] = new Dictionary<float, GameObject>();
        }

        // Add the object to the dictionary corresponding to its column and row
        columnObjectsDictionary[column][row] = obj;
    }

    private void PrintObjectsInDictionary()
    {
        Debug.Log("Objects in Dictionary:");
        foreach (var kvp in columnObjectsDictionary)
        {
            foreach (var rowEntry in kvp.Value)
            {
                Debug.Log("column: "+kvp.Key + "  row: " + rowEntry.Key);
            }
        }
    }

    public bool ObjectMoving(){
        return isObjectMoving;
    }
}
