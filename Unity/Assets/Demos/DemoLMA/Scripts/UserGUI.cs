#define DEBUGMODE

using System;
using UnityEngine;
using System.Collections;


enum ExistCheck {
    Unchecked,
    Unique,
    TriedOnce
}

public class UserGUI : MonoBehaviour {

    static string _ageStr = "0";
    static string _errorStr = "";
    static string _nullIdStr = "";

    static bool _showLabel = false;
    public string[] GenderStr = new string[] { "Male", "Female" };
    public string[] YesNoStr = new string[] { "Yes", "No" };
    public int GenderInd = 0;
    public int NativeInd = 0;
    private string _info = "waiting...";

    int _exists = (int)ExistCheck.Unchecked;
    bool _posted = false;
    static bool _userInfoTaken = false;
    static bool _idCheckDone = false;

    bool _askDemographics = true;

    void Start() {

        _askDemographics = true;

        String evalStr = "u.getUnity().SendMessage (" + "\"" + gameObject.name + "\",\"GetUserInfo\", userId+\"\\t\"+hit);";
        if (Application.isWebPlayer) {
            Application.ExternalEval(evalStr);
        }


    }


    void OnGUI() {


        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.normal.textColor = Color.white;

        if (_userInfoTaken && !_idCheckDone) {
            this.StartCoroutine(IdExists());

        }


        if (_askDemographics) {

            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100f, 150, 25), "Gender:  ", style);
            GUI.color = Color.white;
            GenderInd = GUI.SelectionGrid(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100f, 200, 25), GenderInd,
                                          GenderStr, 2);

            UserInfo.IsMale = GenderInd == 0;



            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 50f, 150, 25), "Age:  ", style);
            _ageStr = GUI.TextField(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50f, 100, 25), _ageStr, 2);




            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2, 300, 25), "Are you a native English speaker?  ",
                      style);
            GUI.color = Color.white;
            NativeInd = GUI.SelectionGrid(new Rect(Screen.width / 2 + 100, Screen.height / 2, 200, 25), NativeInd, YesNoStr,
                                          2);

            UserInfo.IsNative = (NativeInd == 0);


            int.TryParse(_ageStr, out UserInfo.Age);
        }

        style.normal.textColor = Color.red;
        style.fontSize = 22;
        //GUILayout.Label("UserId: " + UserInfo.UserId + " Hit number:" + UserInfo.Hit, style);
        GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 100f, 400, 25), _nullIdStr, style);

        style.normal.textColor = Color.white;
        style.fontSize = 18;

        //now compute hit information
        UserInfo.ComputeQuestionInfo();


        if (_posted) //to cause delay
            Application.LoadLevel("DemoPersonalityComparison2");

        if (_exists == (int)ExistCheck.Unique) { //to cause delay in coroutine output                      
            this.StartCoroutine(PostUserInfo()); //post only if id does not exist                                                               
        }

        if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 150f, 150, 30), "Start")) {
            if (UserInfo.UserId.Equals(""))
                _nullIdStr = "You need to accept the HIT first.";

            else {
                _nullIdStr = "";

                this.StartCoroutine(IdForHitExists());
            }
        }

        style.fontSize = 20;

        style.normal.textColor = Color.red;
        GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 + 200f, 250, 25), _errorStr, style);
        style.normal.textColor = Color.white;



    }

    IEnumerator IdExists() {
        string resultURL = "https://fling.seas.upenn.edu/~fundad/cgi-bin/mturk4/getIdInfo.php";

        var form = new WWWForm();
        // Assuming the perl script manages high scores for different games
        form.AddField("userId", UserInfo.UserId);

        // Create a download object
        var download = new WWW(resultURL, form);

        // Wait until the download is done
        yield return download;
        _errorStr = download.text;

        if (download.error != null) {
            print("Error: " + download.error);
        }
        else {
            _errorStr = "";


            _idCheckDone = true;

            if (download.text.Equals("false")) {
                _askDemographics = true;
            }
            else {
                _askDemographics = false;
                var info = download.text;
                String[] vals = info.Split('\t');
                //Assign drive values 
                //should be exactly in this order
                int i = 0;
                UserInfo.Age = int.Parse(vals[i++]);
                string gender = vals[i++];
                string nativity = vals[i];

                UserInfo.IsMale = gender.Equals("male");
                UserInfo.IsNative = nativity.Equals("native");


            }
        }

    }

    IEnumerator IdForHitExists() {
        string resultURL = "https://fling.seas.upenn.edu/~fundad/cgi-bin/mturk4/checkId.php";

        var form = new WWWForm();
        // Assuming the perl script manages high scores for different games
        form.AddField("userId", UserInfo.UserId);
        form.AddField("hit", UserInfo.Hit);

        // Create a download object
        var download = new WWW(resultURL, form);

        // Wait until the download is done
        yield return download;
        _errorStr = download.text;


        if (download.error != null) {
            print("Error: " + download.error);
        }
        else {
#if DEBUGMODE
            _exists = (int)ExistCheck.Unique;
            _errorStr = "";

#else

            if (download.text.Equals("true")) {
                _exists = (int)ExistCheck.TriedOnce;
                _errorStr = "You have already completed this HIT.";
            }
            else {
                _exists = (int)ExistCheck.Unique;
                _errorStr = "";
            }
#endif
        }
    }

    IEnumerator PostUserInfo() {
        string resultURL = "https://fling.seas.upenn.edu/~fundad/cgi-bin/mturk4/putUserInfo.php";

        // Create a form object for sending high score data to the server
        var form = new WWWForm();
        form.AddField("userId", UserInfo.UserId);
        form.AddField("hit", UserInfo.Hit.ToString());
        form.AddField("age", UserInfo.Age.ToString());
        form.AddField("gender", UserInfo.IsMale ? "male" : "female");
        form.AddField("nativity", UserInfo.IsNative ? "native" : "nonnative");

        // Create a download object
        var download = new WWW(resultURL, form);

        // Wait until the download is done
        yield return download;
        _posted = true;


    }



    public void GetUserInfo(string paramStr) {

        String[] tokens = paramStr.Split('\t');

        UserInfo.UserId = tokens[0];
        UserInfo.Hit = int.Parse(tokens[1]); //assuming hit starts from 1


        _userInfoTaken = true;
    }




}
