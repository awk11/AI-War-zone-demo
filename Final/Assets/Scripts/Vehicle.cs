using UnityEngine;
using System.Collections;
//add generic collection
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]


abstract public class Vehicle : MonoBehaviour 
{
	//steering variables
	public float maxSpeed = 6.0f;
	public float maxForce = 3.0f;
	public float mass = 1.0f;
	public float radius = 2.0f;
	public float gravity = 100.0f;

	//wander variables
	float xoff = 0;
	float yoff = 0;
	float wanderRad = 10.0f;
	float wanderDist = 10.0f;
	//float wanderRand = 2.0f;
	//float wanderAng = 360.0f;


	protected CharacterController characterController;
	protected Vector3 acceleration;	//change in velocity per second
	protected Vector3 velocity;		//change in position per second
	protected Vector3 dv;           //desired velocity
	public Vector3 Velocity {
		get { return velocity; }
		set { velocity = value;}
	}


	//flock stuff
	public int flocknum = -1;
	public int FlockNum { get { return flocknum; } set { flocknum = value; } }

	//bool indicating whether or not the npc is currenty in the process of dying
	public bool isDying = false;

	//Classes that extend Vehicle must override CalcSteeringForce
	abstract protected void CalcSteeringForce();

	GameManager gm1;

	//initializer function
	virtual public void Start()
	{
		acceleration = Vector3.zero;
		velocity = transform.forward;
		//get component references
		characterController = GetComponent<CharacterController> ();
		gm1 = GameObject.Find("MainGO").GetComponent<GameManager>();
	}


	
	//Update function that is called once per frame
	public void Update () 
	{
		//if the npc is currenty dying, dont do any of its normal stuff
		if(isDying)
		{
			this.GetComponent<CapsuleCollider>().enabled = false;
			DyingAnim();
		}
		//otherwise...
		else
		{
			CalcSteeringForce ();

			velocity += acceleration * Time.deltaTime;
			velocity.y = 0;
			velocity = Vector3.ClampMagnitude (velocity, maxSpeed);

			if (velocity != Vector3.zero)
				transform.forward = velocity.normalized;


			velocity.y -= gravity * Time.deltaTime;

			characterController.Move (velocity * Time.deltaTime);

			acceleration = Vector3.zero;
		}

	}

	//apply the force
	public void ApplyForce (Vector3 steeringForce){
		acceleration += steeringForce/mass;
	}

	
	//pursue the target given (technically what makes the steering algorithm pursue isnt in here, this just seeks the target)
	public Vector3 Pursue (Vector3 targetPos)
	{
		//find dv, desired velocity
		dv = targetPos - transform.position;		
		dv = dv.normalized * maxSpeed; 	//scale by maxSpeed
		dv -= velocity;
		dv.y = 0;						// only steer in the x/z plane
		return dv;
	}

	//avoids the obstacle given if needed
	public Vector3 AvoidObstacle (GameObject obst, float safeDistance)
	{ 
		dv = Vector3.zero;
		float obRadius = obst.GetComponent<ObstacleScript> ().Radius;
		safeDistance += radius + obRadius;

		//vector from vehicle to center of obstacle
		Vector3 vecToCenter = obst.transform.position - transform.position;
		//eliminate y component so we have a 2D vector in the x, z plane
		vecToCenter.y = 0;

		// distance should not be allowed to be zero or negative because 
		// later we will divide by it and do not want to divide by zero
		// or cause an inadvertent sign change.
		float dist = Mathf.Max(vecToCenter.magnitude - obRadius - radius, 0.1f);
		
		// if too far to worry about, out of here
		if (dist > safeDistance)
			return Vector3.zero;
		
		//if behind us, out of here
        if (Vector3.Dot(vecToCenter, transform.forward) < 0)
            //------------------------ flocking change ---------------------------
            //return Seek function - makes steering smoother
            //return Seek(targetPos);
			return Vector3.zero;
            //--------------------------------------------------------------------
		
		float rightDotVTC = Vector3.Dot (vecToCenter, transform.right);
		
		//if we can pass safely, out of here
		if (Mathf.Abs (rightDotVTC) > radius + obRadius)
			return Vector3.zero;
		
		//obstacle on right so we steer to left
		if (rightDotVTC > 0)
			dv = transform.right * -maxSpeed * safeDistance / dist;
		else
			//obstacle on left so we steer to right
			dv = transform.right * maxSpeed * safeDistance / dist;
		dv -= velocity;
		dv.y = 0;				// only steer in the x/z plane
		return dv;
	}

	//wanders using perlin noise
	public Vector3 Wander()
	{
		xoff += .2f;
		yoff += .2f;
		float WanderX = Mathf.PerlinNoise (xoff, 0f);
		float WanderZ = Mathf.PerlinNoise (yoff, 0f);
		Vector3 target = gm1.GetCentroid(flocknum) + gm1.GetFlockDirection(flocknum) * wanderDist;
		Vector3 offset = new Vector3 (WanderX * gm1.GetFlockDirection(flocknum).x, 0, WanderZ * gm1.GetFlockDirection(flocknum).z);
		target += offset * wanderRad;
		return Pursue (target);

	}

    //-------------- flocking functions ----------------------
    public Vector3 Separation(List<GameObject> neighbors, float separationDistance)
    {
        //create vector to hold total
        Vector3 total = Vector3.zero;
        //check distance from each neighbor flocker
        foreach(GameObject n in neighbors){
            Vector3 dv = transform.position - n.transform.position;
            float dist = dv.magnitude;
            //if neighbor is in my space
            if(dist > 0 && dist < separationDistance){
                //scale for importance based on distance
                dv *= separationDistance / dist;
                //zero out Y plane
                dv.y = 0;
                //gather up all the totals
                total += dv;
            }
        }
        total = total.normalized * maxSpeed;
        total -= velocity;
        return total;
    }

    public Vector3 Alignment(Vector3 direction)
    {
        dv = direction.normalized * maxSpeed;
        dv -= velocity;
        dv.y = 0;
		//dv += Wander(true);
        return dv;
    }

    public Vector3 Cohesion(Vector3 targetPos)
    {
        return Pursue(targetPos);
    }

    public Vector3 Containment(Vector3 center)
    {
    	return Pursue(center);
    }
	

	//this function makes it so that when an npc gets hit and loses, it starts to fall over in order to portray tht its dying.
	//when it finishes falling over, it gets teleported back to their teams spawn point
	public void DyingAnim()
	{
		this.transform.Rotate(60 * Time.deltaTime, 0, 0);

		if(this.transform.eulerAngles.x >= 90)
		{

			isDying = false;
			this.GetComponent<CapsuleCollider>().enabled = true;
			if(this.tag == "BlueNPC")
				this.transform.position = new Vector3(15, 1, 100);
			else if(this.tag == "RedNPC")
				this.transform.position = new Vector3(285, 1, 100);
		}
	}

}