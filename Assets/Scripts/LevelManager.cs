using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
using Random = Unity.Mathematics.Random;

public class LevelManager : MonoBehaviour
{
    public SplineMovement player;
    public SplineMovement chaser;
    public GameObject[] trackPrefabs;
    private int maxIndex;
    private int playerIndex;
    private int chaserIndex;
    private GameObject activeTrack;
    private SplineContainer spline;
    private Vector3 splineEnd;
    
    private List<GameObject> tracks = new List<GameObject>();
    private GameObject track;
    void Start()
    {
        maxIndex = trackPrefabs.Length;
        SpawnTrack(0, new Vector3(0, 0, 0));
        player.activeSpline = tracks[0].GetComponent<SplineContainer>();
        player.splineLength = player.activeSpline.CalculateLength();
        
        chaser.activeSpline = tracks[0].GetComponent<SplineContainer>();
        chaser.splineLength = chaser.activeSpline.CalculateLength();
        
        SpawnTrack(UnityEngine.Random.Range(0, maxIndex), splineEnd);
    }

    void Update()
    {
        if (player.progress >= 1f)
        {
            playerIndex++;
            player.activeSpline = tracks[playerIndex].GetComponent<SplineContainer>();
            player.splineLength = player.activeSpline.CalculateLength();
            player.progress = 0f;
            SpawnTrack(UnityEngine.Random.Range(0, maxIndex), splineEnd);
        }
        
        if (chaser.progress >= 1f)
        {
            chaserIndex++;
            chaser.activeSpline = tracks[chaserIndex].GetComponent<SplineContainer>();
            chaser.splineLength = chaser.activeSpline.CalculateLength();
            chaser.progress = 0f;
            SpawnTrack(UnityEngine.Random.Range(0, maxIndex), splineEnd);
        }
    }

    public Vector3 GetSplineEnd(GameObject obj)
    {
        spline = obj.GetComponent<SplineContainer>();
        splineEnd = spline.EvaluatePosition(1);
        return splineEnd;
    }

    public void SpawnTrack(int i, Vector3 pos)
    {
        track = Instantiate(trackPrefabs[i], pos, Quaternion.identity);
        splineEnd = GetSplineEnd(track);
        tracks.Add(track);
    }
}
