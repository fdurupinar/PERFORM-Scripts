//#define WEBMODE


using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class DrivesGUI : MonoBehaviour {

    private static bool _toggleContinuous = false;
    private static bool _toggleDrawCurves = false;

    private static DriveParams _driveParams = new DriveParams();

   

    //private static bool _useFBIK = true;

    //Timing range
    private float SpeedMin = 0.001f;
    private float SpeedMax = 4.0f;
    private float _vMin = 0f;
    private float _vMax = 1f;
    private float _tMin = 0f;
    private float _tMax = 1f;
    private float TexpMin = 0.0f;
    private float TexpMax = 5.0f;
    private float TValMin = -1f;
    private float TValMax = 1f;
    private float ContinuityMin = -1f;
    private float ContinuityMax = 1f;
    
    //Flourishes range
    private static float _trMin = -1f;
    private static float _trMax = 1f;
    private static float _tfMin = 0f;
    private static float _tfMax = 4f;

    private static float _hrMin = -3f;
    private static float _hrMax = 3f; 
    private static float _hfMin = 0f;
    private static float _hfMax = 10f;//4f;
   private float _squashMin = 0f;
    private float _squashMax = 1f;
    private float SquashFMin = 0f;
    private float SquashFMax = 5f;
    private float _wbMin = -1f;
    private float _wbMax = 1f;
    private float _wxMin = -1.8f;
    private float _wxMax = 1.8f;
    private float _wtMin = 0f;
    private float _wtMax = 1.4f;
    private float _wfMin = 0f;
    private float _wfMax = 4f;
    private float _etMin = 0f;
    private float _etMax = 1.4f;
    private float _efMin = 0f;
    private float _efMax = 4f;
    private float _dMin = 0f;
    private float _dMax = 1.4f;

    //Shape for drives range
    private float _shapeMin = -1f;
    private float _shapeMax = 1f;


    private TorsoController _torso;

    private bool[] _armShapeChanged = new bool [2];
    private bool _isPlaying;

    private static int _driveInd = 0 ;
    private static int _shapeInd = 0; 


    private int[,] _effortList = {{-1, -1, -1, 0}, {-1, -1, 1, 0}, {-1, 1, -1, 0}, {-1, 1, 1, 0}, {1, -1, -1, 0}, {1, -1, 1, 0}, {1, 1, -1, 0}, {1, 1, 1, 0}, 
                                        {-1, -1, 0, -1}, {-1, -1, 0, 1}, {-1, 1,0,  -1}, {-1, 1, 0, 1}, {1, -1, 0, -1}, {1, -1, 0,  1}, {1, 1, 0, -1}, {1, 1, 0,  1},
                                        {-1,  0, -1, -1}, {-1, 0, -1,  1}, {-1, 0, 1,  -1}, {-1, 0, 1, 1}, {1, 0, -1, -1}, {1, 0, -1,  1}, {1, 0, 1, -1}, {1,  0, 1, 1},
                                        { 0, -1, -1, -1}, {0, -1, -1,  1}, {0, -1, 1,  -1}, {0, -1, 1, 1}, {0, 1, -1, -1}, {0, 1, -1,  1}, {0, 1, 1, -1}, {0, 1, 1, 1}};

    private string[] _shapeStr = { "Enclosing", "Spreading", "Sinking", "Rising", "Retreating", "Advancing" };
    

    
    public string ShapeInfo = "";
    private static Vector2 _scrollPosition;

    private static string _questionNo = "";
    private static string _questionStr = "";
    
    private static string[] _isSubmittedStrDrive = new string[32];

    //UI related
    private float _scrollWidth = 320f;


    private string[] _animName;
    
    private static int _animIndex = 0;

    GameObject _agent;
    void Start(){



        //_agent = GameObject.Find("MichaelPrefab");
        _agent = GameObject.Find("AgentPrefab");
   //     _agent = GameObject.Find("AdamPrefab");

        if(!_agent) {
            Debug.Log("Prefab not found");
            return;
        }
        _torso = _agent.GetComponent<TorsoController>();
        

        for (int i = 0; i < _isSubmittedStrDrive.Length; i++)
            _isSubmittedStrDrive[i] = "Answer NOT submitted";

        
        
        UpdateDriveParameters();

        //Assign cameras

        UpdateCameraBoundaries( _toggleDrawCurves);


        
        //Get animation clips
        _animName = new string[_agent.GetComponent<Animation>().GetClipCount()];
        
        int c = 0;        
        foreach (AnimationState clip in _agent.GetComponent<Animation>()) 
            _animName[c++] = clip.name;


        
       // _agent.GetComponent<AnimationInfo>().Reset(_animName[_animIndex]);
       // _agent.GetComponent<IKAnimator>().Reset();

        ResetComponents(_agent);
        UpdateEmoteParams();


        StopAtFirstFrame(_agent);

        //_agent.animation.Play(_animName[_animIndex]);
        //StopAnim(_agent);




        //   DontDestroyOnLoad(GameObject.Find("Camera1")); //don't change camera views
        //   DontDestroyOnLoad(GameObject.Find("Camera2")); //don't change camera views
        //   DontDestroyOnLoad(GameObject.Find("Camera3")); //don't change camera views
        //   DontDestroyOnLoad(GameObject.Find("Camera4")); //don't change camera views

    }

   
    void UpdateDriveParameters() {


#if !WEBMODE

        _driveParams.ReadValuesDrives(_driveInd);
#elif WEBMODE
        this.StartCoroutine(_driveParams.GetValuesDrives(_driveInd, UserInfo.UserId));
#endif

    }

    void UpdateCameraBoundaries(bool mode) {
        
        
        GameObject cam1 = GameObject.Find("Camera1");
        GameObject cam2 = GameObject.Find("Camera2");
        GameObject cam3 = GameObject.Find("Camera3");
        GameObject cam4 = GameObject.Find("Camera4");

        if (mode == true ) {
            
            cam1.GetComponent<Camera>().rect = new Rect(0, 0, _scrollWidth / Screen.width, 1); //320 is the width of the parameters
            cam2.GetComponent<Camera>().rect = new Rect(_scrollWidth / Screen.width, 0, ((Screen.width - _scrollWidth) / 2f) / Screen.width, 0.6f);
            cam2.GetComponent<Camera>().rect = new Rect(_scrollWidth / Screen.width, 0, ((Screen.width - _scrollWidth)) / Screen.width, 0.6f);
            //cam3.camera.rect = new Rect((Screen.width - (Screen.width - _scrollWidth) / 2f) / Screen.width, 0, ((Screen.width - _scrollWidth) / 2f) / Screen.width, 0.6f);
            cam4.GetComponent<Camera>().rect = new Rect(_scrollWidth / Screen.width, 0.6f, (Screen.width - _scrollWidth) / Screen.width, 0.4f);
        }
        else {
            
            cam1.GetComponent<Camera>().rect = new Rect(0, 0, _scrollWidth / Screen.width, 1); //320 is the width of the parameters
            //cam2.camera.rect = new Rect(_scrollWidth / Screen.width, 0, ((Screen.width - _scrollWidth) / 2f) / Screen.width, 1);
            cam2.GetComponent<Camera>().rect = new Rect(_scrollWidth / Screen.width, 0, ((Screen.width - _scrollWidth)) / Screen.width, 1);
            //cam3.camera.rect = new Rect((Screen.width - (Screen.width - _scrollWidth) / 2f) / Screen.width, 0, ((Screen.width - _scrollWidth) / 2f) / Screen.width, 1);
            cam4.GetComponent<Camera>().rect = new Rect(0, 0, 0, 0);
        }
         
    }

    void Update() {

        
        UpdateCameraBoundaries(_toggleDrawCurves);
        

        
            if (Input.GetKeyDown("up")) {
                _animIndex++;
                if (_animIndex > _animName.Length - 1)
                    _animIndex = _animName.Length - 1;

                StopAtFirstFrame(_agent);
                ResetComponents(_agent);
                StopAtFirstFrame(_agent);
                
                /*
                
                StopAnimations();
                
               _torso.AssignInitRootandFootPos();
                _torso.Reset();
 

                PlayAnim(_agent, _animName[_animIndex]);
                StopAnim(_agent);
                 */
            }
            else if (Input.GetKeyDown("down")) {
                _animIndex--;
                if (_animIndex < 0)
                    _animIndex = 0;
                
                
               // UpdateEmoteParams();
                StopAtFirstFrame(_agent);
                ResetComponents(_agent);
                StopAtFirstFrame(_agent);

              /*  StopAnimations();
                _torso.AssignInitRootandFootPos();
                _torso.Reset();
                PlayAnim(_agent, _animName[_animIndex]);
                StopAnim(_agent);
               */
            }
        

        if (Input.GetKeyDown("left")) {
            _driveInd--;
            if (_driveInd < 0)
                _driveInd = 0;
            //ResetDriveParameters();                
            // UpdateDriveParameters();
            StopAnimations();

      //      UpdateDriveParameters();

        }
        
        else if (Input.GetKeyDown("right")) {
            _driveInd++;
            if (_driveInd >= 31)
                _driveInd = 31;
            //ResetDriveParameters();                
            //UpdateDriveParameters();
            StopAnimations();
     //       UpdateDriveParameters();

        }


            
        
    }

	void OnGUI () {


        
        GUIStyle style = new GUIStyle();

        GUI.color = Color.white;

        
        GUILayout.BeginArea (new Rect (320,10,300,250));


        style.fontSize = 18;
        style.normal.textColor = Color.white;
        GUILayout.Label(_animName[_animIndex]);
        _toggleDrawCurves = GUILayout.Toggle(_toggleDrawCurves, "Draw velocity curves");
       
        
        GUILayout.Label(_questionNo, style);

        
        GUILayout.Label(_questionStr, style); //effortstr


        GUILayout.EndArea();


        style.normal.textColor = Color.black;
        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect( Screen.width - 100, Screen.height - 30,  200, 200));
        if (GUILayout.Button("Reset Scene", GUILayout.Width(100))) {            
            Application.LoadLevel(1);            
        }
            
        GUILayout.EndArea();


        GUILayout.BeginArea (new Rect (Screen.width/2f, Screen.height-150, 300, 200));
        GUILayout.Space (10);
        GUI.color = Color.black;

        
        _toggleContinuous = GUILayout.Toggle(_toggleContinuous, "Animation looping");

        GUI.color = Color.white;
        _armShapeChanged[0] = _armShapeChanged[1] = false;


        
        if (GUILayout.Button("Play")) {
            /*
            _torso.Reset();
          
            //now play
            UpdateEmoteParams();
            PlayAnim(_agent, _animName[_animIndex]);
            
            */

            
            ResetComponents(_agent);

            UpdateEmoteParams();
            
            Play(_agent);
            
            

            _armShapeChanged[0] = _armShapeChanged[1] = true;

          
            //_agent.GetComponent<TorsoController>().ResetToInitRootPos();
        }
                    
        GUILayout.Label("");
        GUILayout.BeginHorizontal ();	
        GUI.color = Color.white;        

        if(GUILayout.Button ( "Previous question")) {
                _driveInd--;
                if (_driveInd < 0)
                    _driveInd = 0;
                //ResetDriveParameters();
                //this.StartCoroutine(GetValuesDrives());
                StopAnimations();
         //       UpdateDriveParameters();

                
        }        
        GUI.color = Color.white;    
        if(GUILayout.Button ( "Next question")) {
                _driveInd++;
                if (_driveInd >= 31)
                    _driveInd = 31;
                //ResetDriveParameters();
                StopAnimations();
         //       UpdateDriveParameters();
            

        }
        GUILayout.EndHorizontal ();


        GUI.color = Color.white;  

        if(GUILayout.Button("Submit")){            

                _isSubmittedStrDrive[_driveInd] = "Answer submitted";
                
                #if !WEBMODE
                       _driveParams.RecordValuesDrives(_driveInd);
#elif WEBMODE
                this.StartCoroutine(_driveParams.PostValuesDrives(_driveInd, "https://fling.seas.upenn.edu/~fundad/cgi-bin/v2/putDriveData.php"));
                #endif

        }

	    
       GUILayout.Label(_isSubmittedStrDrive[_driveInd], style);
        GUILayout.EndArea();

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(_scrollWidth), GUILayout.Height(Screen.height * 0.98f));
            
        GUILayout.Space (10);
        style.fontSize = 11;
        style.normal.textColor = Color.black;            
        GUI.color = Color.grey;

        
        GUI.color = Color.Lerp(Color.black, Color.grey, 0.5f);


        //_useFBIK = GUILayout.Toggle(_useFBIK, "Use Full Body IK");
	    //_agent.GetComponent<IKAnimator>().UseFBIK = _useFBIK;

        if (GUILayout.Button("Reset All"))
            _driveParams.ResetDriveParameters();


            //ExtraGoal
        GUILayout.Label("Add extra goal point(s)");
        GUILayout.Label("false\ttrue");
        _driveParams.ExtraGoal = (int)GUILayout.HorizontalSlider(_driveParams.ExtraGoal, 0, 1, GUILayout.Width(50));
        GUILayout.Label("Animation speed");  
        GUILayout.Label("");
        //speed
        GUILayout.BeginHorizontal();
            
        GUILayout.Label("" + SpeedMin);
        GUILayout.Label("" + _driveParams.Speed);
        GUILayout.Label("" + SpeedMax);
        if (GUILayout.Button("Reset"))
            _driveParams.Speed = 0.0f; //funda 0
        GUILayout.EndHorizontal();
        GUI.SetNextControlName("speed");
        _driveParams.Speed = GUILayout.HorizontalSlider(_driveParams.Speed, SpeedMin, SpeedMax).Truncate(3);
        GUILayout.Label("");

        bool animChanged = GUI.changed;
           
        //V0  T0    
        GUILayout.BeginHorizontal();            
        GUILayout.Label("v0 (Anticipation velocity)");
        GUILayout.Label("t0 (Anticipation time)");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
            _driveParams.V0 = 0;
        if (GUILayout.Button("Reset"))
            _driveParams.T0 = 0f;        
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("\t\t\t" + _driveParams.V0, GUILayout.Width(140));
        GUILayout.Label("\t\t\t" + _driveParams.T0);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();        
        _driveParams.V0 = GUILayout.HorizontalSlider(_driveParams.V0, _vMin, _vMax).Truncate(2);
        _driveParams.T0 = GUILayout.HorizontalSlider(_driveParams.T0, _tMin, _driveParams.Ti).Truncate(2);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _vMin + "\t\t\t\t\t\t\t\t" + _vMax);
        GUILayout.Label(" " + _tMin + "\t\t\t\t\t\t\t\t" + _driveParams.Ti);
        GUILayout.EndHorizontal();
        GUILayout.Label("");

       
        //V1  
        GUILayout.BeginHorizontal();
        GUILayout.Label("v1 (Overshoot velocity)");
        GUILayout.Label("t1 (Overshoot time)");
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
            _driveParams.V1 = 0;
        if (GUILayout.Button("Reset"))
            _driveParams.T1 = 1f;
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label("\t\t\t" + _driveParams.V1, GUILayout.Width(140));
        GUILayout.Label("\t\t\t" + _driveParams.T1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        _driveParams.V1 = GUILayout.HorizontalSlider(_driveParams.V1, _vMin, _vMax).Truncate(2);
        _driveParams.T1 = GUILayout.HorizontalSlider(_driveParams.T1, _driveParams.Ti, _tMax).Truncate(2);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _vMin + "\t\t\t\t\t\t\t\t" + _vMax);
        GUILayout.Label(" " + _driveParams.Ti + "\t\t\t\t\t\t\t\t" + _tMax);
        GUILayout.EndHorizontal();

        GUILayout.Label("");



        //ti
        GUILayout.Label("ti (Inflection time where movement changes from accelerating to decelerating)");  
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _tMin);
        GUILayout.Label("" + _driveParams.Ti);
        GUILayout.Label("" + _tMax);
        if (GUILayout.Button("Reset"))
            _driveParams.Ti = 0.5f;
        GUILayout.EndHorizontal();
        _driveParams.Ti = GUILayout.HorizontalSlider(_driveParams.Ti, _tMin, _tMax).Truncate(2);
        GUILayout.Label("");


        //texp
        GUILayout.Label("Time exponent to magnify acceleration or deceleration");  
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + TexpMin);
        GUILayout.Label("" + _driveParams.Texp);
        GUILayout.Label("" + TexpMax);
        if (GUILayout.Button("Reset"))
            _driveParams.Texp = 1f;
        GUILayout.EndHorizontal();
        _driveParams.Texp = GUILayout.HorizontalSlider(_driveParams.Texp, TexpMin, TexpMax).Truncate(2);
        GUILayout.Label("");

        //GoalThreshold
        GUILayout.Label("Goal Threshold");
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + 0);
        GUILayout.Label("" + _driveParams.GoalThreshold);
        GUILayout.Label("" + 1);
        if (GUILayout.Button("Reset"))
            _driveParams.GoalThreshold = 0f;
        GUILayout.EndHorizontal();
        _driveParams.GoalThreshold = GUILayout.HorizontalSlider(_driveParams.GoalThreshold, 0, 1).Truncate(2);
        GUILayout.Label("");

        //Tval     
        GUILayout.Label("Tension");                    
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + TValMin);
        GUILayout.Label("" + _driveParams.Tval);
        GUILayout.Label("" + TValMax);
        if (GUILayout.Button("Reset"))
            _driveParams.Tval = 0;
        GUILayout.EndHorizontal();
        _driveParams.Tval = GUILayout.HorizontalSlider(_driveParams.Tval, TValMin, TValMax).Truncate(2);
        GUILayout.Label("");

        //Continuity   
        GUILayout.Label("Continuity");          
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + ContinuityMin);
        GUILayout.Label("" + _driveParams.Continuity);
        GUILayout.Label("" + ContinuityMax);
        if (GUILayout.Button("Reset"))
            _driveParams.Continuity = 0;
        GUILayout.EndHorizontal();
        _driveParams.Continuity = GUILayout.HorizontalSlider(_driveParams.Continuity, ContinuityMin, ContinuityMax).Truncate(2);
        GUILayout.Label("");


       
          
          
        GUI.color = Color.grey;
        GUILayout.Label("Flourishes");

        GUI.color = Color.Lerp(Color.black, Color.grey, 0.5f);


        //TorsoRotMag TfMag
        GUILayout.BeginHorizontal();
        GUILayout.Label("Torso rotation magnitude");
        GUILayout.Label("Torso rotation frequency");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
            _driveParams.TrMag = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.TfMag = 0f;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("\t\t\t" + _driveParams.TrMag, GUILayout.Width(140));
        GUILayout.Label("\t\t\t" + _driveParams.TfMag);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        _driveParams.TrMag = GUILayout.HorizontalSlider(_driveParams.TrMag, _trMin, _trMax).Truncate(2);
        _driveParams.TfMag = GUILayout.HorizontalSlider(_driveParams.TfMag, _tfMin, _tfMax).Truncate(2);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _trMin + "\t\t\t\t\t\t\t\t" + _trMax);
        GUILayout.Label("" + _tfMin + "\t\t\t\t\t\t\t\t" + _tfMax);
        GUILayout.EndHorizontal();
        
        GUILayout.Label("");



        GUILayout.Label("Fixed target");
        GUILayout.Label("false\ttrue");
        _driveParams.FixedTarget = (int)GUILayout.HorizontalSlider(_driveParams.FixedTarget, 0, 1, GUILayout.Width(50));

        GUILayout.Label("");

        if (_driveParams.FixedTarget == 0) {


            //HeadRotMag HfMag    
            GUILayout.BeginHorizontal();
            GUILayout.Label("Head rotation magnitude");
            GUILayout.Label("Head rotation frequency");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset"))
                _driveParams.HrMag = 0f;
            if (GUILayout.Button("Reset"))
                _driveParams.HfMag = 0f;
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("\t\t\t" + _driveParams.HrMag, GUILayout.Width(140));
            GUILayout.Label("\t\t\t" + _driveParams.HfMag);            
            GUILayout.EndHorizontal();
                    
            
            GUILayout.BeginHorizontal();
            _driveParams.HrMag = GUILayout.HorizontalSlider(_driveParams.HrMag, _hrMin, _hrMax).Truncate(2);
            _driveParams.HfMag = GUILayout.HorizontalSlider(_driveParams.HfMag, _hfMin, _hfMax).Truncate(2);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("" + _hrMin + "\t\t\t\t\t\t\t\t" + _hfMax);
            GUILayout.Label("" + _hfMin + "\t\t\t\t\t\t\t\t" + _hfMax);
            GUILayout.EndHorizontal();

            GUILayout.Label("");
        }



        //SquashMag//squashF
        GUILayout.BeginHorizontal();
        GUILayout.Label("Breathing magnitude");
        GUILayout.Label("Breathing frequency");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
            _driveParams.SquashMag = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.SquashF = 0f;        
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("\t\t\t" + _driveParams.SquashMag, GUILayout.Width(140));
        GUILayout.Label("\t\t\t" + _driveParams.SquashF);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        _driveParams.SquashMag = GUILayout.HorizontalSlider(_driveParams.SquashMag, _squashMin, _squashMax).Truncate(2);
        _driveParams.SquashF = GUILayout.HorizontalSlider(_driveParams.SquashF, SquashFMin, SquashFMax).Truncate(2);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _squashMin + "\t\t\t\t\t\t\t\t" + _squashMax);
        GUILayout.Label("" + SquashFMin + "\t\t\t\t\t\t\t\t" + SquashFMax);
        GUILayout.EndHorizontal();

        GUILayout.Label("");

        //WbMag
        GUILayout.Label("Wrist bend ");
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _wbMin);
        GUILayout.Label("" + _driveParams.WbMag);
        GUILayout.Label("" + _wbMax);
        if (GUILayout.Button("Reset"))
            _driveParams.WbMag = 0;
        GUILayout.EndHorizontal();
        _driveParams.WbMag = GUILayout.HorizontalSlider(_driveParams.WbMag, _wbMin, _wbMax).Truncate(2);
        GUILayout.Label("");

        //WxMag
        GUILayout.Label("Wrist extension (initial extension)");
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _wxMin);
        GUILayout.Label("" + _driveParams.WxMag);
        GUILayout.Label("" + _wxMax);
        if (GUILayout.Button("Reset"))
            _driveParams.WxMag = 0f;
        GUILayout.EndHorizontal();
        _driveParams.WxMag = GUILayout.HorizontalSlider(_driveParams.WxMag, _wxMin, _wxMax).Truncate(2);
        GUILayout.Label("");

        //Wtmag
        GUILayout.Label("Wrist twist");
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _wtMin);
        GUILayout.Label("" + _driveParams.WtMag);
        GUILayout.Label("" + _wtMax);
        if (GUILayout.Button("Reset"))
            _driveParams.WtMag = 0f;
        GUILayout.EndHorizontal();
        _driveParams.WtMag = GUILayout.HorizontalSlider(_driveParams.WtMag, _wtMin, _wtMax).Truncate(2);
        GUILayout.Label("");


        //WfMag
	    GUILayout.Label("Wrist frequecy");
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _wfMin);
        GUILayout.Label("" + _driveParams.WfMag);
        GUILayout.Label("" + _wfMax);
        if (GUILayout.Button("Reset"))
            _driveParams.WfMag = 0f;
        GUILayout.EndHorizontal();
        _driveParams.WfMag = GUILayout.HorizontalSlider(_driveParams.WfMag, _wfMin, _wfMax).Truncate(2);
        GUILayout.Label("");



        //EtMag
        GUILayout.Label("Elbow twist");
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _etMin);
        GUILayout.Label("" + _driveParams.EtMag);
        GUILayout.Label("" + _etMax);
        if (GUILayout.Button("Reset"))
            _driveParams.EtMag = 0f;
        GUILayout.EndHorizontal();
        _driveParams.EtMag = GUILayout.HorizontalSlider(_driveParams.EtMag, _etMin, _etMax).Truncate(2);
        GUILayout.Label("");

        //DMag
        GUILayout.Label("Elbow displacement");
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _dMin);
        GUILayout.Label("" + _driveParams.DMag);
        GUILayout.Label("" + _dMax);
        if (GUILayout.Button("Reset"))
            _driveParams.DMag = 0f;
        GUILayout.EndHorizontal();
        _driveParams.DMag = GUILayout.HorizontalSlider(_driveParams.DMag, _dMin, _dMax).Truncate(2);
        GUILayout.Label("");

        //EfMag
        GUILayout.Label("Elbow frequecy");
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _efMin);
        GUILayout.Label("" + _driveParams.EfMag);
        GUILayout.Label("" + _efMax);
        if (GUILayout.Button("Reset"))
            _driveParams.EfMag = 0f;
        GUILayout.EndHorizontal();
        _driveParams.EfMag = GUILayout.HorizontalSlider(_driveParams.EfMag, _efMin, _efMax).Truncate(2);
        GUILayout.Label("");

      

        GUI.color = Color.grey;
        GUILayout.Label("Shape");
        GUI.color = Color.Lerp(Color.black, Color.grey, 0.5f);


        //shapeTi        

        GUILayout.Label("Shape inflection time");
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _tMin);
        GUILayout.Label("" + _driveParams.ShapeTi);
        GUILayout.Label("" + _tMax);
        if (GUILayout.Button("Reset"))
            _driveParams.ShapeTi = 0f;
        GUILayout.EndHorizontal();
        _driveParams.ShapeTi = GUILayout.HorizontalSlider(_driveParams.ShapeTi, _tMin, _tMax).Truncate(2);
        GUILayout.Label("");
    



        //encSpr
        GUILayout.Label("Enclosing Spreading");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Value at T= 0");
        GUILayout.Label("Value at Ti");
        GUILayout.Label("Value at T = 1");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
            _driveParams.EncSpr0 = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.EncSpr1 = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.EncSpr2 = 0f;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("\t" + _driveParams.EncSpr0, GUILayout.Width(100));
        GUILayout.Label("\t" + _driveParams.EncSpr1, GUILayout.Width(100));
        GUILayout.Label("\t" + _driveParams.EncSpr2);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        _driveParams.EncSpr0 = GUILayout.HorizontalSlider(_driveParams.EncSpr0, _shapeMin, _shapeMax).Truncate(2);
        _driveParams.EncSpr1 = GUILayout.HorizontalSlider(_driveParams.EncSpr1, _shapeMin, _shapeMax).Truncate(2);
        _driveParams.EncSpr2 = GUILayout.HorizontalSlider(_driveParams.EncSpr2, _shapeMin, _shapeMax).Truncate(2);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.EndHorizontal();  


        //SinRis
        GUILayout.Label("Sinking Rising");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Value at T= 0");
        GUILayout.Label("Value at Ti");
        GUILayout.Label("Value at T = 1");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
            _driveParams.SinRis0 = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.SinRis1 = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.SinRis2 = 0f;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("\t" + _driveParams.SinRis0, GUILayout.Width(100));
        GUILayout.Label("\t" + _driveParams.SinRis1, GUILayout.Width(100));
        GUILayout.Label("\t" + _driveParams.SinRis2);
        GUILayout.EndHorizontal();
        

	    GUILayout.BeginHorizontal();
        _driveParams.SinRis0 = GUILayout.HorizontalSlider(_driveParams.SinRis0, _shapeMin, _shapeMax).Truncate(2);
        _driveParams.SinRis1 = GUILayout.HorizontalSlider(_driveParams.SinRis1, _shapeMin, _shapeMax).Truncate(2);
        _driveParams.SinRis2 = GUILayout.HorizontalSlider(_driveParams.SinRis2, _shapeMin, _shapeMax).Truncate(2);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.EndHorizontal();  



        //RetAdv

        GUILayout.Label("Retreating Advancing");        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Value at T= 0");
        GUILayout.Label("Value at Ti");
        GUILayout.Label("Value at T = 1");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
            _driveParams.RetAdv0 = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.RetAdv1 = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.RetAdv2 = 0f;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("\t" + _driveParams.RetAdv0, GUILayout.Width(100));
        GUILayout.Label("\t" + _driveParams.RetAdv1, GUILayout.Width(100));
        GUILayout.Label("\t" + _driveParams.RetAdv2);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        _driveParams.RetAdv0 = GUILayout.HorizontalSlider(_driveParams.RetAdv0, _shapeMin, _shapeMax).Truncate(2);
        _driveParams.RetAdv1 = GUILayout.HorizontalSlider(_driveParams.RetAdv1, _shapeMin, _shapeMax).Truncate(2);
        _driveParams.RetAdv2 = GUILayout.HorizontalSlider(_driveParams.RetAdv2, _shapeMin, _shapeMax).Truncate(2);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.EndHorizontal();  

	    bool changed = GUI.changed;


        GUILayout.Label("");
        GUILayout.Label("Use curve keys");
        GUILayout.Label("false\ttrue");
        _driveParams.UseCurveKeys = (int)GUILayout.HorizontalSlider(_driveParams.UseCurveKeys, 0, 1, GUILayout.Width(50));
        if (GUI.changed && !changed) {
            _armShapeChanged[0] = true; //left arm shape is changed, update in IKAnimator
            _armShapeChanged[1] = true; //left arm shape is changed, update in IKAnimator
        }

        changed = GUI.changed;

        GUILayout.Label("");


        //Right Arm
        GUILayout.BeginHorizontal();
        GUILayout.Label("Right Arm X");
        GUILayout.Label("Right Arm Y");
        GUILayout.Label("Right Arm Z");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
            _driveParams.Arm[1].x = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.Arm[1].y = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.Arm[1].z = 0f;
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _driveParams.Arm[1].x, GUILayout.Width(100));
        GUILayout.Label("" + _driveParams.Arm[1].y, GUILayout.Width(100));
        GUILayout.Label("" + _driveParams.Arm[1].z);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        _driveParams.Arm[1].x = GUILayout.HorizontalSlider(_driveParams.Arm[1].x, _shapeMin, _shapeMax).Truncate(2);
        _driveParams.Arm[1].y = GUILayout.HorizontalSlider(_driveParams.Arm[1].y, _shapeMin, _shapeMax).Truncate(2);
        _driveParams.Arm[1].z = GUILayout.HorizontalSlider(_driveParams.Arm[1].z, _shapeMin, _shapeMax).Truncate(2);
        GUILayout.EndHorizontal();
        

        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.EndHorizontal();  



        if (GUI.changed && !changed) {
            _armShapeChanged[1] = true; //right arm shape is changed, update in IKAnimator
        }

        changed = GUI.changed;
        GUILayout.Label("");
        //Left Arm
        GUILayout.BeginHorizontal();
        GUILayout.Label("Left Arm X");
        GUILayout.Label("Left Arm Y");
        GUILayout.Label("Left Arm Z");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
            _driveParams.Arm[0].x = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.Arm[0].y = 0f;
        if (GUILayout.Button("Reset"))
            _driveParams.Arm[0].z = 0f;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _driveParams.Arm[0].x, GUILayout.Width(100));
        GUILayout.Label("" + _driveParams.Arm[0].y, GUILayout.Width(100));
        GUILayout.Label("" + _driveParams.Arm[0].z);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        _driveParams.Arm[0].x = GUILayout.HorizontalSlider(_driveParams.Arm[0].x, _shapeMin, _shapeMax).Truncate(2);
        _driveParams.Arm[0].y = GUILayout.HorizontalSlider(_driveParams.Arm[0].y, _shapeMin, _shapeMax).Truncate(2);
        _driveParams.Arm[0].z = GUILayout.HorizontalSlider(_driveParams.Arm[0].z, _shapeMin, _shapeMax).Truncate(2);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.Label("" + _shapeMin + "\t\t\t\t\t" + _shapeMax);
        GUILayout.EndHorizontal();  

        if (GUI.changed && !changed) {
            _armShapeChanged[0] = true; //left arm shape is changed, update in IKAnimator
        }
            
            
        UpdateEmoteParams();


        _questionNo = "Question: " + (_driveInd + 1) + " of 32";
        _questionStr = ComputeEffortCombinationStr(_driveInd);
        switch(_driveInd) {
            case 0:
                _questionStr += " = Float";
                break;
            case 1:
                _questionStr += "  = Flick";
                break;
            case 2:
                _questionStr += "  = Wring";
                break;
            case 3:
                _questionStr += "  = Slash";
                break;
            case 4:
                _questionStr += "  = Glide";
                break;
            case 5:
                _questionStr += "  = Dab";
                break;
            case 6:
                _questionStr += "  = Press";
                break;
            case 7:
                _questionStr += "  = Punch";
                break;

        }
        
      
     
    GUILayout.EndScrollView();                  
         
        
}

    string ComputeEffortCombinationStr(int driveInd) {
        string str = "";
        if (_effortList[driveInd, 0] == -1)
            str += "Indirect";
        else if (_effortList[driveInd, 0] == 1)
            str += "Direct";

        str += " ";
        if (_effortList[driveInd, 1] == -1)
            str += "Light";
        else if (_effortList[driveInd, 1] == 1)
            str += "Strong";

        str += " ";
        if (_effortList[driveInd, 2] == -1)
            str += "Sustained";
        else if (_effortList[driveInd, 2] == 1)
            str += "Sudden";


        str += " ";
        if (_effortList[driveInd, 3] == -1)
            str += "Free";
        else if (_effortList[driveInd, 3] == 1)
            str += "Bound";

        return str;

    }
    public void ResetComponents(GameObject agent) {
        agent.GetComponent<Animation>().Stop();
        agent.GetComponent<TorsoController>().Reset();
        agent.GetComponent<AnimationInfo>().Reset(_animName[_animIndex]);    
        agent.GetComponent<IKAnimator>().Reset();
       

        
    }
    public void StopAtFirstFrame(GameObject agent) {
        if (!agent.GetComponent<Animation>().isPlaying)
            agent.GetComponent<Animation>().Play(_animName[_animIndex]);

        agent.GetComponent<Animation>().clip.SampleAnimation(agent, 0f); //instead of rewind
        agent.GetComponent<Animation>().Stop();

    }
    public void Play(GameObject agent) {
        AnimationInfo animInfo = _agent.GetComponent<AnimationInfo>();


        agent.GetComponent<Animation>().enabled = true;

        animInfo.IsContinuous = _toggleContinuous;
        agent.GetComponent<Animation>().wrapMode = animInfo.IsContinuous ? WrapMode.Loop : WrapMode.ClampForever;   
        agent.GetComponent<Animation>().Play(_animName[_animIndex]);

    }



    public void Reset(GameObject agent) {
        /*InitAgent(agent);

//        PlayAnim(agent, agent.AnimInd);
        StopAnim(agent);

 */

        ResetComponents(agent);
        StopAtFirstFrame(agent);


    }


    
    void StopAnim(GameObject agent){

        if (agent.GetComponent<Animation>().isPlaying) {
            agent.GetComponent<Animation>().clip.SampleAnimation(agent, 0); //instead of rewind
            agent.GetComponent<Animation>().Stop();           
        }        
        
    }
    void StopAnimations() {

        _agent.GetComponent<TorsoController>().Reset();

        
        StopAnim(_agent);
        PlayAnim(_agent, _animName[_animIndex]); //start the next animation
        StopAnim(_agent);


        //changes the names of the drives
        UpdateDriveParameters();
        UpdateEmoteParams();

    }
    void InitAgent(GameObject agent, string animName) {
        if (!agent) 
            return;

        
        agent.GetComponent<AnimationInfo>().Reset(animName);                            
        agent.GetComponent<IKAnimator>().Reset();
        

        agent.GetComponent<Animation>().enabled = true;
    
        //alredy done in animinfo reset?
        //agent.GetComponent<AnimationInfo>().UpdateInterpolators();

        agent.GetComponent<Animation>().Play(animName);

        
    }

    void PlayAnim( GameObject agent, string animName) {

        
        AnimationInfo animInfo = _agent.GetComponent<AnimationInfo>();
        animInfo.IsContinuous = _toggleContinuous;
        agent.GetComponent<Animation>().Stop(); //in order to restart animation
        InitAgent(agent, animName);
        
        if(animInfo.IsContinuous){
            agent.GetComponent<Animation>()[animInfo.AnimName].wrapMode = WrapMode.Loop;            
        }
        else {
            agent.GetComponent<Animation>()[animInfo.AnimName].wrapMode = WrapMode.ClampForever; //we don't want fbik to run all the time
        }        
        

    }
    

    void UpdateEmoteParams() {
		
        if (_armShapeChanged[0]) {
            _agent.GetComponent<AnimationInfo>().Hor = _driveParams.Arm[0].x;
            _agent.GetComponent<AnimationInfo>().Ver = _driveParams.Arm[0].y;
            _agent.GetComponent<AnimationInfo>().Sag = _driveParams.Arm[0].z;
            _agent.GetComponent<AnimationInfo>().UpdateKeypointsByShape(0); //Update keypoints
        }
        if(_armShapeChanged[1]){
            //RightArm 
            //Only horizontal motion is the opposite for each arm
            _agent.GetComponent<AnimationInfo>().Hor = -_driveParams.Arm[1].x;
            _agent.GetComponent<AnimationInfo>().Ver = _driveParams.Arm[1].y;
            _agent.GetComponent<AnimationInfo>().Sag = _driveParams.Arm[1].z;
            _agent.GetComponent<AnimationInfo>().UpdateKeypointsByShape(1); //Update keypoints

        }

        _agent.GetComponent<AnimationInfo>().AnimSpeed = _driveParams.Speed;
        _agent.GetComponent<AnimationInfo>().V0 = _driveParams.V0;
        _agent.GetComponent<AnimationInfo>().V1 = _driveParams.V1;

        _agent.GetComponent<AnimationInfo>().T0 = _driveParams.T0;
        _agent.GetComponent<AnimationInfo>().T1 = _driveParams.T1;
        _agent.GetComponent<AnimationInfo>().Ti = _driveParams.Ti;


        _agent.GetComponent<AnimationInfo>().Texp = _driveParams.Texp;

        _agent.GetComponent<AnimationInfo>().GoalThreshold = _driveParams.GoalThreshold;

        float prevTVal = _agent.GetComponent<AnimationInfo>().Tval;
        float prevContinuity = _agent.GetComponent<AnimationInfo>().Continuity;
        _agent.GetComponent<AnimationInfo>().Tval = _driveParams.Tval;
        _agent.GetComponent<AnimationInfo>().Continuity = _driveParams.Continuity;

        if (_driveParams.Tval != prevTVal || _driveParams.Continuity != prevContinuity)
            _agent.GetComponent<AnimationInfo>().InitInterpolators(_driveParams.Tval, _driveParams.Continuity, 0);



        _agent.GetComponent<FlourishAnimator>().TrMag = _driveParams.TrMag;
        _agent.GetComponent<FlourishAnimator>().TfMag = _driveParams.TfMag * _agent.GetComponent<AnimationInfo>().AnimLength/1.625f;

        _agent.GetComponent<IKAnimator>().FixedTarget = _driveParams.FixedTarget;
        _agent.GetComponent<IKAnimator>().HrMag = _driveParams.HrMag;
        _agent.GetComponent<IKAnimator>().HfMag = _driveParams.HfMag * _agent.GetComponent<AnimationInfo>().AnimLength / 1.625f;
        _agent.GetComponent<AnimationInfo>().ExtraGoal = _driveParams.ExtraGoal;
        _agent.GetComponent<AnimationInfo>().UseCurveKeys = _driveParams.UseCurveKeys;

        _agent.GetComponent<IKAnimator>().SquashMag = _driveParams.SquashMag; //breathing affects keypoints
        _agent.GetComponent<IKAnimator>().SquashF = _driveParams.SquashF * _agent.GetComponent<AnimationInfo>().AnimLength / 1.625f; //breathing affects keypoints

        _agent.GetComponent<FlourishAnimator>().WbMag = _driveParams.WbMag;
        _agent.GetComponent<FlourishAnimator>().WxMag = _driveParams.WxMag;
        _agent.GetComponent<FlourishAnimator>().WfMag = _driveParams.WfMag * _agent.GetComponent<AnimationInfo>().AnimLength / 1.625f;
        _agent.GetComponent<FlourishAnimator>().WtMag = _driveParams.WtMag;
        _agent.GetComponent<FlourishAnimator>().EfMag = _driveParams.EfMag * _agent.GetComponent<AnimationInfo>().AnimLength / 1.625f;
        _agent.GetComponent<FlourishAnimator>().EtMag = _driveParams.EtMag;
        _agent.GetComponent<FlourishAnimator>().DMag = _driveParams.DMag;


        _agent.GetComponent<IKAnimator>().ShapeTi = _driveParams.ShapeTi;

        _agent.GetComponent<IKAnimator>().EncSpr[0] = _driveParams.EncSpr0;
        _agent.GetComponent<IKAnimator>().SinRis[0] = _driveParams.SinRis0;
        _agent.GetComponent<IKAnimator>().RetAdv[0] = _driveParams.RetAdv0;

        _agent.GetComponent<IKAnimator>().EncSpr[1] = _driveParams.EncSpr1;
        _agent.GetComponent<IKAnimator>().SinRis[1] = _driveParams.SinRis1;
        _agent.GetComponent<IKAnimator>().RetAdv[1] = _driveParams.RetAdv1;


        _agent.GetComponent<IKAnimator>().EncSpr[2] = _driveParams.EncSpr2;
        _agent.GetComponent<IKAnimator>().SinRis[2] = _driveParams.SinRis2;
        _agent.GetComponent<IKAnimator>().RetAdv[2] = _driveParams.RetAdv2;

        
    }

   

   
    
}		

