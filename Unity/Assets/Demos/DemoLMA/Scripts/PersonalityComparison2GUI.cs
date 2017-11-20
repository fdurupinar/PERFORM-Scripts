#define WEBMODE

#define DEBUGMODE


using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;



public class PersonalityComparison2GUI : GUIController {
    private const int UNSELECTED = -10;
    public Texture TexBorder = new Texture();
    public GUISkin ButtonSkin;



    private static int _taskQInd = 0; //question index for the task [0-9]


    private static int _selectPersonality = UNSELECTED; //to ensure that none is selected by default
    private static int _answerPersonality = UNSELECTED; //to ensure that none is selected by default
    

    public string ShapeInfo = "";
    public string Info = "waiting...";
    private static Vector2 _scrollPosition;

    private static string _questionNoStr = "";

    private static string[] _isSubmittedStr;
    private static bool[] _isSubmitted;
    private static bool[] _alreadyPlayed;

    private static string[] _personalityQuestionStr = {
        "calm and emotionally stable?",    
        "reserved and quiet?",
        "dependable and self-disciplined?",
        "sympathetic and warm?",
        "conventional and uncreative?",
        "critical and quarrelsome?",
        "open to new experiences and complex?", 
        "extraverted and enthusiastic?",
        "anxious and easily upset?",        
        "disorganized and careless?"};

private int _submittedCnt = 0;

    private float _scrollWidth = 0;


    private AnimationInfo _agentLeft, _agentRight;
    

    private int _qCnt;
    private  bool _firstQFirstPlay; //special case for the first question


    private PersonalityMapper _persMapper;
    private static DriveParams[] _driveParams = new DriveParams[32];

    private bool _drivesAchieved = false;

    
    private bool _drawWhiteScreen = false;

    private AnimationInfo[] _agentsLeft = new AnimationInfo[3];
    private AnimationInfo[] _agentsRight = new AnimationInfo[3];
        
    
    void Start() {

        //FUNDA
     //   UserInfo.Hit = 0;

        
        for (int i = 0; i < _agentsLeft.Length; i++) {
            _agentsLeft[i] = GameObject.Find("AgentPrefabLeft" + i).GetComponent<AnimationInfo>();
            _agentsRight[i] = GameObject.Find("AgentPrefabRight" + i).GetComponent<AnimationInfo>(); 


            //Make all invisible so that users can't see the models
            _agentsLeft[i].gameObject.SetActive(false);
            _agentsRight[i].gameObject.SetActive(false);
        }


        //TODO: Hits start from 0!!
        _agentsLeft[(UserInfo.Hit / 3)].gameObject.SetActive(true);
        _agentsRight[(UserInfo.Hit / 3)].gameObject.SetActive(true);

        for (int i = 0; i < 32; i++) {
            _driveParams[i] = new DriveParams();
            
        }

        _qCnt = 10;        
        
        
#if DEBUGMODE

        UserInfo.Hit = 0;
        UserInfo.IsMale =true;
        UserInfo.IsNative =true;
        UserInfo.Age = 20;
        UserInfo.UserId = "fd";
        
#endif


        //_taskQInd = 9; 

        _taskQInd = 0; //initial question's index
        
        _isSubmittedStr = new string[_qCnt];
        _isSubmitted = new bool[_qCnt];
        _alreadyPlayed = new bool[_qCnt];

        
        
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

        
#endif
       

        
        
        
        _persMapper = new PersonalityMapper();
        _firstQFirstPlay = true;

        _agentLeft = GameObject.Find("AgentPrefabLeft"  + (UserInfo.Hit/3)).GetComponent<AnimationInfo>();
        _agentRight = GameObject.Find("AgentPrefabRight" + (UserInfo.Hit / 3)).GetComponent<AnimationInfo>();
      

        if ((UserInfo.Hit % 3) == 2) { //walking
            _agentLeft.transform.position -= Vector3.forward * 10;
            _agentRight.transform.position -= Vector3.forward * 10;
        }

        Reset(_agentLeft);
        Reset(_agentRight);

    }



