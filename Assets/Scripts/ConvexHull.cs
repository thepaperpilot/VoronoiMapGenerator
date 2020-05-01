﻿/* 
 * Convex hull algorithm - Library (C#)
 * 
 * Copyright (c) 2017 Project Nayuki
 * https://www.nayuki.io/page/convex-hull-algorithm
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program (see COPYING.txt and COPYING.LESSER.txt).
 * If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public sealed class ConvexHull
{

    // Returns a new list of points representing the convex hull of
    // the given set of points. The convex hull excludes collinear points.
    // This algorithm runs in O(n log n) time.
    public static List<Vector2> MakeHull(List<Vector2> points)
    {
        List<Vector2> newPoints = new List<Vector2>(points);
        newPoints.Sort(new HullVector2Comparer());
        return MakeHullPresorted(newPoints);
    }


    // Returns the convex hull, assuming that each points[i] <= points[i + 1]. Runs in O(n) time.
    public static List<Vector2> MakeHullPresorted(List<Vector2> points)
    {
        if (points.Count <= 1)
            return new List<Vector2>(points);

        // Andrew's monotone chain algorithm. Positive y coordinates correspond to "up"
        // as per the mathematical convention, instead of "down" as per the computer
        // graphics convention. This doesn't affect the correctness of the result.

        List<Vector2> upperHull = new List<Vector2>();
        foreach (Vector2 p in points)
        {
            while (upperHull.Count >= 2)
            {
                Vector2 q = upperHull[upperHull.Count - 1];
                Vector2 r = upperHull[upperHull.Count - 2];
                if ((q.x - r.x) * (p.y - r.y) >= (q.y - r.y) * (p.x - r.x))
                    upperHull.RemoveAt(upperHull.Count - 1);
                else
                    break;
            }
            upperHull.Add(p);
        }
        upperHull.RemoveAt(upperHull.Count - 1);

        IList<Vector2> lowerHull = new List<Vector2>();
        for (int i = points.Count - 1; i >= 0; i--)
        {
            Vector2 p = points[i];
            while (lowerHull.Count >= 2)
            {
                Vector2 q = lowerHull[lowerHull.Count - 1];
                Vector2 r = lowerHull[lowerHull.Count - 2];
                if ((q.x - r.x) * (p.y - r.y) >= (q.y - r.y) * (p.x - r.x))
                    lowerHull.RemoveAt(lowerHull.Count - 1);
                else
                    break;
            }
            lowerHull.Add(p);
        }
        lowerHull.RemoveAt(lowerHull.Count - 1);

        if (!(upperHull.Count == 1 && Enumerable.SequenceEqual(upperHull, lowerHull)))
            upperHull.AddRange(lowerHull);
        return upperHull;
    }
}

public class HullVector2Comparer : IComparer<Vector2>
{
    public int Compare(Vector2 v1, Vector2 v2)
    {
        if (v1.x < v2.x)
            return -1;
        else if (v1.x > v2.x)
            return +1;
        else if (v1.y < v2.y)
            return -1;
        else if (v1.y > v2.y)
            return +1;
        else
            return 0;
    }
}