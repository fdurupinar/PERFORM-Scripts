
using System;
using UnityEngine;
using System.Collections;
using System.IO;


public class DriveParams {
    public string Info = "waiting.. waiting";



    //Timing
    public  float Speed = 1f;
    public  float V0 = 0f;
    public  float V1 = 0f;
    public  float Ti = 0.5f;
    public  float Texp = 1.0f;
    public  float Tval = 0f;
    public  float Continuity = 0f;
    public  float T0 = 0f;
    public  float T1 = 1f;
    public float GoalThreshold = 0f;
    //Flourishes
    public  float TrMag = 0f; //torso rotation
    public  float TfMag = 0f;

    public int FixedTarget = 0; //for direct head movement
    public  float HrMag = 0f; //head rotation
    public  float HfMag = 0f;
    public int HSign; //sign of hr magnitude
    public  int ExtraGoal = 0;
    public  int UseCurveKeys = 0;


    public  float SquashMag = 0f;
    public  float SquashF = 0f; //breah frequench
    public  float WbMag = 0f;
    public  float WxMag = 0f;
    public  float WtMag = 0f;
    public  float WfMag = 0f;
    public  float EtMag = 0f;
    public  float DMag = 0f;
    public  float EfMag = 0f;

    //Shape for drives
    public  float EncSpr0 = 0f;
    public  float SinRis0 = 0f;
    public  float RetAdv0 = 0f;

    public  float EncSpr1 = 0f;
    public  float SinRis1 = 0f;
    public  float RetAdv1 = 0f;

    public float EncSpr2 = 0f;
    public float SinRis2 = 0f;
    public float RetAdv2 = 0f;

    public  float ShapeTi = 0f;

    
    

    public bool DrivesAchieved = false;
    //Arm shape for drives
    public  Vector3[] Arm = new Vector3[2];

    public DriveParams() {
        ResetDriveParameters();
        DrivesAchieved = false;
    }

    public void ResetDriveParameters() {
		Speed = 1f;
        Tval = 0;
        V0 = 0;
        V1 = 0;
        Ti = 0.5f;
        Texp = 1f;
        GoalThreshold = 0f;
        Tval = 0f;
        Continuity = 0f;
        T0 = 0.0f;
        T1 = 1f;
        TrMag = 0f;
        TfMag = 0f;
        FixedTarget = 0;
        HrMag = 0f;
        HfMag = 0f;
        HSign = 1;
        ExtraGoal = 0;
        UseCurveKeys = 0;
        SquashMag = 0f;
        SquashF = 0f;
        WbMag = 0f;
        WxMag = 0f;
        WtMag = 0f;
        WfMag = 0f;
        EtMag = 0f;
        EfMag = 0f;
        DMag = 0f;
        EncSpr0 = 0f;
        SinRis0 = 0f;
        RetAdv0 = 0f;
        EncSpr1 = 0f;
        SinRis1 = 0f;
        RetAdv1 = 0f;
        EncSpr2 = 0f;
        SinRis2 = 0f;
        RetAdv2 = 0f;
        Arm[0] = Vector3.zero;
        Arm[1] = Vector3.zero;

    }



    // remember to use StartCoroutine when calling this function!
    public IEnumerator GetValuesDrives(int driveInd, string userId) {
        string url = "https://fling.seas.upenn.edu/~fundad/cgi-bin/v2/getDriveData.php";

        // Create a form object for sending high score data to the server
        var form = new WWWForm();
        form.AddField("userId", userId);
        form.AddField("driveInd", driveInd.ToString());


        // Create a download object
        var download = new WWW(url, form);

        // Wait until the download is done
        yield return download;

        if (download.error != null) {
            Info = download.error;
            Debug.Log(download.error);
        }
        else {
            Info = download.text;
            String[] vals = Info.Split('\t');
            if (Info.Length == 0)
                ResetDriveParameters();
            //Assign drive values 
            //should be exactly in this order
            int i = 0;
            Speed = float.Parse(vals[i++]);
            V0 = float.Parse(vals[i++]);
            V1 = float.Parse(vals[i++]);
            Ti = float.Parse(vals[i++]);
            Texp = float.Parse(vals[i++]);
            Tval = float.Parse(vals[i++]);
            Continuity = float.Parse(vals[i++]);
            T0 = float.Parse(vals[i++]);
            T1 = float.Parse(vals[i++]);
            TrMag = float.Parse(vals[i++]);
            TfMag = float.Parse(vals[i++]);
            FixedTarget = int.Parse(vals[i++]);
            HrMag = float.Parse(vals[i++]);
            HSign = HrMag >= 0 ? 1 : -1;
            HfMag = float.Parse(vals[i++]);
            ExtraGoal = int.Parse(vals[i++]);
            UseCurveKeys = int.Parse(vals[i++]);
            SquashMag = float.Parse(vals[i++]);
            SquashF = float.Parse(vals[i++]);
            WbMag = float.Parse(vals[i++]);
            WxMag = float.Parse(vals[i++]);
            WtMag = float.Parse(vals[i++]);
            WfMag = float.Parse(vals[i++]);
            EtMag = float.Parse(vals[i++]);
            EfMag = float.Parse(vals[i++]);
            DMag = float.Parse(vals[i++]);
            ShapeTi = float.Parse(vals[i++]);
            EncSpr0 = float.Parse(vals[i++]);
            SinRis0 = float.Parse(vals[i++]);
            RetAdv0 = float.Parse(vals[i++]);
            EncSpr1 = float.Parse(vals[i++]);
            SinRis1 = float.Parse(vals[i++]);
            RetAdv1 = float.Parse(vals[i++]);
            EncSpr2 = float.Parse(vals[i++]);
            SinRis2 = float.Parse(vals[i++]);
            RetAdv2 = float.Parse(vals[i++]);
            Arm[0].x = float.Parse(vals[i++]);
            Arm[0].y = float.Parse(vals[i++]);
            Arm[0].z = float.Parse(vals[i++]);
            Arm[1].x = float.Parse(vals[i++]);
            Arm[1].y = float.Parse(vals[i++]);
            Arm[1].z = float.Parse(vals[i++]);
            

            DrivesAchieved = true;
        }

        
    }