    void UpdateCameraBoundaries() {
        GameObject cam1 = GameObject.Find("Camera1");
        GameObject cam2 = GameObject.Find("Camera2");
        GameObject cam3 = GameObject.Find("Camera3");

        cam1.GetComponent<Camera>().rect = new Rect(0, 0, _scrollWidth / Screen.width, 1); //320 is the width of the parameters

        cam2.GetComponent<Camera>().rect = new Rect(_scrollWidth / Screen.width, 0, ((Screen.width - _scrollWidth) *0.5f) / Screen.width, 1);
        cam3.GetComponent<Camera>().rect = new Rect((Screen.width - (Screen.width - _scrollWidth)  * 0.5f) / Screen.width, 0, ((Screen.width - _scrollWidth)  * 0.5f) / Screen.width, 1);
        
    }

   

    void Update() {
        
        
        //check if the last drive has been achieved
        if (_driveParams[31].DrivesAchieved  && _firstQFirstPlay ) { //special case for the first playing of the first question       

            UpdateParameters();  //map personality to motion callls initkeypoints which stops the animation
            ResetComponents(_agentRight);
            ResetComponents(_agentLeft);

          
            _persMapper.ComputeMotionEffortCoefs(_driveParams);
            

            _persMapper.MapPersonalityToMotion(_agentLeft.GetComponent<PersonalityComponent>());
            _persMapper.MapPersonalityToMotion(_agentRight.GetComponent<PersonalityComponent>());
            Play(_agentRight);
            Play(_agentLeft);

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

        if (_taskQInd < _qCnt - 1) 
            _taskQInd++;
#if DEBUGMODE
   
        else if(_taskQInd == _qCnt - 1) {
            _taskQInd = 0;


            if (UserInfo.Hit < 8)
                UserInfo.Hit++;
        }
       
#endif
        else
            return;



        UpdateParameters();
        ResetComponents(_agentLeft);
        ResetComponents(_agentRight);

        _persMapper.MapPersonalityToMotion(_agentLeft.GetComponent<PersonalityComponent>());
        _persMapper.MapPersonalityToMotion(_agentRight.GetComponent<PersonalityComponent>());

        Play(_agentLeft);
        Play(_agentRight);



        

    }
    void GetPrevQuestion() {

        if (_taskQInd > 0) {
            _taskQInd--;
        }
      
#if DEBUGMODE
        else if (_taskQInd == 0) {
            _taskQInd = 9;
            if (UserInfo.Hit > 0)
                UserInfo.Hit--;
        }
    
#endif
    else
        return;


        UpdateParameters();
        ResetComponents(_agentLeft);
        ResetComponents(_agentRight);

        _persMapper.MapPersonalityToMotion(_agentLeft.GetComponent<PersonalityComponent>());
        _persMapper.MapPersonalityToMotion(_agentRight.GetComponent<PersonalityComponent>());

        Play(_agentLeft);
        Play(_agentRight);
            
        
    }

    
    
    

    void UpdateParameters() {

        for (int i = 0; i < 3; i++) {
   
            if(i == UserInfo.Hit/3) {
                _agentsLeft[i].gameObject.SetActive(true);
                _agentsRight[i].gameObject.SetActive(true);
                _agentLeft = _agentsLeft[UserInfo.Hit / 3];
                _agentRight = _agentsRight[UserInfo.Hit / 3];  
            }
            else {
                _agentsLeft[i].gameObject.SetActive(false);
                _agentsRight[i].gameObject.SetActive(false);                
            }            
        }

        //Personality question
        int persInd = 0;
        int persVal = 0;
        switch(_taskQInd) {
            case 0:
                persInd = 4;
                persVal = -1;
                break;
            case 1:
                persInd = 2;
                persVal = -1;
                break;
            case 2:
                persInd = 1;
                persVal = 1;
                break;
            case 3:
                persInd = 3;
                persVal = 1;
                break;
            case 4:
                persInd = 0;
                persVal = -1;
                break;
            case 5:
                persInd = 3;
                persVal = -1;
                break;
            case 6:
                persInd = 0;
                persVal = 1;
                break;
            case 7:
                persInd = 2;
                persVal = 1;
                break;
            case 8:
                persInd = 4;
                persVal = 1;
                break;
            case 9:
                persInd = 1;
                persVal = -1;
                break;

        }

        _agentRight.GetComponent<PersonalityComponent>().Personality[persInd] = persVal;

        for (int i = 0; i < 5; i++) {//right
            if(persInd !=i)
                _agentRight.GetComponent<PersonalityComponent>().Personality[i] = 0;
        }

        for (int i = 0; i < 5; i++) //left is allways zero
            _agentLeft.GetComponent<PersonalityComponent>().Personality[0] = 0;

        _agentRight.AnimName = _agentRight.AnimNames[UserInfo.Hit%3];
        _agentLeft.AnimName = _agentLeft.AnimNames[UserInfo.Hit%3];


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
        agent.GetComponent<FlourishAnimator>().TfMag = _driveParams[driveInd].TfMag * agent.GetComponent<AnimationInfo>().AnimLength / 1.625f;

        agent.GetComponent<IKAnimator>().HrMag = _driveParams[driveInd].HrMag;
        agent.GetComponent<IKAnimator>().HfMag = _driveParams[driveInd].HfMag * agent.GetComponent<AnimationInfo>().AnimLength / 1.625f;
        agent.GetComponent<AnimationInfo>().ExtraGoal = _driveParams[driveInd].ExtraGoal;
        

        agent.GetComponent<IKAnimator>().SquashMag = _driveParams[driveInd].SquashMag; //breathing affects keypoints
        agent.GetComponent<IKAnimator>().SquashF = _driveParams[driveInd].SquashF * agent.GetComponent<AnimationInfo>().AnimLength / 1.625f; //breathing affects keypoints

        agent.GetComponent<FlourishAnimator>().WbMag = _driveParams[driveInd].WbMag;
        agent.GetComponent<FlourishAnimator>().WxMag = _driveParams[driveInd].WxMag;
        agent.GetComponent<FlourishAnimator>().WfMag = _driveParams[driveInd].WfMag * agent.GetComponent<AnimationInfo>().AnimLength / 1.625f;
        agent.GetComponent<FlourishAnimator>().WtMag = _driveParams[driveInd].WtMag;
        agent.GetComponent<FlourishAnimator>().EfMag = _driveParams[driveInd].EfMag * agent.GetComponent<AnimationInfo>().AnimLength / 1.625f;
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
        style.normal.textColor = Color.white;


	    //Border between the two images
	    GUI.DrawTexture(new Rect(Screen.width/2f - 1f, 0, 2, Screen.height), TexBorder, ScaleMode.ScaleToFit, true,
	                    2f/Screen.height);

	    if (_drawWhiteScreen) {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), TexBorder, ScaleMode.ScaleToFit, true,
                            (float)Screen.width / Screen.height);
            return;
        }
	

