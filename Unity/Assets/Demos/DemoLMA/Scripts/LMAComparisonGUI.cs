//#define WEBMODE

//#define DEBUGMODE


using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;



public class LMAComparisonGUI : MonoBehaviour {
    
    enum OCEAN {
        O,
        C,
        E,
        A,
        N
    }

    public Texture TexBorder = new Texture();
    public GUISkin ButtonSkin;

    
    private static int _animInd = 0;

    int _driveIndLeft = 0, _driveIndRight = 0;
    
    static int _taskQInd = 0; //question index for the task [0-8]
    private int _actualQInd {
        get {
#if WEBMODE            
         return UserInfo.GroupInd * UserInfo.QCnt + _taskQInd % UserInfo.QCnt;
#elif !WEBMODE
             return  _taskQInd;
#endif
        }
    } //question index for all the tasks [0-48]


    private int[,] _effortList =  {{-1, -1, -1, 0}, {-1, -1, 1, 0}, {-1, 1, -1, 0}, {-1, 1, 1, 0}, {1, -1, -1, 0}, {1, -1, 1, 0}, {1, 1, -1, 0}, {1, 1, 1, 0}, 
                                       {-1, -1, 0, -1}, {-1, -1, 0, 1},{-1, 1,0,  -1}, {-1, 1, 0, 1}, {1, -1, 0, -1},{1, -1, 0,  1}, {1, 1, 0, -1}, {1, 1, 0, 1},
                                        {-1,  0, -1, -1}, {-1, 0, -1,  1}, {-1, 0, 1,  -1}, {-1, 0, 1, 1}, {1, 0, -1, -1}, {1, 0, -1,  1}, {1, 0, 1, -1}, {1,  0, 1, 1},
                                       { 0, -1, -1, -1}, {0, -1, -1,  1}, {0, -1, 1,  -1}, {0, -1, 1, 1}, {0, 1, -1, -1}, {0, 1, -1,  1}, {0, 1, 1, -1}, {0, 1, 1, 1}};


    Dictionary<int, int> _effortCombination = new Dictionary<int , int>();

    private string[] _effortNames = { "Space", "Weight", "Time", "Flow" };

    static int _selectPersonality = -1; //to ensure that none is selected by default
    static int _answerPersonality = -1; //to ensure that none is selected by default
    private bool[] _arePositionsSwapped;
    
    public string ShapeInfo = "";
    public string Info = "waiting...";
    private static Vector2 _scrollPosition;

    private static string _questionNoStr = "";

    private static string[] _isSubmittedStr;
    private static bool[] _isSubmitted;
    private static bool[] _alreadyPlayed;


    private int _submittedCnt = 0;

    private float _scrollWidth = 0;

    private int _personality = 0;

    GameObject _agentLeft, _agentRight;

    private int _qCnt;
    private  bool _firstQFirstPlay; //special case for the first question



    private static DriveParams[] _driveParams = new DriveParams[32];

    private bool _drivesAchieved = false;

    

    private int _goldQ1Ind, _goldQ2Ind; //two golden standard questions 
    private bool _goldQ1Asked, _goldQ2Asked; //have we asked gold questions
    private int _quality = 0; 
    private bool _qualityPosted = false;
    private bool _qualitySentToBrowser = false;
    private bool _drawWhiteScreen = false;

