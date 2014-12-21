using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

enum Neighbors { TopNorthEast, TopNorthWest, TopSouth, East, NorthEast, NorthWest, FarTopNorth, West, SouthWest, SouthEast };

public class TerrainCell : MonoBehaviour 
{
    public Vector3 cellIndex;
	
    private PagingTerrain mParent;

	internal byte[] mCrystals;

    private Vector3[] verts;
	private Vector2[] uv;
    private int[] indices;

	#region Helper methods
    internal int CalcCrystalIndex(int x, int y, int z)
    {
        if (
            (x < 0) || (x >= mParent.widthNum) ||
            (y < 0) || (y >= mParent.heightNum) ||
            (z < 0) || (z >= mParent.lengthNum)
            )
            return -1;
        return (mParent.widthNum * y * mParent.lengthNum) + (mParent.widthNum * z) + x;
    }
	
    internal int[] CalcCrystalSteps(int index)
    {
        int[] steps = new int[3];

        steps[1] = index / (mParent.widthNum * mParent.lengthNum);
        index -= steps[1] * mParent.widthNum * mParent.lengthNum;
        steps[2] = index / mParent.widthNum;
        steps[0] = index - (steps[2] * mParent.widthNum);

        return steps;
    }
	
    internal Vector3 CalcLocalCrystalPosition(int index)
    {
        int[] steps = CalcCrystalSteps(index);
        return CalcLocalCrystalPosition(steps[0], steps[1], steps[2]);
    }
	
    internal Vector3 CalcLocalCrystalPosition(int xStep, int yStep, int zStep)
    {
        float xCoord = (-0.5f * mParent.CellWidth) + ((xStep + 0.5f) * mParent.baseSpacing);
        float yCoord = (-0.5f * mParent.CellHeight) + ((yStep + 0.5f) * mParent.baseSpacing * 0.8f);
        float zCoord = (-0.5f * mParent.CellLength) + ((zStep + 0.5f) * mParent.baseSpacing * 0.8660254f);

        if (zStep % 2 == 0)
            xCoord += mParent.baseSpacing / 4.0f;
        else
            xCoord -= mParent.baseSpacing / 4.0f;

        switch (yStep % 3)
        {
            case 0:
                zCoord -= mParent.baseSpacing * 0.8660254f / 1.5f; // eigentlich /3, aber dann muss x-Versatz umgekehrt werden, wenn yStep%3 = 1
                break;
            case 2:
                zCoord += mParent.baseSpacing * 0.8660254f / 1.5f; // eigentlich /3, aber dann muss x-Versatz umgekehrt werden, wenn yStep%3 = 1
                break;
        }

        return new Vector3(xCoord, yCoord, zCoord);
    }
    internal int[] GetNeighborCrystalIndices(int index)
    {
        List<int> nb = new List<int>();

        int[] baseSteps = CalcCrystalSteps(index);
        int xDir = baseSteps[2] % 2;  // Versatz-Vorhandensein
        int zDir = 0;
        if (baseSteps[1] % 3 == 2)
            zDir = 2; // Versatz-Vorhandensein

        int[] nSteps = new int[3];

        // Oben-Nord-Ost
        nSteps[0] = baseSteps[0] + 1 - xDir;
        nSteps[1] = baseSteps[1] + 1;
        nSteps[2] = baseSteps[2] - 1 + zDir;
        nb.Add(CalcCrystalIndex(nSteps[0], nSteps[1], nSteps[2]));

        // Oben-Nord-West
        nSteps[0] = baseSteps[0] - xDir;
        nSteps[1] = baseSteps[1] + 1 ;
        nSteps[2] = baseSteps[2] - 1 + zDir;
        nb.Add(CalcCrystalIndex(nSteps[0], nSteps[1], nSteps[2]));

        // Oben-Süd
        nSteps[0] = baseSteps[0];
        nSteps[1] = baseSteps[1] + 1;
        nSteps[2] = baseSteps[2] + zDir;
        nb.Add(CalcCrystalIndex(nSteps[0], nSteps[1], nSteps[2]));

        // Ost
        nSteps[0] = baseSteps[0] + 1;
        nSteps[1] = baseSteps[1];
        nSteps[2] = baseSteps[2];
        nb.Add(CalcCrystalIndex(nSteps[0], nSteps[1], nSteps[2]));

        // Nord-Ost
        nSteps[0] = baseSteps[0] + 1 - xDir;
        nSteps[1] = baseSteps[1];
        nSteps[2] = baseSteps[2] - 1;
        nb.Add(CalcCrystalIndex(nSteps[0], nSteps[1], nSteps[2]));

        // Nord-West
        nSteps[0] = baseSteps[0] - xDir;
        nSteps[1] = baseSteps[1];
        nSteps[2] = baseSteps[2] - 1;
        nb.Add(CalcCrystalIndex(nSteps[0], nSteps[1], nSteps[2]));

        // Oben-Weit-Nord
        nSteps[0] = baseSteps[0];
        nSteps[1] = baseSteps[1] + 1;
        nSteps[2] = baseSteps[2] - 2 + zDir;
        nb.Add(CalcCrystalIndex(nSteps[0], nSteps[1], nSteps[2]));

        /*
        // West
        nSteps[0] = baseSteps[0] - 1;
        nSteps[1] = baseSteps[1];
        nSteps[2] = baseSteps[2];
        nb.Add(CalcCrystalIndex(nSteps[0], nSteps[1], nSteps[2]));

        // Süd-West
        nSteps[0] = baseSteps[0] - xDir;
        nSteps[1] = baseSteps[1];
        nSteps[2] = baseSteps[2] + 1;
        nb.Add(CalcCrystalIndex(nSteps[0], nSteps[1], nSteps[2]));

        // Süd-Ost
        nSteps[0] = baseSteps[0] + 1 - xDir;
        nSteps[1] = baseSteps[1];
        nSteps[2] = baseSteps[2] + 1;
        nb.Add(CalcCrystalIndex(nSteps[0], nSteps[1], nSteps[2]));
         */

        return nb.ToArray();
    }
	#endregion
	
