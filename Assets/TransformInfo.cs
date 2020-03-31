using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformInfo : MonoBehaviour
{
    public Vector3 positon;
    public Quaternion rotation;
    public float nodeWidth, nodeLength;
    public Vector3 endPoint;
    public float param;
    public int animationID = 0;

    public class TIC : IComparer<TransformInfo>
    {
        public int Compare(TransformInfo x, TransformInfo y)
        {

            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    int retval = x.nodeLength.CompareTo(y.nodeLength);

                    if (retval != 0)
                    {
                        // If the strings are not of equal length,
                        // the longer string is greater.
                        //
                        return retval;
                    }
                    else
                    {
                        // If the strings are of equal length,
                        // sort them with ordinary string comparison.
                        //
                        return 0;
                    }
                }
            }
        }
    }
   

}
