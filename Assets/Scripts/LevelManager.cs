using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
using Random = Unity.Mathematics.Random;

public class LevelManager : MonoBehaviour
{
    private SplineMovement player;
    public GameObject[] trackPrefabs;
    private int maxIndex;
    private int index;
    private GameObject activeTrack;
    private SplineContainer spline;
    private Vector3 splineEnd;
    
    private List<GameObject> tracks = new List<GameObject>();
    private GameObject track;
    void Start()
    {
        maxIndex = trackPrefabs.Length;
        SpawnTrack(0, new Vector3(0, 0, 0));
        SplineMovement.activeSpline = tracks[0].GetComponent<SplineContainer>();
        SplineMovement.splineLength = SplineMovement.activeSpline.CalculateLength();
        
        SpawnTrack(UnityEngine.Random.Range(0, maxIndex), splineEnd);
        Debug.Log(tracks[0]);
        
        SpawnTrack(UnityEngine.Random.Range(0, maxIndex), splineEnd);
        Debug.Log(tracks[1]);
    }

    void Update()
    {
        if (SplineMovement.progress >= 1f)
        {
            index++;
            SplineMovement.activeSpline = tracks[index].GetComponent<SplineContainer>();
            SplineMovement.progress = 0f;
            SpawnTrack(UnityEngine.Random.Range(0, maxIndex), splineEnd);
        }
    }

    public Vector3 GetSplineEnd(GameObject obj)
    {
        spline = obj.GetComponent<SplineContainer>();
        splineEnd = spline.EvaluatePosition(1);
        Debug.Log(splineEnd);
        return splineEnd;
    }

    public void SpawnTrack(int i, Vector3 pos)
    {
        track = Instantiate(trackPrefabs[i], pos, Quaternion.identity);
        splineEnd = GetSplineEnd(track);
        tracks.Add(track);
    }
}
