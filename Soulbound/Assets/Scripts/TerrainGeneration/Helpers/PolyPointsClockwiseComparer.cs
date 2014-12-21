using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class PolyPointsClockwiseComparer : IComparer<Vector3>
{
    Vector3 norm, baseP;

    public PolyPointsClockwiseComparer(Vector3 basePoint, Vector3 normalVec)
    {
        baseP = basePoint;
        norm = normalVec;
    }

    public int Compare(Vector3 x, Vector3 y)
    {
        if (Vector3.Dot(Vector3.Cross(x - baseP, y - baseP), norm) < 0)
            return -1;
        if (Vector3.Dot(Vector3.Cross(x - baseP, y - baseP), norm) > 0)
            return 1;
        return 0;
    }
}
