using System.Collections.Generic;
using UnityEngine;

public static class Pathfinder
{
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new();
        Vector2Int current = start;

        while (current != end)
        {
            if (current.x < end.x) current.x++;
            else if (current.x > end.x) current.x--;
            else if (current.y < end.y) current.y++;
            else if (current.y > end.y) current.y--;

            path.Add(current);
        }

        return path;
    }
}