	#region Cell and crystal content methods
    private string GenFilePath(string levelName, Vector3 indexVec)
    {
        return Path.Combine (Path.Combine("levels", mParent.lvlName), (cellIndex.x.ToString() + "_" + cellIndex.y.ToString() + "_" + cellIndex.z.ToString() + ".ccd"));
    }

    public byte GetCrystalValue(int x, int y, int z)
    {
        return mCrystals[CalcCrystalIndex(x, y, z)];
    }

    private IEnumerator Load(string levelName, Vector3 indexVec, TerrainGenerator gen)
    {
		yield return 0;
		
        //mParent.lvlName = levelName;
        //mCellIndex = indexVec;

        bool couldLoadFromFile = false;

        string fileToLoad = GenFilePath(mParent.lvlName, cellIndex);

        if (File.Exists(fileToLoad))
        {
            byte[] cellData = File.ReadAllBytes(fileToLoad);
            if (cellData.Length == mCrystals.Length)
            {
                mCrystals = cellData;
                couldLoadFromFile = true;
            }
        }

        if (!couldLoadFromFile)
        {
            for (int x = 0; x < mParent.widthNum; x++)
			{
                for (int y = 0; y < mParent.heightNum; y++)
				{
                    for (int z = 0; z < mParent.lengthNum; z++)
                    {
                        mCrystals[CalcCrystalIndex(x, y, z)] = gen.Generate(CalcLocalCrystalPosition(x, y, z) + transform.position);
                    }
				}
			}
            Save();
        }

        CalculateMesh();
    }
	#endregion

//    public IAsyncResult BeginLoad(string levelName, Vector3 indexVec, CrystalGenerator gen)
//    {
//        return mLoadDelegate.BeginInvoke(levelName, indexVec, gen, null, null);
//    }

