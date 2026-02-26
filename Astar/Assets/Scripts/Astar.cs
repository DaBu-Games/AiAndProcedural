using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NUnit.Framework;

public class Astar
{
    /// <summary>
    /// TODO: Implement this function so that it returns a list of Vector2Int positions which describes a path from the startPos to the endPos
    /// Note that you will probably need to add some helper functions
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    
    private List<Node> openNodes = new List<Node>();
    private List<Node> closedNodes = new List<Node>();
    private int _evenStep = 10;
    private Vector2Int _endPosition;
    
    public List<Vector2Int> FindPathToTarget(Vector2Int startPos, Vector2Int endPos, Cell[,] grid)
    {
        openNodes.Clear();
        closedNodes.Clear();
        _endPosition = endPos;
        
        Node firstNode = new Node(startPos, null, 0, 0);
        openNodes.Add(firstNode);

        while (openNodes.Count > 0)
        {
            if(openNodes.Count > 1)
                openNodes = openNodes.OrderBy(n => n.FScore).ToList();
            
            Node currentNode = openNodes[0];
            openNodes.RemoveAt(0);
            
            if (currentNode.position == endPos)
                return GetPath(currentNode);
            
            ProcessNeighbours(currentNode, grid);
            
            closedNodes.Add(currentNode);
        }
        
        return null;
    }

    private void ProcessNeighbours(Node currentNode, Cell[,] grid)
    {
        List<Cell> neighbours = grid[currentNode.position.x, currentNode.position.y].GetNeighbours(grid);
        foreach (var neighbour in neighbours)
        {
            HandelNeighbour(neighbour, currentNode, grid);
        }
    }

    private void HandelNeighbour(Cell neighbour, Node currentNode, Cell[,] grid)
    {
        if(HasWall(grid[currentNode.position.x, currentNode.position.y], neighbour.gridPosition))
            return;
        
        int gScore = GetGScore(currentNode.GScore);
        int hScore = GetHScore(neighbour.gridPosition);
        Node newNode = new Node(neighbour.gridPosition, currentNode, gScore, hScore);
                
        Node openNode = openNodes.Find(x => x.position == neighbour.gridPosition);
        if(openNode != null && openNode.FScore < newNode.FScore)
            return;
                
        Node closedNode = closedNodes.Find(x => x.position == neighbour.gridPosition);
        if(closedNode != null && closedNode.FScore < newNode.FScore)
            return;
                
        openNodes.Add(newNode);
    }

    private bool HasWall(Cell currentCell, Vector2Int neighborPos)
    {
        Vector2Int direction = neighborPos - currentCell.gridPosition;

        Wall wallToCheck;

        if (direction == Vector2Int.up)
            wallToCheck = Wall.UP;
        else if (direction == Vector2Int.down)
            wallToCheck = Wall.DOWN;
        else if (direction == Vector2Int.left)
            wallToCheck = Wall.LEFT;
        else if (direction == Vector2Int.right)
            wallToCheck = Wall.RIGHT;
        else 
            return true;
        
        return currentCell.HasWall(wallToCheck);
    }

    private int GetGScore(int currentScore)
    {
        return currentScore + _evenStep;
    }

    private int GetHScore(Vector2Int startPos)
    {
        return Mathf.Abs(startPos.x - _endPosition.x) + Mathf.Abs(startPos.y - _endPosition.y);
    }

    private List<Vector2Int> GetPath(Node endNode)
    {
        if (endNode == null)
            return null;
        
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;
        
        while (currentNode.parent != null)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }
        
        path.Reverse();
        return path;
    }

    /// <summary>
    /// This is the Node class you can use this class to store calculated FScores for the cells of the grid, you can leave this as it is
    /// </summary>
    public class Node
    {
        public Vector2Int position; //Position on the grid
        public Node parent; //Parent Node of this node

        public int FScore { //GScore + HScore
            get { return GScore + HScore; }
        }
        public int GScore; //Current Travelled Distance
        public int HScore; //Distance estimated based on Heuristic

        public Node() { }
        public Node(Vector2Int position, Node parent, int GScore, int HScore)
        {
            this.position = position;
            this.parent = parent;
            this.GScore = GScore;
            this.HScore = HScore;
        }
    }
}