    GUI.skin = ButtonSkin;


    string[] selectStr = { "Strongly disagree", "Moderately disagree", "Slightly disagree", "Neither agree\nnor disagree", "Slightly agree", "Moderately agree", "Strongly agree" };

        style.fontSize = 20;


#if DEBUGMODE
        
        GUILayout.BeginArea(new Rect(20, 10, 2500, 250));
        GUILayout.Label("Debug Mode" , style);
        
        style.fontSize = 12;
            //GUILayout.Label("Left: " + _agentLeft.Effort2String(), style);
            //GUILayout.Label("Right: " + _agentRight.Effort2String(), style);


        GUILayout.Label("Left: " + _agentLeft.GetComponent<PersonalityComponent>().Personality[0] + " " +_agentLeft.GetComponent<PersonalityComponent>().Personality[1] + 
        _agentLeft.GetComponent<PersonalityComponent>().Personality[2] + _agentLeft.GetComponent<PersonalityComponent>().Personality[3] + _agentLeft.GetComponent<PersonalityComponent>().Personality[4] , style);
        GUILayout.Label("Right: " + _agentRight.GetComponent<PersonalityComponent>().Personality[0] + " " + _agentRight.GetComponent<PersonalityComponent>().Personality[1] +
        _agentRight.GetComponent<PersonalityComponent>().Personality[2] + _agentRight.GetComponent<PersonalityComponent>().Personality[3] + _agentRight.GetComponent<PersonalityComponent>().Personality[4], style);
        GUILayout.Label("Hit: " + UserInfo.Hit);
        GUILayout.EndArea();
#endif
#if DEBUGMODE
        if(UserInfo.Hit >= 8 && _taskQInd >= 9){
#else
        if (_submittedCnt >= _qCnt) { //STUDY IS OVER            
#endif
            
            style.fontSize = 16;
            style.normal.textColor = Color.black;
            _isSubmittedStr[_taskQInd] = "HIT is complete. Thank you!";
            
        }

        else { //STUDY CONTINUING

            if (_agentLeft.gameObject && _agentRight.gameObject) {
                if (_agentLeft.AnimationOver() &&
                    _agentRight.AnimationOver()) {
                    //Animation played at least once
                    _alreadyPlayed[_taskQInd] = true;
                }
            }
            if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 60f, 100, 25), "Replay")) {

                UpdateParameters();
                ResetComponents(_agentLeft);
                ResetComponents(_agentRight);

                _persMapper.MapPersonalityToMotion(_agentLeft.GetComponent<PersonalityComponent>());
                _persMapper.MapPersonalityToMotion(_agentRight.GetComponent<PersonalityComponent>());

                Play(_agentLeft);
                Play(_agentRight);
            }

            style.fontSize = 19;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.black;
            GUI.Label(new Rect(10, Screen.height - 165f, 1000, 290), _questionNoStr, style);

            
            style.fontSize = 15;
            style.fontStyle = FontStyle.Normal;
            GUILayout.BeginHorizontal();
            GUI.Label(new Rect(10, Screen.height - 135f, 1000, 290), "If the LEFT character has a NEUTRAL personality, how much do you agree that the RIGHT character is ", style);
            //style.fontStyle = FontStyle.BoldAndItalic;
            GUI.Label(new Rect(700, Screen.height - 135f, 1000, 290), _personalityQuestionStr[_taskQInd].ToUpper(), style);

            //GUI.Label( new Rect(165, Screen.height - 130f, 1000, 290), "MORE \"open to new experiences & complex \", and LESS \"conventional & uncreative\"", style);            
            GUILayout.EndHorizontal();
            style.fontStyle = FontStyle.Normal;
            

            GUILayout.BeginArea(new Rect(50, Screen.height - 110f, 1000, 290));
            _selectPersonality = GUILayout.SelectionGrid(_selectPersonality, selectStr, 7, GUILayout.Width(990));
            GUILayout.EndArea();




            GUI.color = Color.white;

