using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Utils {
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void GenerateObstacle(ref GameObject obstacle, float x, float z) {
        obstacle.transform.localScale = new Vector3(obstacle.transform.localScale.x, obstacle.transform.localScale.y * 2, obstacle.transform.localScale.z);
        Mesh m = obstacle.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = m.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            float newX = 0;
            float newZ = 0;
            if(vertices[i].x > 0)
            {
                newX += x / 2;
            }
            else
            {
                newX -= x / 2;
            }
            if (vertices[i].z > 0)
            {
                newZ += z / 2;
            }
            else
            {
                newZ -= z / 2;
            }
            vertices[i].Set(newX, vertices[i].y, newZ);
        }
        m.vertices = vertices;
        m.RecalculateBounds();
        if (obstacle.GetComponent<NavMeshObstacle>() != null) {
            obstacle.GetComponent<NavMeshObstacle>().size = new Vector3(x, 1, z);
        }
    }

}
