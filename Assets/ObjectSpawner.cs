using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] objectPrefabs; // Array of object prefabs to spawn
    public int numRows = 10;
    public int numColumns = 10;
    private readonly float firstRow = -4f;
    private static bool isObjectMoving = false;
    private static bool stillChecking = false;

    // Dictionary to store objects based on their column number
    public Dictionary<float, Dictionary<float, GameObject>> columnObjectsDictionary = new Dictionary<float, Dictionary<float, GameObject>>();

    void Start()
    {
        SpawnObjectsOnTable();
        SortObjectsInColumns();
        // PrintObjectsInDictionary();
    }

    void Update()
    {
        SortObjectsInColumns();
        
        if (!stillChecking)
        {
            StartCoroutine(UpdateCoroutine());
        }
    }

    private IEnumerator UpdateCoroutine()
    {
        Task<Dictionary<float, List<float>>> findConsecutiveTask = FindConsecutiveOccurrences(columnObjectsDictionary);

        yield return new WaitUntil(() => findConsecutiveTask.IsCompleted);

        Dictionary<float, List<float>> result = findConsecutiveTask.Result;

        StartCoroutine(RemoveObject(result));
        StartCoroutine(AdjustColumnObjects(columnObjectsDictionary));
    }

    private IEnumerator RemoveObject(Dictionary<float, List<float>> result){
        foreach (var YAxisPair in result)
        {
            float YAxisValue = YAxisPair.Key;
            foreach (var XAxisValue in YAxisPair.Value)
            {
                StartCoroutine(RemoveElementInDictionary(XAxisValue, YAxisValue));
            }
        }
        yield return null;
    }

    private IEnumerator AdjustColumnObjects(Dictionary<float, Dictionary<float, GameObject>> columnObjects)
    {
        stillChecking = true;
        foreach (var X in columnObjects.Keys.ToList())
        {
            float previousY = columnObjects[X].Any() ? columnObjects[X].First().Key : 5f;

            foreach (var Y in columnObjects[X].Keys.ToList())
            {
                float currentY = Y;
                GameObject currentObject = columnObjects[X][Y];

                if (currentY > previousY + 1 && currentObject!=null)
                {
                    Vector3 newPosition = new(X, previousY + 1, 0f);
                    StartCoroutine(MoveObjectGradually(currentObject, newPosition, 0.5f));

                    // Update the dictionary with the new row value
                    columnObjects[X].Remove(currentY);
                    columnObjects[X][previousY + 1] = currentObject;
                }
                else if (currentObject==null)
                {
                    columnObjects[X].Remove(currentY);
                }


                if (previousY != firstRow && !columnObjects[X].Keys.ToList().Contains(firstRow))
                {
                    Vector3 newPosition = new(X, firstRow, 0f);
                    StartCoroutine(MoveObjectGradually(currentObject, newPosition, 0.5f));

                    // Update the dictionary with the new row value
                    columnObjects[X].Remove(currentY);
                    columnObjects[X][firstRow] = currentObject;
                }

                previousY = Y;
            }
        }
        stillChecking = false;
        yield return null;
    }



    void SpawnObjectsOnTable()
    {
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numColumns; col++)
            {
                int columnKeyX = numColumns / 2 - col;
                int rowKeyY = numRows / 2 - row;

                // Randomly select an object prefab from the array
                GameObject selectedPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];

                // Calculate the position for the new object
                Vector3 spawnPosition = new Vector3(columnKeyX, rowKeyY, 0f);

                // Instantiate the object at the calculated position
                GameObject spawnedObject = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

                // Attach a script to the spawned object to handle clicks
                ClickableObject clickableObject = spawnedObject.AddComponent<ClickableObject>();
                clickableObject.objectSpawner = this;

                StoreObjectInDictionary(columnKeyX, rowKeyY, spawnedObject);
            }
        }
    }

    public IEnumerator MoveObjectsDown(float column, float row)
    {
        Dictionary<float, GameObject> columnObjects = columnObjectsDictionary[Mathf.RoundToInt(column)];
        
        if (!isObjectMoving && columnObjects.Last().Key!=row-1)
        {
            isObjectMoving = true;

            var lastKeyValuePairByKey = columnObjects.OrderBy(kv => kv.Key).Last();

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
                        StartCoroutine(MoveObjectGradually(currentObject, newPosition, 0.5f));

                        // Update the dictionary with the new row value
                        columnObjectsDictionary[Mathf.RoundToInt(column)].Remove(currentRow);
                        columnObjectsDictionary[Mathf.RoundToInt(column)][currentRow - 1] = currentObject;
                    }
                }
            }
        }
        
        yield return null;
    }


    private IEnumerator MoveObjectGradually(GameObject obj, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startingPos = obj.transform.position;

        while (elapsedTime < duration)
        {
            if (obj!=null)
            {
                obj.transform.position = Vector3.Lerp(startingPos, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
            }
            yield return null;
        }

        obj.transform.position = targetPosition;
        isObjectMoving = false;
    }


    public IEnumerator RemoveElementInDictionary(float column, float row)
    {
        Dictionary<float, GameObject> rowDictionary = columnObjectsDictionary[Mathf.RoundToInt(column)];
        if (rowDictionary.ContainsKey(row) && rowDictionary[row]!=null)
        {
            Destroy(rowDictionary[row]);
        }
        rowDictionary.Remove(row);
        
        yield return null;
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


    private void StoreObjectInDictionary(float columnX, float rowY, GameObject obj)
    {
        // Check if the column key exists in the dictionary
        if (!columnObjectsDictionary.ContainsKey(columnX))
        {
            // If not, add a new dictionary for the column
            columnObjectsDictionary[columnX] = new Dictionary<float, GameObject>();
        }

        // Add the object to the dictionary corresponding to its column and row
        columnObjectsDictionary[columnX][rowY] = obj;
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

    private Task<Dictionary<float, List<float>>> FindConsecutiveOccurrences(Dictionary<float, Dictionary<float, GameObject>> table)
    {
        Dictionary<float, List<float>> resultDictionary = new();

        Dictionary<float, Dictionary<float, GameObject>> rawsDictionary = RowsToColumns(table);

        foreach (var columnDicY in rawsDictionary)
        {
            float YAxis = columnDicY.Key;
            HashSet<float> uniqueRows = new();

            foreach (var rowDicX in columnDicY.Value)
            {
                for (float XAxis = rowDicX.Key - 1; XAxis <= rowDicX.Key + 1; XAxis++)
                {
                    if (columnDicY.Value.ContainsKey(XAxis - 1) && columnDicY.Value.ContainsKey(XAxis + 1) && columnDicY.Value.ContainsKey(XAxis)){
                        if(columnDicY.Value[XAxis - 1].CompareTag(columnDicY.Value[XAxis].tag) && columnDicY.Value[XAxis].CompareTag(columnDicY.Value[XAxis + 1].tag))
                        {

                            uniqueRows.Add(XAxis - 1);
                            uniqueRows.Add(XAxis);
                            uniqueRows.Add(XAxis + 1);
                        }
                    }
                }
            }

            resultDictionary.Add(YAxis, uniqueRows.ToList());
        }
        
        Task<Dictionary<float, List<float>>> completedTask = Task.FromResult(resultDictionary);

        return completedTask;

    }




    static Dictionary<float, Dictionary<float, GameObject>> RowsToColumns(Dictionary<float, Dictionary<float, GameObject>> table)
    {
        Dictionary<float, Dictionary<float, GameObject>> result = new Dictionary<float, Dictionary<float, GameObject>>();

        foreach (var columnX in table)
        {
            foreach (var rowY in columnX.Value)
            {
                float rowNumY = rowY.Key;
                float columnNumX = columnX.Key;
                GameObject gameObject = rowY.Value;

                if (!result.ContainsKey(rowNumY))
                {
                    // If the result dictionary does not contain the row number, add it
                    result[rowNumY] = new Dictionary<float, GameObject>();
                }

                // Add the column number and GameObject to the result dictionary
                result[rowNumY].Add(columnNumX, gameObject);
            }
        }

        return result;
    }
}
