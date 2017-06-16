using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public interface ICellMeshInfoGenerator : IMeshInfoService {

		MeshInfo GenerateMeshInfo (CellInfo cell, Vector3 vertexOffset);

	}

}
