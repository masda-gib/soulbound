using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class TerrainGenerator
{
    public abstract string Name { get; }
    public abstract Dictionary<string, Type> Parameters { get; }

    public abstract object GetParameter(string param);
    public abstract bool SetParameter(string param, object val);

    public abstract void Initialize();
    public abstract byte Generate(Vector3 globalPos);
}
