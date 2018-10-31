using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour {


		// Vector3 mousePosition = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 10f);
		// Vector3 BackgroundPosition;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<Transform>().position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z+10);
	}
}
