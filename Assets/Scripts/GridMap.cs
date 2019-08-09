using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GridMap : MonoBehaviour
{
	[System.Serializable]
	public class GridSquare {
		[SerializeField] public int obj;
		[SerializeField][Range(0,5)] public int value = 1;
		public int x;
		public int y;
		public bool update = false;
	}
	
	[SerializeField] public GridSquare[,] mapgrid;
	public int[,] normalgrid;
	
	public LayerMask _mask; // please write a mask here; i think you need to bitshift for it to work
	public int numOfColumns = 8;
	public float distance = 45f;
	public float rayLength = 360f; // temporarily public
	public string _tag;
	
	private RaycastHit[] vision;
	private bool debug = true;
	private Vector3 pos;
	private List<GameObject> allFound;
	
	// Start is called before the first frame update
	void Start()
	{
		normalgrid = new int[numOfColumns, numOfColumns];
		mapgrid = new GridSquare[numOfColumns, numOfColumns];
		pos = this.transform.position;
		ScanForGridObjects();
		PrintGrid();
	}


	// Searches the world space for objects of interest to make the map grid
	private bool ScanForGridObjects() {
		allFound = new List<GameObject>();
	
		// Creates a column of data each iteration
		for (int i = 0; i < numOfColumns; i++) {
			// Positions the raycast for detection
			pos += new Vector3(distance, 0, 0);
			if (debug) Debug.DrawRay(pos, transform.TransformDirection(Vector3.up) * rayLength, Color.red, 50f);
			vision = Physics.RaycastAll(pos, transform.TransformDirection(Vector3.up), rayLength, _mask);
			
			// Checks all found objects for relevance: searches by tag
			for (int j = 0; j < vision.Length; j++) {
				RaycastHit found = vision[j];
				if (debug) Debug.Log("Raycast #" + i + " found this. (" + j + ") " + found.transform.name);
				if (found.transform.tag == _tag) {
					if (!allFound.Contains(found.transform.gameObject)) {
						allFound.Add(found.transform.gameObject);
						if (debug) Debug.Log(found.transform.gameObject.GetInstanceID());
						
					}
				}
			}
			
			// Sorts list by distance from ray origin
			/*allFound = SortNearestGameObjects(allFound, pos);
			
			// Fills the map grid with the new sorted list obtained
			FillColumn(allFound, i);*/
			
			allFound = allFound.OrderBy(x => Vector2.Distance(pos,x.transform.position)).ToList();
			for(int k = 0; k < allFound.Count; k++) {			
				Debug.Log("obj" + k + "  =  " + allFound[k].name + " : " + allFound[k].GetInstanceID());
			}

		}
		
		return true;
	}
	
	private bool FillColumn(List<GameObject> sortedList, int x_index) {
		bool expected = true;
		if (sortedList.Count > numOfColumns) {
			Debug.Log("Warning! List used is longer than available allocated space! Discarding extra contents...");
			expected = false;
		}
		if (sortedList.Count < numOfColumns) {
			Debug.Log("Expected larger list. Some space may by unallocated.");
			expected = false;
		}
			
		for (int i = 0; (i < numOfColumns) && (i < sortedList.Count); i++) {
			if (debug) Debug.Log(sortedList[i].GetInstanceID());
			if (debug) Debug.Log(x_index + ", " + i + " ; trying to add: " + sortedList[i].name + " : " + sortedList[i].GetInstanceID());

//  !!!		Having difficulty assigning to grid for unknown reasons???
			normalgrid[x_index, i] = sortedList[i].GetInstanceID();
			mapgrid[x_index, i].obj = sortedList[i].GetInstanceID();
			mapgrid[x_index, i].x = x_index;
			mapgrid[x_index, i].y = i;
		}
		
		return expected;
	}
	
	// Restructures new list from old to sort its contents
	private List<GameObject> SortNearestGameObjects(List<GameObject> oldList, Vector3 origin) {
		List<GameObject> newList = new List<GameObject>();
		GameObject closestGameObject;
		int len = oldList.Count;
		
		for (int i = 0; i < len; i++) {
			if (debug) Debug.Log(oldList[i].GetInstanceID());
			closestGameObject = GetNearestGameObject(oldList, origin);
			if (!allFound.Contains(closestGameObject)) {
				newList.Insert(i, closestGameObject);
				if (debug) Debug.Log(newList[i].GetInstanceID());
				oldList.Remove(closestGameObject);
			}
		}
		
		return newList;
	}
	
	// From a list of objects, retrieves the nearest object
	private GameObject GetNearestGameObject(List<GameObject> gameObjectsToConsider, Vector3 origin) {
		float smallestDistance = Mathf.Infinity;
		GameObject nearestGameObject = gameObjectsToConsider[0]; // temp
		int len = gameObjectsToConsider.Count;
		
		for (int i = 0; i < len; i++) {
			float distance = Vector3.Distance(origin, gameObjectsToConsider[i].transform.position);
			if (debug) Debug.Log(gameObjectsToConsider[i].GetInstanceID());
			
			if (distance < smallestDistance) {
				smallestDistance = distance;
				nearestGameObject = gameObjectsToConsider[i];
				
			}
		}
		
		return nearestGameObject;
	}
	
	// Prints the contents of the grid
	private void PrintGrid() {
		Debug.Log("Printing grid.");
		for (int i = 0; i < numOfColumns; i++) {
			for (int j = 0; j < numOfColumns; j++) {
				Debug.Log("(" + i + ", " + j + ") contains instanceID of " + mapgrid[i,j]);
			}
		}
	}
}
