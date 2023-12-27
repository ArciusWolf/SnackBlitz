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

    public List<GameObject> foodToDestroy = new();
    public GameObject foodParent;

    [SerializeField]
    private Food selectedFood;
    [SerializeField]
    private bool isProcessingMove;
    [SerializeField]
    List<Food> foodToRemove = new();

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

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Food>())
            {
                if (isProcessingMove)
                    return;

                Food food = hit.collider.gameObject.GetComponent<Food>();
                Debug.Log("Food Selected" + food.gameObject);

                SelectFood(food);
            }
        }
    }

    void InitializeBoard()
    {
        DestroyFood();
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
                    food.transform.SetParent(foodParent.transform);
                    food.GetComponent<Food>().SetIndicies(x, y);
                    foodBoard[x, y] = new Node(true, food);
                    foodToDestroy.Add(food);
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

    private void DestroyFood()
    {
        if (foodToDestroy != null)
        {
            foreach (GameObject food in foodToDestroy)
            {
                Destroy(food);
            }
            foodToDestroy.Clear();
        }
    }

    public bool CheckBoard()
    {
        Debug.Log("Checking Board");
        bool hasMatched = false;

        foodToRemove.Clear();

        foreach (Node nodeFood in foodBoard)
        {
            if (nodeFood.food != null)
            {
                nodeFood.food.GetComponent<Food>().isMatched = false;
            }
        }

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
                            MatchResult superMatchFood = SuperMatch(matchedFood);
                            foodToRemove.AddRange(superMatchFood.connectedFoods);

                            foreach (Food food1 in superMatchFood.connectedFoods)
                                food1.isMatched = true;

                            hasMatched = true;
                        }
                    }
                }

            }
        }
        return hasMatched;
    }

    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
            foreach (Food FoodToRemove in foodToRemove)
            {
                FoodToRemove.isMatched = false;
            }

        RemoveAndRefill(foodToRemove);
        //GameManager.Instance.ProcessTurn(foodToRemove.Count, _subtractMoves);
        yield return new WaitForSeconds(0.4f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }

    private void RemoveAndRefill(List<Food> _foodToRemove)
    {
        foreach (Food food in _foodToRemove)
        {
            int _xIndex = food.xIndex;
            int _yIndex = food.yIndex;

            //Destroy Food
            Destroy(food.gameObject);
            // Create blank node
            foodBoard[_xIndex, _yIndex] = new Node(true, null);
        }
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (foodBoard[x, y].food == null)
                {
                    Debug.Log("Checking X: " + x + " Y: " + y);
                    RefillFood(x, y);
                }

            }
        }
    }
    private void RefillFood(int x, int y)
    {
        int yOffset = 1;
        while (y + yOffset < height && foodBoard[x, y + yOffset].food == null)
        {
            yOffset++;
        }

        if (y + yOffset < height && foodBoard[x, y + yOffset].food != null)
        {
            Food foodAbove = foodBoard[x, y + yOffset].food.GetComponent<Food>();

            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, foodAbove.transform.position.z);
            foodAbove.moveToTarget(targetPos);

            foodAbove.SetIndicies(x, y);
            foodBoard[x, y] = foodBoard[x, y + yOffset];

            foodBoard[x, y + yOffset] = new Node(true, null);
        }

        if (y + yOffset == height)
        {
            SpawnFoodAtTop(x);
        }
    }

    private void SpawnFoodAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        int locationMoveTo = 8 - index;

        int randomIndex = Random.Range(0, foodPrefab.Length);
        GameObject newFood = Instantiate(foodPrefab[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newFood.transform.SetParent(foodParent.transform);
        // set indicies
        newFood.GetComponent<Food>().SetIndicies(x, index);
        // set it on foodBoard
        foodBoard[x, index] = new Node(true, newFood);
        // move it to the correct location
        Vector3 targetPos = new Vector3(newFood.transform.position.x, newFood.transform.position.y - locationMoveTo, newFood.transform.position.z);
        newFood.GetComponent<Food>().moveToTarget(targetPos);
    }

    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;
        for (int y = 7 ; y >= 0; y--)
        {
            if (foodBoard[x,y].food == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }

    #region CascadeFood

    #endregion

    #region MatchLogic
    private MatchResult SuperMatch(MatchResult _matchedResult)
    {
        if (_matchedResult.direction == MatchDirection.Horizontal || _matchedResult.direction == MatchDirection.LongHorizontal)
        {
            foreach (Food food in _matchedResult.connectedFoods)
            {
                List<Food> extraConnectedFood = new();

                CheckDirection(food, new Vector2Int(0, 1), extraConnectedFood);
                CheckDirection(food, new Vector2Int(0, -1), extraConnectedFood);

                if (extraConnectedFood.Count >= 2)
                {
                    Debug.Log("Super Match Found");
                    extraConnectedFood.AddRange(_matchedResult.connectedFoods);

                    return new MatchResult
                    {
                        connectedFoods = extraConnectedFood,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedFoods = _matchedResult.connectedFoods,
                direction = _matchedResult.direction
            };
        } else if (_matchedResult.direction == MatchDirection.Vertical || _matchedResult.direction == MatchDirection.LongVertical)
        {
            foreach (Food food in _matchedResult.connectedFoods)
            {
                List<Food> extraConnectedFood = new();

                CheckDirection(food, new Vector2Int(1, 0), extraConnectedFood);
                CheckDirection(food, new Vector2Int(-1, 0), extraConnectedFood);

                if (extraConnectedFood.Count >= 2)
                {
                    Debug.Log("Super Match Found");
                    extraConnectedFood.AddRange(_matchedResult.connectedFoods);

                    return new MatchResult
                    {
                        connectedFoods = extraConnectedFood,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedFoods = _matchedResult.connectedFoods,
                direction = _matchedResult.direction
            };
        }
        return null;
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
            return new MatchResult
            {
                connectedFoods = connectedFoods,
                direction = MatchDirection.Horizontal
            };
        } else if (connectedFoods.Count > 3)
        {
            return new MatchResult
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
            return new MatchResult
            {
                connectedFoods = connectedFoods,
                direction = MatchDirection.Vertical
            };
        }
        else if (connectedFoods.Count > 3)
        {
            return new MatchResult
            {
                connectedFoods = connectedFoods,
                direction = MatchDirection.LongVertical
            };
        } else
        {
            return new MatchResult
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
                } 
                else
                {
                    break;
                }
            } 
            else
            {
                break;
            }
        }
    }
    #endregion

    #region SwappingLogic
    // Select Food
    public void SelectFood(Food _food)
    {
        if (isProcessingMove)
        {
            return;
        }
        // If no food is selected
        if (selectedFood == null)
        {
            selectedFood = _food;
        }
        // If selected food is the same as the food we are selecting
        else if (selectedFood == _food)
        {
            selectedFood = null;
        }
        else if (selectedFood != _food)
        {
            SwapFood(selectedFood, _food);
            selectedFood = null;
        }
    }

    // Swap Food Logic
    private void SwapFood(Food _currentFood, Food _targetedFood)
    {
        if (!IsAdjacent(_currentFood, _targetedFood))
        {
            return;
        }

        DoSwap(_currentFood, _targetedFood);
        isProcessingMove = true;

        StartCoroutine(ProcessMatches(_currentFood, _targetedFood));
    }

    // Do Swap
    private void DoSwap(Food _currentFood, Food _targetedFood)
    {
        GameObject temp = foodBoard[_currentFood.xIndex, _currentFood.yIndex].food;

        foodBoard[_currentFood.xIndex, _currentFood.yIndex].food = foodBoard[_targetedFood.xIndex, _targetedFood.yIndex].food;
        foodBoard[_targetedFood.xIndex, _targetedFood.yIndex].food = temp;

        //Update Indicies
        int tempXIndex = _currentFood.xIndex;
        int tempYIndex = _currentFood.yIndex;

        _currentFood.xIndex = _targetedFood.xIndex;
        _currentFood.yIndex = _targetedFood.yIndex;
        _targetedFood.xIndex = tempXIndex;
        _targetedFood.yIndex = tempYIndex;

        //moves current potion to target potion (physically on the screen)
        _currentFood.moveToTarget(foodBoard[_targetedFood.xIndex, _targetedFood.yIndex].food.transform.position);
        //moves target potion to current potion (physically on the screen)
        _targetedFood.moveToTarget(foodBoard[_currentFood.xIndex, _currentFood.yIndex].food.transform.position);
    }

    // Is Adjecent
    private bool IsAdjacent(Food _currentFood, Food _targetedFood)
    {
        return Mathf.Abs(_currentFood.xIndex - _targetedFood.xIndex) + Mathf.Abs(_currentFood.yIndex - _targetedFood.yIndex) == 1;
    }

    private IEnumerator ProcessMatches(Food _currentFood, Food _targetedFood)
    {
        yield return new WaitForSeconds(0.2f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        } else
        {
            DoSwap(_currentFood, _targetedFood);
        }
        isProcessingMove = false;
    }


    #endregion
}

public class MatchResult
{
    public List<Food> connectedFoods;
    public MatchDirection direction;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}