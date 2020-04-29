using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VoronoiMap;
using static Geometry;

public class BeachTree
{
    BeachNode root = null;
    float width;
    float height;
    public float sweep;

    Diagram diagram;
    SortedSet<VoronoiEvent> events;

    public BeachTree(float w, float h, Diagram d, SortedSet<VoronoiEvent> ev)
    {
        width = w;
        height = h;
        diagram = d;
        events = ev;
    }

    public void Insert(Cell c)
    {
        Vertex start;
        Edge edge;
        if(root == null)
        {
            root = new BeachArc(c);
            return;
        }
        if(root.GetType() == typeof(BeachArc) && ((BeachArc)root).site.pos.y - c.pos.y < 1)
        {
            BeachArc temp = (BeachArc)root;
            root = new BeachEdge();
            root.left = temp;
            root.right = new BeachArc(c);
            start = new Vertex((c.pos + temp.site.pos)/2f);
            diagram.vertices.Add(start);
            if (c.pos.x > temp.site.pos.x)
            {
                edge = new Edge(start, temp.site, c);
                ((BeachEdge)root).edge = edge;
            }
            else
            {
                edge = new Edge(start, c, temp.site);
                ((BeachEdge)root).edge = edge;
            }
            diagram.edges.Add(edge);
            return;
        }

        BeachArc par = GetParabolaByX(c.pos.x);

        if (par.ev != null) //If hit arc is the center of a triplet, remove it
        {
            events.Remove(par.ev);
            par.ev = null;
        }

        start = new Vertex((c.pos + par.site.pos)/2f);
        diagram.vertices.Add(start);

        Edge left = new Edge(start, par.site, c);
        Edge right = new Edge(start, c, par.site);

        left.section = right;
        diagram.edges.Add(left);

        BeachArc p0 = new BeachArc(par.site);
        BeachArc p1 = new BeachArc(c);
        BeachArc p2 = new BeachArc(par.site);

        BeachEdge subroot = new BeachEdge();
        subroot.edge = right;
        if (par == par.parent.left)
        {
            par.parent.left = subroot;
        }
        else
        {
            par.parent.right = subroot;
        }

        subroot.right = p2;
        subroot.left = new BeachEdge(left);
        subroot.left.left = p0;
        subroot.left.right = p1;

        CheckCircle(p0);
        CheckCircle(p2);
    }

    public void Remove(VertexEvent e)
    {
        BeachArc mid = e.arcs[1];

        BeachEdge leftEdge = BeachNode.GetLeftParent(mid);
        BeachEdge rightEdge = BeachNode.GetRightParent(mid);

        BeachArc left = BeachNode.GetLeftChild(leftEdge);
        BeachArc right = BeachNode.GetRightChild(rightEdge);

        if(left.ev != null)
        {
            events.Remove(left.ev);
            left.ev = null;
        }
        if(right.ev != null)
        {
            events.Remove(right.ev);
            right.ev = null;
        }

        Vertex end = new Vertex(e.center);
        diagram.vertices.Add(end);
        leftEdge.edge.end = end;
        rightEdge.edge.end = end;

        diagram.edges.Add(leftEdge.edge);
        diagram.edges.Add(rightEdge.edge);

        BeachEdge higher = null;
        BeachNode par = mid;
        while(par != root)
        {
            par = par.parent;
            if (par == leftEdge) higher = leftEdge;
            if (par == rightEdge) higher = rightEdge;
        }
        higher.edge = new Edge(end, left.site, right.site);
        diagram.edges.Add(higher.edge);

        BeachEdge gparent = (BeachEdge)mid.parent.parent;
        if(mid.parent.left == mid)
        {
            if (gparent.left == mid.parent) gparent.left = mid.parent.right;
            if (gparent.right == mid.parent) gparent.right = mid.parent.right;
        }
        else
        {
            if (gparent.left == mid.parent) gparent.left = mid.parent.left;
            if (gparent.right == mid.parent) gparent.right = mid.parent.left;
        }

        CheckCircle(left);
        CheckCircle(right);
    }

    public void CheckCircle(BeachArc mid)
    {
        BeachEdge leftParent = BeachNode.GetLeftParent(mid);
        BeachEdge rightParent = BeachNode.GetRightParent(mid);

        BeachArc left = BeachNode.GetLeftChild(leftParent);
        BeachArc right = BeachNode.GetRightChild(rightParent);

        if(left == null || right == null || left.site == right.site)
        {
            return;
        }

        Vector2 s = GetEdgeIntersection(leftParent.edge, rightParent.edge);
        if (s == Vector2.negativeInfinity)
            return;

        float dx = left.site.pos.x - s.x;
        float dy = left.site.pos.y - s.y;

        float d = Mathf.Sqrt((dx * dx) + (dy * dy));

        if (s.y - d >= sweep)
            return;

        VertexEvent ev = new VertexEvent(left, mid, right);
        mid.ev = ev;
        events.Add(ev);
    }

    public void Finish()
    {
        FinishEdge(root);
    }

