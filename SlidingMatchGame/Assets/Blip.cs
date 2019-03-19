using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blip : MonoBehaviour {
	public enum blipType {red, purple};
	public blipType type = blipType.red;
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
	List<Blip> dragGroup;
	
	void Start(){
		rightEdge = leftEdge + width * cellSize;
		topEdge = bottomEdge + height * cellSize;
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
		SnapToGrid();
	}

	void OnMouseDrag()
	{
		Vector3 mouseCur = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3 mouseDelta = mouseCur - mouseStart;

		float dragThres = 0.3f;
		if (!dragging && (mouseDelta.x > dragThres || mouseDelta.y > dragThres)){
			//figure out drag dir
			if (mouseDelta.x > mouseDelta.y)
				dragDir = Vector3.right;
			else
				dragDir = Vector3.up;
			dragging = true;

			//get drag group
			Collider2D[] colls;
			Vector2 size = new Vector2(0.4f,0.4f);
			size += (Vector2) dragDir * width * cellSize * 2.0f; //make wide on corect side
			colls = Physics2D.OverlapBoxAll(transform.position, size, 0.0f, mask);
			foreach (Collider2D coll in colls)
			{
				if (coll == null)
					continue;
				Blip tile = coll.gameObject.GetComponent<Blip>();
				if (tile == null)
					continue;
				tile.dragStart = transform.position;
				dragGroup.Add(tile);
			}
		}

		if (dragging){
			foreach(Blip tile in dragGroup){
				tile.transform.position = tile.dragStart + mouseDelta;
			}
		}
	}
}
