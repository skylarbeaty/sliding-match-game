﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scorer : MonoBehaviour {
	public Text timerText;
	public Text scoreText;
	float timer = 60.0f;
	int score = 0;
	int pointsPerTiles = 3, pointsMult = 1;
	
	void Update () {
		timer -= Time.deltaTime;
		if (timer <= 0.0f){
			timer = 0.0f;
			print("GAME OVER");
		}
		timerText.text = Mathf.Round(timer).ToString();
	}

	public void AddScore(int numTiles, int numInRow){
		int scoreAdd = numTiles * pointsPerTiles;
		if (numInRow > 1 && Tile.haveDragged) //avoid huge score to start off
			scoreAdd *= numInRow * pointsMult;
		score += scoreAdd;
		scoreText.text = score.ToString();
	}
}
