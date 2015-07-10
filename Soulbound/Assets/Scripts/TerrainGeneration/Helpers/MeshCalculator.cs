using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class MeshInfo
{
	public List<Vector3> vertices;
	public List<Vector3> normals;
	public List<Vector2> uvs;

	public List<int> indices;

	public List<int> polyTriCount;

    public MeshInfo()
    {
		vertices = new List<Vector3>();
		normals = new List<Vector3>();
		uvs = new List<Vector2>();
        indices = new List<int>();
        polyTriCount = new List<int>();
    }
}

class MeshCalculator
{
    private static readonly int[,] Edges4 = new int[,] {
        {0,1}, {0,2}, {0,3}, // base to border
        {1,2}, {2,3}, {3,1} // border
    };

    private static readonly int[,] Edges6 = new int[,] {
        {0,1}, {0,2}, {0,3}, {0,4}, // base to rim
        {5,1}, {5,2}, {5,3}, {5,4}, // far to rim
        {1,2}, {2,3}, {3,4}, {4,1}  // rim
    };

    private TerrainCell mCell;
    private MeshInfo mMi;

    public MeshCalculator(TerrainCell cell)
    {
        mCell = cell;
        mMi = new MeshInfo();
    }

    public MeshInfo Calculate()
    {
        for (int i = 0; i < mCell.crystalValues.Length; i++)
        {
            int[] neighbors = mCell.GetNeighborCrystalIndices(i);

            CreateSeg(new int[] {
                i,
                neighbors[(int)Neighbors.TopNorthEast], 
                neighbors[(int)Neighbors.TopNorthWest], 
                neighbors[(int)Neighbors.TopSouth]
            });
            CreateSeg(new int[] {
                i,
                neighbors[(int)Neighbors.TopNorthEast], 
                neighbors[(int)Neighbors.East], 
                neighbors[(int)Neighbors.NorthEast]
            });
            CreateSeg(new int[] {
                i,
                neighbors[(int)Neighbors.TopNorthWest], 
                neighbors[(int)Neighbors.TopNorthEast], 
                neighbors[(int)Neighbors.NorthEast],
                neighbors[(int)Neighbors.NorthWest], 
                neighbors[(int)Neighbors.FarTopNorth]
            });
        }


        return mMi;
    }

    private void CreateSeg( int[] indices)
    {
        foreach (int i in indices)
        {
            if (i == -1)
                return;
        }

        int[] minorityIndices = FindMinority(indices);

        switch (indices.Length)
        {
        case 4:
            switch (minorityIndices.Length)
            {
            case 1:
                Build4Point(indices, minorityIndices);
                break;
            case 2:
                Build4Line(indices, minorityIndices);
                break;
            }
        	break;
        case 6:
            switch (minorityIndices.Length)
            {
                case 1:
                    Build6Point(indices, minorityIndices);
                    break;
                case 2:
                    if (FindEdge(minorityIndices[0], minorityIndices[1], Edges6) != -1)
                        Build6Line(indices, minorityIndices);
                    else
                        Build6Facing(indices, minorityIndices);
                    break;
                case 3:
                    if ((FindEdge(minorityIndices[0], minorityIndices[1], Edges6) != -1) &&
                        (FindEdge(minorityIndices[1], minorityIndices[2], Edges6) != -1) &&
                        (FindEdge(minorityIndices[2], minorityIndices[0], Edges6) != -1))
                        Build6Plane(indices, minorityIndices);
                    else
                        Build6Curve(indices, minorityIndices); //Curve sollte auf symmetrisch geändert werden
                    break;
            }
            break;
        }
    }

