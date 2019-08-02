using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CellularAutomata : MonoBehaviour
{
    [Header("Parameters")]
    public int CellNumWidth;
    public int CellNumHeight;
    public int maxIterations;
    public MAP_TYPE pattern;
    public bool optimized;
    [Range(0, 100)]
    public int walkablePercentage;
    [Header("Other")]
    public GameObject obstaclePrefab;


    private float cellSizeWidth;
    private float cellSizeHeight;
    private Segment Width;
    private Segment Height;
    private Cell[,] grid;

    #region ui handlers
    public void setOptimized(bool input)
    {
        Debug.Log(input);
        optimized = input;
    }

    public void setPattern(int i)
    {
        pattern = (CellularAutomata.MAP_TYPE)i;
    }
    #endregion

    //generate final map
    public void GenerateMap()
    {
        bool generation = true;
        //get mesh position
        float ix = transform.position.x;
        float iz = transform.position.z;
        Width = new Segment(ix, ix);
        Height = new Segment(iz, iz);
        //calculate bounding vertices
        GetBoundingVertices();
        //calculate cell size
        CalculateCellSizes();
        //creating Grid obj
        CreateGrid();
        //iiteration
        int i = 0;
        while (i < maxIterations)
        {
            Iterate();
            i++;
        }
        //if it's optimized
        if (optimized) {
            generation = FloodFill();
        }
        if(generation) GenerateObstacles();
    }

    //generate obstacles on the grid
    void GenerateObstacles()
    {
        RemoveObstacles();
        //genero gli ostacoli
        for (int x = 0; x < CellNumWidth; x++)
        {
            for (int z = 0; z < CellNumHeight; z++)
            {
                if (grid[x, z].status == Cell_Status.FULL)
                {
                    GameObject ob = Instantiate(obstaclePrefab, grid[x, z].getPosition(), obstaclePrefab.transform.rotation);
                    Utils.GenerateObstacle(ref ob, cellSizeWidth, cellSizeHeight);
                    ob.transform.parent = this.transform;
                }
            }
        }
    }

    //reset
    void Reset()
    {
        //restart value
        grid = null;
        Width = null;
        Height = null;
        cellSizeWidth = 0;
        cellSizeHeight = 0;
    }

    //remove obstacles gameobjects
    void RemoveObstacles()
    {
        for (int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);
    }

    //get 
    void GetBoundingVertices()
    {
        Mesh m = GetComponent<MeshFilter>().mesh;
        List<Vector3> vertices = new List<Vector3>();
        m.GetVertices(vertices);
        for (int i = 0; i < vertices.Count / 3; i++)
        {
            Vector3 worldCoordinate = transform.TransformPoint(vertices[i]);
            Width.max = Mathf.Max(Width.max, worldCoordinate.x);
            Width.min = Mathf.Min(Width.min, worldCoordinate.x);
            Height.max = Mathf.Max(Height.max, worldCoordinate.z);
            Height.min = Mathf.Min(Height.min, worldCoordinate.z);

        }
    }

    //calculate cell size based on grid width and height
    void CalculateCellSizes()
    {
        cellSizeWidth = Width.Length() / CellNumWidth;
        cellSizeHeight = Height.Length() / CellNumHeight;
    }

    //init grid obj
    void CreateGrid()
    {
        grid = new Cell[CellNumWidth, CellNumHeight];
        int ix = 0;
        for (float x = Width.min; x < Width.max; x += cellSizeWidth)
        {
            int iz = 0;
            for (float z = Height.min; z < Height.max; z += cellSizeHeight)
            {
                try
                {
                    grid[ix, iz] = new Cell(new Vector3(x + cellSizeWidth / 2, transform.position.y, z + cellSizeHeight / 2));
                    //init with 45% prob to be Empty
                    Cell_Status status;
                    if (Random.Range(0, 101) <= 45)
                    {
                        status = Cell_Status.EMPTY;
                    }
                    else
                    {
                        status = Cell_Status.FULL;
                    }
                    grid[ix, iz].status = status;
                }

                catch (System.IndexOutOfRangeException e)
                {
                    continue;
                    //due to approximation, is it possible that the size of the cell is not correct and then the for can break
                }
                iz++;
            }
            ix++;
        }
    }

    //pass
    void Iterate()
    {
        Cell[,] newGrid = (Cell[,])grid.Clone();

        int i = 0;
        for (int x = 0; x < CellNumWidth; x++)
        {
            for (int z = 0; z < CellNumHeight; z++)
            {
                ValidateCellStatus(newGrid, x, z);
                i++;
            }
        }
        grid = (Cell[,])newGrid.Clone();
        if(optimized) Optimize();

    }

    //core cellular automata
    void ValidateCellStatus(Cell[,] newGrid, int x, int z)
    {
        int layer1 = 0;
        int layer2 = 0;
        for (int nx = x - 2; nx <= x + 2; nx++)
        {
            for (int nz = z - 2; nz <= z + 2; nz++)
            {
                if (nx >= 0 && nx <= CellNumWidth - 1 && nz >= 0 && nz <= CellNumHeight - 1)
                {
                    int layer = Mathf.Max(Mathf.Abs(x - nx), Mathf.Abs(z - nz));
                    if (layer == 1 && grid[nx, nz].status == Cell_Status.FULL) layer1++;
                    else if (layer == 2 && grid[nx, nz].status == Cell_Status.FULL) layer2++;
                }
            }
        }

        switch (pattern)
        {
            case MAP_TYPE.DISTRICT:
                if (layer1 > 4 || layer2 < Random.Range(2, 5))
                {
                    newGrid[x, z].status = Cell_Status.FULL;
                }
                else
                {
                    newGrid[x, z].status = Cell_Status.EMPTY;
                }
                break;
            case MAP_TYPE.SIMPLE_MAZE:
                if (layer1 > 4 || layer2 < 5)
                {
                    newGrid[x, z].status = Cell_Status.FULL;
                }
                else
                {
                    newGrid[x, z].status = Cell_Status.EMPTY;
                }
                break;
            case MAP_TYPE.ADVANCED_MAZE:
                if (grid[x, z].status == Cell_Status.FULL)
                {
                    if (layer1 >= 4) newGrid[x, z].status = Cell_Status.EMPTY;
                }
                else
                {
                    if (layer1 < 3) newGrid[x, z].status = Cell_Status.FULL;
                }
                break;
            case MAP_TYPE.ISLANDS:
                if (layer1 >= 6)
                {
                    newGrid[x, z].status = Cell_Status.FULL;
                }
                else if (layer1 <= 3)
                {
                    newGrid[x, z].status = Cell_Status.EMPTY;
                }
                break;
            case MAP_TYPE.CAVE:
                if (grid[x, z].status == Cell_Status.FULL)
                {
                    if (layer1 < 3) newGrid[x, z].status = Cell_Status.EMPTY;
                }
                else
                {
                    if (layer1 > 5) newGrid[x, z].status = Cell_Status.FULL;
                }
                break;
        }
    }

    //optimization algorithm
    void Optimize()
    {
        Cell[,] newGrid = (Cell[,])grid.Clone();
        for (int x = 0; x < CellNumWidth; x++)
        {
            for (int z = 0; z < CellNumHeight; z++)
            {
                if (newGrid[x, z].status == Cell_Status.FULL)
                {
                    bool disjointed = false;
                    //check if the current cell is interrupting a path
                    //left-right
                    if ((x - 1 < 0 || newGrid[x - 1, z].status == Cell_Status.EMPTY) && (x + 1 >= CellNumWidth || newGrid[x + 1, z].status == Cell_Status.EMPTY)) disjointed = true;
                    //up-down
                    if ((z + 1 >= CellNumHeight || newGrid[x, z + 1].status == Cell_Status.EMPTY) && (z - 1 < 0 || newGrid[x, z - 1].status == Cell_Status.EMPTY)) disjointed = true;
                    //diagonal /
                    if ((x - 1 < 0 || z - 1 < 0 || newGrid[x - 1, z - 1].status == Cell_Status.EMPTY) && (x + 1 >= CellNumWidth || z + 1 >= CellNumHeight || newGrid[x + 1, z + 1].status == Cell_Status.EMPTY)) disjointed = true;
                    //diagonal \
                    if ((x - 1 < 0 || z + 1 >= CellNumHeight || newGrid[x - 1, z + 1].status == Cell_Status.EMPTY) && (x + 1 >= CellNumWidth || z - 1 < 0 || newGrid[x + 1, z - 1].status == Cell_Status.EMPTY)) disjointed = true;
                    if (disjointed)
                    {
                        newGrid[x, z].status = Cell_Status.EMPTY;
                    }
                }
            }
        }
        grid = (Cell[,])newGrid.Clone();
    }

    //floodfill algorithm for optimization and check validation
    bool FloodFill()
    {
        bool result = false;
        //minimum number of cells empty required
        int maxCoverage = (CellNumWidth * CellNumHeight) * walkablePercentage / 100;
        Cell[,] newGrid = grid;
        for (int x = 0; x < CellNumWidth; x++)
        {
            for (int z = 0; z < CellNumHeight; z++)
            {
                if (newGrid[x, z].status == Cell_Status.EMPTY)
                {
                    Queue<int[]> positions = new Queue<int[]>();
                    int counter = 0;
                    positions.Enqueue(new int[] { x, z });
                    List<int[]> visited = new List<int[]>();
                    while (positions.Count > 0)
                    {
                        int[] c = positions.Dequeue();
                        visited.Add(c);
                        counter++;
                        newGrid[c[0], c[1]].status = Cell_Status.CHECKED;
                        //per ciascuno degli adiacenti
                        //left
                        if (c[0] - 1 >= 0 && newGrid[c[0] - 1, c[1]].status == Cell_Status.EMPTY) {
                            newGrid[c[0] - 1, c[1]].status = Cell_Status.CHECKED;
                            positions.Enqueue(new int[] { c[0] - 1, c[1] });
                        }
                        //right
                        if (c[0] + 1 < CellNumWidth && newGrid[c[0] + 1, c[1]].status == Cell_Status.EMPTY) {
                            newGrid[c[0] + 1, c[1]].status = Cell_Status.CHECKED;
                            positions.Enqueue(new int[] { c[0] + 1, c[1] });
                        }
                        //up
                        if (c[1] - 1 >= 0 && newGrid[c[0], c[1] - 1].status == Cell_Status.EMPTY) {
                            newGrid[c[0], c[1] - 1].status = Cell_Status.CHECKED;
                            positions.Enqueue(new int[] { c[0], c[1] - 1 });
                        }
                        //down
                        if (c[1] + 1 < CellNumHeight && newGrid[c[0], c[1] + 1].status == Cell_Status.EMPTY) {
                            newGrid[c[0], c[1] + 1].status = Cell_Status.CHECKED;
                            positions.Enqueue(new int[] { c[0], c[1] + 1 });
                        }
                    }
                    Debug.Log("walkable area: " + ((float)counter / (float)(CellNumWidth * CellNumHeight) * 100) + "%");
                    if (counter >= maxCoverage) result = true;
                    else {
                        //remove closed area
                        foreach (int[] elem in visited) {
                            newGrid[elem[0], elem[1]].status = Cell_Status.FULL;
                        }
                    }


                }
            }
        }
        return result;
    }

    public enum MAP_TYPE
    {
        DISTRICT, SIMPLE_MAZE, ADVANCED_MAZE, ISLANDS, CAVE
    }
}
