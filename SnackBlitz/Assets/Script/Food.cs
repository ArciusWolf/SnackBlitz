using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public FoodType foodType;

    public int xIndex;
    public int yIndex;

    public bool isMatched;
    private Vector2 currentPos;
    private Vector2 targetPos;

    public bool isMoving;

    public Food(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    public void SetIndicies(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }
}

public enum FoodType
{
    Bacon,
    Cheese,
    Pie,
    Pretzel,
    Shrimp,
    Steak
}