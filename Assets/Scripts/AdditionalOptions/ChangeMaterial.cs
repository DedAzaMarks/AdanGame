using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaterial : MonoBehaviour {

	public Material material;
	Renderer renderer;
	// Use this for initialization
	void Start () {
		renderer = GetComponent<Renderer>();
		renderer.enabled = true;
		renderer.sharedMaterial = material;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
