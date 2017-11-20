using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

struct DriveRatios {
    public string S1, W1, T1, F1;
    public string S2, W2, T2, F2;
    public string[] Ratios;
    public float[] PVals;
    public string[] CellColor;
    public string[] Stars;
};

public class DriveRatioLatexConverter : MonoBehaviour {


    List<DriveRatios> _driveRatiosList;

    public void Start() {
        ParseData();
        WriteData();
    }

    public void ParseCoefs() {

    }

    public void WriteData() {
        using (StreamWriter sw = new StreamWriter("ratiosDrivesSorted.txt")) {
            for (int i = 0; i < _driveRatiosList.Count; i++) {
                if (i / 12 == 0)
                    sw.Write("\\ES");
                else if (i / 12 == 1)
                    sw.Write("\\EW");
                else if (i / 12 == 2)
                    sw.Write("\\ET");
                else if (i / 12 == 3)
                    sw.Write("\\EF");

                sw.WriteLine(" & \\" + _driveRatiosList[i].S1 + " & \\" + _driveRatiosList[i].W1 + " & \\" + _driveRatiosList[i].T1 + " & \\" + _driveRatiosList[i].F1 +
                             " & \\" + _driveRatiosList[i].S2 + " & \\" + _driveRatiosList[i].W2 + " & \\" + _driveRatiosList[i].T2 + " & \\" + _driveRatiosList[i].F2  +
                             " & " + _driveRatiosList[i].CellColor[0] + " $" + _driveRatiosList[i].Ratios[0] + "" + _driveRatiosList[i].Stars[0] + "$" +
                             " & " + _driveRatiosList[i].CellColor[1] + " $" + _driveRatiosList[i].Ratios[1] + "" + _driveRatiosList[i].Stars[1] + "$" +
                             " & " + _driveRatiosList[i].CellColor[2] + " $" + _driveRatiosList[i].Ratios[2] + "" + _driveRatiosList[i].Stars[2] + "$" +
                             " & " + _driveRatiosList[i].CellColor[3] + " $" + _driveRatiosList[i].Ratios[3] + "" + _driveRatiosList[i].Stars[3] + "$" +
                             " & " + _driveRatiosList[i].CellColor[4] + " $" + _driveRatiosList[i].Ratios[4] + "" + _driveRatiosList[i].Stars[4] + "$" +
                             " & " + _driveRatiosList[i].CellColor[5] + " $" + _driveRatiosList[i].Ratios[5] + "" + _driveRatiosList[i].Stars[5] + "$" +
                             " & " + _driveRatiosList[i].CellColor[6] + " $" + _driveRatiosList[i].Ratios[6] + "" + _driveRatiosList[i].Stars[6] + "$" +
                             " & " + _driveRatiosList[i].CellColor[7] + " $" + _driveRatiosList[i].Ratios[7] + "" + _driveRatiosList[i].Stars[7] + "$" +
                             " & " + _driveRatiosList[i].CellColor[8] + " $" + _driveRatiosList[i].Ratios[8] + "" + _driveRatiosList[i].Stars[8] + "$" +
                             " & " + _driveRatiosList[i].CellColor[9] + " $" + _driveRatiosList[i].Ratios[9] + _driveRatiosList[i].Stars[9]  + "$ \\\\");

                if (i % 12 == 11) {
                    sw.WriteLine();
                    sw.WriteLine("\\midrule");
                }
            sw.WriteLine();
            }
        }
        
    }

    public void ParseData() {
          _driveRatiosList = new List<DriveRatios>();
        using (var reader = new StreamReader(File.OpenRead("ratiosDrives.txt"))) {
            reader.ReadLine();
            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                string[] tokens = line.Split('\t');
                DriveRatios dr = new DriveRatios();
                dr.S1 = tokens[0];
                dr.W1 = tokens[1];
                dr.T1 = tokens[2];
                dr.F1 = tokens[3];
                dr.S2 = tokens[4];
                dr.W2 = tokens[5];
                dr.T2 = tokens[6];
                dr.F2 = tokens[7];
                dr.Ratios = new string[10];
                dr.PVals = new float[10];
                dr.CellColor = new string[10];
                dr.Stars = new string[10];
                for (int i = 0, j = 8; i < 10; i++) {
                    float ratio = float.Parse(tokens[j++]);
                    dr.PVals[i] = float.Parse(tokens[j++]);

                    if(dr.PVals[i] < 0.001){
                        dr.Stars[i] = "^{***}";
                    }
                    else if(dr.PVals[i] < 0.01){
                        dr.Stars[i] = "^{**}";
                    }
                    else if(dr.PVals[i] < 0.05){
                        dr.Stars[i] = "^{*}";
                    }

                    //Funda
                    dr.Stars[i] = "";

                    if(dr.PVals[i] < 0.05){
                        if(ratio > 0.5f)
                            dr.CellColor[i] = "\\cellcolor{darkGray}";
                        else
                            dr.CellColor[i] = "\\cellcolor{lightGray}";
                    }
                    else
                        dr.CellColor[i] = "\\cellcolor{white}";


                    dr.Ratios[i] = string.Format("{0:0.000}", ratio);
                }
                
                _driveRatiosList.Add(dr);
        }
        }
    }
}