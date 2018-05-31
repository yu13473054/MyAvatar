using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour {

	void Start () {
		
	}
	
	void Update () {
		transform.Translate(0.5f*Time.deltaTime,0,0);
	}
}
