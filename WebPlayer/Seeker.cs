using UnityEngine;
using System.Collections;
//use generic class
using System.Collections.Generic;

public class Seeker : Vehicle 
{

	//reference to an array of obstacles
	private  GameObject[] obstacles; 

	//the npc's current target
	public GameObject Target = null;

	//The steering force weights
	public float pursueWt = 40.0f;
	public float avoidWt = 50.0f;
	public float avoidDist = 10.0f;
	public float wanderWt = 10.0f;
    public float containWt = 1000f;
	public float contain2Wt = 15f;
    public float alignmentWt = 20f;
    public float separationWt = 30f;
    public float separationDist = 5f;
    public float cohesionWt = 30f;

	


    //a reference to the gamemanager
    private GameManager gm;

	//if the npc is within bounds of the overall batlefield
	public bool withinBounds = true;

	//initializer function
	override public void Start () 
	{
		base.Start();

		//fills the obstacle list
		obstacles = GameObject.FindGameObjectsWithTag ("Obstacle");

        //gets the gamemanager
        gm = GameObject.Find("MainGO").GetComponent<GameManager>();
	}

	//calculates the steering force
	protected override void CalcSteeringForce()
	{
		//the overall force to be applied
		Vector3 force = Vector3.zero;	

		//adds the wander force associated with the npc's specified flcok
		force += gm.Wander[FlockNum] * wanderWt;

		//adds the target pursue if the npc is allowed to pursue
		//also, this is where the velocity (a forward vector of the target), is added to the position the npc is seking, 
		//making the algorithm pursue instead of just a simple seek
		if(gm.canPursue[FlockNum])
			if(Target != null)
				force += Pursue(Target.transform.position + Target.GetComponent<Seeker>().Velocity) * pursueWt;

        //flocking weights
        force += alignmentWt * Alignment(gm.GetFlockDirection(FlockNum));
        force += separationWt * Separation(GetFlock(), separationDist);
        force += cohesionWt * Cohesion(gm.GetCentroid(FlockNum));

		//adds the ontainment force if needed
		if(!withinBounds)
		{
			if(FlockNum < 3)
        		force += containWt * Containment(new Vector3(50, 2, 90));
			else
				force += containWt * Containment(new Vector3(260, 2, 90));
		}

		//adds the containment force into the middle of the battlefield if needed
		if(!gm.inMiddle[FlockNum])
		{
			force += Containment(new Vector3(150,0,100)) * contain2Wt;
		}

		//avoid all of the obstacles on the battlefield
		for (int i=0; i < obstacles.Length; i++) {	
			force += avoidWt * AvoidObstacle(obstacles[i], avoidDist);
		}
		
		//limits the force to the maxForce and applies it
		force = Vector3.ClampMagnitude (force, maxForce);
		ApplyForce(force);

		//show force as a blue line pushing the guy like a jet stream
		Debug.DrawLine(transform.position, transform.position - force,Color.blue);

	}

	//gets the npc's flock list
	private List<GameObject> GetFlock()
	{
		if (FlockNum == 0)
			return gm.BFlock1;
		else if (FlockNum == 1)
			return gm.BFlock2;
		else if (FlockNum == 2)
			return gm.BFlock3;
		else if (FlockNum == 3)
			return gm.RFlock1;
		else if (FlockNum == 4)
			return gm.RFlock2;
		else if (FlockNum == 5)
			return gm.RFlock3;
		else
			return null;

	}

	//for attacking
	//if the npc collides with an enemy npc, the one with the lower velocity will die
	public void OnTriggerEnter(Collider other)
	{
		if(this.tag == "BlueNPC")
		{
			if(other.gameObject.tag == "RedNPC")
			{
				if(velocity.magnitude > other.gameObject.GetComponent<Seeker>().velocity.magnitude)
					other.gameObject.GetComponent<Seeker>().isDying = true;
				else
					isDying = true;
			}
		}
		else if(this.tag == "RedNPC")
		{
			if(other.gameObject.tag == "BlueNPC")
			{
				if(velocity.magnitude > other.gameObject.GetComponent<Seeker>().velocity.magnitude)
					other.gameObject.GetComponent<Seeker>().isDying = true;
				else
					isDying = true;
			}
		}
	}





}
