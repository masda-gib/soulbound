using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrystalWorld {

	public struct MeshInfo
	{
		public List<Vector3> vertices;
		public List<Vector3> normals;

		public List<int> indices;

		public bool IsValid {
			get { return vertices != null && indices != null; }
		}

		public void Init() {
			vertices = new List<Vector3> ();
			normals = new List<Vector3> ();
			indices = new List<int> ();
		}

		public void Append(MeshInfo other, float positionTolerance)
		{
			if (!(this.IsValid) || !(other.IsValid)) {
				return;
			}

			var pointMap = new Dictionary<int, int> ();
			var newVerts = new List<Vector3> ();

			var o = 0;
			foreach (var p in other.vertices) {

				var vIndex = -1;
				var i = 0;
				foreach (var op in this.vertices) {
					if (Vector3.Distance (op, p) <= positionTolerance) {
						vIndex = i;
					}
					i++;
				}

				if (vIndex < 0) {
					pointMap.Add (o, this.vertices.Count + newVerts.Count);
					newVerts.Add (p);
				} else {
					pointMap.Add (o, vIndex);
				}
				o++;
			}
			this.vertices.AddRange (newVerts);
		
			this.indices.AddRange(other.indices.Select (x => pointMap[x]));
		}

		public void GenerateNormals() {

			if (!(this.IsValid)) {
				return;
			}

			if (normals == null) {
				normals = new List<Vector3> ();
			} else {
				normals.Clear ();
			}

			var faceNormals = new List<Vector3>();
			for (int i = 0; i < indices.Count / 3; i++) {
				var i0 = indices[i * 3];
				var i1 = indices[(i * 3) + 1];
				var i2 = indices[(i * 3) + 2];
				faceNormals.Add(Vector3.Cross (vertices [i1] - vertices [i0], vertices [i2] - vertices [i0]));
			}

			for (int vi = 0; vi < vertices.Count; vi++) {
				var n = new Vector3 ();
				var si = 0;
				var fi = 0;
				do {
					fi = indices.IndexOf(vi, si);
					if ( fi >= 0) {
						n += faceNormals[fi / 3];
						si = fi + 1;
					}
				} while (fi >= 0);
				normals.Add(n.normalized);
			}

		}

		public void Smooth(float smooth, float relax = 0) {

			var smoothVerts = GetSmothedVertices (smooth, relax);
			if (smoothVerts != null) {
				vertices = smoothVerts;
			}
		}

		public List<Vector3> GetSmothedVertices(float smooth, float relax = 0) {

			if (!(this.IsValid)) {
				return null;
			}

			if ((smooth != 0) && (normals == null || vertices.Count != normals.Count)) {
				return null;
			}

			var smoothVerts = new List<Vector3>();

			var vi = 0;
			foreach (var v in vertices) {
				var adjIndices = new List<int> ();

				var si = 0;
				var fi = 0;
				do {
					fi = indices.IndexOf(vi, si);
					if ( fi >= 0) {
						switch (fi % 3) {
						case 0:
							if (!(adjIndices.Contains(indices[fi + 1]))) {adjIndices.Add(indices[fi + 1]);}
							if (!(adjIndices.Contains(indices[fi + 2]))) {adjIndices.Add(indices[fi + 2]);}
							break;
						case 1:
							if (!(adjIndices.Contains(indices[fi - 1]))) {adjIndices.Add(indices[fi - 1]);}
							if (!(adjIndices.Contains(indices[fi + 1]))) {adjIndices.Add(indices[fi + 1]);}
							break;
						case 2:
							if (!(adjIndices.Contains(indices[fi - 2]))) {adjIndices.Add(indices[fi - 2]);}
							if (!(adjIndices.Contains(indices[fi - 1]))) {adjIndices.Add(indices[fi - 1]);}
							break;
						}
						si = fi + 1;
					}
				} while (fi >= 0);

				var adjCenter = Vector3.zero;
				foreach (var ai in adjIndices) {
					adjCenter += vertices [ai];
				}
				adjCenter = adjCenter * (1.0f / adjIndices.Count);
				var diff = adjCenter - v;
				var normDiff = Vector3.zero;
				if (smooth != 0) {
					normDiff = normals [vi] * Vector3.Dot (normals [vi], diff);
				}
				smoothVerts.Add(v + (normDiff * smooth) + (diff * relax));

				vi++;
			}

			return smoothVerts;
		}

	}

}
