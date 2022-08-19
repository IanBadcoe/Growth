using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class RockGeneratorBhv : MonoBehaviour
{
    public CellGridBhv Grid;
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
        ZPlane(0, Grid.XSize, Grid.YSize);

        Hill(0, Grid.XSize, 0, Grid.ZSize, 0, 10, 0.5f, 0.75f);

        Hill(Grid.XSize * 6 / 10, Grid.XSize * 8 / 10, Grid.ZSize * 2 / 10, Grid.ZSize * 4 / 10, 1, 5, 0.9f, 1.0f);
    }

    private void Hill(
        int x_start, int x_end, 
        int z_start, int z_end, 
        int curr_h, int lim,
        float min_shrink, float max_shrink)
    {
        FillBox(x_start, x_end, curr_h, curr_h + 1, z_start, z_end);

        int x_size = x_end - x_start;
        int z_size = z_end - z_start;

        float x_cell_as_fraction = 1 / (float)x_size;
        float z_cell_as_fraction = 1 / (float)z_size;

        // the fraction here lets us remain the same size if we get within 1/2 cell of 1.0
        // and makes sure we are always able to shrink at least one cell
        float x_shrink = Random.FloatRange(min_shrink - x_cell_as_fraction, max_shrink + x_cell_as_fraction);
        float z_shrink = Random.FloatRange(min_shrink - z_cell_as_fraction, max_shrink + z_cell_as_fraction);

        int new_x_size = (int)(x_size * x_shrink);
        if (new_x_size < lim)
            return;

        int new_z_size = (int)(z_size * z_shrink);
        if (new_z_size < lim)
            return;

        int new_x_start = Random.IntRange(0, x_size - new_x_size) + x_start;
        int new_z_start = Random.IntRange(0, z_size - new_z_size) + z_start;

        for (int i = 0; i < 2; i++)
        {
            Hill(
                new_x_start, new_x_start + new_x_size,
                new_z_start, new_z_start + new_z_size,
                curr_h + 1, lim,
                min_shrink, max_shrink);
        }
    }

    private void FillBox(int sx, int ex, int sy, int ey, int sz, int ez)
    {
        sx = Math.Max(0, sx);
        sy = Math.Max(0, sy);
        sz = Math.Max(0, sz);

        ex = Math.Min(Grid.XSize, ex);
        ey = Math.Min(Grid.YSize, ey);
        ez = Math.Min(Grid.ZSize, ez);

        for (int x = sx; x < ex; x++)
        {
            for (int y = sy; y < ey; y++)
            {
                for (int z = sz; z < ez; z++)
                {
                    var pos = new Vector3Int(x, y, z);

                    if (Grid[pos] == null)
                    {
                        Grid.Instantiate(CellPrefab, pos);
                    }
                }
            }
        }
    }

    private void XPlane(int x, int y_size, int z_size)
    {
        FillBox(x, x + 1, 0, y_size, 0, z_size);
    }

    private void YPlane(int y, int x_size, int z_size)
    {
        FillBox(0, x_size, y, y + 1, 0, z_size);
    }

    private void ZPlane(int z, int x_size, int y_size)
    {
        FillBox(0, x_size, 0, y_size, z, z + 1);
    }

    private void YZCol(int y, int z, int x_size)
    {
        FillBox(0, x_size, y, y + 1, z, z + 1);
    }

    private void XZCol(int x, int z, int y_size)
    {
        FillBox(x, x + 1, 0, y_size, z, z + 1);
    }

    private void XYCol(int x, int y, int z_size)
    {
        FillBox(x, x + 1, y, y + 1, 0, z_size);
    }

    private void YZThickCol(int y, int z, int x_size, int thick)
    {
        int mt = thick / 2;
        int pt = (thick + 1) / 2;

        FillBox(0, x_size, y - mt / 2, y + pt, z - mt, z + pt);
    }

    private void XZThickCol(int x, int z, int y_size, int thick)
    {
        int mt = thick / 2;
        int pt = (thick + 1) / 2;

        FillBox(x - mt / 2, x + pt, 0, y_size, z - mt, z + pt);
    }

    private void XYThickCol(int x, int y, int z_size, int thick)
    {
        int mt = thick / 2;
        int pt = (thick + 1) / 2;

        FillBox(x - mt / 2, x + pt, y - mt, y + pt, 0, z_size);
    }
}