    void Start(){

        for (int i = 0; i < 32; i++) {
            _driveParams[i] = new DriveParams();
            
        }
            
        _agentLeft = GameObject.Find("AgentPrefabLeft");        
        _agentRight = GameObject.Find("AgentPrefabRight");
        
        
        if (!_agentLeft) {
            
            Debug.Log("AgentLeft prefab not found");
            return;
        }
        if (!_agentRight) {
            Debug.Log("AgentRight prefab not found");
            return;
        }

        _personality = UserInfo.Personality;

        
#if WEBMODE
        _animInd = UserInfo.AnimInd;
        _qCnt = UserInfo.QCnt; 
        
#elif !WEBMODE
        _animInd = 1;
        _qCnt = 48;
#endif
        _taskQInd = 0; //initial question's index
        _arePositionsSwapped = new bool[_qCnt * 2];
        _isSubmittedStr = new string[_qCnt + 2];
        _isSubmitted = new bool[_qCnt + 2];
        _alreadyPlayed = new bool[_qCnt];

        
        //compute effortCombination hashes
        for (int i = 0; i < 32; i++) {
            int val = _effortList[i, 3] + _effortList[i, 2] * 3 + _effortList[i, 1] * 9 + _effortList[i, 0] * 27;
            _effortCombination.Add(val, i);
        }

        for (int i = 0; i < _isSubmittedStr.Length; i++)
            _isSubmittedStr[i] = "";//"Answer NOT submitted";

        for (int i = 0; i < _alreadyPlayed.Length; i++)
            _alreadyPlayed[i] = false;

        UpdateCameraBoundaries();


    //    _drivesAchieved = false;
        //Read all drive and shape parameters

#if !WEBMODE
        for (int i = 0; i < 32; i++)
						_driveParams[i].ReadValuesDrives(i);

        
#elif WEBMODE

        for (int i = 0; i < 32; i++) 
            this.StartCoroutine(_driveParams[i].GetValuesDrives(i, "funda"));


        //wait till drives are achieved
        
        /*
            _drivesAchieved = true;
            for (int i = 0; i < 32; i++)  //make sure all the drives are achieved
                _drivesAchieved = _drivesAchieved && _driveParams[i].DrivesAchieved;

        */


        
#endif
       

        
        //Select if positions are swapped in the beginning
        for (int i = 0; i < _arePositionsSwapped.Length/2; i++) {
            if (MathDefs.GetRandomNumber(2) == 1) {
                //50% chance
                _arePositionsSwapped[i] = true;
            }
            else {
                _arePositionsSwapped[i] = false;
            }
        }


        Reset();

        _firstQFirstPlay = true;

        

        _goldQ1Ind = MathDefs.GetRandomNumber(2,4);
        _goldQ2Ind = MathDefs.GetRandomNumber(4,6); 

        _goldQ1Asked = _goldQ2Asked = false;

        _quality = 0;



    }

 

    public void Reset() {
        
        InitAgent(_agentRight, "Pointing");
        InitAgent(_agentLeft, "Pointing");

        
        UpdateParameters();
        
        PlayAnim(_agentRight, _animInd);
        StopAnim(_agentRight);

        PlayAnim(_agentLeft, _animInd);
        StopAnim(_agentLeft);
       
        
    }

    void UpdateCameraBoundaries() {
        GameObject cam1 = GameObject.Find("Camera1");
        GameObject cam2 = GameObject.Find("Camera2");
        GameObject cam3 = GameObject.Find("Camera3");

        cam1.GetComponent<Camera>().rect = new Rect(0, 0, _scrollWidth / Screen.width, 1); //320 is the width of the parameters

        if (_arePositionsSwapped[_taskQInd]) {
            cam3.GetComponent<Camera>().rect = new Rect(_scrollWidth / Screen.width, 0, ((Screen.width - _scrollWidth) / 2f) / Screen.width, 1);
            cam2.GetComponent<Camera>().rect = new Rect((Screen.width - (Screen.width - _scrollWidth) / 2f) / Screen.width, 0, ((Screen.width - _scrollWidth) / 2f) / Screen.width, 1);
        }
        else {
            cam2.GetComponent<Camera>().rect = new Rect(_scrollWidth / Screen.width, 0, ((Screen.width - _scrollWidth) / 2f) / Screen.width, 1);
            cam3.GetComponent<Camera>().rect = new Rect((Screen.width - (Screen.width - _scrollWidth) / 2f) / Screen.width, 0, ((Screen.width - _scrollWidth) / 2f) / Screen.width, 1);
        }

    }

   

