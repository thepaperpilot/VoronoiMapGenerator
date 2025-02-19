﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VoronoiMap;
using static Geometry;

public class BeachLine
{
    private List<Cell> arcs;

    public BeachLine()
    {
        arcs = new List<Cell>();
    }

    public Cell Search(float k, float y)
    {
        return arcs[SearchIndex(k, y)];
    }

    public HitInfo Insert(Cell c)
    {
        //Validate(c.pos.y);
        if(arcs.Count == 0)
        {
            Debug.Log(c);
            arcs.Insert(0,c);
            return new HitInfo(null, null, null);
        }
        int index = SearchIndex(c.pos.x, c.pos.y);
        Cell hit = arcs[index];
        Cell left = index > 0 ? arcs[index - 1] : null;
        Cell right = index < arcs.Count - 1 ? arcs[index + 1] : null;
        HitInfo output = new HitInfo(hit, left, right);
        arcs.Insert(index + 1, c);
        arcs.Insert(index + 2, hit);

        return output;
    }

    public bool Delete(Cell left, Cell del, Cell right, Vector2 pos)
    {
        int index = SearchIndex(pos.x, pos.y+0.0001f);
        Cell hit = arcs[index];
        if(hit == del)
        {
            arcs.RemoveAt(index);
            return true;
        }
        else if(hit == left && index+2 < arcs.Count)
        {
            if(arcs[index+1] == del && arcs[index+2] == right)
            {
                arcs.RemoveAt(index + 1);
                return true;
            }
        }
        else if(hit == right && index -2 >= 0)
        {
            if(arcs[index-1] == del && arcs[index-2] == left)
            {
                arcs.RemoveAt(index - 1);
                return true;
            }
        }
        return false;
    }

    private int SearchIndex(float k, float y)
    {
        return BinarySearch(0, arcs.Count, k, y);
    }

    private int BinarySearch(int start, int end/*exclusive*/, float k, float y)
    {
        if(end - start == 1)
        {
            return start;
        }
        int size = end - start;
        int mid = Mathf.CeilToInt(size / 2.0f) + start-1;
        Debug.Log("start " + start + " end " + end +" index " + mid + " size " + size + " count " + arcs.Count);
        Vector2 bp = CircumcenterSweep(arcs[mid].pos, arcs[mid + 1].pos, y);

        if (k >= bp.x)
        {
            return BinarySearch(mid + 1, end, k, y);
        }
        else
        {
            return BinarySearch(start, mid + 1, k, y);
        }
    }

    private int Raycast(float x, float y)
    {
        float best = Mathf.Infinity;
        int bestIndex = 0;

        return 0;
    }

    public struct HitInfo
    {
        public Cell target;
        public Cell leftMost;
        public Cell rightMost;

        public HitInfo(Cell t, Cell l, Cell r)
        {
            target = t;
            leftMost = l;
            rightMost = r;
        }
    }

    public List<Vector2> GetPoints(float y)
    {
        List<Vector2> points = new List<Vector2>();
        for(int i = 0; i+1 < arcs.Count; i++)
        {
            points.Add(CircumcenterSweep(arcs[i].pos, arcs[i + 1].pos, y));
        }
        return points;
    }
}
