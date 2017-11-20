using UnityEngine;
using System.Collections.Generic;

public class DropDownRect {

    private bool _showList;
    private Vector2 _scrollView = Vector2.zero;
    private const int _itemHeight = 25;
    private Rect _ddRect;
    public Rect DdRect {
        get {
            return _ddRect;
        }
        set {
            _ddRect = value;
        }
    }

    int _lastSelInd;

    private List<string> _ddList;
    public List<string> DdList {
        set {
            _ddList = value;
        }
        get {
            return _ddList;
        }
    }


    public DropDownRect(Rect rect) {
        DdRect = rect;
    }

    //Returns the index of the selected item    
    public int ShowDropDownRect() {

        int selInd = _lastSelInd;
        GUI.color = Color.white;
        if (GUI.Button(new Rect(0, _ddRect.y, _ddRect.width, _itemHeight), "")) {
            _showList = !_showList;
        }

        if (_showList) {
            _scrollView = GUI.BeginScrollView(new Rect(0, (_ddRect.y + _itemHeight), _ddRect.width, _ddRect.height), _scrollView, new Rect(0, 0, _ddRect.width, (_ddList.Count * _itemHeight)), false, false);

            GUI.Box(new Rect(0, 0, _ddRect.width, _ddList.Count * _itemHeight), "");


            for (int index = 0; index < _ddList.Count; index++) {
                if (GUI.Button(new Rect(0, (index * _itemHeight), _ddRect.width, _itemHeight), "")) {
                    _showList = false;
                    selInd = index;
                }

                GUI.Label(new Rect(0, (index * _itemHeight), _ddRect.width, _itemHeight), _ddList[index]);

            }

            GUI.EndScrollView();
        }
        else {
            GUI.Label(new Rect(0, _ddRect.y, _ddRect.width, _itemHeight), _ddList[_lastSelInd]);

        }

        _lastSelInd = selInd;
        return selInd;
    }


}


