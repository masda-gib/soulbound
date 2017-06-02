using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrystalWorld {

	public struct MeshInfo
	{
		public Vector3[] vertices;
		public Vector3[] normals;
		public Vector3[] uvs;

		public int[] indices;

		public bool isValid {
			get { return vertices != null && indices != null; }
		}

		public MeshInfo(int points, int tris)
		{
			vertices = new Vector3[points];
			normals = new Vector3[points];
			uvs = new Vector3[points];
			indices = new int[tris * 3];
		}

		public void Append(MeshInfo other)
		{
			if (!(this.isValid && other.isValid)) {
				return;
			}

			var pointMap = new Dictionary<int, int> ();
			var newVerts = new List<Vector3> ();

			var o = 0;
			foreach (var p in other.vertices) {
				var i = System.Array.IndexOf (this.vertices, p);
				if (i < 0) {
					pointMap.Add (o, this.vertices.Length + newVerts.Count);
					newVerts.Add (p);
				} else {
					pointMap.Add (o, i);
				}
				o++;
			}
			this.vertices = this.vertices.Concat (newVerts).ToArray ();
		
			this.indices = this.indices.Concat (other.indices.Select (x => pointMap[x])).ToArray();
		}
	}

}
