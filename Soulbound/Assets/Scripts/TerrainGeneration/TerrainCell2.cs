using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TerrainCell2 : MonoBehaviour 
{
	public Index3 Index {
		get;
		private set;
	}

	private Dictionary<Index3, TerrainCell2> terrain;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

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

}
