using UnityEngine;

using System.Collections;
using System.Collections.Generic;


public class WarZone : MonoBehaviour 
{
	//a reference to the game manager
	GameManager gm;

	public void Start()
	{
		gm = GameObject.Find("MainGO").GetComponent<GameManager>();

	}

	//sets the npc's flocks in middle bool to true when it enters the zone
	public void OnTriggerEnter(Collider other)
	{

		if(other.gameObject.tag == "RedNPC" || other.gameObject.tag == "BlueNPC")
		{
			gm.gameObject.GetComponent<GameManager>().inMiddle[other.gameObject.GetComponent<Seeker>().FlockNum] = true;
		} 
	}

	//sets the npc's flocks in middle bool to false when it leaves the zone
	public void OnTriggerExit(Collider other)
	{
		if(other.gameObject.tag == "RedNPC" || other.gameObject.tag == "BlueNPC")
		{
			gm.gameObject.GetComponent<GameManager>().inMiddle[other.gameObject.GetComponent<Seeker>().FlockNum] = false;
		} 
	}
}