    void Update(){

        //check if the last drive has been achieved
        if (_driveParams[31].DrivesAchieved  && _firstQFirstPlay ) { //special case for the first playing of the first question       
         
            _agentLeft.GetComponent<TorsoController>().Reset();
            _agentRight.GetComponent<TorsoController>().Reset();


            
            PlayAnim(_agentRight, _animInd);
            PlayAnim(_agentLeft, _animInd);
            UpdateParameters();  //we need to update after play because playanim resets torso parameters for speed etc. when animinfo is reset       

            _firstQFirstPlay = false;
        }

        UpdateCameraBoundaries();
        

#if DEBUGMODE || !WEBMODE
        if (Input.GetKeyDown("left")) 
            GetPrevQuestion();
        
        else if (Input.GetKeyDown("right")) 
            GetNextQuestion();
        
#endif        
    }
    


    void GetNextQuestion() {
        if (_taskQInd < _qCnt - 1) {            
            _taskQInd++;

#if WEBMODE
            if (!_goldQ1Asked && _taskQInd == _goldQ1Ind + 1 ){
                _taskQInd--;
                _goldQ1Asked = true;
            }
            else if(!_goldQ2Asked && _taskQInd == _goldQ2Ind + 1) {
                _taskQInd--;
                _goldQ2Asked = true;
            }

#endif

            StopAnim(_agentLeft);
            StopAnim(_agentRight);
            _agentLeft.GetComponent<TorsoController>().Reset();
            _agentRight.GetComponent<TorsoController>().Reset();

         

            
#if WEBMODE
            if (_taskQInd == _goldQ1Ind && !_goldQ1Asked) {
                PlayAnim(_agentRight, 0);
                PlayAnim(_agentLeft, 1);
            }
            else if (_taskQInd == _goldQ2Ind && !_goldQ2Asked) {
                PlayAnim(_agentRight, 0);
                PlayAnim(_agentLeft, 1);
            }
            else 
#endif
            {
                PlayAnim(_agentRight, _animInd);
                PlayAnim(_agentLeft, _animInd);
            }



            UpdateParameters();
        }

    }
    void GetPrevQuestion() {

        if (_taskQInd > 0) {
            _taskQInd--;

            if (!_goldQ1Asked && _taskQInd == _goldQ1Ind - 1) {
                _taskQInd++;
                _goldQ1Asked = true;
            }
            else if (!_goldQ2Asked && _taskQInd == _goldQ2Ind - 1) {
                _taskQInd++;
                _goldQ2Asked = true;
            }


            StopAnim(_agentLeft);
            StopAnim(_agentRight);

            _agentLeft.GetComponent<TorsoController>().Reset();
            _agentRight.GetComponent<TorsoController>().Reset();



            PlayAnim(_agentRight, _animInd);
            PlayAnim(_agentLeft, _animInd);
            UpdateParameters();
        }
    }