    private void CalculateMesh()
    {
        // TEST
        MeshCalculator mc = new MeshCalculator(this);
        MeshInfo mi = mc.Calculate();

        if (mi.vertices.Count > 0)
        {
	        Mesh mesh = new Mesh();
	        gameObject.GetComponent<MeshFilter>().mesh = mesh;
	        mesh.vertices = mi.vertices.ToArray();
			mesh.normals = mi.normals.ToArray();
	        mesh.uv = mi.uvs.ToArray();
	        mesh.triangles = mi.indices.ToArray();
        }

        mc = null;
        mi = null;
        // END TEST

        /*
        int numCrystals = 0;
        List<VertexPositionNormalTexture> vList = new List<VertexPositionNormalTexture>();
        List<int> iList = new List<int>();

        Vector3 vUp = new Vector3(0, mParent.baseSpacing * 1.0f, 0);
        Vector3 vEast = new Vector3(mParent.baseSpacing * 1.0f, 0, 0);
        Vector3 vNorth = new Vector3(0, 0, -mParent.baseSpacing * 1.0f);

        for (int i = 0; i < mCrystals.Length; i++)
        {
            if (mCrystals[i] == 1)
            {
                int firstIndex = vList.Count;

                Vector3 vBase = CalcLocalCrystalPosition(i);
                GetNeighborCrystalIndices(i);

                vList.Add(new VertexPositionNormalTexture(vBase + vUp, Vector3.Normalize(vUp), Vector2.Zero));
                vList.Add(new VertexPositionNormalTexture(vBase + vEast, Vector3.Normalize(vEast), Vector2.Zero));
                vList.Add(new VertexPositionNormalTexture(vBase + vNorth, Vector3.Normalize(vNorth), Vector2.Zero));
                vList.Add(new VertexPositionNormalTexture(vBase - vEast, Vector3.Normalize(-vEast), Vector2.Zero));
                vList.Add(new VertexPositionNormalTexture(vBase - vNorth, Vector3.Normalize(-vNorth), Vector2.Zero));
                vList.Add(new VertexPositionNormalTexture(vBase - vUp, Vector3.Normalize(-vUp), Vector2.Zero));

                iList.AddRange(new int[] { firstIndex, firstIndex + 2, firstIndex + 1 });
                iList.AddRange(new int[] { firstIndex, firstIndex + 3, firstIndex + 2 });
                iList.AddRange(new int[] { firstIndex, firstIndex + 4, firstIndex + 3 });
                iList.AddRange(new int[] { firstIndex, firstIndex + 1, firstIndex + 4 });
                iList.AddRange(new int[] { firstIndex + 1, firstIndex + 2, firstIndex + 5 });
                iList.AddRange(new int[] { firstIndex + 2, firstIndex + 3, firstIndex + 5 });
                iList.AddRange(new int[] { firstIndex + 3, firstIndex + 4, firstIndex + 5 });
                iList.AddRange(new int[] { firstIndex + 4, firstIndex + 1, firstIndex + 5 });
            }
        }

        if (vList.Count > 0)
        {
            vb = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vList.Count, BufferUsage.None);
            vb.SetData<VertexPositionNormalTexture>(vList.ToArray());
            ib = new IndexBuffer(GraphicsDevice, typeof(int), iList.Count, BufferUsage.None);
            ib.SetData<int>(iList.ToArray());
        }

        vList = null;
        iList = null;
         */
    }

    public bool Save()
    {
        string fileToSave = GenFilePath(mParent.lvlName, cellIndex);

        FileInfo fi = new FileInfo(fileToSave);
        if (!Directory.Exists(fi.DirectoryName))
        {
            Directory.CreateDirectory(fi.DirectoryName);
        }

        try
        {
            File.WriteAllBytes(fileToSave, mCrystals);
            return true;
        }
        catch (System.Exception ex)
        {
            System.Console.Out.WriteLine(ex.Message);
        }

        return false;
    }

    public void UnLoad()
    {
		Destroy(this.gameObject);
    }

	// Use this for initialization
	void Start () 
	{
		
		if(mParent == null)
        	mParent = transform.parent.gameObject.GetComponent<PagingTerrain>();
		
        mCrystals = new byte[mParent.widthNum * mParent.heightNum * mParent.lengthNum];

        StartCoroutine(Load (mParent.lvlName, cellIndex, mParent.terrainGenerator));
	}
}
