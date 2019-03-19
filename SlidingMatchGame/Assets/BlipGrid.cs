using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlipGrid : MonoBehaviour {
	int width = 8, height = 15;
	Blip[,] grid;

	float bottomOfGrid = 0, cellSize = 1; 

	void Start(){
		grid = new Blip[width,height];

	}

}
