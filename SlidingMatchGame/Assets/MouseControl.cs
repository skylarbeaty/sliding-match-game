using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour {
	public LayerMask mask;
	void OnMouseDown()
	{
		print("MOUSE DOWN");
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit2D hit = Physics2D.GetRayIntersection(ray,Mathf.Infinity,mask);
		
		if (hit.collider != null){
			print("CLICKED A BLIP");
		}
	}
}
