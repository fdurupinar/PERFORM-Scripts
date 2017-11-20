//#define WEBMODE
//#define TEXTMODE
using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class MappingGUI : GUIController {
    RaycastHit _hit;
    [SerializeField]
    public static bool DisableLMA = false;

    private static float _goalThreshold;
    int _agentSelInd;
    

    public GUISkin ButtonSkin;

    private string nationality_s;
    private string profession_s;
    private float n_p_weight= 0.5f;
    private float prev_n_p_weight = 0.5f;


    private float[] _personality = new float[5]; //-1 0 1 
    private string[] _personalityName = {"O", "C", "E", "A", "N"};

    private int _persMin = -1;
    private int _persMax = 1;
     
    
    private  DriveParams[] _driveParams = new DriveParams[32];

    private PersonalityMapper _persMapper;

    private AnimationInfo[] _agentScripts;


    private DropDownRect _dropDownRectAnimNames;
    private DropDownRect _dropDownRectAgents;

    private DropDownRect _dropDownRectNationality;
    private DropDownRect _dropDownRectProfession;

    bool _firstRun = true;


    private bool _lockHand = false;
    

    private void Start() {


        _agentScripts = transform.GetComponentsInChildren<AnimationInfo>();


        
        _dropDownRectAgents = new DropDownRect(new Rect(115, 20, 90, 300));
        _dropDownRectAnimNames = new DropDownRect(new Rect(210, 20, 90, 300));

        _dropDownRectNationality = new DropDownRect(new Rect(115, 60, 90, 300));
        _dropDownRectProfession = new DropDownRect(new Rect(210, 60, 90, 300));

        for (int i = 0; i < 32; i++) {
            _driveParams[i] = new DriveParams();
        }



#if !WEBMODE
        for (int i = 0; i < 32; i++)
            _driveParams[i].ReadValuesDrives(i);


#elif WEBMODE
        for (int i = 0; i < 32; i++ ) {
             this.StartCoroutine(_driveParams[i].GetValuesDrives(i, "funda"));
        }

#endif




        _firstRun = true;


        

            
        _agentSelInd = 0;
  


        _persMapper = new PersonalityMapper();


         

        foreach (AnimationInfo t in _agentScripts)
            Reset(t);

        AgentText = new GUIText[_agentScripts.Length];
        
        //   FormatData("motionEffortCoefs.txt");

        MathDefs.SetSeed(30);

    }


    
    //void FormatData(string fileName) {


    //    string[] content = File.ReadAllLines(fileName);

    //    using (StreamWriter sw = new StreamWriter("Parsed" + fileName)) {
    //        sw.WriteLine(content[0]);
    //        for (int i = 1; i < content.Length; i++) {
    //            String[] tokens = content[i].Split('\t');

    //            sw.Write("new float[] {");
    //            for (int j = 0; j < tokens.Length-1; j++) {
    //                string t = tokens[j];
    //                sw.Write(String.Format("{0:0.000f}, ", float.Parse(tokens[j])));
    //            }

    //            sw.WriteLine(String.Format("{0:0.000f} }}, ", float.Parse(tokens[tokens.Length - 1])));
    //        }
    //    }

    //}
   
    

    void Update() {
        


     

        if (_firstRun && _driveParams[31].DrivesAchieved) {
            _persMapper.ComputeMotionEffortCoefs(_driveParams);
           
            _firstRun = false; //no need to compute again

        }           


        else if (Input.GetKeyDown("0"))                 
            Time.timeScale = 0;                    
        else if (Input.GetKeyDown("1"))
            Time.timeScale = 1f;
        else if (Input.GetKeyDown("2"))
            Time.timeScale = 2f;
        else if (Input.GetKeyDown("3"))
            Time.timeScale = 3f;
        else if (Input.GetKeyDown("4"))
            Time.timeScale = 4f;
#if !WEBMODE
        else if (Input.GetKeyDown("s")) {
            GameObject.Find("Camera").GetComponent<Screenshot>().IsRunning =
                !GameObject.Find("Camera").GetComponent<Screenshot>().IsRunning;

            if (GameObject.Find("Camera").GetComponent<Screenshot>().IsRunning) {
                Time.timeScale = 0.5f;
                RecordPersonalities();
            }
            else
                Time.timeScale = 1f;

        }
        else if (Input.GetKeyDown("l"))
            LoadPersonalities();

            //Show keypoints
        else if (Input.GetKeyDown("g")) {
            Debug.Log(_agentScripts[_agentSelInd].CharacterName + " " +
                      _agentScripts[_agentSelInd].CurrKeyInd);

        }
#endif



        if (Input.GetMouseButtonDown(0)) {


            if (Camera.main != null) {


                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out _hit);
                if (_hit.collider) {
                    for(int i = 0; i <_agentScripts.Length; i++){
                        if (_hit.collider.transform.parent && _agentScripts[i].gameObject.Equals(_hit.collider.transform.parent.gameObject)) {
                            _agentSelInd = i;
                            
                            break;
                        }

                    }
                }

            }


        }


    }



    private void OnGUI() {
        GUI.skin = ButtonSkin;


        bool disableBefore = DisableLMA;
        
        DisableLMA = GUILayout.Toggle(DisableLMA, "Disable LMA");

        if (disableBefore != DisableLMA) {    //a change has been made in the toggle         
            foreach (AnimationInfo a in _agentScripts)
                a.DisableLMA = DisableLMA;
        }

        


        GUI.color = Color.white;
        GUILayout.BeginArea(_dropDownRectAgents.DdRect);
        GUILayout.Label("Character");
         _dropDownRectAgents.DdList = _agentScripts.Select(s => s.CharacterName).ToList();
         _agentSelInd = _dropDownRectAgents.ShowDropDownRect();
        
        GUILayout.EndArea();


        GUILayout.BeginArea(_dropDownRectAnimNames.DdRect);
        GUILayout.Label("Animation");
        _dropDownRectAnimNames.DdList = _agentScripts[_agentSelInd].AnimNames.ToList();        
        int ind = _dropDownRectAnimNames.ShowDropDownRect();
        _agentScripts[_agentSelInd].AnimName = _agentScripts[_agentSelInd].AnimNames[ind];
        GUILayout.EndArea();

        GUILayout.BeginArea(_dropDownRectNationality.DdRect);
        GUILayout.Label("Nationality");
        _dropDownRectNationality.DdList = _agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().listNationality();
        int ind_nationality = _dropDownRectNationality.ShowDropDownRect();
        nationality_s = _dropDownRectNationality.DdList[ind_nationality];
        _agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().Culture = nationality_s;
        GUILayout.EndArea();

        GUILayout.BeginArea(_dropDownRectProfession.DdRect);
        GUILayout.Label("Profession");
        _dropDownRectProfession.DdList = _agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().listProfession();
        int ind_profession = _dropDownRectProfession.ShowDropDownRect();
        profession_s = _dropDownRectProfession.DdList[ind_profession];
        _agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().Profession = profession_s;
        GUILayout.EndArea();



        GUILayout.BeginArea(new Rect(5, 20, 105, Screen.height));

        
        GUILayout.Label("Personality");
        GUI.color = Color.white;    

        GUILayout.Label("");
        for (int i = 0; i < 5; i++) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("" + _persMin);
            GUILayout.Label("" + _personalityName[i]);
            GUILayout.Label("" + _persMax);            
            GUILayout.EndHorizontal();

            _personality[i] = _agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().Personality[i];
            GUI.color = Color.white;



            GUI.backgroundColor = Color.white;
            _personality[i] = GUILayout.HorizontalSlider(_personality[i], _persMin, _persMax).Truncate(1);
            
            //Assign agent personality
            _agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().Personality[i] = _personality[i];
                

            string[] ocean = { "O", "C", "E", "A", "N" };
            for (int j = 0; j < 5; j++)
                if (_agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().Personality[j]== -1)
                    ocean[j] += "-";
                else if (_agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().Personality[j] == 1)
                    ocean[j] += "+";
                else
                    ocean[j] = "";

            
            /*
            _agentSel.AgentText.text = string.Format(ocean[0] + "  " + ocean[1] + "  " + ocean[2] + "  " + ocean[3] + "  " + ocean[4]);


            _agentSel.AgentText.text += string.Format("\nS: {0:0.00} W: {1:0.00} T: {2:0.00} F: {3:0.00}", _agentSel.Effort[0], _agentSel.Effort[1], _agentSel.Effort[2], _agentSel.Effort[3]);
        */
        }


        GUI.color = Color.white;


        _lockHand = GUILayout.Toggle(_lockHand, "Lock hand");
        foreach (AnimationInfo a in _agentScripts)
            a.GetComponent<IKAnimator>().LockHand = _lockHand;


        GUILayout.BeginHorizontal();
        GUILayout.Label("" + 0);
        GUILayout.Label("W");
        GUILayout.Label("" + 1);
        GUILayout.EndHorizontal();
        GUI.color = Color.white;
        GUI.backgroundColor = Color.white;
        n_p_weight= GUILayout.HorizontalSlider(n_p_weight, 0, 1).Truncate(1);
        if (n_p_weight != prev_n_p_weight)
        {
            prev_n_p_weight = n_p_weight;
            foreach (AnimationInfo a in _agentScripts)
                a.GetComponent<PersonalityComponent>().BlendWeight = n_p_weight;
        }
        if (_driveParams[31].DrivesAchieved) {
            if (GUILayout.Button("Reset scene")) {
                Application.LoadLevel(Application.loadedLevel);     

            }
            if (GUILayout.Button("Randomize")) {
                    foreach (AnimationInfo a in _agentScripts) 
                        for(int i = 0; i < 5; i++) 
                            a.GetComponent<PersonalityComponent>().Personality[i] = MathDefs.GetRandomNumber(-1f, 1f);                             
                                                                  
            }
            if (GUILayout.Button("Reset")) {
                foreach (AnimationInfo a in _agentScripts)
                    for (int i = 0; i < 5; i++)
                        a.GetComponent<PersonalityComponent>().Personality[i] = 0;                            

            }
            if (GUILayout.Button("Assign All")) {
                foreach (AnimationInfo a in _agentScripts)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        a.GetComponent<PersonalityComponent>().Personality[i] = _personality[i];
                    }
                    //will cause small diff in ocean values if I do this, for unknown reasons: so I comment this out
                    //a.GetComponent<PersonalityComponent>().Culture = nationality_s;
                    //a.GetComponent<PersonalityComponent>().Profession = profession_s;
                }

            }
            if (GUILayout.Button("Assign All Variation")) {
                foreach (AnimationInfo a in _agentScripts)
                    for (int i = 0; i < 5; i++) {
                        a.GetComponent<PersonalityComponent>().Personality[i] = _personality[i] +
                                                                                MathDefs.GetRandomNumber(-0.2f, 0.2f);
                        if (a.GetComponent<PersonalityComponent>().Personality[i] > 1)
                            a.GetComponent<PersonalityComponent>().Personality[i] = 1;
                        else if (a.GetComponent<PersonalityComponent>().Personality[i] <-1)
                            a.GetComponent<PersonalityComponent>().Personality[i] = -1;
                    }
            }
            if (GUILayout.Button("Play")) {


                int agentInd = 0;

               

                foreach (AnimationInfo t in _agentScripts) {

                    ResetComponents(t);

                    
                    _persMapper.MapPersonalityToMotion(t.GetComponent<PersonalityComponent>()); //calls initkeypoints, which stops the animation
                    
                    Play(t);

                    GUI.color = Color.white;
                    agentInd++;
                }
                _persMapper.MapAnimSpeeds(_agentScripts, 0.9f, 1.3f); //map them to the range


            }

            if (GUILayout.Button("Record")) {
                GameObject.Find("Camera").GetComponent<Screenshot>().IsRunning = true;
                
                Time.timeScale = 0.25f;                
                
                int agentInd = 0;
                foreach (AnimationInfo t in _agentScripts) {

                    ResetComponents(t);

                    
                    _persMapper.MapPersonalityToMotion(t.GetComponent<PersonalityComponent>()); //calls initkeypoints, which stops the animation
                    Play(t);

                    GUI.color = Color.white;
                    agentInd++;
                }


                _persMapper.MapAnimSpeeds(_agentScripts, 0.9f, 1.3f); //map them to the range
            }
            
            
              

            //we need to update after play because playanim resets torso parameters for speed etc. when animinfo is reset

        }

           GUI.color = Color.yellow;
        GUILayout.Label("S:" + _agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().Effort[0] + " W:" + _agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().Effort[1] + " T:" +
            _agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().Effort[2] + " F:" + _agentScripts[_agentSelInd].GetComponent<PersonalityComponent>().Effort[3]);
        
        GUILayout.EndArea();
        
    }


    void RecordPersonalities() {
         using (StreamWriter sw = new StreamWriter("personalities.txt")) {
             foreach(AnimationInfo a in _agentScripts) {
                 sw.WriteLine(a.CharacterName + "\t" + a.GetComponent<PersonalityComponent>().Personality[0] + "\t" +
                              a.GetComponent<PersonalityComponent>().Personality[1] + "\t" + a.GetComponent<PersonalityComponent>().Personality[2] + "\t" + a.GetComponent<PersonalityComponent>().Personality[3] + "\t" +
                              a.GetComponent<PersonalityComponent>().Personality[4]);
             }
         }        
    }
    void LoadPersonalities() {
        using (StreamReader sr = new StreamReader("personalities.txt")) {
            foreach (AnimationInfo a in _agentScripts) {
                string s = sr.ReadLine();
                String[] p = s.Split('\t');
                for (int i = 0; i < 5; i++)
                {
                    a.GetComponent<PersonalityComponent>().Personality[i] = float.Parse(p[i + 1]);
                   // Console.WriteLine(p[i + 1]);
                }
                    
            }
        }
        
    }

    

}