    void StopAnimations() {

       
         _agentLeft.GetComponent<TorsoController>().Reset();
         _agentRight.GetComponent<TorsoController>().Reset();
    
        StopAnim(_agentLeft);
        PlayAnim(_agentLeft, _animInd); //start the next animation
        StopAnim(_agentLeft);

        StopAnim(_agentRight);
        PlayAnim(_agentRight, _animInd); //start the next animation
        StopAnim(_agentRight);


        //changes the names of the drives
       UpdateParameters();

    }

    
   
    
    //Left and right drive indices
    //map ind of 48 to left and right drive indices
    //qInd = actualQInd bw [0 48]
    void ComputeBothDriveInds(int qInd) {
        int space, weight, time, flow;
        int key = -1; //key for effort combination dictionary
        //Find which effort is compared
        int cVal = qInd % 4;//qInd / 12;
        int qVal = qInd / 4; //qInd % 12; // between 0 and 11 
        int[,] othersList = {{-1, -1, 0},  {-1, 1, 0},  {1, -1, 0},  {1, 1, 0},  {-1, 0, -1}, {-1, 0, 1}, {1, 0, -1}, {1, 0, 1}, {0, -1, -1}, {0, -1, 1}, {0, 1, -1}, {0, 1, 1}};


        if (cVal == 0) { //space

            weight = othersList[qVal,0];
            time = othersList[qVal,1];
            flow = othersList[qVal,2];

            //Left
            space = -1;
            key = space * 27 + weight * 9 + time * 3 + flow;            
            _driveIndLeft = _effortCombination[key];
            //right
            space = 1;
            key = space * 27 + weight * 9 + time * 3 + flow;
            _driveIndRight = _effortCombination[key];
        }

        else if (cVal == 1) { //weight

            space = othersList[qVal,0];
            time = othersList[qVal,1];
            flow = othersList[qVal,2];

            //Left
            weight = -1;
            key = space * 27 + weight * 9 + time * 3 + flow;
            _driveIndLeft = _effortCombination[key];


            //right
            weight = 1;
            key = space * 27 + weight * 9 + time * 3 + flow;
            _driveIndRight = _effortCombination[key];
        }

        else if (cVal == 2) { //time

            space = othersList[qVal,0];
            weight = othersList[qVal,1];
            flow = othersList[qVal,2];

            //Left
            time = -1;
            key = space * 27 + weight * 9 + time * 3 + flow;
            _driveIndLeft = _effortCombination[key];
     
            //right
            time = 1;
            key = space * 27 + weight * 9 + time * 3 + flow;
            _driveIndRight = _effortCombination[key];
        }

        else if (cVal == 3) { //flow

            space = othersList[qVal,0];
            weight = othersList[qVal,1];
            time = othersList[qVal,2];

            //Left
            flow = -1;
            key = space * 27 + weight * 9 + time * 3 + flow;
            _driveIndLeft = _effortCombination[key];


            //right
            flow = 1;
            key = space * 27 + weight * 9 + time * 3 + flow;
            _driveIndRight = _effortCombination[key];
        }

        
    }
    

