using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
	public static bool draggingHappening = false;
	public static bool haveDragged = false;
	public static int matchesInRow = 0;
	public enum tyleType {red, purple, yellow, green, blue, cyan, pink, orange};
	public tyleType type = tyleType.red;
	bool falling = false;
	float fallSpeed = 5.0f;

	float cellSize = 0.5f;
	int width = 8, height = 15;//in number of tiles 
	float leftEdge = 0;
	float rightEdge;
	float bottomEdge = 0.0f;
	float topEdge;

	public LayerMask mask;
	bool dragging = false, clicked = false, goingBack = false;
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
		GoBackStep();
	}

	public void OnEnd(){
		if (dragging)
			goingBack = true;
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
		matchesInRow = 0;
	}

	void OnMouseUp()
	{
		if(Scorer.ended)
			return;
		if (!dragging)//on just click, check match
			Match();
		clicked = false;
		if (dragging){
			//check to see if match has been made
			bool anyMatch = false;
			foreach(Tile tile in dragGroup){
				if (tile.Match(true)){
					anyMatch = true;
					break;
				}
			}
			//lock in if it has
			if (anyMatch){
				PopDragGroup();
			}
			//go back if it hasnt
			else{
				goingBack = true;
			}
		}
	}

	void PopDragGroup(){
		dragging = false;
		draggingHappening = false;
		foreach(Tile tile in dragGroup){
			tile.SnapToGrid();
		}
		foreach(Tile tile in dragGroup){
			Match();
		}
		dragGroup.Clear();
	}

	void GoBackStep(){
		if (!goingBack)
			return;
		Vector3 dragDelta = (transform.position - dragStart) * 0.9f;
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
		if (dragDelta.magnitude < cellSize / 2.0f){
			goingBack = false;
			PopDragGroup();
		}
	}

	void OnMouseDrag()
	{
		if(Scorer.ended)
			return;
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
			haveDragged = true;

			//get drag group
			Collider2D[] colls;
			Vector2 size = new Vector2(0.4f,0.4f);
			size += (Vector2) dragDir * height * cellSize * 2.0f; //make wide on corect side
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

	public void Match(){//if not checking just pass false
		Match(false);
	}
	bool Match(bool justCheck){
		List<Tile> linkedTiles = new List<Tile>();
		List<Tile> buffer = new List<Tile>();
		linkedTiles.Add(this);
		
		//find any tiles to remove
		bool foundNone = false;
		while(!foundNone){
			foundNone = true;
			foreach(Tile tile in linkedTiles){
				//left
				Vector2 point = (Vector2) tile.transform.position + Vector2.left * cellSize;
				Collider2D coll = Physics2D.OverlapCircle(point, 0.24f, mask);
				if (coll != null){
					Tile myTile = coll.GetComponent<Tile>();
					if (myTile.type == type && !linkedTiles.Contains(myTile)){
						buffer.Add(myTile);
						foundNone = false;
					}
				}
				//right
				point = (Vector2) tile.transform.position + Vector2.right * cellSize;
				coll = Physics2D.OverlapCircle(point, 0.24f, mask);
				if (coll != null){
					Tile myTile = coll.GetComponent<Tile>();
					if (myTile.type == type && !linkedTiles.Contains(myTile)){
						buffer.Add(myTile);
						foundNone = false;
					}
				}
				//up
				point = (Vector2) tile.transform.position + Vector2.up * cellSize;
				coll = Physics2D.OverlapCircle(point, 0.24f, mask);
				if (coll != null){
					Tile myTile = coll.GetComponent<Tile>();
					if (myTile.type == type && !linkedTiles.Contains(myTile)){
						buffer.Add(myTile);
						foundNone = false;
					}
				}
				//down
				point = (Vector2) tile.transform.position + Vector2.down * cellSize;
				coll = Physics2D.OverlapCircle(point, 0.24f, mask);
				if (coll != null){
					Tile myTile = coll.GetComponent<Tile>();
					if (myTile.type == type && !linkedTiles.Contains(myTile)){
						buffer.Add(myTile);
						foundNone = false;
					}
				}
			}
			foreach (Tile tile in buffer)
			{
				if (!linkedTiles.Contains(tile))
					linkedTiles.Add(tile);
			}
		}
		if (linkedTiles.Count >=3 && justCheck)
			return true;
		if (linkedTiles.Count >= 3){
			matchesInRow++;
			FindObjectOfType<Scorer>().AddScore(linkedTiles.Count, matchesInRow);
			foreach(Tile tile in linkedTiles)
				Destroy(tile.gameObject);
		}
	return false;
	}
}