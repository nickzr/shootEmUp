using UnityEngine;
using System.Collections;

public class BGScroller : MonoBehaviour {
	public float scrollSpeed;
	public float tileSizeY;

	private Vector3 startPosition;

	// Use this for initialization
	void Start () {
		startPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		//Take the length of the tile (tileSizeZ), repeat it before we exceed the length 
		float newPosition = Mathf.Repeat (Time.time * scrollSpeed, tileSizeY);
		transform.position = startPosition + Vector3.down * newPosition;
	}
}
