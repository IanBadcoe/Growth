using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class RockGeneratorBehaviour : MonoBehaviour
{
    public CellGrid Grid;
    public GameObject CellPrefab;

    ClRand Random;
    public int RSeed = 1;

    private void Start()
    {
        Random = new ClRand(RSeed);

        Grid.Init();

        YZThickCol(0, 0, Grid.XSize, Random.IntRange(2, 7));
        YZThickCol(0, Grid.ZSize - 1, Grid.XSize, Random.IntRange(2, 7));
        YZThickCol(Grid.YSize - 1, 0, Grid.XSize, Random.IntRange(2, 7));
        YZThickCol(Grid.YSize - 1, Grid.ZSize - 1, Grid.XSize, Random.IntRange(2, 7));

        XZThickCol(0, 0, Grid.YSize, Random.IntRange(2, 7));
        XZThickCol(0, Grid.ZSize - 1, Grid.YSize, Random.IntRange(2, 7));
        XZThickCol(Grid.XSize - 1, 0, Grid.YSize, Random.IntRange(2, 7));
        XZThickCol(Grid.XSize - 1, Grid.ZSize - 1, Grid.YSize, Random.IntRange(2, 7));

        XYThickCol(0, 0, Grid.ZSize, Random.IntRange(2, 7));
        XYThickCol(0, Grid.YSize - 1, Grid.ZSize, Random.IntRange(2, 7));
        XYThickCol(Grid.XSize - 1, 0, Grid.ZSize, Random.IntRange(2, 7));
        XYThickCol(Grid.XSize - 1, Grid.YSize - 1, Grid.ZSize, Random.IntRange(2, 7));

        XPlane(0, Grid.YSize, Grid.ZSize);
        YPlane(0, Grid.XSize, Grid.ZSize);
        ZPlane(0, Grid.XSize, Grid.YSize);
    }

    private void XPlane(int x, int y_size, int z_size)
    {
        for (int z = 0; z < z_size; z++)
        {
            for (int y = 0; y < y_size; y++)
            {
                var pos = new Vector3Int(x, y, z);

                if (Grid[pos] == null)
                {
                    Grid.Instantiate(CellPrefab, pos);
                }
            }
        }
    }

    private void YPlane(int y, int x_size, int z_size)
    {
        for (int x = 0; x < x_size; x++)
        {
            for (int z = 0; z < z_size; z++)
            {
                var pos = new Vector3Int(x, y, z);

                if (Grid[pos] == null)
                {
                    Grid.Instantiate(CellPrefab, pos);
                }
            }
        }
    }

    private void ZPlane(int z, int x_size, int y_size)
    {
        for (int x = 0; x < x_size; x++)
        {
            for (int y = 0; y < y_size; y++)
            {
                var pos = new Vector3Int(x, y, z);

                if (Grid[pos] == null)
                {
                    Grid.Instantiate(CellPrefab, pos);
                }
            }
        }
    }

    private void YZCol(int y, int z, int x_size)
    {
        for (int x = 0; x < x_size; x++)
        {
            var pos = new Vector3Int(x, y, z);

            if (Grid[pos] == null)
            {
                Grid.Instantiate(CellPrefab, pos);
            }
        }
    }

    private void XZCol(int x, int z, int y_size)
    {
        for (int y = 0; y < y_size; y++)
        {
            var pos = new Vector3Int(x, y, z);

            if (Grid[pos] == null)
            {
                Grid.Instantiate(CellPrefab, pos);
            }
        }
    }

    private void XYCol(int x, int y, int z_size)
    {
        for (int z = 0; z < z_size; z++)
        {
            var pos = new Vector3Int(x, y, z);

            if (Grid[pos] == null)
            {
                Grid.Instantiate(CellPrefab, pos);
            }
        }
    }

    private void YZThickCol(int y, int z, int x_size, int thick)
    {
        for (int hy = y - thick / 2; hy < y + (thick + 1) / 2; hy++)
        {
            if (Grid.InRangeY(hy))
            {
                for (int hz = z - thick / 2; hz < z + (thick + 1) / 2; hz++)
                {
                    if (Grid.InRangeZ(hz    ))
                    {
                        YZCol(hy, hz, x_size);
                    }
                }
            }
        }
    }

    private void XZThickCol(int x, int z, int y_size, int thick)
    {
        for (int hx = x - thick / 2; hx < x + (thick + 1) / 2; hx++)
        {
            if (Grid.InRangeX(hx))
            {
                for (int hz = z - thick / 2; hz < z + (thick + 1) / 2; hz++)
                {
                    if (Grid.InRangeZ(hz))
                    {
                        XZCol(hx, hz, y_size);
                    }
                }
            }
        }
    }

    private void XYThickCol(int x, int y, int z_size, int thick)
    {
        for (int hx = x - thick / 2; hx < x + (thick + 1) / 2; hx++)
        {
            if (Grid.InRangeX(hx))
            {
                for (int hy = y - thick / 2; hy < y + (thick + 1) / 2; hy++)
                {
                    if (Grid.InRangeY(hy))
                    {
                        XYCol(hx, hy, z_size);
                    }
                }
            }
        }
    }
}
