using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class MB_DynamicAddDeleteExample : MonoBehaviour {
	public GameObject prefab;
	List<GameObject> objsInCombined = new List<GameObject>();
	MB3_MeshBaker mbd;
	GameObject[] objs;
	void Start(){
		mbd = GetComponentInChildren<MB3_MeshBaker>(); 
		
        Stopwatch sw = Stopwatch.StartNew();

		// instantiate game objects
		int dim = 25;
		GameObject[] gos = new GameObject[dim * dim];
		for (int i = 0; i < dim; i++){
			for (int j = 0; j < dim; j++){
				GameObject go = (GameObject) Instantiate(prefab);
				gos[i*dim + j] = go.GetComponentInChildren<MeshRenderer>().gameObject;
				go.transform.position = (new Vector3(9f*i,0,9f * j));
				//put every third object in a list so we can add and delete it later
				if ((i*dim + j) % 3 == 0){
					objsInCombined.Add(gos[i*dim + j]);
				}
			}
		}
		//add objects to combined mesh
		mbd.AddDeleteGameObjects(gos, null, true);
		mbd.Apply();

	    Debug.Log("init "+sw.ElapsedMilliseconds);

		objs = objsInCombined.ToArray();
		//start routine which will periodically add and delete objects
		StartCoroutine(largeNumber());
	}
	
	IEnumerator largeNumber() {
		while(true){
			yield return new WaitForSeconds(1.5f);
            Stopwatch sw = Stopwatch.StartNew();
			//Delete every third object
			mbd.AddDeleteGameObjects(null, objs, true);
			mbd.Apply();
            Debug.Log("delete "+sw.ElapsedMilliseconds);
			
			yield return new WaitForSeconds(1.5f);
            sw = Stopwatch.StartNew();
			//Add objects back
			mbd.AddDeleteGameObjects(objs, null, true);
			mbd.Apply();
            Debug.Log("add "+sw.ElapsedMilliseconds);
		}
	}
	
	void OnGUI(){
		GUILayout.Label ("Dynamically instantiates game objects. \nRepeatedly adds and removes some of them\n from the combined mesh.");	
	}
}
