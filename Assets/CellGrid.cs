using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGrid : MonoBehaviour
{
    private Cell[,,] Cells;

    public GameObject CellsContainer;

    public int XSize;
    public int YSize;
    public int ZSize;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init()
    {
        if (Cells == null)
        {
            Cells = new Cell[XSize, YSize, ZSize];
        }
    }

    public Cell this[Vector3Int pos]
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

            Cell old_content = this[pos];
            if (old_content != null)
            {
                Destroy(old_content);
            }

            Cells[pos.x, pos.y, pos.z] = value;
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
        return Instantiate(prefab, pos, Quaternion.identity, CellsContainer.transform);
    }

    public Vector3Int Bounds
    {
        get {
            return new Vector3Int(XSize, YSize, ZSize);
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
