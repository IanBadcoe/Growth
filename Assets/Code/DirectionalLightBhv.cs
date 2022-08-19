using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalLightBhv : MonoBehaviour
{
    public CellGridBhv Grid;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(Grid.XSize * 2, Grid.YSize * 2, Grid.ZSize * 2);
        transform.LookAt(new Vector3(0, 0, 0));        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
