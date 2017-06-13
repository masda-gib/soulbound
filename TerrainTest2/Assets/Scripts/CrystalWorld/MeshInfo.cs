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

		public void GenerateNormals() {
		
			if (!(this.IsValid)) {
				return;
			}

			if (normals == null || normals.Length != vertices.Length) {
				normals = new Vector3[vertices.Length];
			}

			var faceNormals = new Vector3[indices.Length / 3];
			for (int i = 0; i < indices.Length / 3; i++) {
				var i0 = indices[i * 3];
				var i1 = indices[(i * 3) + 1];
				var i2 = indices[(i * 3) + 2];
				faceNormals [i] = Vector3.Cross (vertices [i1] - vertices [i0], vertices [i2] - vertices [i0]);
			}

			var vi = 0;
			foreach (var v in vertices) {
				var n = new Vector3 ();
				var si = 0;
				var fi = 0;
				do {
					fi = System.Array.IndexOf(indices, vi, si);
					if ( fi >= 0) {
						n += faceNormals[fi / 3];
						si = fi + 1;
					}
				} while (fi >= 0);
				normals [vi] = n.normalized;
				vi++;
			}

		}
	}

}
