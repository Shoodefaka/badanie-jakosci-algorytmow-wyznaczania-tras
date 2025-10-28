using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using System.IO;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;

public class Grid : MonoBehaviour {
    public Transform player;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public GameObject roadPrefab;
    public GameObject shopPrefab;
    public GameObject homePrefab;
    public GameObject seekerPrefab;
    public GameObject pauseMenuUI;
    public GameObject algorithmOptionsPanel;
    public GameObject scoreTypeOptionsPanel;
    public GameObject buildBorder;
    public InputField cycleNumberInput;
    public InputField vehicleNumberInput;
    public float nodeRadius;
    public List<Node> gridHome = new List<Node>();
    public List<Node> gridShop = new List<Node>();
    public Vector3? lastClickPosition = null;
    public int stopScore = 0;
    public int pathTimeSum;
    public int pathTimeCount;
    public int closedSetSum;
    public int closedSetCount;
    Node[,] grid;
    float nodeDiamater;
    int gridSizeX, gridSizeY;
    private bool buildMode = false;
    private bool pauseMenuActive = false;
    Pathfinding pathClass;
    TextManager textManager;
    GameObject spawnedObject;
    ToggleGroup toggleGroupAlgorithm;
    private int cycleNumber;
    private int vehicleNumber;

    public int[,] tiles;

