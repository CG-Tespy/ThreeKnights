using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
	[System.Serializable]
	public class GridUnit {
		[SerializeField] public GameObject structure;
		[SerializeField][Range(0,1)] public float probability = 1.0f;
		//[HideInInspector]
		public float adjustedProbability;
		public bool update = false;
	}
	
	[SerializeField] public List<GridUnit> tiles;
	public GameObject specialTile;
	public GameObject backgroundTile;
	public Transform tilesDestination;
	public Transform boardDestination;
	
	public int numOfRows = 8;
	public int numOfColumns = 8;
	public float buildingFootprint = 10f;
	
	private float[,] mapgrid;
	private bool debug = true;

	// Start is called before the first frame update
	void Start()
	{
		// Quits if length is zero or if list of tiles is empty
		if (numOfRows < 1 || numOfColumns < 1 || tiles.Count < 1)
			return;
		
		//Define mapgrid to be size of map
		mapgrid = new float[numOfRows, numOfColumns];
		
		// Assigns parent to this object if not chosen
		if (tilesDestination == null)
			tilesDestination = this.transform;
		if (boardDestination == null)
			boardDestination = this.transform;
		
		// Generates probability range for each tile
		FixProbability();
		
		// Generate map data and store it into that specific x,y coordinate
		for (int i = 0; i < numOfRows; i++) {
			for (int j = 0; j < numOfColumns; j++) {
				mapgrid[i,j] = (float) Random.Range(0,100) / 10;
			}
		}
		
		//Generate voxel
		for (int i = 0; i < numOfRows; i++) {
			for (int j = 0; j < numOfColumns; j++) {
				// Sends 2D array and normal vector
				Spawn(mapgrid[i,j], new Vector3Int(i, j, 0));
			}
		}
		
		
	}

	/// Adjusts probability of each block's rng spawn
	private void FixProbability() {
		// actual total = 10f;
		float ratio = 0f;
		float pass = 0f;
		
		// Provides base ratio multiplier to reach range between 0..10
		foreach (GridUnit b in tiles) {
			ratio += b.probability;
		}
		ratio = 10f / ratio;

		// Creates upper range for spawning
		foreach (GridUnit b in tiles) {
			pass = b.adjustedProbability = pass + b.probability * ratio;
		}
	}
	
	/// Instantiates all of the grid units from a given result using probability table and normal vector
	private void Spawn(float result, Vector3Int coord) {
		bool done = false;
		Vector3 pos = new Vector3(coord.x * buildingFootprint, coord.y * buildingFootprint, coord.z * buildingFootprint);
		
		// First four tiles are completely random in 2x2 corner
		if (coord.x < 2 && coord.y < 2) {
			foreach (GridUnit b in tiles) {
				if (!done && result < b.adjustedProbability) {
					Instantiate(b.structure, pos, Quaternion.identity, tilesDestination);
					// Optionally add components or adjust scripts in objects here
					if (backgroundTile != null) Instantiate(backgroundTile, pos + new Vector3(0, 0, 1), Quaternion.identity, boardDestination);
					done = true;	
				}
			}
		} else {
			bool horiz = false;
			bool vert = true;
			bool far = false;
			bool far2 = false;
			bool close = false;
			bool close2 = false;
			bool success = false;
		
			foreach (GridUnit b in tiles) {
				if (!done) {
					if (coord.x >= 2) {
						close = (!close && result < b.adjustedProbability && mapgrid[coord.x - 1, coord.y] < b.adjustedProbability) ? false : true;
						far = (!far && result < b.adjustedProbability && mapgrid[coord.x - 2, coord.y] < b.adjustedProbability) ? false : true;
					} else horiz = true;
					
					if (coord.y >= 2) {
						close2 = (!close2 && result < b.adjustedProbability && mapgrid[coord.x, coord.y - 1] < b.adjustedProbability) ? false : true;
						far2 = (!far2 && result < b.adjustedProbability && mapgrid[coord.x, coord.y - 2] < b.adjustedProbability) ? false : true;
					} else vert = true;
					
					if (close && far) horiz = true;
					if (close2 && far2) vert = true;
					
					if (horiz && vert) success = true;
						else mapgrid[coord.x, coord.y] = b.adjustedProbability;
					
					if (success && result < b.adjustedProbability) {
					Instantiate(b.structure, pos, Quaternion.identity, tilesDestination);
						// Optionally add components or adjust scripts in objects here
						if (backgroundTile != null) Instantiate(backgroundTile, pos + new Vector3(0, 0, 1), Quaternion.identity, boardDestination);
						done = true;
					}
				}
			}
			
			if (!done) {
				Debug.Log("One more! " + coord + mapgrid[coord.x, coord.y]);
				mapgrid[coord.x, coord.y] = 0f;
				
				mapgrid[coord.x,coord.y] = -1f;
				Instantiate(specialTile, pos, Quaternion.identity, tilesDestination);
				// Optionally add components or adjust scripts in objects here
				if (backgroundTile != null) Instantiate(backgroundTile, pos + new Vector3(0, 0, 1), Quaternion.identity, boardDestination);
				done = true;
			}
		}
	}
}