    // remember to use StartCoroutine when calling this function!
    public IEnumerator PostValuesDrives(int driveInd, string url) {
        

        // Create a form object for sending high score data to the server
        var form = new WWWForm();
        form.AddField("userId", UserInfo.UserId);
        form.AddField("driveInd", driveInd.ToString());


        form.AddField("speed", Speed.ToString());
        form.AddField("v0", V0.ToString());
        form.AddField("v1", V1.ToString());
        form.AddField("ti", Ti.ToString());
        form.AddField("texp", Texp.ToString());
        form.AddField("tval", Tval.ToString());
        form.AddField("continuity", Continuity.ToString());
        form.AddField("t0", T0.ToString());
        form.AddField("t1", T1.ToString());

        form.AddField("tr", TrMag.ToString());
        form.AddField("tf", TfMag.ToString());

        form.AddField("fixedTarget", FixedTarget.ToString());        
        form.AddField("hr", HrMag.ToString());
        form.AddField("hf", HfMag.ToString());
        form.AddField("extraGoal", ExtraGoal.ToString());
        form.AddField("useCurveKeys", UseCurveKeys.ToString());

        form.AddField("squash", SquashMag.ToString());
        form.AddField("squashF", SquashF.ToString());
        form.AddField("wb", WbMag.ToString());
        form.AddField("wx", WxMag.ToString());
        form.AddField("wt", WtMag.ToString());
        form.AddField("wf", WfMag.ToString());
        form.AddField("et", EtMag.ToString());
        form.AddField("d", DMag.ToString());
        form.AddField("ef", EfMag.ToString());

        form.AddField("shapeTi", ShapeTi.ToString());

        form.AddField("encSpr0", EncSpr0.ToString());
        form.AddField("sinRis0", SinRis0.ToString());
        form.AddField("retAdv0", RetAdv0.ToString());
        form.AddField("encSpr1", EncSpr1.ToString());
        form.AddField("sinRis1", SinRis1.ToString());
        form.AddField("retAdv1", RetAdv1.ToString());
        form.AddField("encSpr2", EncSpr2.ToString());
        form.AddField("sinRis2", SinRis2.ToString());
        form.AddField("retAdv2", RetAdv2.ToString());


        form.AddField("armLX", Arm[0].x.ToString());
        form.AddField("armLY", Arm[0].y.ToString());
        form.AddField("armLZ", Arm[0].z.ToString());
        form.AddField("armRX", Arm[1].x.ToString());
        form.AddField("armRY", Arm[1].y.ToString());
        form.AddField("armRZ", Arm[1].z.ToString());




        // Create a download object
        var download = new WWW(url, form);

        // Wait until the download is done
        yield return download;

        if (download.error != null) {
            Info = download.error;
            Debug.Log(download.error);
        }
        else {
            Info = "success " + download.text;
        }
    }





