using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class DirectionalLightBhv : MonoBehaviour
{
    public CellGridBhv Grid;
    ClRand Random;
    public int RSeed = 1;

    float StabXMin = 0;
    float StabXMax = 0;
    float StabYMin = 0;
    float StabYMax = 0;

    float MaxRange = 0;

    int CellIdx = 0;

    float Heading = 0;
    readonly float Distance = 1000;
    readonly float YDist;
    readonly float XZDist;

    public float AngularVelocity = 0.05f;
    public float CellStabs = 20;

    DirectionalLightBhv()
    {
        YDist = Mathf.Sqrt(Distance * Distance / 3);
        XZDist = Mathf.Sqrt(Distance * Distance - YDist * YDist);
    }

    // Start is called before the first frame update
    void Start()
    {
        Random = new ClRand(RSeed);

        UpdatePosition();

        Vector3[] corners = Grid.Corners;

        foreach (var corner in corners)
        {
            var transformed = transform.InverseTransformPoint(corner);

            StabYMin = Mathf.Min(StabYMin, transformed.y);
            StabYMax = Mathf.Max(StabYMax, transformed.y);
            StabXMin = Mathf.Min(StabXMin, transformed.x);
            StabXMax = Mathf.Max(StabXMax, transformed.x);

            MaxRange = Mathf.Max(MaxRange, transformed.z);
        }
    }

    private void UpdatePosition()
    {
        float sk = Mathf.Sin(Heading);
        float ck = Mathf.Cos(Heading);

        Vector3 centre = new Vector3(0, 0, 0);

        var pos = centre + new Vector3(
            XZDist * sk,
            YDist,
            XZDist * ck);

        transform.position = pos;
        transform.LookAt(centre);
    }

    // Update is called once per frame
    void Update()
    {
        IReadOnlyList<CellBhv> cells_list = Grid.RandomOrderedCells;
        if (cells_list.Count == 0)
            return;

        for(int i = 0; i < Mathf.Min(100, cells_list.Count); i++)
        {
            CellIdx++;

            if (CellIdx >= cells_list.Count)
            {
                CellIdx = 0;
            }

            var cell = cells_list[CellIdx];

            TryCellStabs(cell);
        }

        Heading += AngularVelocity * Time.deltaTime;
        if (Heading > Mathf.PI * 2)
        {
            Heading -= Mathf.PI * 2;
        }
        else if ( Heading < 0)
        {
            Heading += Mathf.PI * 2;
        }

        UpdatePosition();
    }

    private void TryCellStabs(CellBhv cell)
    {
        int num_points = cell.FacesPointCount();

        if (num_points == 0)
            return;

        float stab_prob = (float)CellStabs / (float)num_points;

        float hits = 0;
        float attempts = 0;

        foreach(Vector3 pos in cell.FacesPointSequence())
        {
            if (Random.FloatRange(0, 1) < stab_prob)
            {
                attempts += 1;

                if (TryStabPoint(pos, cell)) {
                    hits += 1;
                }
            }
        }

        if (attempts > 1)
        {
            cell.SetLight(hits / attempts);
        }
    }


    private bool TryStabPoint(Vector3 pos, CellBhv cell)
    {
        var back_transformed = transform.InverseTransformPoint(pos);

        var stab_start = transform.TransformPoint(new Vector3(back_transformed.x, back_transformed.y, 0));
        Ray ray = new Ray(stab_start, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, MaxRange))
        {
            var hit_cell = hit.collider.GetComponent<CellBhv>();

            if (hit_cell == cell)
            {
                Debug.DrawLine(stab_start, hit.point, Color.green, 0.5f);

                return true;
            }
        }

        return false;
    }
}
