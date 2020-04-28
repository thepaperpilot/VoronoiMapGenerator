using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geometry
{
    public static Vector2 CircumcenterPoints(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Line bisector1 = PerpendicularBisector(p1, p2);
        Line bisector2 = PerpendicularBisector(p2, p3);
        return IntersectLines(bisector1, bisector2);
    }

    //Thank you https://en.wikipedia.org/wiki/Special_cases_of_Apollonius%27_problem#Type_4:_Two_points,_one_line
    public static Vector2 CircumcenterSweep(Vector2 p1, Vector2 p2, float ySweep)
    {
        Line sweep = new Line(new Vector2(0, ySweep), new Vector2(1, ySweep));
        Vector2 g = IntersectLines(sweep, new Line(p1, p2));
        float dist = Mathf.Sqrt((p1 - g).magnitude * (p2 - g).magnitude);

        //There are two candidates
        Vector2 gLeft = new Vector2(g.x - dist, ySweep);
        Vector2 gRight = new Vector2(g.x + dist, ySweep);

        Vector2 cInner; //Lies below the reference line
        Vector2 cOuter; //Lies above the reference line

        if (new Line(p1, p2).slope < 0) //Meeting g happens to the right
        {
            cInner = CircumcenterPoints(p1, p2, gLeft);
            cOuter = CircumcenterPoints(p1, p2, gRight);
        }
        else //If g is on the left, reverse the result set
        {
            cOuter = CircumcenterPoints(p1, p2, gLeft);
            cInner = CircumcenterPoints(p1, p2, gRight);
        }

        if (p1.x < p2.x) //If moving L to R, return the inner result
        {
            return cInner;
        }
        else //Otherwise return the outer result
        {
            return cOuter;
        }
    }

    //ax+by=c
    public class Line
    {
        public float a;
        public float b;
        public float c;
        public float slope { get { return -a / b; } }

        public Line(float a, float b, float c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public Line(Vector2 p1, Vector2 p2)
        {
            a = p2.y - p1.y;
            b = p1.x - p2.x;
            c = a * p1.x + b * p1.y;
        }
    }

    public static Line PerpendicularBisector(Vector2 p1, Vector2 p2)
    {
        Line side = new Line(p1, p2);
        Vector2 midpoint = 0.5f * (p1 + p2);
        return new Line(-side.b, side.a, -side.b * midpoint.x + side.a * midpoint.y);
    }

    public static Vector2 IntersectLines(Line l1, Line l2)
    {
        float det = l1.a * l2.b - l2.a * l1.b;
        if (det == 0)
        {
            return Vector2.zero; //Your triangle should not have parallel lines :(
        }
        float x = (l2.b * l1.c - l1.b * l2.c) / det;
        float y = (l1.a * l2.c - l2.a * l1.c) / det;
        return new Vector2(x, y);
    }
}
