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

    // Start is called before the first frame update
    void Start()
    {
        Random = new ClRand(RSeed);

        transform.position = new Vector3(Grid.XSize * 2, Grid.YSize * 2, Grid.ZSize * 2);
        transform.LookAt(new Vector3(0, 0, 0));

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

    // Update is called once per frame
    void Update()
    {
        TryStabLight();
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
