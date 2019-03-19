using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
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
		rightEdge = leftEdge + width * cellSize;
		topEdge = bottomEdge + height * cellSize;
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
		foreach(Tile tile in dragGroup){
			tile.SnapToGrid();
		}
		dragGroup.Clear();
	}

	void OnMouseDrag()
	{
		Vector3 mouseCur = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3 mouseDelta = mouseCur - mouseStart;

		float dragThres = 0.3f;
		if (!dragging && (Mathf.Abs(mouseDelta.x) > dragThres || Mathf.Abs(mouseDelta.y) > dragThres)){
			print("DRAG TRIGGERED");
			//figure out drag dir
			if (Mathf.Abs(mouseDelta.x) > Mathf.Abs(mouseDelta.y))
				dragDir = Vector3.right;
			else
				dragDir = Vector3.up;
			dragging = true;

			//get drag group
			Collider2D[] colls;
			Vector2 size = new Vector2(0.4f,0.4f);
			size += (Vector2) dragDir * width * cellSize * 2.0f; //make wide on corect side
			colls = Physics2D.OverlapBoxAll(transform.position, size, 0.0f, mask);
			print("Colls length: " + colls.Length);
			foreach (Collider2D coll in colls)
			{
				if (coll == null)
					continue;
				Tile tile = coll.gameObject.GetComponent<Tile>();
				if (object.ReferenceEquals(tile,null)){
					print("NULL");
					continue;
				}
				tile.dragStart = tile.transform.position;
				dragGroup.Add(tile);
			}
		}

		if (dragging){
			Vector3 dragDelta = new Vector3(mouseDelta.x * dragDir.x, mouseDelta.y * dragDir.y, 0.0f);
			foreach(Tile tile in dragGroup){
				tile.transform.position = tile.dragStart + dragDelta;
			}
		}
	}
}