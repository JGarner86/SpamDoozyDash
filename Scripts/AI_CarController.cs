using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_CarController : MonoBehaviour {

    [SerializeField] NavMeshAgent agent;
    [SerializeField] float yieldSpeed;
    IntersectionController intersection;
    GameObject carInFront;
    CarHeading heading;
    Vector3 waypoint;
    RaycastHit rayHit;
    Ray rayToCast;
    Vector3 target;
    float startSpeed;
    bool lightRed = true;
    bool goodToGo = true;
    public bool test = false;
    bool carNotGone = false;
    // Use this for initialization
	void Start () {
        agent = GetComponent<NavMeshAgent>();
        SetRayToCast();
        GetFirstIntersection();
        intersection.AddCarToIntersection();
        FindInitialWaypoint();
        GoToInitialWaypoint();
        startSpeed = agent.speed;
        StartCoroutine(FenderBenderPreventor());
        StartCoroutine(MoveToNextWayPoint());
	}
	
	// Update is called once per frame
	void Update () {

	}

    #region Initializers

    void FindInitialWaypoint()
    {
        float north = (intersection.North.gameObject != null) ? Vector3.Distance(transform.position, intersection.North.position) : 500f;
        float south = (intersection.South.gameObject != null) ? Vector3.Distance(transform.position, intersection.South.position) : 500f;
        float east = (intersection.East.gameObject != null) ? Vector3.Distance(transform.position, intersection.East.position) : 500f;
        float west = (intersection.West.gameObject != null) ? Vector3.Distance(transform.position, intersection.West.position) : 500f;

        float shortest = 400f;
        if(north <= shortest)
        {
            waypoint = intersection.North.position;
            heading = CarHeading.North;
            intersection.AddCarToStreetLightEvent(heading, this);
            shortest = north;
        }
        if(south <= shortest)
        {
            waypoint = intersection.South.position;
            heading = CarHeading.South;
            intersection.AddCarToStreetLightEvent(heading, this);
            shortest = south;
        }
        if(east <= shortest)
        {
            waypoint = intersection.East.position;
            heading = CarHeading.East;
            intersection.AddCarToStreetLightEvent(heading, this);
            shortest = east;
        }
        if(west <= shortest)
        {
            waypoint = intersection.West.position;
            heading = CarHeading.West;
            intersection.AddCarToStreetLightEvent(heading, this);
        }
    }

    void GoToInitialWaypoint()
    {
        agent.SetDestination(waypoint);
    }

    bool GetFirstIntersection()
    {
        if (Physics.Raycast(rayToCast, out rayHit, 200f))
        {
            target = rayHit.point;
            return ((intersection = rayHit.collider.GetComponent<IntersectionController>()) != null);
        }
        return false;
    }

    #endregion


    IEnumerator FenderBenderPreventor()
    {
        CheckIfCarIsInFront();
        yield return new WaitForSeconds(.01f);
        StartCoroutine(FenderBenderPreventor());
    }

    public void LightChange(StreetLight light)
    {
        switch (light)
        {
            case StreetLight.Green:
                lightRed = false;
                agent.speed = startSpeed;
                StartCoroutine(MoveToNextWayPoint());
                break;
            case StreetLight.Yellow:
                if(agent.remainingDistance < 20)
                {
                    agent.speed = yieldSpeed;
                }
                lightRed = true;
                break;
            case StreetLight.Red:
                lightRed = true;
                break;
        }
    }

    
    void CheckIfCarIsInFront()
    {
        Ray ray = new Ray(transform.localPosition + new Vector3(0f,.05f, 0f), transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 30f))
        {
            if (hit.collider.name == "CarBase" && !carNotGone)
            {
                carInFront = hit.collider.gameObject;
                carNotGone = true;
                StartCoroutine(DistanceFromCar());
            }
        }
    }
    
    IEnumerator DistanceFromCar()
    {
        float dist = Vector3.Distance(transform.position, carInFront.transform.position);
        while (dist < 15)
        {
            dist = Vector3.Distance(transform.position, carInFront.transform.position);
            float multiplier = 1f / 30f;
            agent.speed = dist * multiplier;
            if (dist < 10)
            {
                agent.isStopped = true;
            }
            else
            {
                agent.speed = startSpeed;
                agent.isStopped = false;
            }
            yield return null;
        }
        agent.isStopped = false;
        carNotGone = false;
    }
    




    void SetRayToCast()
    {
        rayToCast = new Ray(transform.localPosition + new Vector3(0, 4, 0), transform.forward);
    }

    IEnumerator MoveToNextWayPoint()
    {
        while (!lightRed)
        {
            if (CheckIfAtWaypoint() )
            {
                waypoint = intersection.NextWaypoint(ref heading, out intersection, this);
                agent.SetDestination(waypoint);
            }
            yield return null;
        }
    }

    bool StillGreenSoGo()
    {
        if (agent.remainingDistance <= 1 && !lightRed && goodToGo)
        {
            agent.ResetPath();
            goodToGo = false;
            StartCoroutine(ResetGoodToGo());
            return true;
        }
        return false;
    }

    IEnumerator ResetGoodToGo()
    {
        yield return new WaitForSeconds(1);
        goodToGo = true;
    }

    bool CheckIfAtWaypoint()
    {
        if (!agent.pathPending)
        {
            if(agent.remainingDistance <= agent.stoppingDistance)
            {
                if(!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
