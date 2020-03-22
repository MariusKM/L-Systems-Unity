/* This is taken from this blog post:
 * http://loyc-etc.blogspot.ca/2014/05/2d-convex-hull-in-c-45-lines-of-code.html
 *
 * All I have done is renamed "DList" to "CircularList" and then wrote a wrapper for the generic C# list.
 * The structure that is supposed to be used is *much* more efficient, but this works for my purposes.
 *
 * This can be dropped right into your Unity project and will work without any adjustments.
 */
using System.Collections.Generic;
using UnityEngine;

/**
 * @file   ConvexHull.cs
 * @author Benjamin Williams <bwilliams@lincoln.ac.uk>
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static partial class ConvexHull
{
    /// <summary>
    /// Computes the convex hull of a list of points.
    /// </summary>
    /// <param name="points">Points.</param>
    /// <param name="loop">If set to <c>true</c> loop.</param>
    public static List<Vector3> compute(List<Vector3> points, bool loop = false)
    {
        //The points which will be returned
        var returnPoints = new List<Vector3>();

        //Find left-most point on x axis
        var currentPoint = lowestXCoord(points);

        //The endpoint to compare against
          Vector3 endpoint = Vector3.zero;

        while (true)
        {
            //Add current point
            returnPoints.Add(currentPoint);

            //Set endpoint back to the first point in the list of points
            endpoint = points[0];

            for(var j = 1; j < points.Count; j++)
            {
                //Run through points -- if the turn from this point to the other is greater, set endpoint to this
                if ((endpoint == currentPoint) || (ccw(currentPoint, endpoint, points[j]) < 0))
                    endpoint = points[j];
            }

            //Set current point
            currentPoint = endpoint;

            //Break condition -- if we've looped back around then we've made a convex hull!
            if (endpoint == returnPoints[0])
                break;
        }

        //If we want to loop, include the first vertex again
        if (loop)
            returnPoints.Add(returnPoints[0]);

        //And finally, return the points
        return returnPoints;
    }

    /// <summary>
    /// Finds the lowest x value.
    /// </summary>
    /// <returns>The X coordinate.</returns>
    /// <param name="array">Array.</param>
    private static Vector3 lowestXCoord(List<Vector3> array)
    {
        return array.Where(p => p.x == (array.Min(y => y.x))).First();
    }


    /// <summary>
    /// Swap two elements in an array, given two indices
    /// </summary>
    /// <param name="array">Array.</param>
    /// <param name="idxA">Index a.</param>
    /// <param name="idxB">Index b.</param>
    private static void swap(ref Vector3[] array, int idxA, int idxB)
    {
        //temp = a
        var temp = array[idxA];

        //a overwritten with b
        array[idxA] = array[idxB];

        //b overwritten with temp
        array[idxB] = temp;
    }


    /// <summary>
    /// Determines if three points are in a counter-clockwise turn. Returns:
    /// n < 0: If clockwise
    /// n > 0: If counter-clockwise
    /// n = 0: If collinear
    /// </summary>
    /// <param name="p1">P1.</param>
    /// <param name="p2">P2.</param>
    /// <param name="p3">P3.</param>
    public static float ccw(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Determinant
        return Mathf.Sign((p2.x - p1.x) * (p3.z - p1.z) - (p3.x - p1.x) * (p2.z - p1.z));
    }
}
/*public static class ConvexHull
{
    public static IList<Vector2> ComputeConvexHull(List<Vector2> points, bool sortInPlace = false)
    {
        if (!sortInPlace)
            points = new List<Vector2>(points);
        points.Sort((a, b) =>
            a.x == b.x ? a.y.CompareTo(b.y) : (a.x > b.x ? 1 : -1));

        // Importantly, DList provides O(1) insertion at beginning and end
        CircularList<Vector2> hull = new CircularList<Vector2>();
        int L = 0, U = 0; // size of lower and upper hulls

        // Builds a hull such that the output polygon starts at the leftmost Vector2.
        for (int i = points.Count - 1; i >= 0; i--)
        {
            Vector2 p = points[i], p1;

            // build lower hull (at end of output list)
            while (L >= 2 && (p1 = hull.Last).Sub(hull[hull.Count - 2]).Cross(p.Sub(p1)) >= 0)
            {
                hull.PopLast();
                L--;
            }
            hull.PushLast(p);
            L++;

            // build upper hull (at beginning of output list)
            while (U >= 2 && (p1 = hull.First).Sub(hull[1]).Cross(p.Sub(p1)) <= 0)
            {
                hull.PopFirst();
                U--;
            }
            if (U != 0) // when U=0, share the Vector2 added above
                hull.PushFirst(p);
            U++;
            Debug.Assert(U + L == hull.Count + 1);
        }
        hull.PopLast();
        return hull;
    }

    private static Vector2 Sub(this Vector2 a, Vector2 b)
    {
        return a - b;
    }

    private static float Cross(this Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private class CircularList<T> : List<T>
    {
        public T Last
        {
            get
            {
                return this[this.Count];
            }
            set
            {
                this[this.Count] = value;
            }
        }

        public T First
        {
            get
            {
                return this[0];
            }
            set
            {
                this[0] = value;
            }
        }

        public void PushLast(T obj)
        {
            this.Add(obj);
        }

        public T PopLast()
        {
            T retVal = this[this.Count - 1];
            this.RemoveAt(this.Count - 1);
            return retVal;
        }

        public void PushFirst(T obj)
        {
            this.Insert(0, obj);
        }

        public T PopFirst()
        {
            T retVal = this[0];
            this.RemoveAt(0);
            return retVal;
        }
    }
}*/
