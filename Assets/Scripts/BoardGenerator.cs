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
		public GameObject backgroundTile;
		
	}

	[System.Serializable]
	public class GridUnit 
	{
		public TileType type;
		[SerializeField]
		[Range(0,1)] 
		public float probability = 			1.0f;
		//[HideInInspector]
		public float adjustedProbability;
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
	List<TileType> tileTypes = 				new List<TileType>();
	public List<GridUnit> Tiles 			{ get { return tileSettings.tiles; } }
	public GameObject backgroundTile 		{ get { return tileSettings.backgroundTile; } }

	TileController[,] spawnedTiles;

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
		
		// SetupRandomTileNumbers(); Better to have the numbers generated as they're needed
		
		// Generates probability range for each tile
		FixProbability();

		RegisterTileTypes();

		spawnedTiles = 						new TileController[numOfColumns, numOfRows];
		GenerateBoard();
	}

	void Update()
	{
		// Debug
		/*
		if (true)
		{
			string thing = null;
		}
		*/
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
	
	void GenerateBoard()
	{
		// TODO: Fix whatever isn't making the algorithm work quite right
		
		// Spawn all the tiles randomly in the appropriate locations, while making sure there aren't too
		// many with the same type in a line.
		Vector3Int location = 					Vector3Int.zero;
		List<TileType> typesInRow = 			new List<TileType>(); // Helps with the latter goal.
		List<TileType> typesInColumn = 			new List<TileType>();
		const int maxInLine = 					2;
		TileController newTile = 				null;

		for (int column = 0; column < numOfColumns; column++) 
		{
			for (int row = 0; row < numOfRows; row++) 
			{
				location.Set(column, row, 1);
				newTile = 						SpawnTileAt(location);
				newTile.BoardPos.Set(column, row);
				spawnedTiles[column, row] = 	newTile;
				
				if (backgroundTile != null) 
					Instantiate(backgroundTile, location + Vector3.forward, Quaternion.identity, boardHolder);
				
				if (!typesInColumn.Contains(newTile.Type)) 
					// As we move through the rows, we're assessing the contents of one column.
					typesInColumn.Add(newTile.Type);

				if ( (row > 0 && row % maxInLine == 0) || row == numOfRows - 1) // Check for tile type redundancy
				{
					if (typesInColumn.Count == 1) // We found some!
					{
						RandomlyChangeTileType(newTile);
					}
					
					// For the next time we check for tile type redundancy in a column
					typesInColumn.Clear();
					typesInColumn.Add(newTile.Type);
				}

			}

			typesInColumn.Clear();

		}

	}

	TileController SpawnTileAt(Vector3Int boardCoord)
	{
		// Generate a random number to decide which tile should be spawned, to go with
		// how each tile type has its own probability of being assigned to a tile
		float randNum = 					Random.Range(0f, 100f) / 10f;
		Vector3 worldPos = 					new Vector3(boardCoord.x * buildingFootprint, 
											boardCoord.y * buildingFootprint, 
											boardCoord.z * buildingFootprint);

		foreach (GridUnit tile in Tiles)
		{
			if (randNum < tile.adjustedProbability)
			{
				TileController newTile = 	Instantiate(tileSettings.baseTilePrefab, worldPos, 
											Quaternion.identity, tileHolder);
				newTile.Type = 				tile.type;
				return newTile;
			}
		}

		return null; 
		// ^ Couldn't find a tile to spawn. Must be an issue with the choice of random number,
		// or the probability system.
	}

	void RandomlyChangeTileType(TileController tile)
	{
		TileType oldType = 		tile.Type;
		tileTypes.Remove(oldType); // To make sure we select a different type. We'll put it back in later.
		int randIndex = 		Random.Range(0, tileTypes.Count);
		tile.Type = 			tileTypes[randIndex];
		tileTypes.Add(oldType);
	}

	/// <summary>
	/// Makes sure there aren't too many of the same tile type repeated in a line. May not 
	/// work if there aren't enough tile types to draw from.
	/// </summary>
	void ReduceTileRedundancy(int maxRepeated = 2)
	{
		TileController currentTile = 				null;
		List<TileController> tilesToCheck = 		new List<TileController>();
		List<TileType> typesInTiles = 				new List<TileType>();
		int column = 0, row = 0;

		// Check vertically
		for (column = 0; column < numOfColumns; column++)
		{
			for (row = maxRepeated; row < numOfRows; row += maxRepeated)
			{
				currentTile = 						spawnedTiles[column, row];
				tilesToCheck.Add(currentTile);
				typesInTiles.Add(currentTile.Type);

				// Get the previous tiles
				for (int i = 1; i <= maxRepeated; i++)
				{
					TileController previousTile = 		spawnedTiles[column, row - i];
					tilesToCheck.Add(previousTile);
					typesInTiles.Add(previousTile.Type);
				}

				bool changeType = 						typesInTiles.Count > 1;

				if (changeType)
				{
					// Randomly select a different type
					TileType oldType = 					currentTile.Type;
					tileTypes.Remove(oldType); // So we don't select the same type
					int randIndex = 					Random.Range(0, tileTypes.Count);
					currentTile.Type = 					tileTypes[randIndex];
					tileTypes.Add(oldType);
				}

				// For the next iteration
				tilesToCheck.Clear();
				typesInTiles.Clear();

			}
			
		}
	}

	void RegisterTileTypes()
	{
		foreach (GridUnit tile in Tiles)
		{
			if (tileTypes.Contains(tile.type))
				continue;
			
			tileTypes.Add(tile.type);
		}
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


	void CreateAndSetUpTile(TileType tileType, Vector3 position, Quaternion rotation, Vector2Int boardPos)
	{
		TileController newTile = 				Instantiate(tileSettings.baseTilePrefab, position, 
												rotation, tileHolder);
		newTile.Type = 							tileType;
		newTile.BoardPos = 						boardPos;
	}
}
