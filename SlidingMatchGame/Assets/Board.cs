using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
	float nextCheck = -1, timeBetweenChecks = 0.5f;

	void Start () {
		nextCheck = Time.time + timeBetweenChecks;
	}
	
	// Update is called once per frame
	void Update () {
		CheckAllMatches();
	}
	void CheckAllMatches(){
		if (Time.time < nextCheck || Tile.draggingHappening)
			return;
		nextCheck = Time.time + timeBetweenChecks;
		foreach(Tile tile in FindObjectsOfType<Tile>()){
			tile.Match();
		}
	}
}
