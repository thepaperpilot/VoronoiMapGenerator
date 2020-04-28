using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VoronoiMap;
using static Geometry;

//Here a B+ tree was chosen because the data (site centers) is not useful on its own for determining search path.
//This is because we need two adjacent beach arcs site centers to determine a break point to know if we need to look left or right.
//In an ordinary binary tree, adjacent beach sites in the order could be any number of levels apart.
//As an additional benefit, it maintains the actual beach line on a single level.

public class BeachTree
{
    static readonly int b = 6; //branching factor
    Node root = null;

    public void Insert(Cell c)
    {
        if(root == null)
        {
            root = new Node();
            root.links[0] = c;
            root.keys[0] = float.PositiveInfinity;
            root.count++;
        }
        else
        {
            Node p = FindParent(c.pos.x);
            Node[] buckets = new Node[] { p };
            if(p.count+2 > b - 1) //Split before adding
            {
                buckets = SplitLeaf(p, c.pos.x);
            }

        }
    }

    private Node[] SplitLeaf(Node n, float k)
    {
        if(n.parent == null)
        {
            root = new Node();
            n.parent = root;
        }
        else if(n.parent.count+1 > b)
        {
            SplitInternal(n.parent);
        }
        Node newSibling = new Node();
        newSibling.parent = n.parent;
        newSibling.links[b - 1] = n.links[b - 1];
        n.links[b - 1] = newSibling;

        int left = Mathf.CeilToInt((n.count) / 2.0f); //2 or 3
        int right = Mathf.FloorToInt((n.count) / 2.0f); //2
        if (k <= n.keys[left - 2])//New elements will go in left so put more stuff on the right
        {
            int temp = left;
            left = right;
            right = left;
            for(int i = 0; i+left < left + right; i++)
            {
                newSibling.links[i] = n.links[i + left];
                newSibling.keys[i] = n.keys[i + left];
            }
            InsertInternal(n.parent, newSibling);
            return new Node[] { n };
        }
        else if(k > n.keys[left-1])//New elements will go in right so put more stuff on the left
        {
            left++;
            right--;
            for(int i = 0; i+left < left + right; i++)
            {
                newSibling.links[i] = n.links[i + left];
                newSibling.keys[i] = n.keys[i + left];
            }
            InsertInternal(n.parent, newSibling);
            return new Node[] { newSibling };
        }
        else//Split the new items between the two nodes
        {
            for (int i = 0; i + left < left + right; i++)
            {
                newSibling.links[i] = n.links[i + left];
                newSibling.keys[i] = n.keys[i + left];
            }
            InsertInternal(n.parent, newSibling);
            return new Node[] { n, newSibling };
        }
    }

    private void SplitInternal(Node n)
    {
        if (n.parent == null)
        {
            root = new Node();
            n.parent = root;
        }
        else if (n.parent.count + 1 > b)
        {
            SplitInternal(n.parent);
        }
        Node newSibling = new Node();

        int left = Mathf.FloorToInt((n.count) / 2.0f); //3
        int right = Mathf.CeilToInt((n.count) / 2.0f); //3
        for (int i = 0; i + left < left + right; i++)
        {
            newSibling.links[i] = n.links[i + left];
            newSibling.keys[i] = n.keys[i + left];
        }
        InsertInternal(n.parent, newSibling);
    }

    private void InsertInternal(Node p, Node n)
    {

    }

    private void AddRecord(Node p, Cell c)
    {

    }

    public Cell Search(float k)
    {
        Node parent = FindParent(k);
        return LeafSearch(parent, k);
    }

    Node FindParent(float k)
    {
        if(root == null)
        {
            return null;
        }
        return TreeSearch(root, k);
    }

    Node TreeSearch(Node n, float k)
    {
        if(n.links[0].GetType() == typeof(Cell))
        {
            return n;
        }
        else
        {
            for(int i = 0; i < n.count; i++)
            {
                if (k <= n.keys[i])
                {
                    return TreeSearch((Node)n.links[i], k);
                }
            }
            return TreeSearch((Node)n.links[n.count], k);
        }
        
    }

    Cell LeafSearch(Node n, float k)
    {
        for (int i = 0; i < n.count; i++)
        {
            if (k <= n.keys[i])
            {
                return (Cell)n.links[i];
            }
        }
        return (Cell)n.links[n.count];
    }

    private class Node
    {
        public int count = 0;
        public float[] keys;
        public System.Object[] links;
        public Node parent;

        public Node()
        {
            keys = new float[b-1];
            links = new System.Object[b];
        }

        public void Update()
        {

        }

        public void UpdateUp()
        {

        }
    }
}


