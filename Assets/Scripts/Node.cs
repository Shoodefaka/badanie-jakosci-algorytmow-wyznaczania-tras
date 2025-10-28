using UnityEngine;

public class Node : IHeapItem<Node>
{
    public Vector2 worldPosition;
    public Vector3 spawnedObjectPosition;
    public bool isWalkable;
    public string type;
    public int cellX;
    public int cellY;
    public int gCost;
    public int hCost;
    public Node parent;
    int heapIndex;

    public Node(Vector2 _position, bool _isWalkable, string _type, int _cellX, int _cellY) {
        worldPosition = _position;
        isWalkable = _isWalkable;
        type = _type;
        this.cellX = _cellX;
        this.cellY = _cellY;
    }

    public void ChangeType(string _type) {
        type = _type;
        isWalkable = true;
    }

    public void ChangeIsWalkable(bool _isWalkable) {
        isWalkable = _isWalkable;
    }

    public int fCost {
        get {
            return gCost + hCost;
        }
    }

    public int HeapIndex {
        get {
            return heapIndex;
        }
        set {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare) {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0) {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }

}
