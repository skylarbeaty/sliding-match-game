using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
	static bool draggingHappening = false;
	public enum tyleType {red, purple, yellow, green};
	public tyleType type = tyleType.red;
	bool falling = false;
	float fallSpeed = 5.0f;

	float cellSize = 0.5f;
	int width = 8, height = 15;//in number of blips 
	float leftEdge = 0;
	float rightEdge;
	float bottomEdge = 0.0f;
	float topEdge;

	public LayerMask mask;
	bool dragging = false, clicked = false;
	Vector3 mouseStart, dragStart, dragDir;
	List<Tile> dragGroup;
	
	void Start(){
		rightEdge = leftEdge + (width - 1) * cellSize;
		topEdge = bottomEdge + (height - 1) * cellSize;
		dragGroup = new List<Tile>();
	}
	void Update(){
		Move();
		CheckBellow();
	}

	void Move(){
		if (falling){
			float yPos = transform.position.y;
			if (yPos <= bottomEdge){
				falling = false;
				SnapToGrid();
			}
			else
				transform.position = transform.position + Vector3.down * fallSpeed * Time.deltaTime;
		}
	}

	void CheckBellow(){
		if (draggingHappening)//dont do drops while dragging
			return;
		Vector2 point = (Vector2) transform.position + Vector2.down * cellSize;
		Collider2D other = Physics2D.OverlapCircle(point, 0.2f, mask);
		if (other == null)
			falling = true;
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.CompareTag("Blip") && falling){
			// if (other.GetComponent<Tile>().falling){
			// 	falling = false;
			// 	//undo to stop overlap
			// 	transform.position = transform.position - Vector3.down * fallSpeed * Time.deltaTime;
			// 	return;
			// }
			falling = false;
			//adjust position to previous before snapping
			transform.position = transform.position - Vector3.down * fallSpeed * Time.deltaTime;
			SnapToGrid();
		}
	}

	void SnapToGrid(){
		float xPos, yPos;
		xPos = Mathf.Round(transform.position.x * 2.0f) / 2.0f;
		yPos = Mathf.Round(transform.position.y * 2.0f) / 2.0f;
		transform.position = new Vector3(xPos,yPos,transform.position.z);
		//check for outside grid
		if (transform.position.x < leftEdge){
			transform.position = transform.position + Vector3.right * width * cellSize;
		}
		if (transform.position.x > rightEdge){
			transform.position = transform.position - Vector3.right * width * cellSize;
		}
		if (transform.position.y < bottomEdge){
			transform.position = transform.position + Vector3.up * height * cellSize;
		}
		if (transform.position.y > topEdge){
			transform.position = transform.position - Vector3.up * height * cellSize;
		}
	}

	
	void OnMouseDown()
	{
		clicked = true;
		mouseStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		dragStart = transform.position;
	}
	void OnMouseUp()
	{
		print("MOUSE UP");
		if (!dragging)//on just click, check match
			Match();
		clicked = false;
		dragging = false;
		draggingHappening = false;
		foreach(Tile tile in dragGroup){
			tile.SnapToGrid();
		}
		dragGroup.Clear();
	}

	void OnMouseDrag()
	{
		Vector3 mouseCur = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3 mouseDelta = mouseCur - mouseStart;

		//start drag
		float dragThres = 0.3f;
		if (!dragging && (Mathf.Abs(mouseDelta.x) > dragThres || Mathf.Abs(mouseDelta.y) > dragThres)){
			//figure out drag dir
			if (Mathf.Abs(mouseDelta.x) > Mathf.Abs(mouseDelta.y))
				dragDir = Vector3.right;
			else
				dragDir = Vector3.up;
			dragging = true;
			draggingHappening = true;

			//get drag group
			Collider2D[] colls;
			Vector2 size = new Vector2(0.4f,0.4f);
			size += (Vector2) dragDir * width * cellSize * 2.0f; //make wide on corect side
			colls = Physics2D.OverlapBoxAll(transform.position, size, 0.0f, mask);
			foreach (Collider2D coll in colls){ //add all in line to group
				if (coll == null)
					continue;
				Tile tile = coll.gameObject.GetComponent<Tile>();
				tile.dragStart = tile.transform.position;
				dragGroup.Add(tile);
			}
		}
		//dragging step
		if (dragging){
			Vector3 dragDelta = new Vector3(mouseDelta.x * dragDir.x, mouseDelta.y * dragDir.y, 0.0f);
			foreach(Tile tile in dragGroup){
				//move the tile
				tile.transform.position = tile.dragStart + dragDelta;
				//warp around hor
				if (tile.transform.position.x < leftEdge - 0.49){
					tile.transform.position = tile.transform.position + Vector3.right * width * cellSize;
					tile.dragStart = tile.transform.position - dragDelta;
				}
				if (tile.transform.position.x > rightEdge + 0.49){
					tile.transform.position = tile.transform.position - Vector3.right * width * cellSize;
					tile.dragStart = tile.transform.position - dragDelta;
				}
				//warp around vert
				if (tile.transform.position.y < bottomEdge - 0.49){
					tile.transform.position = tile.transform.position + Vector3.up * height * cellSize;
					tile.dragStart = tile.transform.position - dragDelta;
				}
				if (tile.transform.position.y > topEdge + 0.49){
					tile.transform.position = tile.transform.position - Vector3.up * height * cellSize;
					tile.dragStart = tile.transform.position - dragDelta;
				}
			}
		}
	}

	void Match(){
		List<Tile> linkedTiles = new List<Tile>();
		linkedTiles.Add(this);
		
		//find any tiles to remove
		bool foundNone = false;
		while(!foundNone){
			foundNone = true;
			foreach(Tile tile in linkedTiles){
				//left
				Vector2 point = (Vector2) transform.position + Vector2.left * cellSize;
				Collider2D coll = Physics2D.OverlapCircle(point, 0.24f, mask);
				if (coll == null){
					Tile myTile = coll.GetComponent<Tile>();
					if (myTile.type != type && !linkedTiles.Contains(myTile)){
						linkedTiles.Add(myTile);
						foundNone = false;
					}
				}
				//right
				point = (Vector2) transform.position + Vector2.right * cellSize;
				coll = Physics2D.OverlapCircle(point, 0.24f, mask);
				if (coll == null){
					Tile myTile = coll.GetComponent<Tile>();
					if (myTile.type != type && !linkedTiles.Contains(myTile)){
						linkedTiles.Add(myTile);
						foundNone = false;
					}
				}
				//up
				point = (Vector2) transform.position + Vector2.up * cellSize;
				coll = Physics2D.OverlapCircle(point, 0.24f, mask);
				if (coll == null){
					Tile myTile = coll.GetComponent<Tile>();
					if (myTile.type != type && !linkedTiles.Contains(myTile)){
						linkedTiles.Add(myTile);
						foundNone = false;
					}
				}
				//down
				point = (Vector2) transform.position + Vector2.down * cellSize;
				coll = Physics2D.OverlapCircle(point, 0.24f, mask);
				if (coll == null){
					Tile myTile = coll.GetComponent<Tile>();
					if (myTile.type != type && !linkedTiles.Contains(myTile)){
						linkedTiles.Add(myTile);
						foundNone = false;
					}
				}
			}
		}
		if (linkedTiles.Count >= 3){
			foreach(Tile tile in linkedTiles)
				Destroy(tile.gameObject);
		}

	}

	// void Match(){
	// 	List<Tile> tilesLeft = new List<Tile>();
	// 	List<Tile> tilesRight = new List<Tile>();
	// 	List<Tile> tilesTop = new List<Tile>();
	// 	List<Tile> tilesBot = new List<Tile>();

	// 	//add tiles in each dir to lists
	// 	for(int i = 1; i < width; ++i){//left
	// 		Vector2 point = (Vector2) transform.position - Vector2.right * i * cellSize;
	// 		Collider2D coll = Physics2D.OverlapCircle(point, 0.24f, mask);
	// 		if (coll == null)
	// 			break;//end on blank space
	// 		Tile myTile = coll.GetComponent<Tile>();
	// 		if (myTile.type != type)
	// 			break;
	// 		tilesLeft.Add(myTile);
	// 	}
	// 	for(int i = 1; i < width; ++i){//right
	// 		Vector2 point = (Vector2) transform.position + Vector2.right * i * cellSize;
	// 		Collider2D coll = Physics2D.OverlapCircle(point, 0.24f, mask);
	// 		if (coll == null)
	// 			break;//end on blank space
	// 		Tile myTile = coll.GetComponent<Tile>();
	// 		if (myTile.type != type)
	// 			break;
	// 		tilesRight.Add(myTile);
	// 	}
	// 	for(int i = 1; i < height; ++i){//up
	// 		Vector2 point = (Vector2) transform.position + Vector2.up * i * cellSize;
	// 		Collider2D coll = Physics2D.OverlapCircle(point, 0.24f, mask);
	// 		if (coll == null)
	// 			break;//end on blank space
	// 		Tile myTile = coll.GetComponent<Tile>();
	// 		if (myTile.type != type)
	// 			break;
	// 		tilesTop.Add(myTile);
	// 	}
	// 	for(int i = 1; i < height; ++i){//down
	// 		Vector2 point = (Vector2) transform.position - Vector2.up * i * cellSize;
	// 		Collider2D coll = Physics2D.OverlapCircle(point, 0.24f, mask);
	// 		if (coll == null)
	// 			break;//end on blank space
	// 		Tile myTile = coll.GetComponent<Tile>();
	// 		if (myTile.type != type)
	// 			break;
	// 		tilesBot.Add(myTile);
	// 	}
	// 	//check tile lists for valid moves
	// 	bool destHor = false;
	// 	bool destVert = false;
	// 	int matchNum = 3;
		
	// 	print("Top: " + tilesTop.Count + " Bot: " + tilesBot.Count);

	// 	if (tilesLeft.Count + tilesRight.Count + 1 >= matchNum)
	// 		destHor = true;
	// 	if (tilesTop.Count + tilesBot.Count + 1 >= matchNum)
	// 		destVert = true;

	// 	//break tiles inside of valid moves
	// 	if (destHor){
	// 		foreach(Tile tile in tilesLeft)
	// 			Destroy(tile.gameObject);
	// 		foreach(Tile tile in tilesRight)
	// 			Destroy(tile.gameObject);
	// 	}
	// 	if (destVert){
	// 		foreach(Tile tile in tilesTop)
	// 			Destroy(tile.gameObject);
	// 		foreach(Tile tile in tilesBot)
	// 			Destroy(tile.gameObject);
	// 	}
	// 	if (destHor || destVert)
	// 		Destroy(gameObject);
	// }
}