using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
	[System.Serializable]
	public class TileSettings
	{
		public List<GridUnit> tiles;
		public TileController baseTilePrefab;
		public GameObject specialTile;
		public TileType specialTileType;
		public GameObject backgroundTile;
		
	}

	[System.Serializable]
	public class GridUnit 
	{
		public TileType type;
		public GameObject prefab;
		[SerializeField]
		[Range(0,1)] 
		public float probability = 			1.0f;
		//[HideInInspector]
		public float adjustedProbability;
		public bool update = 				false;
	}
	
	public TileSettings tileSettings;
	public Transform tileHolder;
	public Transform boardHolder;
	
	public int numOfRows = 					8;
	public int numOfColumns = 				8;
	public float buildingFootprint = 		10f;
	
	// Contains random numbers for each grid coordinate, deciding which tiles are spawned where.
	private float[,] mapGrid;
	private bool debug = 					true;
	public List<GridUnit> Tiles 			{ get { return tileSettings.tiles; } }
	public GameObject SpecialTile 			{ get { return tileSettings.specialTile; } }
	public TileType SpecialTileType 		{ get { return tileSettings.specialTileType; } }
	public GameObject backgroundTile 		{ get { return tileSettings.backgroundTile; } }

	// Best generate in Awake, before the board-controller is set to register the tiles
	void Awake()
	{
		// Quits if board dimensions are zero, or if there are no tiles set to populate it with
		if (numOfRows < 1 || numOfColumns < 1 || Tiles.Count < 1)
			return;
		
		// Assigns parent to this object if not chosen
		if (tileHolder == null)
			tileHolder = 					this.transform;
		if (boardHolder == null)
			boardHolder = 					this.transform;

		SetupRandomTileNumbers();
		
		// Generates probability range for each tile
		FixProbability();
		
		// Generate voxel
		for (int i = 0; i < numOfRows; i++) 
		{
			for (int j = 0; j < numOfColumns; j++) 
			{
				// Sends 2D array and normal vector 
				// CG-Tespy: Why call it a normal vector? This doesn't seem to involve the physics system;
				// it seems to just be the grid coordinate.
				Spawn(mapGrid[i, j], new Vector3Int(i, j, 0));
			}
		}
		
	}

	// Adjusts probability of each block's rng spawn, affecting the prefab a block is spawned with
	private void FixProbability() 
	{
		// actual total = 10f;
		float ratio = 						0f;
		float pass = 						0f;
		
		// Provides base ratio multiplier to reach range between 0..10
		foreach (GridUnit tile in Tiles) 
		{
			ratio += 						tile.probability;
		}
		ratio = 							10f / ratio;

		// Creates upper range for spawning
		foreach (GridUnit tile in Tiles) 
		{
			pass = 							tile.adjustedProbability = pass + tile.probability * ratio;
		}

	}
	
	// Instantiates all of the grid units from a given result using probability table and normal vector
	private void Spawn(float randomNumber, Vector3Int boardCoord) 
	{
		bool done = 					false;
		Vector3 pos = 					new Vector3(boardCoord.x * buildingFootprint, 
										boardCoord.y * buildingFootprint, 
										boardCoord.z * buildingFootprint);
		Vector2Int boardCoord2D = 		new Vector2Int(boardCoord.x, boardCoord.y);
		
		// First four tiles are completely random in a 2x2 corner
		if (boardCoord.x < 2 && boardCoord.y < 2) 
		{
			HandleFirstFourTiles(randomNumber, boardCoord, pos);
		} 
		else 
		{
			// CG-Tespy: Been having a hard time understanding this part of the algorithm...
			bool horiz = 							false;
			bool vert = 							true;
			bool far = 								false;
			bool far2 = 							false;
			bool close = 							false;
			bool close2 = 							false;
			bool success = 							false;
		
			foreach (GridUnit tile in Tiles) 
			{
				if (!done) 
				{
					// CG-Tespy: The use of tertiary operators in this part of the algorithm really hurt 
					// the code's readability.
					if (boardCoord.x >= 2) 
					{
						close = 
							(!close && randomNumber < tile.adjustedProbability && 
							mapGrid[boardCoord.x - 1, boardCoord.y] < tile.adjustedProbability) ? false : true;
						far = 
							(!far && randomNumber < tile.adjustedProbability && 
							mapGrid[boardCoord.x - 2, boardCoord.y] < tile.adjustedProbability) ? false : true;
					} 
					else 
						horiz = 					true;
					
					if (boardCoord.y >= 2) 
					{
						close2 = 
							(!close2 && randomNumber < tile.adjustedProbability && 
							mapGrid[boardCoord.x, boardCoord.y - 1] < tile.adjustedProbability) ? false : true;
						far2 = 
							(!far2 && randomNumber < tile.adjustedProbability && 
							mapGrid[boardCoord.x, boardCoord.y - 2] < tile.adjustedProbability) ? false : true;
					} 
					else vert = 					true;
					
					if (close && far) horiz = 		true;
					if (close2 && far2) vert = 		true;
					
					if (horiz && vert) success = 	true;
						else mapGrid[boardCoord.x, boardCoord.y] = 	tile.adjustedProbability;
					
					if (success && randomNumber < tile.adjustedProbability) 
					{
						//CreateAndSetUpTile(tile.prefab, pos, Quaternion.identity, boardCoord2D);
						CreateAndSetUpTile(tile.type, pos, Quaternion.identity, boardCoord2D);

						// Optionally add components or adjust scripts in objects here
						if (backgroundTile != null) 
							Instantiate(backgroundTile, pos + new Vector3(0, 0, 1), Quaternion.identity, boardHolder);
						done = 						true;
					}
				}
			}
			
			if (!done) 
			{
				Debug.Log("One more! " + boardCoord + mapGrid[boardCoord.x, boardCoord.y]);
				//mapGrid[boardCoord.x, boardCoord.y] = 			0f;
				mapGrid[boardCoord.x, boardCoord.y] = 			-1f;

				CreateAndSetUpTile(SpecialTileType, pos, Quaternion.identity, boardCoord2D);

				// Optionally add components or adjust scripts in objects here
				if (backgroundTile != null)
					Instantiate(backgroundTile, pos + Vector3.forward, Quaternion.identity, boardHolder);
				done = 											true;
			}
		}
	}

	void SetupRandomTileNumbers()
	{
		mapGrid = 							new float[numOfRows, numOfColumns];

		// Set up the map grid, for the tile randomization.
		for (int i = 0; i < numOfRows; i++) 
			for (int j = 0; j < numOfColumns; j++) 
				mapGrid[i, j] = 			(float) Random.Range(0, 100) / 10f;
	}

	void HandleFirstFourTiles(float randomNumber, Vector3Int boardCoord, Vector3 pos)
	{
		Vector2Int boardCoord2D = 				new Vector2Int(boardCoord.x, boardCoord.y);

		foreach (GridUnit tile in Tiles) 
		{
			if (randomNumber < tile.adjustedProbability) 
			{
				//CreateAndSetUpTile(tile.prefab, pos, Quaternion.identity, boardCoord2D);
				CreateAndSetUpTile(tile.type, pos, Quaternion.identity, boardCoord2D);

				// Optionally add components or adjust scripts in objects here
				if (backgroundTile != null) 
					Instantiate(backgroundTile, pos + Vector3.forward, Quaternion.identity, boardHolder);
				return;
			}
		}
	}

	void CreateAndSetUpTile(GameObject prefab, Vector3 position, Quaternion rotation, Vector2Int boardPos)
	{
		GameObject tileGO = 					Instantiate(prefab, position, rotation, tileHolder);
		TileController tileCont = 				tileGO.GetComponent<TileController>();
		tileCont.BoardPos = 					boardPos;
	}

	void CreateAndSetUpTile(TileType tileType, Vector3 position, Quaternion rotation, Vector2Int boardPos)
	{
		TileController newTile = 				Instantiate(tileSettings.baseTilePrefab, position, 
												rotation, tileHolder);
		newTile.Type = 							tileType;
		newTile.BoardPos = 						boardPos;
	}
}
