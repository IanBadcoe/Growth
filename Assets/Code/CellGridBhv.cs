using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Growth.Util;

namespace Growth {

public class CellGridBhv : MonoBehaviour
{
    private CellBhv[,,] Cells;

    private List<CellBhv> RandomCells = new List<CellBhv>();

    public IReadOnlyList<CellBhv> RandomOrderedCells
    {
        get {
            return RandomCells.AsReadOnly();
        }
    }

    public GameObject CellsContainer;

    public int XSize;
    public int YSize;
    public int ZSize;

    ClRand Random {
        get {
            if (_rand == null)
            {
                _rand = new ClRand(RSeed);
            }

            return _rand;
        }
    }
    ClRand _rand;
    public int RSeed = 1;

    public void Init()
    {
        if (Cells == null)
        {
            Cells = new CellBhv[XSize, YSize, ZSize];
        }
    }

    public CellBhv this[Vector3Int pos]
    {
        get
        {
            if (!InRange(pos))
            {
                return null;
            }

            return Cells[pos.x, pos.y, pos.z];
        }

        set
        {
            if (!InRange(pos))
            {
                throw new ArgumentOutOfRangeException("pos");
            }

            CellBhv old_content = this[pos];
            if (old_content != null)
            {
                RandomCells.Remove(old_content);
                Destroy(old_content);
            }

            Cells[pos.x, pos.y, pos.z] = value;

            UpdateNeighbours(pos, value);

            if (value != null)
            {
                RandomCells.Insert(Random.IntRange(0, RandomCells.Count + 1), value);
            }
        }
    }

    private CellBhv RawNeighbour(Vector3Int pos, Neighbours.NeighbourDir dir)
    {
        Vector3Int n_pos = pos + dir.Offset();

        return this[n_pos];
    }

    private void UpdateNeighbours(Vector3Int pos, CellBhv changed_cell)
    {
        // we cache "face neighbours"

        // update references to this cell in any neighbours it has
        // and vice-versa
        UpdateNeighbour(changed_cell, RawNeighbour(pos, Neighbours.NeighbourDir.PlusX),  Neighbours.NeighbourDir.PlusX);
        UpdateNeighbour(changed_cell, RawNeighbour(pos, Neighbours.NeighbourDir.MinusX), Neighbours.NeighbourDir.MinusX);
        UpdateNeighbour(changed_cell, RawNeighbour(pos, Neighbours.NeighbourDir.PlusY),  Neighbours.NeighbourDir.PlusY);
        UpdateNeighbour(changed_cell, RawNeighbour(pos, Neighbours.NeighbourDir.MinusY), Neighbours.NeighbourDir.MinusY);
        UpdateNeighbour(changed_cell, RawNeighbour(pos, Neighbours.NeighbourDir.PlusZ),  Neighbours.NeighbourDir.PlusZ);
        UpdateNeighbour(changed_cell, RawNeighbour(pos, Neighbours.NeighbourDir.MinusZ), Neighbours.NeighbourDir.MinusZ);
    }

    private void UpdateNeighbour(CellBhv cell_from, CellBhv cell_to, Neighbours.NeighbourDir dir)
    {
        if (cell_from != null)
        {
            cell_from.Neighbours[dir] = cell_to;
        }

        if (cell_to != null)
        {
            cell_to.Neighbours[dir.Reverse()] = cell_from;
        }
    }

    public bool InRange(Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < XSize
            && pos.y >= 0 && pos.y < YSize
            && pos.z >= 0 && pos.z < ZSize;
    }

    public GameObject Instantiate(GameObject prefab, Vector3Int pos)
    {
        Assert.IsNotNull(prefab.GetComponent<CellBhv>());

        GameObject go = Instantiate(prefab, pos, Quaternion.identity, CellsContainer.transform);

        this[pos] = go.GetComponent<CellBhv>();

        return go;
    }

    public Vector3Int Bounds
    {
        get {
            return new Vector3Int(XSize, YSize, ZSize);
        }
    }

    public Vector3[] Corners {
        get
        {
            return new Vector3[8]
            {
                new Vector3(0, 0, 0),
                new Vector3(XSize, 0, 0),
                new Vector3(0, YSize, 0),
                new Vector3(XSize, YSize, 0),
                new Vector3(0, 0, ZSize),
                new Vector3(XSize, 0, ZSize),
                new Vector3(0, YSize, ZSize),
                new Vector3(XSize, YSize, ZSize),
            };
        }
    }

    public bool InRangeY(int y)
    {
        return y >= 0 && y < YSize;
    }

    public bool InRangeX(int x)
    {
        return x >= 0 && x < XSize;
    }

    public bool InRangeZ(int z)
    {
        return z >= 0 && z < ZSize;
    }
}

}