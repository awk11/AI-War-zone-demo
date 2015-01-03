using UnityEngine;

using System.Collections;
using System.Collections.Generic;


public class Containment : MonoBehaviour 
{
	//a reference to the game manager
	GameManager gm;

	public void Start()
	{
		gm = GameObject.Find("MainGO").GetComponent<GameManager>();

	}

	//when an npc enters, it switches its withinbounds bool to true
	public void OnTriggerEnter(Collider other)
	{

		if(other.gameObject.tag == "RedNPC" || other.gameObject.tag == "BlueNPC")
		{
			other.gameObject.GetComponent<Seeker>().withinBounds = true;
		} 
	}

	//when an npc exits the zone, it sets its withinbounds bool to false
	public void OnTriggerExit(Collider other)
	{
		if(other.gameObject.tag == "RedNPC" || other.gameObject.tag == "BlueNPC")
		{
			other.gameObject.GetComponent<Seeker>().withinBounds = false;
		} 
	}
}
