using UnityEngine;
using System;

public class Cell
{
    Vector3 position;
    public Cell_Status status;

    public Cell(Vector3 pos)
    {
        position = pos;
        status = Cell_Status.EMPTY;
    }

    public Vector3 getPosition()
    {
        return position;
    }

    public void Draw(float radius)
    {
        switch (status)
        {
            case Cell_Status.EMPTY:
                Gizmos.color = Color.red;
                break;
            case Cell_Status.FULL:
                Gizmos.color = Color.green;
                break;
        }
        Gizmos.DrawSphere(this.position, radius);
    }


}