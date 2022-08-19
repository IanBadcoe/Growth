using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Util;

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

    private void Start()
    {
        if (Materials == null) {
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
        Assert.IsTrue(Inited);

        Cycler++;

        if (Cycler == CycleMax)
        {
            CurrentLightLevel *= 0.99f;

            int new_mat_num = (int)(Mathf.Clamp01(CurrentLightLevel) * NumShades);

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
    }

    public void LightHit()
    {
        CurrentLightLevel++;
    }

    public void Init(ClRand random)
    {
        Cycler = random.IntRange(0, CycleMax);
        Inited = true;
    }
}
