using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
	static bool draggingHappening = false;
	public enum tyleType {red, purple};
	public tyleType type = tyleType.red;
	bool falling = true;
	float fallSpeed = 3.0f;

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
		if(other.CompareTag("Blip")){
			falling = false;
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
		print("MOUSE DOWN");
		clicked = true;
		mouseStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		dragStart = transform.position;
	}
	void OnMouseUp()
	{
		print("MOUSE UP");
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
			print("DRAG TRIGGERED");
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
			print("Colls length: " + colls.Length);
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
}