    void Awake() {
        pathClass = GetComponent<Pathfinding>();
        toggleGroupAlgorithm = algorithmOptionsPanel.GetComponent<ToggleGroup>();
        textManager = FindFirstObjectByType<TextManager>();
        nodeDiamater = nodeRadius * 2;
        pathTimeSum = 0;
        pathTimeCount = 0;
        closedSetSum = 0;
        closedSetCount = 0;
        //UnityEngine.Random.InitState(1506); // Setting a fixed seed
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiamater);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiamater);
        CreateGrid();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (buildMode) buildMode = false;
            if (pauseMenuActive) {
                pauseMenuUI.SetActive(false);
                pauseMenuActive = false;
            } else {
                pauseMenuUI.SetActive(true);
                pauseMenuActive = true;
            }
            
        }

        if (buildMode) {
            buildBorder.SetActive(true);
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                Node clickedNode = GetClickedNode();
                grid[clickedNode.cellX, clickedNode.cellY].ChangeType("road");
                Instantiate(roadPrefab, clickedNode.worldPosition, Quaternion.identity);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                Node clickedNode = GetClickedNode();
                grid[clickedNode.cellX, clickedNode.cellY].ChangeType("shop");
                gridShop.Add(clickedNode);
                spawnedObject = Instantiate(shopPrefab, clickedNode.worldPosition, Quaternion.identity);
                grid[clickedNode.cellX, clickedNode.cellY].spawnedObjectPosition = spawnedObject.transform.position;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                Node clickedNode = GetClickedNode();
                grid[clickedNode.cellX, clickedNode.cellY].ChangeType("home");
                gridHome.Add(clickedNode);
                Instantiate(homePrefab, clickedNode.worldPosition, Quaternion.identity);
            }
        } else {
            buildBorder.SetActive(false);
        }

        if (stopScore > 0) 
            if (textManager.startSimulation) {
                if (textManager.score == stopScore) {
                    textManager.startSimulation = false;
                    print("Average path time: " + (float)pathTimeSum / pathTimeCount);
                    print("Average closedSet value: " + (float)closedSetSum / closedSetCount);
                    pathTimeSum = 0;
                    pathTimeCount = 0;
                    closedSetSum = 0;
                    closedSetCount = 0;
                }    
            }
    }


    void CreateGrid() {
        grid = new Node[gridSizeX, gridSizeY];
        Vector2 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int i = 0; i < gridSizeX; i++) {
            for (int j = 0; j < gridSizeY; j++) {
                Vector2 worldPoint = worldBottomLeft + Vector2.right * (i * nodeDiamater + nodeRadius) +
                                                       Vector2.up * (j * nodeDiamater + nodeRadius);
                grid[i, j] = new Node(worldPoint, false, "none", i, j);
            }
        }
    }

    public List<Node> GetNeighbours(Node node) {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0) 
                    continue;
                if (x == -1 && y == -1)
                    continue;
                if (x == -1 && y == 1)
                    continue;
                if (x == 1 && y == -1)
                    continue;
                if (x == 1 && y == 1)
                    continue;

                int checkX = node.cellX + x;
                int checkY = node.cellY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node GetClickedNode() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Node clickedNode = NodeFromWorldPoint(mousePos);
        return clickedNode;
    }

    public Node NodeFromWorldPoint(Vector2 worldPosition) {
        float percentX = (worldPosition.x + gridWorldSize.x/2) /gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y/2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    public void SaveGridToFile() {
        using (StreamWriter writer = new StreamWriter("Assets/tilesData.txt")) {
            for (int i = 0; i < gridSizeX; i++) {
                string line = "";
                for (int j = 0; j < gridSizeY; j++) {
                    switch (grid[i, j].type) {
                        case "road":
                            line += "1";
                            if (j < gridSizeY - 1)
                                line += ",";
                            break;
                        case "shop":
                            line += "2";
                            if (j < gridSizeY - 1)
                                line += ",";
                            break;
                        case "home":
                            line += "3";
                            if (j < gridSizeY - 1)
                                line += ",";
                            break;
                        default:
                            line += "0";
                            if (j < gridSizeY - 1)
                                line += ",";
                            break;
                    }
                }
                writer.WriteLine(line);
            }
        }
        Debug.Log("Zapisano stan mapy do pliku tilesData.txt");
    }

    public void LoadGridFromFile() {
        List<int[]> rows = new List<int[]>();
        foreach (string line in File.ReadAllLines("Assets/tilesData.txt")) {
            string[] parts = line.Split(",");
            int[] row = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++) {
                row[i] = int.Parse(parts[i]);
            }
            rows.Add(row);
        }

        for (int i = 0; i < gridSizeX; i++) {
            for (int j = 0; j < gridSizeY; j++) { 
                switch (rows[i][j]) {
                    case 1:
                        grid[i, j].ChangeType("road");
                        Instantiate(roadPrefab, grid[i,j].worldPosition, Quaternion.identity);
                        break;
                    case 2:
                        grid[i, j].ChangeType("shop");
                        gridShop.Add(grid[i, j]);
                        Instantiate(shopPrefab, grid[i, j].worldPosition, Quaternion.identity);
                        break;
                    case 3:
                        grid[i, j].ChangeType("home");
                        gridHome.Add(grid[i, j]);
                        Instantiate(homePrefab, grid[i, j].worldPosition, Quaternion.identity);
                        break;
                    default:
                        break;
                }
            }
        }
        Debug.Log("Wczytano stan mapy z pliku tilesData.txt");
    }

    public void ManualBuildClicked() {
        buildMode = true;
        pauseMenuUI.SetActive(false);
        pauseMenuActive = false;
    }

    public void StartSimulationCliced() {
        Toggle toggle = toggleGroupAlgorithm.ActiveToggles().FirstOrDefault();
        pauseMenuUI.SetActive(false);
        pauseMenuActive = false;

        int.TryParse(cycleNumberInput.GetComponent<InputField>().text, out cycleNumber);
        int.TryParse(vehicleNumberInput.GetComponent<InputField>().text, out vehicleNumber);

        stopScore += vehicleNumber * cycleNumber;
        print(toggle.name);

        if (toggle.name == "AStarToggle") {
            textManager.startSimulation = true;
            StartCoroutine(SpawnSeekers());
        }
        if (toggle.name == "JPSToggle") {
            textManager.startSimulation = true;
            StartCoroutine(SpawnSeekersJPS());
        }
    }

    IEnumerator SpawnSeekers() {
        for (int i = 0; i < cycleNumber; i++) {
            for (int j = 0; j < vehicleNumber; j++) {
                GameObject seekerObj = Instantiate(seekerPrefab, gridHome[UnityEngine.Random.Range(0, gridHome.Count)].worldPosition, Quaternion.identity);
                Unit unit = seekerObj.GetComponent<Unit>();
                unit.StartRequest(false);
            }
            yield return new WaitForSeconds(1f);
        }
    }
    IEnumerator SpawnSeekersJPS() {
        for (int i = 0; i < cycleNumber; i++) {
            for (int j = 0; j < vehicleNumber; j++) {
                GameObject seekerObj = Instantiate(seekerPrefab, gridHome[UnityEngine.Random.Range(0, gridHome.Count)].worldPosition, Quaternion.identity);
                Unit unit = seekerObj.GetComponent<Unit>();
                if (unit != null) {
                    unit.useJPS = true;
                }
                yield return null;
                unit.StartRequest(true);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public int Width => grid.GetLength(0);
    public int Height => grid.GetLength(1);

    public int MaxSize {
        get {
            return gridSizeX * gridSizeY;
        }
    }

    public bool InBounds(int x, int y) {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public Node GetNodeAt(int x, int y) {
        return grid[x, y];
    }

    public bool IsWalkableAt(int x, int y) {
        return InBounds(x, y) && grid[x, y].isWalkable;
    }

    //Drawing Cubes in Editor for debugging
    private void OnDrawGizmos() {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));
        if (grid != null) {
            foreach (Node n in grid) {
                Gizmos.DrawCube(n.worldPosition, Vector2.one * (nodeDiamater - .1f));
            }
        }
    }
}
