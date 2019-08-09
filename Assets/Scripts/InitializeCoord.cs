using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeCoord : MonoBehaviour
{

	public Transform topright;
	public Transform bottomleft;
	public int numOfRows;
	public int numOfColumns;
	public int x_coord;
	public int y_coord;
	
	private float x_delta;
	private float y_delta;
	
	// Start is called before the first frame update
	void Start()
	{
		topright = GameObject.FindWithTag("TopRight").transform;
		bottomleft = GameObject.FindWithTag("BottomLeft").transform;
		
		x_delta = (topright.position.x - bottomleft.position.x) / numOfColumns;
		y_delta = (topright.position.y - bottomleft.position.y) / numOfRows;
		
		x_coord = (int) Mathf.Round(topright.position.x - bottomleft.position.x - transform.position.x / x_delta);
		y_coord = (int) Mathf.Round(topright.position.y - bottomleft.position.y - transform.position.y / y_delta);
	}
}