    void FinishEdge(BeachNode node)
    {
        if (node.GetType() == typeof(BeachArc)) return;
        BeachEdge n = (BeachEdge)node;
        float mx;
        if (n.edge.direction.x > 0)
            mx = Mathf.Max(width, n.edge.start.pos.x + 10);
        else
            mx = Mathf.Max(0, n.edge.start.pos.x - 10);

        Vertex end = new Vertex(new Vector2(mx, mx * n.edge.f + n.edge.g));
        n.edge.end = end;
        diagram.vertices.Add(end);
        diagram.edges.Add(n.edge);

        FinishEdge(n.left);
        FinishEdge(n.right);
    }

    public float GetY(Vector2 site, float x)
    {
        float dp = 2f * (site.y - sweep);
        float a1 = 1f / dp;
        float b1 = -2f * site.x / dp;
        float c1 = sweep + dp / 4 + site.x * site.x / dp;

        return (a1 * x * x + b1 * x + c1);
    }

    public BeachArc GetParabolaByX(float x)
    {
        BeachNode par = root;
        float cur = 0;

        while(par.GetType() != typeof(BeachArc))
        {
            cur = GetXOfEdge((BeachEdge)par, sweep);
            if (cur > x)
                par = par.left;
            else
                par = par.right;

        }
        return (BeachArc)par;
    }

    public float GetXOfEdge(BeachEdge n, float y)
    {
        BeachArc left = BeachNode.GetLeftChild(n);
        BeachArc right = BeachNode.GetRightChild(n);

        Vector2 p = left.site.pos;
        Vector2 r = right.site.pos;

        float dp = 2f * (p.y - y);
        float a1 = 1f / dp;
        float b1 = -2f * p.x / dp;
        float c1 = y + dp / 4 + p.x * p.x / dp;

        dp = 2f * (r.y - y);
        float a2 = 1f / dp;
        float b2 = -2f * r.x / dp;
        float c2 = sweep + dp / 4 + r.x * r.x / dp;

        float a = a1 - a2;
        float b = b1 - b2;
        float c = c1 - c2;

        float disc = b * b - 4 * a * c;
        float x1 = (-b + Mathf.Sqrt(disc)) / (2 * a);
        float x2 = (-b - Mathf.Sqrt(disc)) / (2 * a);

        float ry;
        if (p.y < r.y)
            ry = Mathf.Max(x1, x2);
        else
            ry = Mathf.Min(x1, x2);
        return ry;
    }

    public Vector2 GetEdgeIntersection(Edge a, Edge b)
    {
        float x = (b.g - a.g) / (a.f - b.f);
        float y = a.f * x + a.g;

        if ((x - a.start.pos.x) / a.direction.x < 0)
            return Vector2.negativeInfinity;
        if ((y - a.start.pos.y) / a.direction.y < 0)
            return Vector2.negativeInfinity;
        if ((x - b.start.pos.x) / b.direction.x < 0)
            return Vector2.negativeInfinity;
        if ((y - b.start.pos.y) / b.direction.y < 0)
            return Vector2.negativeInfinity;

        return new Vector2(x, y);
    }

    public class BeachArc : BeachNode
    {
        public Cell site;
        public VertexEvent ev;

        public BeachArc(Cell c)
        {
            site = c;
        }
    }

    public class BeachEdge : BeachNode
    {
        public Edge edge;

        public BeachEdge(Edge e)
        {
            edge = e;
        }

        public BeachEdge() { }
    }

    public class BeachNode
    {
        public BeachNode parent;
        public BeachNode left { get { return _left; } set { _left = value; value.parent = this; } }
        public BeachNode right { get { return _right; } set { _right = value; value.parent = this; } }
        protected BeachNode _left;
        protected BeachNode _right;

        public static BeachEdge GetLeftParent(BeachNode n)
        {
            BeachNode p = n.parent;
            BeachNode last = n;
            while (p.left == last)
            {
                if (p.parent == null)
                    return null;
                last = p;
                p = p.parent;
            }
            return (BeachEdge)p;
        }

        public static BeachEdge GetRightParent(BeachNode n)
        {
            BeachNode p = n.parent;
            BeachNode last = n;
            while (p.right == last)
            {
                if (p.parent == null)
                    return null;
                last = p;
                p = p.parent;
            }
            return (BeachEdge)p;
        }

        public static BeachArc GetLeftChild(BeachNode n)
        {
            if (n == null)
                return null;
            BeachNode child = n.left;
            while (child.GetType() != typeof(BeachArc))
                child = child.right;
            return (BeachArc)child;
        }

        public static BeachArc GetRightChild(BeachNode n)
        {
            if (n == null)
                return null;
            BeachNode child = n.right;
            while (child.GetType() != typeof(BeachArc))
                child = child.left;
            return (BeachArc)child;
        }

        public BeachNode GetLeft(BeachNode n)
        {
            return GetLeftChild(GetLeftParent(n));
        }

        public BeachNode GetRight(BeachNode n)
        {
            return GetRightChild(GetRightParent(n));
        }
    }
}


