using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Growth.Util;

namespace Growth
{

public static class NeughbourDirExtensions
{
    static public Neighbours.NeighbourDir Reverse(this Neighbours.NeighbourDir dir)
    {
        return (Neighbours.NeighbourDir)((int)dir ^ 1);
    }

    static Vector3Int[] Offsets = new Vector3Int[]
    {
    new Vector3Int(1, 0, 0),
    new Vector3Int(-1, 0, 0),
    new Vector3Int(0, 1, 0),
    new Vector3Int(0, -1, 0),
    new Vector3Int(0, 0, 1),
    new Vector3Int(0, 0, -1),
    };

    static public Vector3Int Offset(this Neighbours.NeighbourDir dir)
    {
        return Offsets[(int)dir];
    }
}

public class Neighbours
{
    public enum NeighbourDir
    {
        PlusX = 0,
        MinusX = 1,
        PlusY = 2,
        MinusY = 3,
        PlusZ = 4,
        MinusZ = 5
    }

    CellBhv[] Data = new CellBhv[6];

    public CellBhv this[NeighbourDir neighb]
    {
        get
        {
            return Data[(int)neighb];
        }

        set
        {
            Data[(int)neighb] = value;
        }
    }

    public static IEnumerable<NeighbourDir> All()
    {
        return Enum.GetValues(typeof(NeighbourDir)).Cast<NeighbourDir>();
    }
}

public class CellBhv : MonoBehaviour
{
    public MeshRenderer Renderer;
    float CurrentLightLevel;
    int MaterialNum = -1;

    bool Inited = false;

    int Cycler;
    const int CycleMax = 5;

    static Material[] Materials;
    const int NumShades = 10;

    public Material BaseMaterial;

    public readonly Neighbours Neighbours = new Neighbours();

    public bool GatherLight = true;

    private void Start()
    {
        if (Materials == null)
        {
            Materials = new Material[NumShades];

            for (int i = 0; i < NumShades; i++)
            {
                float frac = (float)i / (NumShades - 1);

                Materials[i] = new Material(BaseMaterial);
                Materials[i].color = Color.gray * (1 - frac) + Color.white * frac;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        MyAssert.IsTrue(Inited, "Initialised");

        Cycler++;

        if (Cycler == CycleMax)
        {
            int new_mat_num = (int)(CurrentLightLevel * NumShades);

            if (new_mat_num == NumShades)
            {
                new_mat_num--;
            }

            if (new_mat_num != MaterialNum)
            {
                Renderer.material = Materials[new_mat_num];
                MaterialNum = new_mat_num;
            }

            Cycler = 0;
        }

        Debug.DrawLine(transform.position + new Vector3(0.2f, 0, 0), transform.position + new Vector3(-0.2f, 0, 0), Color.red, 0.5f);
        Debug.DrawLine(transform.position + new Vector3(0, 0.2f, 0), transform.position + new Vector3(0, -0.2f, 0), Color.red, 0.5f);
        Debug.DrawLine(transform.position + new Vector3(0, 0, 0.2f), transform.position + new Vector3(0, 0, -0.2f), Color.red, 0.5f);
    }

    internal void SetLight(float light)
    {
        if (CurrentLightLevel == 0)
        {
            CurrentLightLevel = light;
        }
        else
        {
            CurrentLightLevel = CurrentLightLevel * 0.9f + light * 0.1f;
        }

        if (CurrentLightLevel < 0.5f / NumShades)
        {
            CurrentLightLevel = 0;
        }
    }

    public void Init(ClRand random)
    {
        Cycler = random.IntRange(0, CycleMax);
        Inited = true;
    }

    static readonly Vector2[] FacePoints = new Vector2[] {
    new Vector2(0.079151f, 0.735459f),
    new Vector2(0.703860f, 0.081259f),
    new Vector2(0.152978f, 0.721733f),
    new Vector2(0.122317f, 0.078323f),
    new Vector2(0.135474f, 0.470970f),
    new Vector2(0.512153f, 0.997676f),
    new Vector2(0.064814f, 0.906687f),
    new Vector2(0.985661f, 0.431769f),
    new Vector2(0.030833f, 0.257656f),
    new Vector2(0.956672f, 0.004360f),
};

    public int FacesPointCount(Vector3 light_point)
    {
        int ret = 0;

        // do not consider faces which are burried against a neighbour
        // or those facing away from the light...
        foreach (var dir in Neighbours.All())
        {
            if (Neighbours[dir] == null && FaceFacesPoint(dir, light_point))
            {
                ret += FacePoints.Length;
            }
        }

        return ret;
    }

    private bool FaceFacesPoint(Neighbours.NeighbourDir dir, Vector3 point)
    {
        // the light has to be further out than the face to shine on it
        // and the face is +/- 0.5 from our centre...
        switch (dir)
        {
            case Neighbours.NeighbourDir.PlusX:
                return point.x > transform.position.x + 0.5f;
            case Neighbours.NeighbourDir.PlusY:
                return point.y > transform.position.y + 0.5f;
            case Neighbours.NeighbourDir.PlusZ:
                return point.z > transform.position.z + 0.5f;
            case Neighbours.NeighbourDir.MinusX:
                return point.x < transform.position.x - 0.5f;
            case Neighbours.NeighbourDir.MinusY:
                return point.y < transform.position.y - 0.5f;
            case Neighbours.NeighbourDir.MinusZ:
                return point.z < transform.position.z - 0.5f;
        }

        throw new Exception("What direction is that?");
    }

    public IEnumerable<Vector3> FacesPointSequence(Vector3 light_point)
    {
        foreach (var dir in Neighbours.All())
        {
            if (Neighbours[dir] == null && FaceFacesPoint(dir, light_point))
            {
                foreach (Vector3 ret in FacePointSequence(dir))
                {
                    yield return ret;
                }
            }
        }
    }

    public IEnumerable<Vector3> FacePointSequence(Neighbours.NeighbourDir dir)
    {
        foreach (var p in FacePoints)
        {
            yield return TransformIntoFace(p, dir);
        }
    }

    private Vector3 TransformIntoFace(Vector2 p, Neighbours.NeighbourDir dir)
    {
        var f_p = ToInFaceCoords(p, dir);

        return FaceCentre(dir) + f_p;
    }

    private Vector3 ToInFaceCoords(Vector2 p, Neighbours.NeighbourDir dir)
    {
        switch (dir)
        {
            case Neighbours.NeighbourDir.PlusX:
            case Neighbours.NeighbourDir.MinusX:
                return new Vector3(0, p.x - 0.5f, p.y - 0.5f);
            case Neighbours.NeighbourDir.PlusY:
            case Neighbours.NeighbourDir.MinusY:
                return new Vector3(p.x - 0.5f, 0, p.y - 0.5f);
            case Neighbours.NeighbourDir.PlusZ:
            case Neighbours.NeighbourDir.MinusZ:
                return new Vector3(p.x - 0.5f, p.y - 0.5f, 0);
        }

        throw new Exception("Bad NeighbourDir");
    }

    private Vector3 FaceCentre(Neighbours.NeighbourDir dir)
    {
        return transform.position + (Vector3)dir.Offset() / 2;
    }
}

}