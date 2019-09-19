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
		[SerializeField] TileType type;
		[SerializeField]
		[Range(0,1)] 
		public float probability = 			1.0f;
		//[HideInInspector]
		public float adjustedProbability;

		public TileType Type
		{
			get { return type; }
		}
	}
	
	public TileSettings tileSettings;
	public Transform tileHolder;
	public Transform bgTileHolder;
	public Transform boardHolder;
	
	public int numOfRows = 					8;
	public int numOfColumns = 				8;
	public float buildingFootprint = 		10f;
	private float adjustedBFootprint; // building foor print adjusted for the board holder's scale.
	
	// Incorporate fungus value in this variable as an pre-processing directive
	// #ifdef Fungus exists, use TileBoardVals.minAmountForMatch - 1 #else maxInLine = 2;
	public int maxInLine = 				2;
	
	// Contains information for each grid coordinate, such as the tile type.
	public TileController[,] spawnedTiles;
	
	private bool debug = 					true;
	List<TileType> tileTypes = 				new List<TileType>();
	public List<GridUnit> Tiles 			{ get { return tileSettings.tiles; } }
	public GameObject backgroundTile 		{ get { return tileSettings.backgroundTile; } }
	private enum Direction:int { Left, Down, Right, Up, All };


	// Best generate in Awake, before the board-controller is set to register the tiles
	void Awake() {
		Initialize();
	}

	//void Update() {	/* Debug */	if (true) { string thing = null; } }

	bool Initialize() 
	{
		// Quits if board dimensions are zero, or if there are no tiles set to populate it with
		if (numOfRows < 1 || numOfColumns < 1 || Tiles.Count < 1)
			return false;
		
		// Assigns parent to this object if not chosen
		if (tileHolder == null)
			tileHolder = 					this.transform;
		if (boardHolder == null)
			boardHolder = 					this.transform;

		adjustedBFootprint = 				(boardHolder.localScale.x + boardHolder.localScale.y) / 2f;
		
		// Generates probability range for each tile
		FixProbability();
		RegisterTileTypes();

		spawnedTiles = 						new TileController[numOfColumns, numOfRows];
		
		return true;
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
	
	public TileController[,] GenerateBoard(TileBoardController boardController)
	{
		// Spawn all the tiles randomly in the appropriate locations, while making sure there aren't too
		// many with the same type in a line.
		Vector3Int location = 					Vector3Int.zero;
		TileController newTile = 				null;

		for (int column = 0; column < numOfColumns; column++) 
		{
			for (int row = 0; row < numOfRows; row++) 
			{
				location.Set(column, row, 0);
				newTile = 						SpawnTileAt(location, boardController);
				newTile.BoardPos = 				new Vector2Int(column, row);
				spawnedTiles[column, row] = 	newTile;
				
				if (backgroundTile != null)
					SpawnBackgroundTile(location);
			}
		}

		return spawnedTiles;
	}

	TileController SpawnTileAt(Vector3Int boardCoord, TileBoardController boardTileIsFor)
	{
		// Generate a random number to decide which tile should be spawned, to go with
		// how each tile type has its own probability of being assigned to a tile
		float randNum = 					Random.Range(0f, 100f) / 10f;
		Vector3 localPos = 					(Vector3)boardCoord * adjustedBFootprint;

		bool done = false;
		while (!done) {
			foreach (GridUnit tile in Tiles)
			{
				if (randNum < tile.adjustedProbability)
				{
					/* Although functional, this is wasteful on resources */ 
						//if (RedundancyCheckTrue(boardCoord, Direction.All, maxInLine, tile)) { continue; }
					
					// Checks for horizontal redundancy
					if (RedundancyCheckTrue(boardCoord, Direction.Left, maxInLine, tile)) {
						continue;
					}
					
					// Checks for vertical redundancy
					if (RedundancyCheckTrue(boardCoord, Direction.Down, maxInLine, tile)) {
						continue;
					}
					
					// If not redundant, creates a new tile
					TileController newTile = 	Instantiate(tileSettings.baseTilePrefab, Vector3.zero, 
												Quaternion.identity);
					newTile.transform.SetParent(tileHolder);
					newTile.transform.localPosition = localPos;
					newTile.Type = 				tile.Type;
					newTile.Board = boardTileIsFor;
					done = true;
					return newTile;
				}
			}
			// If end of tile list is reached, start from beginning of list
			if (randNum >= 10) 
				randNum -= 10;
			
			// Failsafe for corner cases regarding bad initialization
			if (tileTypes.Count - 1 < Tiles.Count)
				done = true;
		}

		// If somehow everything fails from empty list of Tiles
		TileController newAltTile =	Instantiate(tileSettings.baseTilePrefab, Vector3.zero, 
									Quaternion.identity, tileHolder);
		newAltTile.transform.SetParent(tileHolder);
		newAltTile.transform.localPosition = localPos;
		newAltTile.Type = 				Tiles[0].Type;
		newAltTile.Board = boardTileIsFor;
		return newAltTile;
	}
	
	/// <summary>
	/// Starts the redundancy check process but with a temporary data structure GridUnit appended
	/// Use this to check for redundancy *before* generating a tile
	/// To check redundancy on an already existing tile, use the alternate call with 3 parameters
	/// </summary>
	bool RedundancyCheckTrue(Vector3Int oldBoardCoord, Direction dir, int depth, GridUnit tempTile) 
	{
		// Needs to check after current tile
		if (depth < 1)
			return false;
			
		Vector3Int newBoardCoord = oldBoardCoord;
		
		// Only checks one tile ahead before transitioning to main function
		switch((Direction) dir) {
			case Direction.Left: {
				newBoardCoord += Vector3Int.left;
				if (oldBoardCoord.x < depth) {
					return false;
				}
				if (spawnedTiles[oldBoardCoord.x - 1, oldBoardCoord.y].Type != tempTile.Type) {
					return false;
				} else return RedundancyCheckTrue(newBoardCoord, dir, depth - 1);
			}
			case Direction.Down: {
				newBoardCoord += Vector3Int.down;
				if (oldBoardCoord.y < depth) {
					return false;
				}
				if (spawnedTiles[oldBoardCoord.x, oldBoardCoord.y - 1].Type != tempTile.Type) {
					return false;
				} else return RedundancyCheckTrue(newBoardCoord, dir, depth - 1);
			}
			case Direction.Right: {
				newBoardCoord += Vector3Int.right;
				if (numOfColumns - oldBoardCoord.x - 1 < depth) {
					return false;
				}
				if (spawnedTiles[oldBoardCoord.x + 1, oldBoardCoord.y]?.Type != tempTile.Type) {
					return false;
				} else return RedundancyCheckTrue(newBoardCoord, dir, depth - 1);
			}
			case Direction.Up: {
				newBoardCoord += Vector3Int.up;
				if (numOfRows - oldBoardCoord.y - 1 < depth) {
					return false;
				}
				if (spawnedTiles[oldBoardCoord.x, oldBoardCoord.y + 1]?.Type != tempTile.Type) {
					return false;
				} else return RedundancyCheckTrue(newBoardCoord, dir, depth - 1);
			}
			case Direction.All: {
				// Please do not pre-generate bidirectionally or else this will not observe central pivots
				if (RedundancyCheckTrue(oldBoardCoord, Direction.Left, depth, tempTile)
					|| RedundancyCheckTrue(oldBoardCoord, Direction.Down, depth, tempTile)
					|| RedundancyCheckTrue(oldBoardCoord, Direction.Right, depth, tempTile)
					|| RedundancyCheckTrue(oldBoardCoord, Direction.Up, depth, tempTile))
					return true;
				else return false;
			}
			default: return false;
		}
	}

	
	/// <summary>
	/// Recursively checks in a specific direction for matches up to a specified depth
	/// If there are not enough tiles in a direction, it is not redundant
	/// If at least one tile in the specified depth is different, it is not redundant
	/// If all of the tiles in the specified depth are the same, it is redundant
	/// </summary>
	bool RedundancyCheckTrue(Vector3Int boardCoord, Direction dir, int depth) {
		// Needs to check after current tile
		if (depth < 1) {
			return false;
		}
		
		// Differentiates the direction on the grid and then searches for a match among all of the tiles
		switch((Direction) dir) {
			case Direction.Left: {
				if (boardCoord.x < depth) {
					return false;
				}
				for (int i = 1; i <= depth; i++) {
					if (spawnedTiles[boardCoord.x, boardCoord.y].Type == spawnedTiles[boardCoord.x - i, boardCoord.y].Type)
						return true;
				}
				return false;
			}
			case Direction.Down: {
				if (boardCoord.y < depth) {
					return false;
				}
				for (int i = 1; i <= depth; i++) {
					if (spawnedTiles[boardCoord.x, boardCoord.y].Type == spawnedTiles[boardCoord.x, boardCoord.y - i].Type)
						return true;
				}
				return false;
			}
			case Direction.Right: {
				if (numOfColumns - boardCoord.x - 1 < depth) {
					return false;
				}
				for (int i = 1; i <= depth; i++) {
					if (spawnedTiles[boardCoord.x, boardCoord.y].Type == spawnedTiles[boardCoord.x + i, boardCoord.y].Type)
						return true;
				}
				return false;
			}
			case Direction.Up: {
				if (numOfRows - boardCoord.y - 1 < depth) {
					return false;
				}
				for (int i = 1; i <= depth; i++) {
					if (spawnedTiles[boardCoord.x, boardCoord.y].Type == spawnedTiles[boardCoord.x, boardCoord.y + i].Type)
						return true;
				}
				return false;
			}
			case Direction.All: {
				// Checks in all directions for conflicts, post generation only
				if (RedundancyCheckTrue(boardCoord, Direction.Left, maxInLine))
					return true;
				if (RedundancyCheckTrue(boardCoord, Direction.Down, maxInLine))
					return true;
				if (RedundancyCheckTrue(boardCoord, Direction.Right, maxInLine))
					return true;
				if (RedundancyCheckTrue(boardCoord, Direction.Up, maxInLine))
					return true;
				
				int bools_count = 0;
				// Horizontal sliding checks
				for (int i = 1; i < maxInLine; i++) {
					if (RedundancyCheckTrue(boardCoord, Direction.Left, i))
						bools_count++;
					if (RedundancyCheckTrue(boardCoord, Direction.Right, maxInLine - i))
						bools_count++;
					
				}
				if (bools_count >= maxInLine)
					return true;
				else bools_count = 0;
				
				// Vertical sliding checks
				for (int i = 1; i < maxInLine; i++) {
					if (RedundancyCheckTrue(boardCoord, Direction.Down, i))
						bools_count++;
					if (RedundancyCheckTrue(boardCoord, Direction.Up, maxInLine - i))
						bools_count++;
				
				}
				if (bools_count >= maxInLine)
					return true;
				else bools_count = 0;

				// Passed all checks
				return false;
			}
			default: return false;
		}
	}

	void SpawnBackgroundTile(Vector3Int boardCoord)
	{
		Vector3 localPos = 					(Vector3)boardCoord * adjustedBFootprint + Vector3.forward;
		GameObject bgTile = 				Instantiate(backgroundTile, Vector3.zero, Quaternion.identity);
		// Make sure positioning and scale fit the parent
		bgTile.transform.SetParent(bgTileHolder);
		bgTile.transform.localPosition = localPos;

	}

	void RandomlyChangeTileType(Vector3Int boardCoord)
	{
		TileController tile = spawnedTiles[boardCoord.x, boardCoord.y];
		TileType oldType = 		tile.Type;
		tileTypes.Remove(oldType); // To make sure we select a different type. We'll put it back in later.
		int randIndex = 		Random.Range(0, tileTypes.Count);
		tile.Type = 			tileTypes[randIndex];
		tileTypes.Add(oldType);
		
		if (RedundancyCheckTrue(boardCoord, Direction.All, maxInLine)) {
			RandomlyChangeTileType(boardCoord);
		}
	}

	/// <summary>
	/// Makes sure there aren't too many of the same tile type repeated in a line.
	/// </summary>
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
			if (tileTypes.Contains(tile.Type))
				continue;
			
			tileTypes.Add(tile.Type);
		}
	}

}
