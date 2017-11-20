using UnityEngine;
using System.Collections;
using System.IO;

public class EditDriveData : MonoBehaviour {

	// Use this for initialization
	void Start () {
        string fileName = "drivesFunda.txt";
        string outFileName = "drivesOut.txt";
        StreamReader sr = new StreamReader(fileName);
        StreamWriter sw = new StreamWriter(outFileName);

        string[] content = File.ReadAllLines(fileName);

       
        for (int i = 0; i < content.Length; i++) {
            string[] tokens = content[i].Split('\t');

            sw.WriteLine("INSERT INTO driveData(userId, driveInd, animName, speed, v0, v1, ti, texp, tval, t0, t1, hr, hf, isDirect, squash, wb, wx, wt,wf, et, ef, d,  tr, tf, shapeT0, shapeT1, encSpr0, sinRis0, retAdv0, encSpr1, sinRis1, retAdv1, continuity, bias, armLX, armLY,	armLZ,	armRX,	armRY,	armRZ)");
            sw.Write("VALUES (");

            for (int j = 0; j < tokens.Length; j++) {
                sw.Write("'" + tokens[j] + "'");
                if (j < tokens.Length - 1)
                    sw.Write(",");
                
            }
            sw.WriteLine(");\n\n");



        }
        
        sw.Close();
        sr.Close();


	
	}

    
	
}
