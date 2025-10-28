using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{

    PathRequestManager requestManager;
    Grid grid;
    public List<Node> pathGizmos = new List<Node>();

    void Awake() {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();

    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos) {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector2 startPos, Vector2 targetPos) {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode.isWalkable && targetNode.isWalkable) {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode) {
                    sw.Stop();
                    print("Path found: " + sw.ElapsedTicks + " ticks");
                    print("ClosedSet: " + closedSet.Count);
                    grid.pathTimeSum += (int)sw.ElapsedTicks;
                    grid.pathTimeCount++;
                    grid.closedSetSum += closedSet.Count;
                    grid.closedSetCount++;
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
                    if (!neighbour.isWalkable || closedSet.Contains(neighbour))
                        continue;

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }
        }
        yield return null;
        if (pathSuccess) {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    public void StartFindPathJPS(Vector3 startPos, Vector3 targetPos) {
        StartCoroutine(FindPathJPS(startPos, targetPos));
    }

    IEnumerator FindPathJPS(Vector2 startPos, Vector2 targetPos) {

        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode.isWalkable && targetNode.isWalkable) {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            startNode.gCost = 0;
            startNode.hCost = GetDistance(startNode, targetNode);
            startNode.parent = null;
            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Node current = openSet.RemoveFirst();
                if (current == targetNode) {
                    sw.Stop();
                    print("JPS Path found: " + sw.ElapsedTicks + " ticks");
                    print("JPS ClosedSet: " + closedSet.Count);
                    grid.pathTimeSum += (int)sw.ElapsedTicks;
                    grid.pathTimeCount++;
                    grid.closedSetSum += closedSet.Count;
                    grid.closedSetCount++;
                    pathSuccess = true;
                    break;
                }

                closedSet.Add(current);

                foreach (Node succ in IdentifySuccessors(current, targetNode)) {
                    if (closedSet.Contains(succ)) continue;

                    int tentativeG = current.gCost + GetDistance(current, succ);
                    if (tentativeG < succ.gCost || !openSet.Contains(succ)) {
                        succ.gCost = tentativeG;
                        succ.hCost = GetDistance(succ, targetNode);

                        succ.parent = current;

                        if (!openSet.Contains(succ)) {
                            openSet.Add(succ);
                        }
                        else {
                            openSet.UpdateItem(succ);
                        }
                    }
                }
            }
        }

        yield return null;
        if (pathSuccess) {
            waypoints = RetraceJPSPath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    List<Node> IdentifySuccessors(Node current, Node target) {
        List<Node> successors = new List<Node>();

        foreach (var dir in PruneDirections(current)) {
            Node jp = Jump(current, dir.dx, dir.dy, target);
            if (jp != null && jp.isWalkable) {
                successors.Add(jp);
            }
        }
        return successors;
    }

    Node Jump(Node current, int dx, int dy, Node target) {
        int x = current.cellX;
        int y = current.cellY;

        while (true) {
            x += dx;
            y += dy;

            if (!grid.InBounds(x, y) || !grid.IsWalkableAt(x, y))
                return null;

            Node node = grid.GetNodeAt(x, y);

            if (node == target) {
                return node;
            }
                
            if (HasForcedNeighbour(node, dx, dy)) {
                return node;
            }
        }
    }

    bool HasForcedNeighbour(Node node, int dx, int dy) {
        int x = node.cellX;
        int y = node.cellY;

        if (dx != 0) {
            if (grid.InBounds(x - dx, y + 1) && !grid.IsWalkableAt(x - dx, y + 1) &&
                grid.InBounds(x, y + 1) && grid.IsWalkableAt(x, y + 1)) {
                return true;
            }
                
            if (grid.InBounds(x - dx, y - 1) && !grid.IsWalkableAt(x - dx, y - 1) &&
                grid.InBounds(x, y - 1) && grid.IsWalkableAt(x, y - 1)) {
                return true;
            }
        }

        if (dy != 0) {
            if (grid.InBounds(x + 1, y - dy) && !grid.IsWalkableAt(x + 1, y - dy) &&
                grid.InBounds(x + 1, y) && grid.IsWalkableAt(x + 1, y)) {
                return true;
            }

            if (grid.InBounds(x - 1, y - dy) && !grid.IsWalkableAt(x - 1, y - dy) &&
                grid.InBounds(x - 1, y) && grid.IsWalkableAt(x - 1, y)) {
                return true;
            }
        }

        return false;
    }

    List<(int dx, int dy)> PruneDirections(Node current) {
        List<(int dx, int dy)> dirs = new List<(int dx, int dy)>();

        if (current.parent == null) {
            TryAddDir(current, 1, 0, dirs);
            TryAddDir(current, -1, 0, dirs);
            TryAddDir(current, 0, 1, dirs);
            TryAddDir(current, 0, -1, dirs);
            return dirs;
        }

        int px = current.parent.cellX;
        int py = current.parent.cellY;
        int dx = Math.Sign(current.cellX - px);
        int dy = Math.Sign(current.cellY - py);

        if (dx != 0) {
            TryAddDir(current, dx, 0, dirs);
            if (grid.InBounds(current.cellX, current.cellY + 1) && grid.IsWalkableAt(current.cellX, current.cellY + 1) &&
                grid.InBounds(current.cellX - dx, current.cellY + 1) && !grid.IsWalkableAt(current.cellX - dx, current.cellY + 1))
                TryAddDir(current, 0, 1, dirs);
            if (grid.InBounds(current.cellX, current.cellY - 1) && grid.IsWalkableAt(current.cellX, current.cellY - 1) &&
                grid.InBounds(current.cellX - dx, current.cellY - 1) && !grid.IsWalkableAt(current.cellX - dx, current.cellY - 1))
                TryAddDir(current, 0, -1, dirs);
        }

        if (dy != 0) {
            TryAddDir(current, 0, dy, dirs);
            if (grid.InBounds(current.cellX + 1, current.cellY) && grid.IsWalkableAt(current.cellX + 1, current.cellY) &&
                grid.InBounds(current.cellX + 1, current.cellY - dy) && !grid.IsWalkableAt(current.cellX + 1, current.cellY - dy))
                TryAddDir(current, 1, 0, dirs);
            if (grid.InBounds(current.cellX - 1, current.cellY) && grid.IsWalkableAt(current.cellX - 1, current.cellY) &&
                grid.InBounds(current.cellX - 1, current.cellY - dy) && !grid.IsWalkableAt(current.cellX - 1, current.cellY - dy))
                TryAddDir(current, -1, 0, dirs);
        }

        return dirs;
    }


    void TryAddDir(Node from, int dx, int dy, List<(int dx, int dy)> dirs) {
        int nx = from.cellX + dx;
        int ny = from.cellY + dy;
        if (grid.InBounds(nx, ny) && grid.IsWalkableAt(nx, ny)) {
            dirs.Add((dx, dy));
        }
    }

    Vector3[] RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        pathGizmos = path;
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] RetraceJPSPath(Node startNode, Node endNode) {
        List<Vector3> points = new List<Vector3>();
        Node current = endNode;

        while (current != null) {
            points.Add(current.worldPosition);
            if (current == startNode) break;
            current = current.parent;
        }

        points.Reverse();
        return points.ToArray();
    }

    Vector3[] SimplifyPath(List<Node> path) {
        List<Vector3> waypoints = new List<Vector3>();

        for (int i = 1; i < path.Count; i++) {
               waypoints.Add(path[i].worldPosition);
        }
        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB) { 
        int dstX = Mathf.Abs(nodeA.cellX - nodeB.cellX);
        int dstY = Mathf.Abs(nodeA.cellY - nodeB.cellY);
        return 10*(dstX + dstY);
    }
}
