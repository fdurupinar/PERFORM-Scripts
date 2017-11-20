
using UnityEngine;
using Meta.Numerics.Statistics;
using System.Collections.Generic;

public struct BBox {
    public Vector3 Min;
    public Vector3 Max;
}

public class KeyframeExtractor{
    
    private List<Vector3> _posListUpper;
    private List<Vector3> _posListLower;

    private List<float> _traceList;

    public KeyframeExtractor() {
        _posListUpper = new List<Vector3>();
        _posListLower = new List<Vector3>();
        _traceList = new List<float>();
    }
    private void ClearLists() {        
        _posListUpper.Clear();
        _posListLower.Clear();
    }

    
    public void Reset() {
        _traceList.Clear();
    }
    public void AddLower(List<Vector3> posList) {
        foreach (Vector3 p in posList)
            _posListLower.Add(p);
    }

    public void ComputeBoundingBoxVolume(List<Vector3> posList) {
        Vector3 min, max;

        min = max = posList[0];
        foreach(Vector3 v in posList) {
            if (v.x < min.x)
                min.x = v.x;
            if (v.y < min.y)
                min.y = v.y;
            if (v.z < min.z)
                min.z = v.z;


            if (v.x > max.x)
                max.x = v.x;
            if (v.y > max.y)
                max.y = v.y;
            if (v.z > max.z)
                max.z = v.z;
        }

        _traceList.Add((max - min).x*(max - min).y*(max - min).z);

        
        
    }
    //Find the local minima indices in traceList
    public List<int> ExtractKeys() {
        List<int> minList = new List<int>();
        if ((_traceList.Count > 1 && _traceList[0] < _traceList[1]) || _traceList.Count == 1) {
            minList.Add(0);
        }

        for (int i = 1; i < _traceList.Count - 1; i++) {

            if (_traceList[i] < _traceList[i - 1] && _traceList[i] < _traceList[i + 1])
                minList.Add(i);
        }

        if ((_traceList.Count > 1 && _traceList[_traceList.Count - 1] < _traceList[_traceList.Count - 2]))
            minList.Add(_traceList.Count - 1);

        
        List<int> maxList = new List<int>();
        if ((_traceList.Count > 1 && _traceList[0] > _traceList[1]) || _traceList.Count == 1) {
            maxList.Add(0);
        }

        for (int i = 1; i < _traceList.Count - 1; i++) {

            if (_traceList[i] > _traceList[i - 1] && _traceList[i] > _traceList[i + 1])
                maxList.Add(i);
        }

        if ((_traceList.Count > 1 && _traceList[_traceList.Count - 1] > _traceList[_traceList.Count - 2]))
            maxList.Add(_traceList.Count - 1);

        minList.AddRange(maxList);

        if (!minList.Contains(0)) //first and last keys must always be in
            minList.Add(0);

        if (!minList.Contains(_traceList.Count-1)) //first and last keys must always be in
            minList.Add(_traceList.Count - 1);

        minList.Sort();

        
        return minList;


    }

}