    public void ReadValuesDrives(int driveInd) {
        string fileName = "drivesFunda.txt";
        StreamReader sr = new StreamReader(fileName);


        string[] content = File.ReadAllLines(fileName);

        String[] tokens = content[driveInd + 1].Split('\t');

        int i = 2;
        Speed = float.Parse(tokens[i++]);
        V0 = float.Parse(tokens[i++]);
        V1 = float.Parse(tokens[i++]);
        Ti = float.Parse(tokens[i++]);
        Texp = float.Parse(tokens[i++]);
        Tval = float.Parse(tokens[i++]);
        T0 = float.Parse(tokens[i++]);
        T1 = float.Parse(tokens[i++]);        
        HrMag = float.Parse(tokens[i++]);
        HSign = HrMag >= 0 ? 1 : -1;
        HfMag = float.Parse(tokens[i++]);
        SquashMag = float.Parse(tokens[i++]);
        WbMag = float.Parse(tokens[i++]);
        WxMag = float.Parse(tokens[i++]);
        WtMag = float.Parse(tokens[i++]);
        WfMag = float.Parse(tokens[i++]);
        EtMag = float.Parse(tokens[i++]);
        EfMag = float.Parse(tokens[i++]);
        DMag = float.Parse(tokens[i++]);
        TrMag = float.Parse(tokens[i++]);
        TfMag = float.Parse(tokens[i++]);
        EncSpr0 = float.Parse(tokens[i++]);
        SinRis0 = float.Parse(tokens[i++]);
        RetAdv0 = float.Parse(tokens[i++]);
        EncSpr1 = float.Parse(tokens[i++]);
        SinRis1 = float.Parse(tokens[i++]);
        RetAdv1 = float.Parse(tokens[i++]);
        Continuity = float.Parse(tokens[i++]);
        Arm[0].x = float.Parse(tokens[i++]);
        Arm[0].y = float.Parse(tokens[i++]);
        Arm[0].z = float.Parse(tokens[i++]);
        Arm[1].x = float.Parse(tokens[i++]);
        Arm[1].y = float.Parse(tokens[i++]);
        Arm[1].z = float.Parse(tokens[i++]);        
        ExtraGoal = int.Parse(tokens[i++]);
        UseCurveKeys = int.Parse(tokens[i++]);
        SquashF = float.Parse(tokens[i++]);
        FixedTarget = int.Parse(tokens[i++]);
        EncSpr2 = float.Parse(tokens[i++]);
        SinRis2 = float.Parse(tokens[i++]);
        RetAdv2 = float.Parse(tokens[i++]);
        ShapeTi = float.Parse(tokens[i++]);        
        GoalThreshold = float.Parse(tokens[i++]);

        DrivesAchieved = true;
        sr.Close();

    }

    public void RecordValuesDrives(int driveInd) {

        string fileName = "drivesFunda.txt";
        if (!File.Exists(fileName)) {
            StreamWriter sw = new StreamWriter(fileName);

            sw.WriteLine("UserId\tDriveInd\tSpeed\tv0\tv1\tti\ttexp\ttval\tt0\tt1thr\thf\textraGoal\tsquash\tsquashF\twb\twx\twt\twf\tet\tef\td\ttr\ttf\tshapeT0\tshapeT1\tencSpr0\tsinRis0\tretAdv0\tencSpr1\tsinRis1\tretAdv1\tcontinuity\tarmLX\tarmLY\tarmLZ\tarmRX\tarmRY\tarmRZ\tGoalThreshold");

            for (int j = 0; j < 32; j++) { // 32 drives
                sw.WriteLine("funda\t" + j + "\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000\t0.0000");
            }

            sw.Close();
        }

        string[] content = File.ReadAllLines(fileName);

        //float hrSigned = HSign * HrMag;

        content[driveInd + 1] = string.Format("funda\t " + driveInd + "\t{0:0.0000}\t{1:0.0000}\t{2:0.0000}\t{3:0.0000}\t{4:0.0000}\t{5:0.0000}\t{6:0.0000}\t{7:0.0000}\t{8:0.0000}\t{9:0.0000}\t{10:00000}\t{11:0.0000}\t{12:0.0000}\t{13:0.0000}\t{14:0.0000}\t{15:0.0000}\t{16:0.0000}\t{17:0.0000}\t{18:0.0000}\t{19:0.0000}\t{20:0.0000}\t{21:0.0000}\t{22:0.0000}\t{23:0.0000}\t{24:0.0000}\t{25:0.0000}\t{26:0.0000}\t{27:0.0000}\t{28:0.0000}\t{29:0.0000}\t{30:0.0000}\t{31:0.0000}\t{32:0.0000}\t{33:0}\t{34:0}\t{35:0.0000}\t{36:0}\t{37:0.0000}\t{38:0.0000}\t{39:0.0000}\t{40:0.0000}\t{41:0.0000}",
                                Speed, V0, V1, Ti, Texp, Tval, T0, T1, HrMag, HfMag, SquashMag, WbMag, WxMag, WtMag, WfMag, EtMag, EfMag, DMag, TrMag, TfMag, EncSpr0, SinRis0, RetAdv0, EncSpr1, SinRis1, RetAdv1, Continuity, Arm[0].x, Arm[0].y, Arm[0].z, Arm[1].x, Arm[1].y, Arm[1].z, ExtraGoal, UseCurveKeys, SquashF, FixedTarget, EncSpr2, SinRis2, RetAdv2, ShapeTi, GoalThreshold);


        using (StreamWriter sw = new StreamWriter(fileName)) {
            for (int i = 0; i < content.Length; i++)
                sw.WriteLine(content[i]);
        }


    }



    
}
