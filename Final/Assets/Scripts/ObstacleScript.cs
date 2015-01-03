using UnityEngine;
using System.Collections;


//Obstacle is just a GameObject that we need to avoid

public class ObstacleScript : MonoBehaviour {
	public float radius = 3.0f; //hard coded or set in inspector for now
	
	public float Radius {
		get { return radius; }
	}
	
	
}

	