#if !DEBUGMODE
            if (_agentLeft.AnimationOver() && _agentRight.AnimationOver() || _alreadyPlayed[_taskQInd])
#endif
            {

                if (GUI.Button(new Rect(Screen.width/2f - 110, Screen.height - 45f, 200, 25), "Submit Answer")) {


                    if (_selectPersonality == UNSELECTED) { 
                        _isSubmittedStr[_taskQInd] = "  Please select an answer first.";
                    }

                    else { //ANSWER SELECTED AND SUBMIT PRESSED
                        if (_isSubmitted[_taskQInd] == false) { //NOT ALREADY SUBMITTED
                            _isSubmitted[_taskQInd] = true;
                            _submittedCnt++;
                        }

                        //change signs
                        _answerPersonality = _selectPersonality - 3;
                          
                        

#if WEBMODE
                        this.StartCoroutine(PostValues(_taskQInd+1, _answerPersonality));                        
#endif
                     
                        //Move on to the next question
                        GetNextQuestion();                        
                        StartCoroutine(DrawWhiteScreen());
                        _answerPersonality = UNSELECTED;
                        _selectPersonality = UNSELECTED;
                    }

                }

            }
        }

	    if (_submittedCnt >= _qCnt) {
            style.fontSize = 20;
            
            GUI.Label(new Rect(50, Screen.height - 80f, 800, 100), _isSubmittedStr[_taskQInd], style);
              
        }
        else
            GUI.Label(new Rect(Screen.width / 2f - 110, Screen.height - 35f, 500, 25), _isSubmittedStr[_taskQInd], style);


            _questionNoStr = "Question " + (_taskQInd + 1) + " of " + (_qCnt) + ":";

        

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


    
   
    // remember to use StartCoroutine when calling this function!   
    //qInd = actualqInd to store in the db
    IEnumerator PostValues( int qInd, int answerPersonality) {
        string resultURL = "https://fling.seas.upenn.edu/~fundad/cgi-bin/mturk4/putComparisonData.php";
               
     // Create a form object for sending high score data to the server
        var form = new WWWForm();        
        form.AddField( "userId", UserInfo.UserId);    
        
        form.AddField( "hit", UserInfo.Hit.ToString());
        form.AddField("qInd", qInd.ToString());  //starts from 1      
        form.AddField( "answer", answerPersonality.ToString());
        
        
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

