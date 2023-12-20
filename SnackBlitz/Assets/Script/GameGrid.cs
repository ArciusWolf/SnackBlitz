using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGrid : MonoBehaviour
{
    public enum CellType
    {
        Empty,
        Count,
    };

    [System.Serializable]
    public struct CellPrefab
    {
        public CellType type;
        public GameObject prefab;
    };

    public CellPrefab[] cells;
    public GameObject backgroundPrefab;

    public int width;
    public int height;

    private Dictionary<CellType, GameObject> cellPrefabDict;

    private GameIcon[,] icons;
    // Start is called before the first frame update
    void Start()
    {
        cellPrefabDict = new Dictionary<CellType, GameObject>();

        for (int i = 0; i < cells.Length; i++)
        {
            if (!cellPrefabDict.ContainsKey(cells[i].type))
            {
                cellPrefabDict.Add(cells[i].type, cells[i].prefab);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y <height; y++)
            {
                GameObject background = Instantiate(backgroundPrefab, transform);
                background.transform.position = new Vector3(x, y, 0);
            }
        }

        icons = new GameIcon[width, height];
        for( int x = 0; x < width; x++)
        {
            for (int y = 0; y <height; y++)
            {
                GameObject newIcon = Instantiate(cellPrefabDict[CellType.Empty], transform);
                newIcon.transform.position = new Vector3(x, y, 0);
                icons[x, y] = newIcon.GetComponent<GameIcon>();
                icons[x, y].Init(x, y, this, CellType.Empty);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
