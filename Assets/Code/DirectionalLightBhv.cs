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

        for(int i = 0; i < 100; i++)
        {
            CellIdx++;

            if (CellIdx >= cells_list.Count)
            {
                CellIdx = 0;
            }

            var cell = cells_list[CellIdx];

            TryStabCell(cell);
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

    private void TryStabCell(CellBhv cell)
    {
        var back_transformed = transform.InverseTransformPoint(cell.transform.position);

        var stab_start = transform.TransformPoint(new Vector3(back_transformed.x, back_transformed.y, 0));
        Ray ray = new Ray(stab_start, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, MaxRange))
        {
            var hit_cell = hit.collider.GetComponent<CellBhv>();

            if (hit_cell != null)
            {
                hit_cell.LightHit();
                Debug.DrawLine(stab_start, hit.point, Color.green, 0.5f);
            }
        }
    }

    private void TryStabLight()
    {
        float xoffset = Random.FloatRange(StabXMin, StabXMax);
        float yoffset = Random.FloatRange(StabYMin, StabYMax);

        Vector3 stab_start = transform.TransformPoint(new Vector3(xoffset, yoffset, 0));
        //        Vector3 stab_end = stab_start + transform.forward * MaxRange;

        Ray ray = new Ray(stab_start, transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, MaxRange))
        {
            var cell = hit.collider.GetComponent<CellBhv>();

            if (cell != null)
            {
                cell.LightHit();
            }
            //Debug.DrawLine(stab_start, hit.point, Color.green, 0.5f);
        }
        //else
        //{
        //    Debug.DrawLine(stab_start, stab_start + transform.forward * MaxRange * 2, Color.red, 0.5f);
        //}
    }
}
