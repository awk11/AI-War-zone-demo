
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {


	//NOTE: In all lists containing data for both sets of npc's, 
	//the first 3 spots are for blue tea, the next 3 spots are for red team



	//The prefabs to use
	public GameObject RedTeamPrefab;
	public GameObject BlueTeamPrefab;
	public GameObject ObstaclePrefab;

	//list of obstacles
	private  GameObject[] obstacles; 

	//the flock the camera is following
	public int FlockToFollow = 0;

	//the amount of npc's in each flock
    public int numPerFlock = 1;

	//used to help align forces with entire flock
	public Vector3[] Wander = new Vector3[6];
	public Vector3[] Pursue = new Vector3[6];
	public bool[] canPursue = new bool[6];

	//the blue flocks
    private List<List<GameObject>> BlueFlocks;
    public List<GameObject> BFlock1 { get { return BlueFlocks[0]; } }
	public List<GameObject> BFlock2 { get { return BlueFlocks[1]; } }
	public List<GameObject> BFlock3 { get { return BlueFlocks[2]; } }

	//the red flocks
	private List<List<GameObject>> RedFlocks;
	public List<GameObject> RFlock1 { get { return RedFlocks[0]; } }
	public List<GameObject> RFlock2 { get { return RedFlocks[1]; } }
	public List<GameObject> RFlock3 { get { return RedFlocks[2]; } }


	//lists of all npc's on the respective team
	public GameObject[] RedNPCs;
	public GameObject[] BlueNPCs;

	//a list holding al of the flocks respective directions
    private Vector3[] flockDirection = new Vector3[6];
    public Vector3 GetFlockDirection(int i) { return flockDirection[i]; }

	//a list holding all of the flocks respective centroids
    private Vector3[] centroid = new Vector3[6];
    public Vector3 GetCentroid(int i) { return centroid [i]; }

	//if the flock is in the middle zone
	public bool[] inMiddle = new bool[6];


	//the initializer function
	void Start () 
	{
		Vector3 pos = new Vector3(Random.Range(100,200), 2f, Random.Range(50,150));

		//instantiates the flock lists
        BlueFlocks = new List<List<GameObject>>();
		RedFlocks = new List<List<GameObject>>();

		//instantiates with flocks
		for (int i = 0; i < 3; i++) 
		{
			BlueFlocks.Add (new List<GameObject>());
			RedFlocks.Add(new List<GameObject>());
		}



        //makes all of the npc's and adds them to their flocks
        for (int i = 0; i < numPerFlock; i++ )
        {
            pos = new Vector3(Random.Range(20,40), 1f, Random.Range(150,170));
            GameObject guy = (GameObject)GameObject.Instantiate(BlueTeamPrefab, pos, Quaternion.identity);
			guy.GetComponent<Seeker>().FlockNum = 0;
			BlueFlocks[0].Add(guy);

			pos = new Vector3(Random.Range(40,60), 1f, Random.Range(80,100));
			guy = (GameObject)GameObject.Instantiate(BlueTeamPrefab, pos, Quaternion.identity);
			guy.GetComponent<Seeker>().FlockNum = 1;
			BlueFlocks[1].Add(guy);

			pos = new Vector3(Random.Range(20,40), 1f, Random.Range(0,20));
			guy = (GameObject)GameObject.Instantiate(BlueTeamPrefab, pos, Quaternion.identity);
			guy.GetComponent<Seeker>().FlockNum = 2;
			BlueFlocks[2].Add(guy);


			pos = new Vector3(Random.Range(270,290), 1f, Random.Range(150,170));
			guy = (GameObject)GameObject.Instantiate(RedTeamPrefab, pos, Quaternion.identity);
			guy.GetComponent<Seeker>().FlockNum = 3;
			RedFlocks[0].Add(guy);
			
			pos = new Vector3(Random.Range(250,270), 1f, Random.Range(80,100));
			guy = (GameObject)GameObject.Instantiate(RedTeamPrefab, pos, Quaternion.identity);
			guy.GetComponent<Seeker>().FlockNum = 4;
			RedFlocks[1].Add(guy);
			
			pos = new Vector3(Random.Range(270,290), 1f, Random.Range(0,20));
			guy = (GameObject)GameObject.Instantiate(RedTeamPrefab, pos, Quaternion.identity);
			guy.GetComponent<Seeker>().FlockNum = 5;
			RedFlocks[2].Add(guy);


        }


		//makes 150 obstacles (rocks) and scatters them randomly throughout the batlefield
		for (int i = 0; i < 150; i++) 
		{
			pos =  new Vector3(Random.Range(20, 280), 0f, Random.Range(-100, 300));

			Quaternion rot = Quaternion.Euler(0, Random.Range(0, 90), 0);
			GameObject.Instantiate(ObstaclePrefab, pos, rot);
		}

		//fills all of the global lists
		obstacles = GameObject.FindGameObjectsWithTag ("Obstacle");
		BlueNPCs = GameObject.FindGameObjectsWithTag ("BlueNPC");
		RedNPCs = GameObject.FindGameObjectsWithTag ("RedNPC");

        //tells the camera to follow the main gameobject
        Camera.main.GetComponent<SmoothFollow>().target = gameObject.transform;

		
	}

	//is called every frame
	void Update () 
	{

		//resets the targets of all of the npc's so they can change if needed
		for(int i = 0; i < BlueNPCs.Length; i++)
			BlueNPCs[i].GetComponent<Seeker>().Target = null;

		for(int i = 0; i < RedNPCs.Length; i++)
			RedNPCs[i].GetComponent<Seeker>().Target = null;


		//goes through and gets all npc's appropriate targets
		//increments the amount of distance away from the npc the target can be so closer enemies are targeted with priority
		for(int r = 10; r < 70; r+=20)
		{
			for(int i = 0; i < BlueNPCs.Length; i++)
			{
				for(int j = 0; j < RedNPCs.Length; j++)
				{
					if((RedNPCs[j].transform.position.x < (BlueNPCs[i].transform.position.x + r)) && (RedNPCs[j].transform.position.x > (BlueNPCs[i].transform.position.x - r)) && (RedNPCs[j].transform.position.z < (BlueNPCs[i].transform.position.z + r)) && (RedNPCs[j].transform.position.z > (BlueNPCs[i].transform.position.z - r)) && BlueNPCs[i].GetComponent<Seeker>().Target == null)
					{
						BlueNPCs[i].GetComponent<Seeker>().Target = RedNPCs[j];
					}
				}
			}

			for(int i = 0; i < RedNPCs.Length; i++)
			{
				for(int j = 0; j < BlueNPCs.Length; j++)
				{
					if((BlueNPCs[j].transform.position.x < (RedNPCs[i].transform.position.x + r)) && (BlueNPCs[j].transform.position.x > (RedNPCs[i].transform.position.x - r)) && (BlueNPCs[j].transform.position.z < (RedNPCs[i].transform.position.z + r)) && (BlueNPCs[j].transform.position.z > (RedNPCs[i].transform.position.z - r)) && RedNPCs[i].GetComponent<Seeker>().Target == null)
					{
						RedNPCs[i].GetComponent<Seeker>().Target = BlueNPCs[j];
					}
				}
			}
		}


		//checks for whether or not the npc's should be wandering or being contained, and tells the npc whether or not it can be pursuing targets
		//for blue team
		for(int i = 0; i < 3; i++)
		{
			if(inMiddle[i])
			{
				Wander[i] = BlueFlocks[i][0].GetComponent<Seeker>().Wander();
				canPursue[i] = true;
			}
			else
			{
				canPursue[i] = false;
				Wander[i] = new Vector3(0,0,0);
			}
		}

		//for red team
		for(int i = 3; i < 6; i++) 
		{
			if(inMiddle[i])
			{
				Wander[i] = RedFlocks[i-3][0].GetComponent<Seeker>().Wander();		
				canPursue[i] = true;
			}
			else
			{
				canPursue[i] = false;
				Wander[i] = new Vector3(0,0,0);
			}
		}
		
        for(int i = 0; i < 3; i++)
        {
			 //calculate the centroid and flock direction for each blue flock
			 calcCentroid(BlueFlocks[i], i);
			 calcFlockDirection(BlueFlocks[i], i);

			 //calculate the centroid and flock direction for each red flock
			 calcCentroid(RedFlocks[i], (i+3));
			 calcFlockDirection(RedFlocks[i], i+3);
        }

		//camera control stuff
		//this check makes the camera follow the specified flocks centroid
		if(FlockToFollow > -1 && FlockToFollow < 6)
		{
			gameObject.transform.position = centroid[FlockToFollow];
			gameObject.transform.forward = flockDirection[FlockToFollow];
			Camera.main.GetComponent<GhostFreeRoamCamera>().enabled = false;
			Camera.main.GetComponent<SmoothFollow>().enabled = true;
		}

		//enables ghost cam
		if(FlockToFollow == 6)
		{
			Camera.main.GetComponent<GhostFreeRoamCamera>().enabled = true;
			Camera.main.GetComponent<SmoothFollow>().enabled = false;
		}

		//swtches camera view
		if(Input.GetKeyDown(KeyCode.RightArrow))
	    {
			FlockToFollow++;
			if(FlockToFollow > 6)
				FlockToFollow = 0;
		}

		//switches camra view
		if(Input.GetKeyDown(KeyCode.LeftArrow))
		{
			FlockToFollow--;
			if(FlockToFollow < 0)
				FlockToFollow = 6;
		}
		

	}

	//calculate the centroid of the specified flock
    private void calcCentroid(List<GameObject> flock, int i)
    {
        centroid[i] = Vector3.zero;
        foreach(GameObject f in flock){
            centroid[i] += f.transform.position;
        }
        centroid[i] /= flock.Count;
    }

	//calculate the direction of the specified flock
	private void calcFlockDirection(List<GameObject> flock, int i)
    {
        flockDirection[i] = Vector3.zero;
        foreach (GameObject f in flock)
        {
            flockDirection[i] += f.transform.forward;
        }
        flockDirection[i] /= flock.Count;
    }


}