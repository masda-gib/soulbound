using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ChacheElement
{
    public TerrainCell Content;
    public DateTime LastTimeUsed;

    public ChacheElement(TerrainCell pContent, DateTime pInsertTime)
    {
        Content = pContent;
        LastTimeUsed = pInsertTime;
    }
}

internal class CellCache
{
    private int cacheSize;
    private Dictionary<Vector3, ChacheElement> data;

    public int Count
    {
        get { return data.Count; }
    }

    public CellCache(int size)
    {
        cacheSize = size;
        data = new Dictionary<Vector3, ChacheElement>();
    }

    public void Set(Vector3 position, TerrainCell dataElement)
    {
        DateTime insertTime = DateTime.Now;
        if (data.ContainsKey(position))
        {
            data[position].Content = dataElement;
            data[position].LastTimeUsed = insertTime;
        } 
        else
        {
            if (data.Count >= cacheSize)
            {
                Vector3 oldestIndex = new Vector3();
                DateTime oldestTime = insertTime;
                foreach (KeyValuePair<Vector3, ChacheElement> kp in data)
                {
                    if (kp.Value.LastTimeUsed < oldestTime)
                    {
                        oldestTime = kp.Value.LastTimeUsed;
                        oldestIndex = kp.Key;
                    }
                }
                data[oldestIndex].Content.UnLoad();
                data[oldestIndex].Content = null;
                data.Remove(oldestIndex);
            }
            data.Add(position, new ChacheElement(dataElement, insertTime));
        }
    }

    public TerrainCell Get(Vector3 position)
    {
        ChacheElement elem;
        bool success = data.TryGetValue(position, out elem);
        if (success)
        {
            elem.LastTimeUsed = DateTime.Now;
            return elem.Content;
        }
        return null;
    }
}
