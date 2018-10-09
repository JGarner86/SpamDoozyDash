using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum CarHeading { North, South, East, West }
public enum Direction { North, South, East, West, Undefined }
public enum StreetLight { Red, Green, Yellow }
public class IntersectionController : MonoBehaviour {

    public Transform North;
    public Transform South;
    public Transform East;
    public Transform West;
    enum StartLight { NorthSouth, EastWest }
    StartLight startLight;
    StreetLight northSouthLight;
    StreetLight eastWestLight;
    public event Action<StreetLight> NorthSouthLightChange;
    public event Action<StreetLight> EastWestLightChange;
    [SerializeField] GameObject northLightGFX;
    [SerializeField] GameObject southLightGFX;
    [SerializeField] GameObject eastLightGFX;
    [SerializeField] GameObject westLightGFX;
    [SerializeField] float stopGoLightTime;
    [SerializeField] float yieldLightTime;
    Dictionary<Direction, IntersectionController> neighbors = new Dictionary<Direction, IntersectionController>();
    int carsCount;
    // Use this for initialization
	void Start () {
        GetNeighbors();
        startLight = (StartLight)UnityEngine.Random.Range(0, 2);
        StartCoroutine(StartLightTimer());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    #region StreetLightsMethods

    IEnumerator StartLightTimer()
    {
        switch (startLight)
        {
            case StartLight.NorthSouth:
                northSouthLight = StreetLight.Green;
                eastWestLight = StreetLight.Red;
                ChangeEastWestLightColor(eastWestLight);
                ChangeNorthSouthLightColor(northSouthLight);
                EastWestLightChange?.Invoke(eastWestLight);
                NorthSouthLightChange?.Invoke(northSouthLight);
                yield return new WaitForSeconds(stopGoLightTime);
                northSouthLight = StreetLight.Yellow;
                ChangeNorthSouthLightColor(northSouthLight);
                NorthSouthLightChange?.Invoke(northSouthLight);
                yield return new WaitForSeconds(yieldLightTime);
                northSouthLight = StreetLight.Red;
                eastWestLight = StreetLight.Green;
                ChangeEastWestLightColor(eastWestLight);
                ChangeNorthSouthLightColor(northSouthLight);
                EastWestLightChange?.Invoke(eastWestLight);
                NorthSouthLightChange?.Invoke(northSouthLight);
                yield return new WaitForSeconds(stopGoLightTime);
                eastWestLight = StreetLight.Yellow;
                ChangeEastWestLightColor(eastWestLight);
                EastWestLightChange?.Invoke(eastWestLight);
                yield return new WaitForSeconds(yieldLightTime);
                StartCoroutine(StartLightTimer());
                break;
            case StartLight.EastWest:
                northSouthLight = StreetLight.Red;
                eastWestLight = StreetLight.Green;
                ChangeEastWestLightColor(eastWestLight);
                ChangeNorthSouthLightColor(northSouthLight);
                EastWestLightChange?.Invoke(eastWestLight);
                NorthSouthLightChange?.Invoke(northSouthLight);
                yield return new WaitForSeconds(stopGoLightTime);
                eastWestLight = StreetLight.Yellow;
                ChangeEastWestLightColor(northSouthLight);
                EastWestLightChange?.Invoke(northSouthLight);
                yield return new WaitForSeconds(yieldLightTime);
                northSouthLight = StreetLight.Green;
                eastWestLight = StreetLight.Red;
                ChangeEastWestLightColor(eastWestLight);
                ChangeNorthSouthLightColor(northSouthLight);
                EastWestLightChange?.Invoke(eastWestLight);
                NorthSouthLightChange?.Invoke(northSouthLight);
                yield return new WaitForSeconds(stopGoLightTime);
                northSouthLight = StreetLight.Yellow;
                ChangeNorthSouthLightColor(eastWestLight);
                NorthSouthLightChange?.Invoke(eastWestLight);
                yield return new WaitForSeconds(yieldLightTime);
                StartCoroutine(StartLightTimer());
                break;
        }
    }

    void ChangeNorthSouthLightColor(StreetLight color)
    {
        Color lightColor = Color.white;
        switch (color)
        {
            case StreetLight.Green:
                lightColor = Color.green;
                break;
            case StreetLight.Red:
                lightColor = Color.red;
                break;
            case StreetLight.Yellow:
                lightColor = Color.yellow;
                break;
        }
        northLightGFX.GetComponent<Renderer>().material.color = lightColor;
        southLightGFX.GetComponent<Renderer>().material.color = lightColor;
    }

    void ChangeEastWestLightColor(StreetLight color)
    {
        Color lightColor = Color.white;
        switch (color)
        {
            case StreetLight.Green:
                lightColor = Color.green;
                break;
            case StreetLight.Red:
                lightColor = Color.red;
                break;
            case StreetLight.Yellow:
                lightColor = Color.yellow;
                break;
        }
        eastLightGFX.GetComponent<Renderer>().material.color = lightColor;
        westLightGFX.GetComponent<Renderer>().material.color = lightColor;
    }

    #endregion

    #region Initialization

    void GetNeighbors()
    {
        Ray[] rays = new Ray[4]
        {
            new Ray(transform.position, Vector3.forward),
            new Ray(transform.position, Vector3.right),
            new Ray(transform.position, Vector3.back),
            new Ray(transform.position, Vector3.left)
        };

        RaycastHit hit;
        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], out hit, 200f))
            {
                if (hit.collider.tag == "Intersection")
                {
                    IntersectionController neighbor = hit.collider.GetComponent<IntersectionController>();
                    switch (i)
                    {
                        case 0:
                            neighbors.Add(Direction.North, neighbor);
                            break;
                        case 1:
                            neighbors.Add(Direction.East, neighbor);
                            break;
                        case 2:
                            neighbors.Add(Direction.South, neighbor);
                            break;
                        case 3:
                            neighbors.Add(Direction.West, neighbor);
                            break;
                    }
                }
            }
        }
    }

    #endregion

    #region EventHelpers

    public void AddCarToStreetLightEvent(CarHeading heading, AI_CarController car)
    {
        switch (heading)
        {
            case CarHeading.North:
            case CarHeading.South:
                NorthSouthLightChange += car.LightChange;
                break;
            case CarHeading.East:
            case CarHeading.West:
                EastWestLightChange += car.LightChange;
                break;
        }
    }

    void RemoveLightListener(CarHeading direction, AI_CarController car)
    {
        switch (direction)
        {
            case CarHeading.North:
            case CarHeading.South:
                NorthSouthLightChange -= car.LightChange;
                break;
            case CarHeading.West:
            case CarHeading.East:
                EastWestLightChange -= car.LightChange;
                break;
        }
    }

    #endregion

    #region WaypointMethods


    public void AddCarToIntersection()
    {
        carsCount++;
    }

    public Vector3 NextWaypoint(ref CarHeading heading, out IntersectionController intersection, AI_CarController car)
    {
        carsCount--;
        RemoveLightListener(heading, car);
        Vector3 waypoint = new Vector3();
        Direction dir = Direction.Undefined;
        int compare = 10;
        foreach(var neighbor in neighbors)
        {
            if(neighbor.Value.carsCount < compare)
            {
                if(NoGoDirection(heading) == neighbor.Key)
                {
                    continue;
                }
                else
                {
                    compare = neighbor.Value.carsCount;
                    dir = neighbor.Key;
                }
            }
        }
        intersection = neighbors[dir];
        intersection.AddCarToIntersection();
        switch (dir)
        {
            case Direction.North:
                waypoint = intersection.North.position;
                heading = CarHeading.North;
                break;
            case Direction.South:
                waypoint = intersection.South.position;
                heading = CarHeading.South;
                break;
            case Direction.East:
                waypoint = intersection.East.position;
                heading = CarHeading.East;
                break;
            case Direction.West:
                waypoint = intersection.West.position;
                heading = CarHeading.West;
                break;
        }
        intersection.AddCarToStreetLightEvent(heading, car);
        return waypoint;
    }

    Direction NoGoDirection(CarHeading heading)
    {
        switch (heading)
        {
            case CarHeading.North:
                return Direction.South;
            case CarHeading.South:
                return Direction.North;
            case CarHeading.East:
                return Direction.West;
            case CarHeading.West:
                return Direction.East;
        }
        return Direction.Undefined;
    }

    #endregion
}
