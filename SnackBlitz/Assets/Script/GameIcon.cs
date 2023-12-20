using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class GameIcon : MonoBehaviour
{
    private int x;
    private int y;

    public int X
    {
        get { return x; }
    }

    public int Y
    {
        get { return y; }
    }

    private GameGrid.CellType type;

    public GameGrid.CellType Type
    {
        get { return type; }
    }

    private GameGrid grid;

    public GameGrid GridRef
    {
        get { return grid; }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(int _x, int _y, GameGrid _grid, GameGrid.CellType _type)
    {
        x = _x;
        y = _y;
        grid = _grid;
        type = _type;
    }
}