    private void Build6Curve(int[] indices, int[] minorityIndices)
    {
        // the groovy stuff :D

        int[][,] adjEdgesArr = new int[3][,];
        adjEdgesArr[0] = GetAdjEdges(minorityIndices[0], Edges6);
        adjEdgesArr[1] = GetAdjEdges(minorityIndices[1], Edges6);
        adjEdgesArr[2] = GetAdjEdges(minorityIndices[2], Edges6);

        // index of "minorityIndices"
        int startPoint = -1, middlePoint = -1, EndPoint = -1;
        // index of "indices"
        int sideA = -1, sideB = -1, otherSide = -1;

        // find middle point
        if((FindEdge(minorityIndices[0], minorityIndices[1], adjEdgesArr[0]) != -1) && (FindEdge(minorityIndices[0], minorityIndices[2], adjEdgesArr[0]) != -1))
        {
            startPoint = 1;
            middlePoint = 0;
            EndPoint = 2;
        }
        if ((FindEdge(minorityIndices[1], minorityIndices[0], adjEdgesArr[1]) != -1) && (FindEdge(minorityIndices[1], minorityIndices[2], adjEdgesArr[1]) != -1))
        {
            startPoint = 0;
            middlePoint = 1;
            EndPoint = 2;
        }
        if ((FindEdge(minorityIndices[2], minorityIndices[0], adjEdgesArr[2]) != -1) && (FindEdge(minorityIndices[2], minorityIndices[1], adjEdgesArr[2]) != -1))
        {
            startPoint = 0;
            middlePoint = 2;
            EndPoint = 1;
        }

        // find the two sides A+B and the point at the opposite side
        for (int h = 0; h <= adjEdgesArr[startPoint].GetUpperBound(0); h++)
        {
            if (FindEdge(minorityIndices[middlePoint], adjEdgesArr[startPoint][h,1], adjEdgesArr[middlePoint]) != -1)
            {
                if (sideA == -1)
                    sideA = adjEdgesArr[startPoint][h, 1];
                else
                    sideB = adjEdgesArr[startPoint][h, 1];
            } 
            else
            {
                if (adjEdgesArr[startPoint][h, 1] != minorityIndices[middlePoint])
                    otherSide = adjEdgesArr[startPoint][h, 1];
            }
        }

        Vector3[] polyPoints = new Vector3[3];
        polyPoints[0] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[startPoint]]) + mCell.CalcLocalCrystalPosition(indices[otherSide])) / 2.0f;
        polyPoints[1] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[startPoint]]) + mCell.CalcLocalCrystalPosition(indices[sideA])) / 2.0f;
        polyPoints[2] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[startPoint]]) + mCell.CalcLocalCrystalPosition(indices[sideB])) / 2.0f;
        // TODO: sort Polypoints
        CreatePoly(polyPoints);

        polyPoints[0] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[EndPoint]]) + mCell.CalcLocalCrystalPosition(indices[otherSide])) / 2.0f;
        polyPoints[1] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[EndPoint]]) + mCell.CalcLocalCrystalPosition(indices[sideA])) / 2.0f;
        polyPoints[2] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[EndPoint]]) + mCell.CalcLocalCrystalPosition(indices[sideB])) / 2.0f;
        // TODO: sort Polypoints
        CreatePoly(polyPoints);

        polyPoints = new Vector3[4];
        polyPoints[0] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[startPoint]]) + mCell.CalcLocalCrystalPosition(indices[sideA])) / 2.0f;
        polyPoints[1] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[startPoint]]) + mCell.CalcLocalCrystalPosition(indices[sideB])) / 2.0f;
        polyPoints[2] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[middlePoint]]) + mCell.CalcLocalCrystalPosition(indices[sideA])) / 2.0f;
        polyPoints[3] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[middlePoint]]) + mCell.CalcLocalCrystalPosition(indices[sideB])) / 2.0f;
        // TODO: sort Polypoints
        CreatePoly(polyPoints);

        polyPoints[0] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[EndPoint]]) + mCell.CalcLocalCrystalPosition(indices[sideA])) / 2.0f;
        polyPoints[1] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[EndPoint]]) + mCell.CalcLocalCrystalPosition(indices[sideB])) / 2.0f;
        polyPoints[2] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[middlePoint]]) + mCell.CalcLocalCrystalPosition(indices[sideA])) / 2.0f;
        polyPoints[3] = (mCell.CalcLocalCrystalPosition(indices[minorityIndices[middlePoint]]) + mCell.CalcLocalCrystalPosition(indices[sideB])) / 2.0f;
        // TODO: sort Polypoints
        CreatePoly(polyPoints);
    }

    private void Build6Plane(int[] indices, int[] minorityIndices)
    {
        int[,] adjEdges1 = GetAdjEdges(minorityIndices[0], Edges6);
        int[,] adjEdges2 = GetAdjEdges(minorityIndices[1], Edges6);
        int[,] adjEdges3 = GetAdjEdges(minorityIndices[2], Edges6);
        int[,] allEdges = new int[6, 2];
        int commonEdgeIndex;

        commonEdgeIndex = FindEdge(minorityIndices[0], minorityIndices[1], adjEdges1);
        if (commonEdgeIndex != -1)
            RemoveFromEdgeArray(ref adjEdges1, commonEdgeIndex);

        commonEdgeIndex = FindEdge(minorityIndices[0], minorityIndices[1], adjEdges2);
        if (commonEdgeIndex != -1)
            RemoveFromEdgeArray(ref adjEdges2, commonEdgeIndex);

        commonEdgeIndex = FindEdge(minorityIndices[1], minorityIndices[2], adjEdges2);
        if (commonEdgeIndex != -1)
            RemoveFromEdgeArray(ref adjEdges2, commonEdgeIndex);

        commonEdgeIndex = FindEdge(minorityIndices[1], minorityIndices[2], adjEdges3);
        if (commonEdgeIndex != -1)
            RemoveFromEdgeArray(ref adjEdges3, commonEdgeIndex);

        commonEdgeIndex = FindEdge(minorityIndices[2], minorityIndices[0], adjEdges3);
        if (commonEdgeIndex != -1)
            RemoveFromEdgeArray(ref adjEdges3, commonEdgeIndex);

        commonEdgeIndex = FindEdge(minorityIndices[2], minorityIndices[0], adjEdges1);
        if (commonEdgeIndex != -1)
            RemoveFromEdgeArray(ref adjEdges1, commonEdgeIndex);

        Array.Copy(adjEdges1, 0, allEdges, 0, 4);
        Array.Copy(adjEdges2, 0, allEdges, 4, 4);
        Array.Copy(adjEdges3, 0, allEdges, 8, 4);

        Vector3[] polyPoints = new Vector3[6];
        polyPoints[0] = (mCell.CalcLocalCrystalPosition(indices[allEdges[0, 0]]) + mCell.CalcLocalCrystalPosition(indices[allEdges[0, 1]])) / 2.0f;
        polyPoints[1] = (mCell.CalcLocalCrystalPosition(indices[allEdges[1, 0]]) + mCell.CalcLocalCrystalPosition(indices[allEdges[1, 1]])) / 2.0f;
        polyPoints[2] = (mCell.CalcLocalCrystalPosition(indices[allEdges[2, 0]]) + mCell.CalcLocalCrystalPosition(indices[allEdges[2, 1]])) / 2.0f;
        polyPoints[3] = (mCell.CalcLocalCrystalPosition(indices[allEdges[3, 0]]) + mCell.CalcLocalCrystalPosition(indices[allEdges[3, 1]])) / 2.0f;
        polyPoints[4] = (mCell.CalcLocalCrystalPosition(indices[allEdges[4, 0]]) + mCell.CalcLocalCrystalPosition(indices[allEdges[4, 1]])) / 2.0f;
        polyPoints[5] = (mCell.CalcLocalCrystalPosition(indices[allEdges[5, 0]]) + mCell.CalcLocalCrystalPosition(indices[allEdges[5, 1]])) / 2.0f;


        // normal approximation, but should face in same direction. Since its only used for sorting and not normal calculation it should suffice
        Vector3 norm = mCell.CalcLocalCrystalPosition(indices[adjEdges1[0, 1]]) - mCell.CalcLocalCrystalPosition(indices[minorityIndices[0]]);
        if (!IsCrystalSolid(indices[minorityIndices[0]]))
        {
            norm = -norm;
        }

        SortPolyPoints(ref polyPoints, norm);
        CreatePoly(new Vector3[] { polyPoints[0], polyPoints[1], polyPoints[2], polyPoints[3] });
        CreatePoly(new Vector3[] { polyPoints[0], polyPoints[3], polyPoints[4], polyPoints[5] });

    }

    private void Build6Facing(int[] indices, int[] minorityIndices)
    {
        Build6Point(indices, new int[] { minorityIndices[0] });
        Build6Point(indices, new int[] { minorityIndices[1] });
    }

    private void Build6Line(int[] indices, int[] minorityIndices)
    {
        int[,] adjEdges1 = GetAdjEdges(minorityIndices[0], Edges6);
        int[,] adjEdges2 = GetAdjEdges(minorityIndices[1], Edges6);
        int commonEdgeIndex;

        commonEdgeIndex = FindEdge(minorityIndices[0], minorityIndices[1], adjEdges1);
        if (commonEdgeIndex != -1)
            RemoveFromEdgeArray(ref adjEdges1, commonEdgeIndex);

        commonEdgeIndex = FindEdge(minorityIndices[0], minorityIndices[1], adjEdges2);
        if (commonEdgeIndex != -1)
            RemoveFromEdgeArray(ref adjEdges2, commonEdgeIndex);

        int[,] sideAEdges = new int[2, 2];
        int[,] sideBEdges = new int[2, 2];
        bool foundSideA = false;

        for (int h = 0; h <= adjEdges1.GetUpperBound(0); h++)
        {
            bool foundEdge = false;
            for (int k = 0; k <= adjEdges2.GetUpperBound(0); k++)
            {
                if ((!foundEdge) && (adjEdges1[h, 1] == adjEdges2[k, 1]))
                {
                    int[,] tArr;
                    if (!foundSideA)
                        tArr = sideAEdges;
                    else
                        tArr = sideBEdges;
                    tArr[0, 0] = adjEdges1[h, 0];
                    tArr[0, 1] = adjEdges1[h, 1];
                    tArr[1, 0] = adjEdges2[k, 0];
                    tArr[1, 1] = adjEdges2[k, 1];

                    RemoveFromEdgeArray(ref adjEdges1, h);
                    RemoveFromEdgeArray(ref adjEdges2, k);
                    foundSideA = true;
                    foundEdge = true;
                    h--;
                }
            }
        }

        // normal approximation, but should face in same direction. Since its only used for sorting and not normal calculation it should suffice
        Vector3 norm = mCell.CalcLocalCrystalPosition(indices[adjEdges1[0, 1]]) - mCell.CalcLocalCrystalPosition(indices[minorityIndices[0]]);
        if (!IsCrystalSolid(indices[minorityIndices[0]]))
        {
            norm = -norm;
        }

        Vector3[] polyPoints = new Vector3[4];

        polyPoints[0] = (mCell.CalcLocalCrystalPosition(indices[adjEdges1[0, 0]]) + mCell.CalcLocalCrystalPosition(indices[adjEdges1[0, 1]])) / 2.0f;
        polyPoints[1] = (mCell.CalcLocalCrystalPosition(indices[adjEdges2[0, 0]]) + mCell.CalcLocalCrystalPosition(indices[adjEdges2[0, 1]])) / 2.0f;
        polyPoints[2] = (mCell.CalcLocalCrystalPosition(indices[sideAEdges[0, 0]]) + mCell.CalcLocalCrystalPosition(indices[sideAEdges[0, 1]])) / 2.0f;
        polyPoints[3] = (mCell.CalcLocalCrystalPosition(indices[sideAEdges[1, 0]]) + mCell.CalcLocalCrystalPosition(indices[sideAEdges[1, 1]])) / 2.0f;
        SortPolyPoints(ref polyPoints, norm);
        CreatePoly(polyPoints);

        polyPoints[0] = (mCell.CalcLocalCrystalPosition(indices[adjEdges1[0, 0]]) + mCell.CalcLocalCrystalPosition(indices[adjEdges1[0, 1]])) / 2.0f;
        polyPoints[1] = (mCell.CalcLocalCrystalPosition(indices[adjEdges2[0, 0]]) + mCell.CalcLocalCrystalPosition(indices[adjEdges2[0, 1]])) / 2.0f;
        polyPoints[2] = (mCell.CalcLocalCrystalPosition(indices[sideBEdges[0, 0]]) + mCell.CalcLocalCrystalPosition(indices[sideBEdges[0, 1]])) / 2.0f;
        polyPoints[3] = (mCell.CalcLocalCrystalPosition(indices[sideBEdges[1, 0]]) + mCell.CalcLocalCrystalPosition(indices[sideBEdges[1, 1]])) / 2.0f;
        SortPolyPoints(ref polyPoints, norm);
        CreatePoly(polyPoints);
    }

    private void Build6Point(int[] indices, int[] minorityIndices)
    {
        int[,] adjEdges = GetAdjEdges(minorityIndices[0], Edges6);

        Vector3[] polyPoints = new Vector3[4];
        for (int h = 0; h <= adjEdges.GetUpperBound(0); h++)
            polyPoints[h] = (mCell.CalcLocalCrystalPosition(indices[adjEdges[h,0]]) + mCell.CalcLocalCrystalPosition(indices[adjEdges[h,1]])) / 2.0f;

        // normal approximation, but should face in same direction. Since its only used for sorting and not normal calculation it should suffice
        Vector3 norm = mCell.CalcLocalCrystalPosition(indices[adjEdges[0,1]]) - mCell.CalcLocalCrystalPosition(indices[minorityIndices[0]]);
        if (!IsCrystalSolid(indices[minorityIndices[0]]))
        {
            norm = -norm;
        }

        SortPolyPoints(ref polyPoints, norm);
        CreatePoly(polyPoints);
    }

    private void Build4Line(int[] indices, int[] minorityIndices)
    {
        List<int> allInd = (new int[] { 0, 1, 2, 3 }).ToList();

        foreach (int minor in minorityIndices)
        {
            int ind = allInd.IndexOf(minor);
            allInd.RemoveAt(ind);
        }

        int[,] allEdges = new int[,]{
            { allInd[0], minorityIndices[0] },
            { allInd[0], minorityIndices[1] },
            { allInd[1], minorityIndices[0] },
            { allInd[1], minorityIndices[1] }
        };

        Vector3[] polyPoints = new Vector3[4];
        for (int h = 0; h <= allEdges.GetUpperBound(0); h++)
            polyPoints[h] = (mCell.CalcLocalCrystalPosition(indices[allEdges[h,0]]) + mCell.CalcLocalCrystalPosition(indices[allEdges[h,1]])) / 2.0f;

        //calculate normal
        /*
        Vector3 startPoint = mCell.CalcLocalCrystalPosition(indices[minorityIndices[0]]) + mCell.CalcLocalCrystalPosition(indices[minorityIndices[1]]);
        Vector3 endPoint = mCell.CalcLocalCrystalPosition(indices[allInd[0]]) + mCell.CalcLocalCrystalPosition(indices[allInd[1]]);
        Vector3 norm = Vector3.Normalize(endPoint - startPoint);
         */
        // normal approximation, but should face in same direction. Since its only used for sorting and not normal calculation it should suffice
        Vector3 norm = mCell.CalcLocalCrystalPosition(indices[allInd[0]]) - mCell.CalcLocalCrystalPosition(indices[minorityIndices[0]]);
        if (!IsCrystalSolid(indices[minorityIndices[0]]))
        {
            norm = -norm;
        }

        SortPolyPoints(ref polyPoints, norm);
        CreatePoly(polyPoints);
    }

    private void Build4Point(int[] indices, int[] minorityIndices)
    {
        int[,] adjEdges = GetAdjEdges(minorityIndices[0], Edges4);

        Vector3[] polyPoints = new Vector3[3];
        for (int h = 0; h <= adjEdges.GetUpperBound(0); h++)
            polyPoints[h] = (mCell.CalcLocalCrystalPosition(indices[adjEdges[h,0]]) + mCell.CalcLocalCrystalPosition(indices[adjEdges[h,1]])) / 2.0f;

        // normal approximation, but should face in same direction. Since its only used for sorting and not normal calculation it should suffice
        Vector3 norm = mCell.CalcLocalCrystalPosition(indices[adjEdges[0,1]]) - mCell.CalcLocalCrystalPosition(indices[minorityIndices[0]]);
        if (!IsCrystalSolid(indices[minorityIndices[0]]))
        {
            norm = -norm;
        }

        SortPolyPoints(ref polyPoints, norm);
        CreatePoly(polyPoints);
    }

    private void SortPolyPoints(ref Vector3[] polyPoints, Vector3 norm)
    {
        //PolyPointsClockwiseComparer polyComparer = new PolyPointsClockwiseComparer(polyPoints[0], norm);
        //Array.Sort(polyPoints, 1, polyPoints.Length - 1, polyComparer);
        bool sthChanged = true;

        while (sthChanged)
        {
            sthChanged = false;
            for (int i = 1; i < polyPoints.Length - 1; i++)
            {
                Vector3 cross = Vector3.Cross(polyPoints[i+1] - polyPoints[0], polyPoints[i] - polyPoints[0]);
                float dot = Vector3.Dot(cross, norm);
                if (dot < 0)
                {
                    Vector3 tmp = polyPoints[i];
                    polyPoints[i] = polyPoints[i+1];
                    polyPoints[i+1] = tmp;
                    sthChanged = true;
                }
            }
        }
    }

    private void CreatePoly(Vector3[] polyPoints)
    {
        int[] polyInd = new int[polyPoints.Length];
        Vector3 norm = Vector3.Normalize(Vector3.Cross(polyPoints[2] - polyPoints[0], polyPoints[1] - polyPoints[0]));

        for(int h=0; h < polyInd.Length; h++)
        {
			mMi.vertices.Add (polyPoints[h]);
			mMi.normals.Add (norm);
			mMi.uvs.Add (new Vector2(0,0));
            polyInd[h] = mMi.vertices.Count - 1;
        }

        for (int h = 1; h < polyInd.Length - 1; h++)
        {
            mMi.indices.Add(polyInd[h+1]);
            mMi.indices.Add(polyInd[h]);
            mMi.indices.Add(polyInd[0]);
        }

        mMi.polyTriCount.Add(polyInd.Length-2);
    }

    private void RemoveFromEdgeArray(ref int[,] edges, int index)
    {
        for (int h = 1; h <= edges.GetUpperBound(0); h++)
        {
            if (h > index)
            {
                edges[h - 1, 0] = edges[h, 0];
                edges[h - 1, 1] = edges[h, 1];
            }
        }
        int[,] finalArr = new int[edges.GetUpperBound(0), 2];
        Array.Copy(edges, finalArr, edges.GetUpperBound(0) * 2);
        edges = finalArr;
    }

    private int[,] GetAdjEdges(int i, int[,] edges)
    {
        int[,] edgeArr = new int[edges.GetUpperBound(0) + 1, 2];

        int k = 0;
        for (int h = 0; h <= edges.GetUpperBound(0); h++)
        {
            if (edges[h, 0] == i)
            {
                edgeArr[k, 0] = edges[h, 0];
                edgeArr[k, 1] = edges[h, 1];
                k++;
            }
            if (edges[h, 1] == i)
            {
                edgeArr[k, 0] = edges[h, 1];
                edgeArr[k, 1] = edges[h, 0];
                k++;
            }
        }

        int[,] finalArr = new int[k, 2];
        Array.Copy(edgeArr, finalArr, k * 2);
        return finalArr;
    }

    private bool IsCrystalSolid(int index)
    {
        return (mCell.crystalValues[index] > 0);
    }

    private int[] FindMinority(int[] indices)
    {
        List<int> solidIndices = new List<int>();
        List<int> emptyIndices = new List<int>();

        for (int h = 0; h < indices.Length; h++)
        {
            if (IsCrystalSolid(indices[h])) // TODO: make better test for solid Crystal
                solidIndices.Add(h);
            else
                emptyIndices.Add(h);
        }

        if (solidIndices.Count <= emptyIndices.Count)
            return solidIndices.ToArray();
        else
            return emptyIndices.ToArray();
    }

    /// <summary>
    /// Checks if two points of a segment defined through their indices share an edge
    /// </summary>
    /// <param name="i1">Index of the List of points of the segment</param>
    /// <param name="i2">Index of the List of points of the segment</param>
    /// <param name="edges">Array of possible edges in the segment</param>
    /// <returns>Returns the index in the possible edges array if an edge was found, -1 otherwise</returns>
    private int FindEdge(int i1, int i2, int[,] edges)
    {
        for (int h = 0; h <= edges.GetUpperBound(0); h++)
        {
            int e1 = edges[h, 0];
            int e2 = edges[h, 1];

            if (((e1 == i1) && (e2 == i2)) || ((e1 == i2) && (e2 == i1)))
                return h;
        }
        return -1;
    }
}
