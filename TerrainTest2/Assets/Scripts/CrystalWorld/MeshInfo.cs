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

		public float positionTolerance;

		public bool IsValid {
			get { return vertices != null && indices != null; }
		}

		public void Init(int points, int tris, float positionTolerance)
		{
			vertices = new Vector3[points];
			normals = new Vector3[points];
			uvs = new Vector3[points];
			indices = new int[tris * 3];
			this.positionTolerance = positionTolerance;
		}

		public void Append(MeshInfo other)
		{
			if (!(this.IsValid && other.IsValid)) {
				return;
			}

			var pointMap = new Dictionary<int, int> ();
			var newVerts = new List<Vector3> ();

			var o = 0;
			foreach (var p in other.vertices) {

				var vIndex = -1;
				for (int i = 0; i < this.vertices.Length; i++) {
					if (Vector3.Distance (this.vertices [i], p) <= positionTolerance) {
						vIndex = i;
					}
				}

				if (vIndex < 0) {
					pointMap.Add (o, this.vertices.Length + newVerts.Count);
					newVerts.Add (p);
				} else {
					pointMap.Add (o, vIndex);
				}
				o++;
			}
			this.vertices = this.vertices.Concat (newVerts).ToArray ();
		
			this.indices = this.indices.Concat (other.indices.Select (x => pointMap[x])).ToArray();
		}
	}

}
