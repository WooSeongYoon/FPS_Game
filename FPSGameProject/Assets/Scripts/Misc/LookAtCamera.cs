using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour {

	private void Start () 
	{
		gameObject.transform.localScale = 
			new Vector3 (-1, 1, 1);
	}
		
	private void Update () 
	{
		transform.LookAt (Camera.main.transform);
	}
}