    void UpdateParameters() {

        
        //Find driveIndLeft and driveIndRight
        ComputeBothDriveInds(_actualQInd);
        //Update right
        UpdateEmoteParams(_agentRight, _driveIndRight);

        //Update left
        UpdateEmoteParams(_agentLeft, _driveIndLeft);

    }
    void UpdateEmoteParams(GameObject agent, int driveInd) {
        if (agent == null) {
            Debug.Log("AgentPrefab not found");
            return;
        }
        
          
        

        agent.GetComponent<AnimationInfo>().AnimSpeed = _driveParams[driveInd].Speed;
        agent.GetComponent<AnimationInfo>().V0 = _driveParams[driveInd].V0;
        agent.GetComponent<AnimationInfo>().V1 = _driveParams[driveInd].V1;

        agent.GetComponent<AnimationInfo>().T0 = _driveParams[driveInd].T0;
        agent.GetComponent<AnimationInfo>().T1 = _driveParams[driveInd].T1;
        agent.GetComponent<AnimationInfo>().Ti = _driveParams[driveInd].Ti;


        agent.GetComponent<AnimationInfo>().Texp = _driveParams[driveInd].Texp;

        float prevTVal = agent.GetComponent<AnimationInfo>().Tval;
        float prevContinuity = agent.GetComponent<AnimationInfo>().Continuity;
        agent.GetComponent<AnimationInfo>().Tval = _driveParams[driveInd].Tval;
        agent.GetComponent<AnimationInfo>().Continuity = _driveParams[driveInd].Continuity;

        if (_driveParams[driveInd].Tval != prevTVal || _driveParams[driveInd].Continuity != prevContinuity)
            agent.GetComponent<AnimationInfo>().InitInterpolators(_driveParams[driveInd].Tval, _driveParams[driveInd].Continuity, 0);



        agent.GetComponent<FlourishAnimator>().TrMag = _driveParams[driveInd].TrMag;
        agent.GetComponent<FlourishAnimator>().TfMag = _driveParams[driveInd].TfMag ;

        agent.GetComponent<IKAnimator>().HrMag = _driveParams[driveInd].HrMag;
        agent.GetComponent<IKAnimator>().HfMag = _driveParams[driveInd].HfMag;
        agent.GetComponent<AnimationInfo>().ExtraGoal = _driveParams[driveInd].ExtraGoal;
        

        agent.GetComponent<IKAnimator>().SquashMag = _driveParams[driveInd].SquashMag; //breathing affects keypoints
        agent.GetComponent<IKAnimator>().SquashF = _driveParams[driveInd].SquashF; //breathing affects keypoints

        agent.GetComponent<FlourishAnimator>().WbMag = _driveParams[driveInd].WbMag;
        agent.GetComponent<FlourishAnimator>().WxMag = _driveParams[driveInd].WxMag;
        agent.GetComponent<FlourishAnimator>().WfMag = _driveParams[driveInd].WfMag;
        agent.GetComponent<FlourishAnimator>().WtMag = _driveParams[driveInd].WtMag;
        agent.GetComponent<FlourishAnimator>().EfMag = _driveParams[driveInd].EfMag;
        agent.GetComponent<FlourishAnimator>().EtMag = _driveParams[driveInd].EtMag;
        agent.GetComponent<FlourishAnimator>().DMag = _driveParams[driveInd].DMag;


        agent.GetComponent<IKAnimator>().ShapeTi = _driveParams[driveInd].ShapeTi;

        agent.GetComponent<IKAnimator>().EncSpr[0] = _driveParams[driveInd].EncSpr0;
        agent.GetComponent<IKAnimator>().SinRis[0] = _driveParams[driveInd].SinRis0;
        agent.GetComponent<IKAnimator>().RetAdv[0] = _driveParams[driveInd].RetAdv0;

        agent.GetComponent<IKAnimator>().EncSpr[1] = _driveParams[driveInd].EncSpr1;
        agent.GetComponent<IKAnimator>().SinRis[1] = _driveParams[driveInd].SinRis1;
        agent.GetComponent<IKAnimator>().RetAdv[1] = _driveParams[driveInd].RetAdv1;

        agent.GetComponent<IKAnimator>().EncSpr[2] = _driveParams[driveInd].EncSpr2;
        agent.GetComponent<IKAnimator>().SinRis[2] = _driveParams[driveInd].SinRis2;
        agent.GetComponent<IKAnimator>().RetAdv[2] = _driveParams[driveInd].RetAdv2;


        agent.GetComponent<AnimationInfo>().UseCurveKeys = _driveParams[driveInd].UseCurveKeys;

        agent.GetComponent<AnimationInfo>().Hor = _driveParams[driveInd].Arm[0].x;
        agent.GetComponent<AnimationInfo>().Ver = _driveParams[driveInd].Arm[0].y;
        agent.GetComponent<AnimationInfo>().Sag = _driveParams[driveInd].Arm[0].z;
        agent.GetComponent<AnimationInfo>().UpdateKeypointsByShape(0); //Update keypoints

        //RightArm 
        //Only horizontal motion is the opposite for each arm
        agent.GetComponent<AnimationInfo>().Hor = -_driveParams[driveInd].Arm[1].x;
        agent.GetComponent<AnimationInfo>().Ver = _driveParams[driveInd].Arm[1].y;
        agent.GetComponent<AnimationInfo>().Sag = _driveParams[driveInd].Arm[1].z;
        agent.GetComponent<AnimationInfo>().UpdateKeypointsByShape(1); //Update keypoints


    }

    


    IEnumerator DrawWhiteScreen() {
        _drawWhiteScreen = true;
        yield return new WaitForSeconds(0.1f);
        _drawWhiteScreen = false;
    }
	void OnGUI () {

	    GUIStyle style = new GUIStyle();

	    //Border between the two images
	    GUI.DrawTexture(new Rect(Screen.width/2f - 1f, 0, 2, Screen.height), TexBorder, ScaleMode.ScaleToFit, true,
	                    2f/Screen.height);

	    if (_drawWhiteScreen) {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), TexBorder, ScaleMode.ScaleToFit, true,
                            (float)Screen.width / Screen.height);
            return;
        }
	

    GUI.skin = ButtonSkin;
	    

	    string[] selectStr = {"Left", "Equal", "Right"};

        style.fontSize = 20;

        style.normal.textColor = Color.white;
