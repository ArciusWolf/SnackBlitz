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

        spacingX = (float)(width - 1) / 2;
        spacingY = (float)((height - 1) / 2) + 1 ;

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
        if (CheckBoard())
        {
            Debug.Log("Match Found");
            InitializeBoard();
        } else
        {
            Debug.Log("No Match Found");
        }
    }

    public bool CheckBoard()
    {
        Debug.Log("Checking Board");
        bool hasMatched = false;

        List<Food> foodToRemove = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (foodBoard[x, y].isUsable)
                {
                    Food food = foodBoard[x, y].food.GetComponent<Food>();

                    //ensure its not matched
                    if (!food.isMatched)
                    {
                        MatchResult matchedFood = isConnected(food);

                        if (matchedFood.connectedFoods.Count >= 3)
                        {
                            foodToRemove.AddRange(matchedFood.connectedFoods);

                            foreach (Food food1 in matchedFood.connectedFoods)
                            {
                                food1.isMatched = true;
                            }

                            hasMatched = true;
                        }
                    }
                }

            }
        }
        return hasMatched;
    }

    MatchResult isConnected(Food food)
    {
        List<Food> connectedFoods = new();
        FoodType foodType = food.foodType;

        connectedFoods.Add(food);

        // Check right
        CheckDirection(food,new Vector2Int(1,0), connectedFoods);
        // Check left
        CheckDirection(food,new Vector2Int(-1,0), connectedFoods);

        if (connectedFoods.Count == 3)
        {
            return new MatchResult()
            {
                connectedFoods = connectedFoods,
                direction = MatchDirection.Horizontal
            };
        } else if (connectedFoods.Count > 3)
        {
            return new MatchResult()
            {
                connectedFoods = connectedFoods,
                direction = MatchDirection.LongHorizontal
            };
        }

        // Clear matched food
        connectedFoods.Clear();

        connectedFoods.Add(food);

        // Check up
        CheckDirection(food,new Vector2Int(0,1), connectedFoods);
        // Check down
        CheckDirection(food,new Vector2Int(0,-1), connectedFoods);

        if (connectedFoods.Count == 3)
        {
            return new MatchResult()
            {
                connectedFoods = connectedFoods,
                direction = MatchDirection.Vertical
            };
        }
        else if (connectedFoods.Count > 3)
        {
            return new MatchResult()
            {
                connectedFoods = connectedFoods,
                direction = MatchDirection.LongVertical
            };
        } else
        {
            return new MatchResult()
            {
                connectedFoods = connectedFoods,
                direction = MatchDirection.None
            };
        }
    }

    void CheckDirection(Food food1, Vector2Int direction, List<Food> connectedFoods)
    {
        FoodType foodType = food1.foodType;
        int x = food1.xIndex + direction.x;
        int y = food1.yIndex + direction.y;

        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (foodBoard[x, y].isUsable)
            {
                Food neighborFood = foodBoard[x, y].food.GetComponent<Food>();

                if (!neighborFood.isMatched && neighborFood.foodType == foodType)
                {
                    connectedFoods.Add(neighborFood);
                    x += direction.x;
                    y += direction.y;
                } else
                {
                    break;
                }
            } else
            {
                break;
            }
        }
    }
}

public class MatchResult
{
    public List<Food> connectedFoods;
    public MatchDirection direction;
}

public enum MatchDirection
{
    Horizontal,
    Vertical,
    LongHorizontal,
    LongVertical,
    Super,
    None
}
