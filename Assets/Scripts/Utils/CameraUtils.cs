using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { Up, Right, Down, Left }
public static class CameraUtils
{
    public static bool IsInCameraBound(Camera cam, Vector3 pos, out Direction outDirection)
    {
        Vector3 pointOnScreen = Camera.main.WorldToScreenPoint(pos);
        pointOnScreen.x /= Screen.width;
        pointOnScreen.y /= Screen.height;

        bool isOutsideH = pointOnScreen.x < 0 || pointOnScreen.x > 1;
        bool isOutsideV = pointOnScreen.y < 0 || pointOnScreen.y > 1;

        outDirection = Direction.Up;

        if (!isOutsideH && !isOutsideV)
            return true;

        if (isOutsideH)
        {
            if (pointOnScreen.x < 0.5f)
                outDirection = Direction.Left;
            else
                outDirection = Direction.Right;
        }
        else if (isOutsideV)
        {
            if (pointOnScreen.y < 0.5f)
                outDirection = Direction.Down;
            else
                outDirection = Direction.Up;
        }

        return false;
    }
}
