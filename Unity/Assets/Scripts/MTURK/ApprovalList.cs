using UnityEngine;
using System.Collections.Generic;
using System.IO;


public class ApprovalList : MonoBehaviour {

    List<string> _userList = new List<string>();
    List<string> _hitList = new List<string>();
    List<string> _qualityList = new List<string>();
    List<string> _approvedList = new List<string>();
    List<string> _rejectedList = new List<string>();

    public void Start() {
        ParseWorkerLists();
        UpdateApprovalList();
        UpdateBatchFile();


    }
    public void ParseWorkerLists() {
        var reader = new StreamReader(File.OpenRead("userInfo.csv"));
        
        while (!reader.EndOfStream) {
            string line = reader.ReadLine();
            string[] tokens = line.Split(',');

            _userList.Add(tokens[0]);
            _hitList.Add(tokens[1]);
            _qualityList.Add(tokens[5]);
        }

    }


    public void UpdateApprovalList() {
        for (int i = 0; i < _userList.Count; i++) {
            if (_qualityList.Count > i)
                if (_qualityList[i].Equals("2"))
                    _approvedList.Add(_userList[i]);
                else
                    _rejectedList.Add(_userList[i]);
        }
    }



    public void UpdateBatchFile() {
        string fileName = "batch.csv";
        string outFileName = "batchUpdated.csv";
        string[] content = File.ReadAllLines(fileName);
         
        for(int i = 1; i < content.Length; i++) { //skip the first line

            string[] tokens = content[i].Split(',');
            string updatedLine="";
            if(_approvedList.Contains(tokens[15])) //"workerId"
                tokens[28] = "x";
            else
                tokens[29] = "You gave incorrect answer(s) to the quality check questions which test your attention. It is important that you don't randomly select the answers.";

            foreach(string token in tokens)
                updatedLine += token + ",";
            
            content[i] = updatedLine;

    }


        //Update content
        using (StreamWriter sw = new StreamWriter(outFileName)) {            
            foreach (string t in content)
                sw.WriteLine(t);
        }
    }
    

}
