using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WayPointsContainer {

    [SerializeField] private GameObject wayPoint;
    [SerializeField] private List<GameObject> nextsWayPoints;

    public WayPointsContainer()
    {

    }

    public GameObject WayPoint
    {
        get
        {
            return wayPoint;
        }

        set
        {
            wayPoint = value;
        }
    }

    public List<GameObject> NextsWayPoints
    {
        get
        {
            return nextsWayPoints;
        }

        set
        {
            nextsWayPoints = value;
        }
    }
}
