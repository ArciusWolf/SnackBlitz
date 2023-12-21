using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBoard : MonoBehaviour
{
    // Size of the board
    public int width;
    public int height;

    // Board Spacing
    public float spacingX;
    public float spacingY;

    // Reference to the food prefab
    public GameObject[] foodPrefab;

    // Reference to the food board
    public Node[,] foodBoard;
    public GameObject foodGO;

    // Array layout
    public ArrayLayout arrayLayout;
    public static FoodBoard Instance;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        InitializeBoard();
    }

    void InitializeBoard()
    {
        foodBoard = new Node[width, height];

        spacingX = (float)(width - 0.5) / 2;
        spacingY = (float)(height - 0.5) / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2(x - spacingX, y - spacingY);
                if (arrayLayout.rows[y].row[x])
                {
                    foodBoard[x, y] = new Node(false, null);
                } else
                {
                    int randomIndex = Random.Range(0, foodPrefab.Length);
                    GameObject food = Instantiate(foodPrefab[randomIndex], position, Quaternion.identity);
                    food.GetComponent<Food>().SetIndicies(x, y);

                    foodBoard[x, y] = new Node(true, food);
                }

            }
        }
    }
}
