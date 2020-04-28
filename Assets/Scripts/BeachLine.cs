using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VoronoiMap;
using static Geometry;

public class BeachLine
{
    private List<Cell> arcs;
    private List<Breakpoint> breaks;
    float xMin;
    float xMax;
    float yMax;

    public BeachLine(float xMin, float xMax, float yMax)
    {
        arcs = new List<Cell>();
        breaks = new List<Breakpoint>();
        this.xMin = xMin;
        this.xMax = xMax;
        this.yMax = yMax;
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
        if (index < breaks.Count)
            breaks.RemoveAt(index);
        breaks.Insert(index, new Breakpoint(CircumcenterSweep(arcs[index].pos, arcs[index + 1].pos, c.pos.y).y));
        breaks.Insert(index + 1, new Breakpoint(CircumcenterSweep(arcs[index+1].pos, arcs[index + 2].pos, c.pos.y).y));
        if(index+3 < arcs.Count)
            breaks.Insert(index + 2, new Breakpoint(CircumcenterSweep(arcs[index+2].pos, arcs[index + 3].pos, c.pos.y).y));
        Debug.Log(arcs.Count);

        return output;
    }

    public bool Delete(Cell left, Cell del, Cell right, Vector2 pos)
    {
        int index = SearchIndex(pos.x, pos.y+0.0001f);
        Cell hit = arcs[index];
        if(hit == del)
        {
            arcs.RemoveAt(index);
            if(index < breaks.Count)
            {
                breaks.RemoveAt(index);
            }
            if(index > 0)
            {
                breaks[index - 1] = new Breakpoint(CircumcenterSweep(arcs[index-1].pos, arcs[index].pos, pos.y).y);
            }
            return true;
        }
        else if(hit == left && index+2 < arcs.Count)
        {
            if(arcs[index+1] == del && arcs[index+2] == right)
            {
                arcs.RemoveAt(index + 1);
                breaks.RemoveAt(index);
                if (index+1 > 0)
                {
                    breaks[index] = new Breakpoint(CircumcenterSweep(arcs[index].pos, arcs[index+1].pos, pos.y).y);
                }
                return true;
            }
        }
        else if(hit == right && index -2 >= 0)
        {
            if(arcs[index-1] == del && arcs[index-2] == left)
            {
                arcs.RemoveAt(index - 1);
                if (index < breaks.Count)
                {
                    breaks.RemoveAt(index);
                }
                breaks[index - 2] = new Breakpoint(CircumcenterSweep(arcs[index - 2].pos, arcs[index-1].pos, pos.y).y);
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
        /*
        if(bp.y > yMax) //Prune nonexistant breakpoints and try again
        {
            if(arcs[mid].pos.y < arcs[mid + 1].pos.y)
            {
                arcs.RemoveAt(mid + 1);
            }
            else
            {
                arcs.RemoveAt(mid);
            }
            return BinarySearch(Mathf.Max(0, start - 1), Mathf.Min(end,arcs.Count), k, y);
        }*/
        if (k >= bp.x)
        {
            return BinarySearch(mid + 1, end, k, y);
        }
        else
        {
            return BinarySearch(start, mid + 1, k, y);
        }
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

    public void Validate(float y)
    {
        int index = 0;
        while(index+1 < arcs.Count)
        {
            Vector2 bp = CircumcenterSweep(arcs[index].pos, arcs[index + 1].pos, y);
            if(!breaks[index].isNew && bp.y > breaks[index].yLast)
            {
                if (arcs[index].pos.y < arcs[index].pos.y)
                {
                    arcs.RemoveAt(index + 1);
                    breaks.RemoveAt(index);
                    if(index < breaks.Count)
                    {
                        breaks[index] = new Breakpoint(CircumcenterSweep(arcs[index].pos, arcs[index + 1].pos, y).y);
                    }
                }
                else
                {
                    arcs.RemoveAt(index);
                    breaks.RemoveAt(index);
                    if (index > 0)
                    {
                        breaks[index-1] = new Breakpoint(CircumcenterSweep(arcs[index-1].pos, arcs[index].pos, y).y);
                    }
                }
            }
            else
            {
                index++;
            }
        }
        foreach(Breakpoint b in breaks)
        {
            b.isNew = false;
        }
        Debug.Log("arcs: " + arcs.Count + "\nbreakpoints" + breaks.Count);
    }

    public List<Vector2> GetPoints(float y)
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(new Vector2(xMin, yMax));
        for(int i = 0; i+1 < arcs.Count; i++)
        {
            points.Add(CircumcenterSweep(arcs[i].pos, arcs[i + 1].pos, y));
        }
        points.Add(new Vector2(xMax, yMax));
        return points;
    }

    private class Breakpoint
    {
        public bool isNew;
        public float yLast;

        public Breakpoint(float y)
        {
            yLast = y;
            isNew = true;
        }
    }
}
