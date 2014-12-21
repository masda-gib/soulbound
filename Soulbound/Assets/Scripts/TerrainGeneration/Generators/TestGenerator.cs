using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class TestGenerator : TerrainGenerator
{
    public override string Name
    {
        get { return "Erster Test"; }
    }

    public override Dictionary<string, Type> Parameters
    {
        get { return new Dictionary<string,Type>(); }
    }

    public override object GetParameter(string param)
    {
        return null;
    }

    public override bool SetParameter(string param, object val)
    {
        return false;
    }

    public override void Initialize()
    {
    }

    public override byte Generate(Vector3 globalPos)
    {
        byte val = 0;

        float height = (float)(10.0 * Math.Sin(globalPos.x / 20.0) + 13.0 * Math.Sin(globalPos.z/ 28.0));
        height += (float)(7.0 * Math.Sin(globalPos.x / 12.0) + 6.0 * Math.Sin(globalPos.z/ 5.0));
		height = Mathf.Max (-6, height);
		if (globalPos.y < height)
        {
            val = 1;
        }
        else
        {
            val = 0;
        }

        return val;
    }
}