#if DEBUGMODE
        
        GUILayout.BeginArea(new Rect(20, 10, 2500, 250));
        GUILayout.Label("Debug Mode" , style);
        style.fontSize = 12;
        if (_arePositionsSwapped[_taskQInd]) {
            GUILayout.Label("Right: " + ComputeEffortCombinationStr(_driveIndLeft), style);
            GUILayout.Label("Left: " + ComputeEffortCombinationStr(_driveIndRight), style);
        }
        else {
            GUILayout.Label("Left: " + ComputeEffortCombinationStr(_driveIndLeft), style);
            GUILayout.Label("Right: " + ComputeEffortCombinationStr(_driveIndRight), style);
        }

        GUILayout.Label("personality: "+ _personality + " anim: "+ UserInfo.AnimInd + " group: " + UserInfo.GroupInd + " hit: " + UserInfo.Hit + " taskQ: " + _taskQInd + " actQ: " + _actualQInd + " quality: " + _quality, style);
        GUILayout.EndArea();
#endif


        if (_submittedCnt >= _qCnt) { //STUDY IS OVER            
#if WEBMODE
            if (!_qualityPosted)
                this.StartCoroutine(PostQualityCheck());
            
            if (!_qualitySentToBrowser) {//quality not sent to browser yet
                Application.ExternalCall("sendQuality", _quality.ToString()); //send quality to browser
                _qualitySentToBrowser = true;
            }   
#endif

            style.fontSize = 16;
            if (_quality >= 2)
                _isSubmittedStr[_taskQInd] = "Study is complete. Thank you!";
            else {
                _isSubmittedStr[_taskQInd] = "Sorry, you haven't passed the quality check.\n Please pay more attention next time.";
            }
        }

        else { //STUDY CONTINUING

            if (_agentLeft.GetComponent<AnimationInfo>().AnimationOver() && _agentRight.GetComponent<AnimationInfo>().AnimationOver()) { //Animation played at least once
                if ((_taskQInd != _goldQ1Ind || _goldQ1Asked) && (_taskQInd != _goldQ2Ind || _goldQ2Asked)) //Animation is not a golden question
                    _alreadyPlayed[_taskQInd] = true;
            }

            if (GUI.Button(new Rect(Screen.width - 120, Screen.height - 60f, 100, 25), "Replay")) {
                _agentLeft.GetComponent<TorsoController>().Reset();
                _agentRight.GetComponent<TorsoController>().Reset();


                if (_taskQInd == _goldQ1Ind && !_goldQ1Asked) {
                    PlayAnim(_agentRight, 0);
                    PlayAnim(_agentLeft, 1);
                }
                else if (_taskQInd == _goldQ2Ind && !_goldQ2Asked) {
                    PlayAnim(_agentRight, 0);
                    PlayAnim(_agentLeft, 1);
                }
                else {
                    PlayAnim(_agentRight, _animInd);
                    PlayAnim(_agentLeft, _animInd);
                }
                UpdateParameters();
                    //we need to update after play because playanim resets torso parameters for speed etc. when animinfo is reset
            }

            style.fontSize = 19;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.black;
            GUI.Label(new Rect(5, Screen.height - 155f, 1000, 290), _questionNoStr, style);


            style.fontSize = 16;
            style.fontStyle = FontStyle.Normal;
            if (_taskQInd == _goldQ1Ind && !_goldQ1Asked) {
                GUI.Label(new Rect(5, Screen.height - 130f, 1000, 290),
                          "One character is pointing and the other is picking up something from the ground.", style);
                style.fontStyle = FontStyle.Bold;
                GUI.Label(new Rect(575, Screen.height - 130f, 1000, 290), "Which one is POINTING?", style);
                style.fontStyle = FontStyle.Normal;
            }
            else if (_taskQInd == _goldQ2Ind && !_goldQ2Asked) {
                GUI.Label(new Rect(5, Screen.height - 130f, 1000, 290),
                          "One character is pointing and the other is picking up something from the ground.", style);
                style.fontStyle = FontStyle.Bold;
                GUI.Label(new Rect(575, Screen.height - 130f, 1000, 290), "Which one is PICKING UP?", style);
                style.fontStyle = FontStyle.Normal;
            }
            else {
                GUILayout.BeginHorizontal();
                GUI.Label(new Rect(5, Screen.height - 130f, 1000, 290), "Which character looks", style);
                style.fontStyle = FontStyle.Bold;

                GUI.Label(new Rect(170, Screen.height - 130f, 1000, 290), UserInfo.PersonalityQuestion, style);
                //GUI.Label( new Rect(165, Screen.height - 130f, 1000, 290), "MORE \"open to new experiences & complex \", and LESS \"conventional & uncreative\"", style);            
                GUILayout.EndHorizontal();
                style.fontStyle = FontStyle.Normal;
            }


            GUILayout.BeginArea(new Rect(Screen.width/2f - 150, Screen.height - 100f, 1000, 290));
            _selectPersonality = GUILayout.SelectionGrid(_selectPersonality, selectStr, 3, GUILayout.Width(300));
            GUILayout.EndArea();




            GUI.color = Color.white;

#if !DEBUGMODE
            if (_agentLeft.GetComponent<AnimationInfo>().AnimationOver() && _agentRight.GetComponent<AnimationInfo>().AnimationOver() || _alreadyPlayed[_taskQInd])
#endif
            {

                if (GUI.Button(new Rect(Screen.width/2f - 110, Screen.height - 65f, 200, 25), "Submit Answer")) {


                    if (_selectPersonality == -1) { 
                        _isSubmittedStr[_taskQInd] = "  Please select an answer first.";
                    }

                    else { //ANSWER SELECTED AND SUBMIT PRESSED
                        if (_isSubmitted[_taskQInd] == false) { //NOT ALREADY SUBMITTED
                            _isSubmitted[_taskQInd] = true;
                            _submittedCnt++;
                        }

                        //change signs
                        if (_arePositionsSwapped[_taskQInd]) { //Agent positions are swapped, left is on the right, and right is on the left                            
                            if (_selectPersonality == 0) //left 
                                _answerPersonality = 1; //right
                            else if (_selectPersonality == 2) //right
                                _answerPersonality = -1;
                            else
                                _answerPersonality = 0; //equal
                        }
                        else { //positions not swapped                            
                            if (_selectPersonality == 0) //left 
                                _answerPersonality = -1; //left
                            else if (_selectPersonality == 2) //right
                                _answerPersonality = 1; //right
                            else
                                _answerPersonality = 0; //equal
                        }


                        //don't post golden questions but check their quality
                        if (_taskQInd == _goldQ1Ind && !_goldQ1Asked) { //Turn of golden question 1 
                            if (_answerPersonality == 1)
                                _quality++;
                        }

                        else if (_taskQInd == _goldQ2Ind && !_goldQ2Asked) { //turn of golden question 2 
                            if (_answerPersonality == -1)
                                _quality++;
                        }
                        else { //regular question asked
                            this.StartCoroutine(PostValues(_taskQInd+1, _answerPersonality,_arePositionsSwapped[_taskQInd]));                        
                        }
                        //Move on to the next question
                        GetNextQuestion();                        
                        StartCoroutine(DrawWhiteScreen());
                        _answerPersonality = -1;
                        _selectPersonality = -1;
                    }

                }

            }
        }

	    if (_submittedCnt >= _qCnt) {
            style.fontSize = 20;
            
            GUI.Label(new Rect(50, Screen.height - 100f, 800, 100), _isSubmittedStr[_taskQInd], style);
            //if (_quality >= 2)
             //   GUI.TextField(new Rect(180, Screen.height - 75f, 80, 20), UserInfo.Code,200);
              
        }
        else
            GUI.Label(new Rect(Screen.width / 2f - 110, Screen.height - 35f, 500, 25), _isSubmittedStr[_taskQInd], style);


        if(_goldQ2Asked)
            _questionNoStr = "Question " + (_taskQInd + 3) + " of " + (_qCnt + 2) + ":";
        else if(_goldQ1Asked)
            _questionNoStr = "Question " + (_taskQInd + 2) + " of " + (_qCnt + 2) + ":";
        else
            _questionNoStr = "Question " + (_taskQInd + 1) + " of " + (_qCnt + 2) + ":";

        

	}


   

    string ComputeEffortCombinationStr(int driveInd) {
        string str = "";
        if (_effortList[driveInd,0] == -1)
            str += "Indirect";
        else if (_effortList[driveInd,0] == 1)
            str += "Direct";

        str += " ";
        if (_effortList[driveInd,1] == -1)
            str += "Light";
        else if (_effortList[driveInd,1] == 1)
            str += "Strong";

        str += " ";
        if (_effortList[driveInd,2] == -1)
            str += "Sustained";
        else if (_effortList[driveInd,2] == 1)
            str += "Sudden";


        str += " ";
        if (_effortList[driveInd,3] == -1)
            str += "Free";
        else if (_effortList[driveInd,3] == 1)
            str += "Bound";

        return str;

    }


    void StopAnim(GameObject agent){
 
        if (agent.GetComponent<Animation>().isPlaying) {
            agent.GetComponent<Animation>().clip.SampleAnimation(agent, 0); //instead of rewind
            agent.GetComponent<Animation>().Stop();           
        }       
    }

    void InitAgent(GameObject agent, string animName) {
        if (!agent) 
            return;
        agent.GetComponent<AnimationInfo>().Reset(animName);                            
        agent.GetComponent<IKAnimator>().Reset();                
        //agent.GetComponent<TorsoAnimator>().Reset();
        //Read values for the shape parameters 
        //Need to call this only once

        agent.GetComponent<Animation>().enabled = true;                        
        agent.GetComponent<Animation>().Play(animName);
        
    }


    void PlayAnim(GameObject agent, int ind) {


        AnimationInfo animInfo = agent.GetComponent<AnimationInfo>();
       // agent.animation.Stop(); //in order to restart animation
        StopAnim(agent); //stop first
        
        switch(ind) {
            case 0:
                InitAgent(agent, "Pointing");                                                
                break;
            case 1:
                InitAgent(agent, "picking");                      
                break;
            
        }

        
        agent.GetComponent<Animation>()[animInfo.AnimName].wrapMode =  WrapMode.ClampForever; //To remain in the final pose of the animation clip                            

    }
     

    IEnumerator PostQualityCheck() {
        string resultURL = "https://fling.seas.upenn.edu/~fundad/cgi-bin/mturk2/putQualityInfo.php";

        // Create a form object for sending high score data to the server
        var form = new WWWForm();
        form.AddField("userId", UserInfo.UserId);
        form.AddField("hit", UserInfo.Hit.ToString());
        form.AddField("quality", _quality.ToString());
        // Create a download object
        var download = new WWW(resultURL, form);

        // Wait until the download is done
        yield return download;

        _qualityPosted = true;



    }
   
    // remember to use StartCoroutine when calling this function!   
    //qInd = actualqInd to store in the db
    IEnumerator PostValues( int qInd, int answerPersonality, bool arePositionsSwapped) {
        string resultURL = "https://fling.seas.upenn.edu/~fundad/cgi-bin/mturk2/putComparisonData.php";
               
     // Create a form object for sending high score data to the server
        var form = new WWWForm();        
        form.AddField( "userId", UserInfo.UserId);    
        
        form.AddField( "hit", UserInfo.Hit.ToString());
        form.AddField("qInd", qInd.ToString());  //starts from 1      
        form.AddField( "answer", answerPersonality.ToString());
        form.AddField("areSwapped", arePositionsSwapped ? "true" : "false");
        
        // Create a download object
        var download = new WWW( resultURL, form );


        
        // Wait until the download is done
        yield return download;

        if(download.error!= null) {
            Info = download.error;
            print( "Error: " + download.error );                         
        } else {
            Info = "success " + download.text;                        
        }
    }

  
   
    }		

