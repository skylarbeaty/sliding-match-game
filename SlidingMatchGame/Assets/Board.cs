using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
	float nextCheck = -1, timeBetweenChecks = 0.5f;
	float nextDrop = -1, timeBetweenDrops = 2.0f, timeBetweenDropsFast = 1.0f;
	float nextStartSpawn = -1, timeBetweenStartSpawns = 0.1f;
	bool starting = true;
	int numStartTiles = 0, numStartTilesMax = 50;
	int width = 8, height = 15, totalTiles;
	float spawnY = 7.0f, spawnXMax = 3.5f;
	public LayerMask mask;
	public GameObject[] tiles;
	int fastThres = 15;
	void Start () {
		Tile.matchesInRow = 0;
		Tile.haveDragged = false;
		totalTiles = width * height;
		nextCheck = Time.time + timeBetweenChecks;
		nextDrop = Time.time + 10.0f;
		nextStartSpawn = Time.time + timeBetweenStartSpawns;
	}
	
	// Update is called once per frame
	void Update () {
		StartSpawns();
		CheckAllMatches();
		CheckDropBlock();
	}

	void StartSpawns(){
		//make sure we want to spawn starting tiles
		if (!starting || Time.time < nextStartSpawn)
			return;
		nextStartSpawn = Time.time + timeBetweenStartSpawns;

		if (numStartTiles >= numStartTilesMax){
			starting = false;
			return;
		}

		//find possible x spawns
		List<float> validX = new List<float>();
		for (int i = 0; i < width; ++i){//create list of all valid positions
			validX.Add((float) i * 0.5f);
		}
		//pick randomly between possible x's
		int index = Mathf.RoundToInt(Random.value * (validX.Count -1));
		Vector3 spawnPos = new Vector3(validX[index], 7.0f, 0.0f);

		//spwan a tile in
		index = Mathf.RoundToInt(Random.value * (tiles.Length -1));
		Instantiate(tiles[index], spawnPos, Quaternion.identity);
		numStartTiles++;
	}
	void CheckAllMatches(){
		if (Time.time < nextCheck || Tile.draggingHappening)
			return;
		nextCheck = Time.time + timeBetweenChecks;
		foreach(Tile tile in FindObjectsOfType<Tile>()){
			tile.Match();
		}
	}
	void CheckDropBlock(){
		if (Time.time < nextDrop || starting)
			return;
		int numTiles = FindObjectsOfType<Tile>().Length;
		if (numTiles < fastThres)
			nextDrop = Time.time + timeBetweenDropsFast;
		else 
			nextDrop = Time.time + timeBetweenDrops;
		if (numTiles >= totalTiles)
			return;
			
		//find which x's have room
		Vector2 point = new Vector2(spawnXMax / 2.0f,7.0f);
		Vector2 size = new Vector2(width / 2.0f, 0.4f);
		Collider2D[] colls = Physics2D.OverlapBoxAll(point, size, 0.0f, mask);
		List<float> validX = new List<float>();
		for (int i = 0; i < width; ++i){//create list of all valid positions
			validX.Add((float) i * 0.5f);
		}
		if (colls.Length != 0)//remove positions that have a tile there
			foreach(Collider2D coll in colls)
				validX.Remove(coll.transform.position.x);

		//pick randomly between possible x's
		int index = Mathf.RoundToInt(Random.value * (validX.Count -1));
		Vector3 spawnPos = new Vector3(validX[index], 7.0f, 0.0f);

		//spwan a tile in
		index = Mathf.RoundToInt(Random.value * (tiles.Length -1));
		Instantiate(tiles[index], spawnPos, Quaternion.identity);
	}
}
