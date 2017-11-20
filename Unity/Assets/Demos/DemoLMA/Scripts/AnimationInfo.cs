#define EDITORMODE
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Meta.Numerics.Statistics;


#if EDITORMODE
using UnityEditor;
#endif

[System.Serializable]
public class KeyInfo  {
	public int FrameNo;
    public float Time;
    public bool IsGoal; //either via or goal    
    public bool IsCurve; //path determining keys
    public Vector3 RootForward;
	public Vector3[] EePos = new Vector3[5]; 	  			//end effector key position	for arms, feet and hips
    public Vector3[] EePosOrig  = new Vector3[5]; 	 //original end effector key position before modified by armshape    
    public Vector3[] ShoulderPos = new Vector3[2]; 	  			//shoulder key position	
    public Vector3[] ShoulderPosOrig = new Vector3[2]; 	  			//shoulder key position	
    
    
    public Vector3[] EeVel = new Vector3[5]; //velocity of the end effector	


    public Vector3[] BodyVel;
    
    public List<Vector3> BodyPos;
    public List<Quaternion> BodyRot;

    public List<Vector3> BodyLocalPos;
    public List<Quaternion> BodyLocalRot;


    public float[] TimeUpdated = new float[2];


}
public enum EEType {
    LeftHand,
    RightHand,
    LeftFoot,
    RightFoot,
    Root   
}

[System.Serializable]
public class MyKeyframe {
   public float time { get; set; }
}
 

public class AnimationInfo : MonoBehaviour {

    public int StartKey, EndKey; //if animation is divided into parts, start and end keys define the boundaries of the animation sequence
    public bool DisableLMA;
    public string CharacterName;
    //Timing parameters
    public float T0 = 0f; //EMOTE values
    public float T1 = 1f; //EMOTE values
    public float Texp = 1f;  //Default time exponent
    public float Ti  = 0.5f; 	 //Inflection point where the movement changes from accelerating to decelerating
    public float V0; 	 //Start velocity
    public float V1; 	 //End velocity
    public float Tval;  //Tension	
    
    public float Continuity = 0f;
    private float _vi; //inflection velocity --cannot be updated
    public int CurrKeyInd;
    public float LocalT;
    public float GlobalT;
    
    public int ExtraGoal; //whether there is an extra goal in the middle
    public int UseCurveKeys;

    
    //Shape parameters
    public float Hor = 0f; //[-1 1]
    public float Ver = 0f;
    public float Sag = 0f;
    public const float Abratio =  2.5f;//1.1f; //shoulde be >1
    public const float MaxdTheta = Mathf.PI / 6.0f; //Mathf.PI/20.0f;  //values in Liwei Zhao's thesis
    
	
    

    List<float> _sArr = new List<float>();
    List<float> _velArr = new List<float>();
    List<float> _tppArr = new List<float>();

	public float Fps;
	public float AnimLength;
	public KeyInfo[] Keys;
  



    public TCBSpline SIKeyTime; //Keyframe to time interpolator
    public TCBSpline[] SIee = new TCBSpline[2]; //End effector position interpolator

    public TCBSpline[] SIBody; //End effector position interpolator
    public TCBSpline[] SIBodyLocal; //End effector position interpolator

    public string[] AnimNames;
	public string AnimName ="";
	public int FrameCnt;

    public bool IsContinuous; // trigger-based vs continuous running of animations

	[SerializeField]
    
    public List<Transform> BodyChainTorso;

    private TorsoController _torso;

    //because editor does not work here we cannot get keyframe information
	public List <int> GoalKeys = new List<int>() ;  //keeps the keyframe number (not actual index)  we need to include the start and end keyframes {0, 3, 5, 8};
    public List<float> GoalSpeeds = new List<float>();  
    public int[] CurveKeys;  //determine the arm path curvature
    public float[] MyKeyTimes;
    public KeyframeExtractor KeyExtractor;

    public float GoalThreshold = 0.1f;

    public int MaxLoopPlayCount;
    public int LoopPlayCount = 0;
    private float _prevNormalizedT;
    public Vector3 DeltaPos; //position difference between first and last frames for loop animations
    private Vector3 _pos0; 

    public float NormalizedTime;
    public float NormalizedT {
        get {
            if (GetComponent<Animation>().isPlaying) //open in demodrives close in democomparison
                return (GetComponent<Animation>()[AnimName].normalizedTime - Mathf.FloorToInt(GetComponent<Animation>()[AnimName].normalizedTime)); //speed * _animInfo.DefaultAnimSpeed;
            return 0;
        }
    }

    public float Speed;
    public float AnimSpeed {
        get {
            if (GetComponent<Animation>().isPlaying) //open in demodrives close in democomparison
                return GetComponent<Animation>()[AnimName].speed;//speed * _animInfo.DefaultAnimSpeed;
            return 0;
        }
        set {
            //if (animation.isPlaying)
                GetComponent<Animation>()[AnimName].speed = value;
        }
    }


    public float Tp {//Normalized time between goal keypoints	
        get {
            if (Curr <= PrevGoal)
                return 0;
            if (NextGoal == PrevGoal)
                return 1;
            return (float)(Curr - PrevGoal) / (NextGoal - PrevGoal);

        }

    }

    //current animation time
    public float Curr {
        get {
            if (IsContinuous && GetComponent<Animation>()[AnimName].time > GetComponent<Animation>()[AnimName].length)
                GetComponent<Animation>()[AnimName].time = 0f;

            return GetComponent<Animation>()[AnimName].time;            
        }
    }

    public int PrevGoalKeyInd {
        get {
            if (Curr >= Keys[GoalKeys[GoalKeys.Count - 1]].Time) {
                if (IsContinuous)
                    return 0;
                else
                    return GoalKeys.Count - 1;
                    //return GoalKeys[GoalKeys.Length - 1];

            }
            for (int i = 0; i < GoalKeys.Count - 1; i++) {
                if (Curr >= Keys[GoalKeys[i]].Time && Curr < Keys[GoalKeys[i + 1]].Time)
                    return i;
                    //return GoalKeys[i];
            }


            throw new System.Exception("Unable to compute previous goal index " + Curr);
        }

    }


     
    //Previous goal keyframe's time
    public float PrevGoal {
        get {
            //return Keys[PrevGoalKeyInd].Time;
            return Keys[GoalKeys[PrevGoalKeyInd]].Time;
        }
    }


    //Next keyframe's frame number
    public float NextGoal {
        get {
            int nextGoalKeyInd = PrevGoalKeyInd + 1;
            if (nextGoalKeyInd > GoalKeys.Count - 1)
                nextGoalKeyInd = GoalKeys.Count - 1;

            return Keys[GoalKeys[nextGoalKeyInd]].Time;

        }
    }
    //To test if animation is playing with the wrapmode = clampforever
    public bool AnimationOver() {
        return (Curr >= AnimLength);
    }
    //To test if animation is playing with the wrapmode = clampforever
    public bool AnimationSoonOver() {
        return (Curr >= AnimLength/2f);
    }




    void Awake() {
        _sArr = new List<float>();
        _velArr = new List<float>();
        _tppArr = new List<float>();

        _torso = GetComponent<TorsoController>();


        CharacterName = CharacterName.ToUpper();


        AnimNames = new string[GetComponent<Animation>().GetClipCount()];

        int c = 0;
        foreach (AnimationState s in GetComponent<Animation>())
            AnimNames[c++] = s.name;

        AnimName = AnimNames[0];

        IsContinuous = true; //funda
        MaxLoopPlayCount = 50; //for protest
 
    }

    public void ResetParameters() {

        V0 = V1 = 0f;
        Ti = 0.5f;
        Texp = 1.0f;
        Tval = 0f;
        Continuity = 0f;
        T0 = 0f;
        T1 = 1f;

    }
    
    public void Reset(string aName) {

        
        AnimName = aName;
        
        GetComponent<Animation>().clip = GetComponent<Animation>()[AnimName].clip;


		Fps = GetComponent<Animation>()[AnimName].clip.frameRate;
        AnimLength = GetComponent<Animation>()[AnimName].clip.length;		        
		FrameCnt = Mathf.CeilToInt(Fps * AnimLength);


        LoopPlayCount = 0;
        _prevNormalizedT = 0;
        AnimSpeed = 1f;

        _velArr = new List<float>();
        _tppArr = new List<float>();

        GetComponent<IKAnimator>().DisableIK(); //so that it can sample correctly

        KeyExtractor = new KeyframeExtractor();
        InitKeyPoints(); //should always update interpolators    
        InitInterpolators(Tval, Continuity, 0);

        GetComponent<IKAnimator>().EnableIK(); //enable it back

        LocalT = 0f;
        GlobalT = 0f;


        //StartKey = 0;
        // EndKey = FrameCnt - 1;

    }

    public void StopAnim() {
        GetComponent<Animation>().Stop();        
    }

    
    //goals keep the key index, not frame index
    void AssignGoalKeys(int ind) {

        //Compute end effector velocity
        for (int i = 1; i < Keys.Length-1; i++) {            
                Keys[i].EeVel[ind] = (Keys[i + 1].EePos[ind] - Keys[i - 1].EePos[ind]) / (Keys[i + 1].Time - Keys[i - 1].Time);
        }
        int goalInd = 0;
      

        Keys[0].IsGoal = true;

        //find local minima
        for (int i = 1; i < Keys.Length - 1; i++) {
            
              if (Keys[i].EeVel[ind].magnitude < Keys[i-1].EeVel[ind].magnitude && Keys[i].EeVel[ind].magnitude <= Keys[i+1].EeVel[ind].magnitude ){
                 // && Keys[i].EeVel[ind].magnitude <=1f  && Keys[i].Time - Keys[GoalKeys[GoalKeys.Count - 1]].Time > 0.5f) {
                  Keys[i].IsGoal = true;
                  GoalKeys.Add(i);
                  GoalSpeeds.Add(Keys[i].EeVel[ind].magnitude);
                  
                  goalInd++;
              }
              else
                  Keys[i].IsGoal = false;
        }
        /*
        for (int i = 1; i < Keys.Length - 1; i++) {

            Debug.Log((Keys[i].EeVel[ind].magnitude));
            if (Keys[i].EeVel[ind].magnitude < GoalThreshold) {    //velocity small enough to be considered 0  
                if (GoalKeys.Count > 0 && Keys[i].FrameNo - Keys[GoalKeys[GoalKeys.Count - 1]].FrameNo > 3) { //if goals are far enough                   
                    Keys[i].IsGoal = true;
                    GoalKeys.Add(i);
                    goalInd++;
                }
            }
            else
                Keys[i].IsGoal = false;
        }
         */

        GoalSpeeds.Add(0);
        GoalKeys.Add(Keys.Length-1);
        Keys[Keys.Length - 1].IsGoal = true;
    }
    
	public void InitKeyPoints() {

#if !EDITORMODE
        Keyframe[] frames = new Keyframe[FrameCnt]; //all frames
        for (int i = 0; i < frames.Length; i++) {
            frames[i] = new Keyframe();
            frames[i].time = AnimLength * ((float)i / (FrameCnt - 1));
            
        }
        animation.Play(AnimName);
	
      /*  Keys = new KeyInfo[frames.Length];

	    KeyExtractor.Reset();
        foreach (Keyframe f in frames) {
            
            animation[AnimName].enabled = true;
            animation[AnimName].time = f.time;
            
            animation.Sample();
            List<Transform> upperBody = _torso.BodyChainToArray(_torso.Spine); //needs to be updated for each keyframe
            List<Vector3> posList = new List<Vector3>();
            posList = _torso.BodyPosArr(upperBody);
            KeyExtractor.ComputeBoundingBoxVolume(posList);
        }
	    List <int> keyInds = KeyExtractor.ExtractKeys();
	    Debug.Log(keyInds.Count);
       */
        Keys = new KeyInfo[frames.Length];
        for (int i = 0; i < frames.Length; i++) {

            Keys[i] = new KeyInfo();

            animation[AnimName].enabled = true;

         

            animation[AnimName].time = frames[i].time;
            Keys[i].Time = animation[AnimName].time;

           // int frameInd = keyInds[i];

           // Keys[i].FrameNo = frameInd;

         //   animation[AnimName].time = frames[frameInd].time;

           
            animation[AnimName].enabled = true;

            
            animation.Sample();
         

            Keys[i].IsCurve = false;
            //body chain and transformation arrays for the specific animation

            BodyChainTorso = _torso.BodyChainToArray(_torso.Root); //needs to be updated for each keyframe
            Keys[i].BodyPos = _torso.BodyPosArr(BodyChainTorso);
            Keys[i].BodyRot = _torso.BodyRotArr(BodyChainTorso);
            Keys[i].BodyLocalPos = _torso.BodyLocalPosArr(BodyChainTorso);
            Keys[i].BodyLocalRot = _torso.BodyLocalRotArr(BodyChainTorso);

            Keys[i].BodyVel = new Vector3[BodyChainTorso.Count];

            //Hands
            for (int ind = 0; ind < 2; ind++) {
                Keys[i].ShoulderPos[ind] = Keys[i].ShoulderPosOrig[ind] = _torso.Shoulder[ind].position;
                Keys[i].EePos[ind] = Keys[i].EePosOrig[ind] = _torso.Wrist[ind].position;

            }
            //Feet
            for (int ind = 2; ind < 4; ind++) {
                Keys[i].EePos[ind] = Keys[i].EePosOrig[ind] = _torso.Foot[ind - 2].position;
            }

            Keys[i].EePos[4] = Keys[i].EePosOrig[4] = _torso.Root.position;
            Keys[i].RootForward = _torso.Root.forward;
        }

        if (AnimName.ToUpper().Contains("SALSA") || AnimName.ToUpper().Contains("BALLET") || AnimName.ToUpper().Contains("CUSTOMER4") || AnimName.ToUpper().Contains("WALK"))
            AssignGoalKeys((int)EEType.RightFoot);
        else
            AssignGoalKeys((int)EEType.RightHand);

        animation.Stop(AnimName);

            
        


#else

	    AnimationCurve xCurve;

        if (AnimName.ToUpper().Contains("SALSA") || AnimName.ToUpper().Contains("WALK"))
            xCurve = AnimationUtility.GetEditorCurve(GetComponent<Animation>()[AnimName].clip, _torso.BodyPath[(int)BodyPart.FootR], typeof(Transform), "m_LocalRotation.x");
       else
            xCurve = AnimationUtility.GetEditorCurve(GetComponent<Animation>()[AnimName].clip, _torso.BodyPath[(int)BodyPart.WristR], typeof(Transform), "m_LocalRotation.x");

	    Keyframe[] frames = xCurve.keys;


   
        Keys = new KeyInfo[frames.Length];
        for (int i = 0; i < frames.Length; i++) {
            Keys[i] = new KeyInfo();

            GetComponent<Animation>()[AnimName].enabled = true;


            GetComponent<Animation>()[AnimName].time = frames[i].time;
            Keys[i].Time = GetComponent<Animation>()[AnimName].time;

        	
        

            GetComponent<Animation>().Sample();

          
            Keys[i].IsCurve = false;


            //body chain and transformation arrays for the specific animation

            BodyChainTorso = _torso.BodyChainToArray(_torso.Root); //needs to be updated for each keyframe
            Keys[i].BodyPos = _torso.BodyPosArr(BodyChainTorso);
            Keys[i].BodyRot = _torso.BodyRotArr(BodyChainTorso);
            Keys[i].BodyLocalPos = _torso.BodyLocalPosArr(BodyChainTorso);
            Keys[i].BodyLocalRot = _torso.BodyLocalRotArr(BodyChainTorso);

            Keys[i].BodyVel = new Vector3[BodyChainTorso.Count];

            //Hands
            for (int ind = 0; ind < 2; ind++) {
                Keys[i].ShoulderPos[ind] = Keys[i].ShoulderPosOrig[ind] = _torso.Shoulder[ind].position;
                Keys[i].EePos[ind] = Keys[i].EePosOrig[ind] = _torso.Wrist[ind].position;
               
            }
            //Feet
            for (int ind = 2; ind < 4; ind++) {
                Keys[i].EePos[ind] = Keys[i].EePosOrig[ind] = _torso.Foot[ind - 2].position;
            }

            Keys[i].EePos[4] = Keys[i].EePosOrig[4] = _torso.Root.position;
            Keys[i].RootForward = _torso.Root.forward;


           
            if (i == 0) {
                _pos0 = _torso.Root.position;
            }
                else if (i == frames.Length - 1 ) { //when loop frame is added 
                DeltaPos = _torso.Root.position - _pos0;
                
                
            }

        }

        GoalKeys.Clear();
        GoalKeys.Add(0);

        GoalSpeeds.Clear();
        GoalSpeeds.Add(0);

        if(AnimName.ToUpper().Contains("WALK")) {
            AssignGoalKeys((int)EEType.RightFoot);
          //  AssignGoalKeys((int)EEType.LeftFoot);
        }
        else if (AnimName.ToUpper().Contains("SALSA") || AnimName.ToUpper().Contains("BALLET") || AnimName.ToUpper().Contains("CUSTOMER4"))
            AssignGoalKeys((int)EEType.RightFoot);
        else
            AssignGoalKeys((int)EEType.RightHand);

        GetComponent<Animation>().Stop(AnimName);

#endif

        /*
        GoalKeys.Clear();
        GoalKeys.Add(0);
        GoalKeys.Add(Keys.Length - 1);
        */

       // int goalKeyInd = 0;
       // int curveKeyInd = 0;

	    /*
#if EDITORMODE	  

        //forearm keys are fewer in number, looks better with  EMOTE
        //AnimationCurve xCurve = AnimationUtility.GetEditorCurve(animation[AnimName].clip, torso.BodyPath[(int)BodyPart.ElbowR], typeof(Transform), "m_LocalRotation.x");
        AnimationCurve xCurve = AnimationUtility.GetEditorCurve(animation[AnimName].clip, _torso.BodyPath[(int)BodyPart.WristR], typeof(Transform), "m_LocalRotation.x");
        //AnimationCurve xCurve = AnimationUtility.GetEditorCurve(animation[AnimName].clip, _torso.BodyPath[(int)BodyPart.PelvisR], typeof(Transform), "m_LocalRotation.x");

        Keyframe[] frames = xCurve.keys;
        
        using (StreamWriter sw = new StreamWriter("keyframes_" + AnimName + ".txt")) {
            sw.WriteLine("MyKeyTimes = new float" + "[" + frames.Length + "];");
            foreach (Keyframe kf in frames) {
                sw.WriteLine("MyKeyTimes[i++]  = " + kf.time + "f; ");                
            }
        }
        
	    
        
#elif !EDITORMODE        
  		AssignKeyFrames();

        Keyframe[] frames = new Keyframe[MyKeyTimes.Length];
        for (int i = 0; i < MyKeyTimes.Length; i++) {
            frames[i] = new Keyframe();           
            frames[i].time = MyKeyTimes[i];           
        }
      
#endif
         


        AssignGoalKeys(frames);

        animation.Play(AnimName);
        int goalKeyInd = 0;
        int curveKeyInd = 0;
	
		Keys = new KeyInfo[frames.Length ];
		for(int i = 0; i < frames.Length; i++) {
            animation[AnimName].enabled = true;
  
        animation[AnimName].time = frames[i].time;


        animation.Sample();	
		Keys[i] = new KeyInfo();			
        Keys[i].Time = animation[AnimName].time;

        if(i == GoalKeys[goalKeyInd]) {            
            Keys[i].IsGoal = true;
            goalKeyInd++;
        }
        else if(i < GoalKeys[goalKeyInd]) 
            Keys[i].IsGoal = false;

        if (i == CurveKeys[curveKeyInd]) {
            Keys[i].IsCurve = true;
            curveKeyInd++;
        }
        else
            Keys[i].IsCurve = false;

        if (Keys[i].FrameNo >= FrameCnt) {                    
            Keys[i].FrameNo = FrameCnt - 1;
        }


            
        //body chain and transformation arrays for the specific animation
        
        
		BodyChainTorso = _torso.BodyChainToArray(_torso.Root); //needs to be updated for each keyframe
        Keys[i].BodyPos = _torso.BodyPosArr(BodyChainTorso);
        Keys[i].BodyRot = _torso.BodyRotArr(BodyChainTorso);
        Keys[i].BodyLocalPos = _torso.BodyLocalPosArr(BodyChainTorso);
        Keys[i].BodyLocalRot = _torso.BodyLocalRotArr(BodyChainTorso);

		Keys[i].BodyVel = new Vector3[BodyChainTorso.Count];
		
            for(int ind = 0; ind < 2; ind++) {
                Keys[i].ShoulderPos[ind] = Keys[i].ShoulderPosOrig[ind] = _torso.Shoulder[ind].position;
                Keys[i].EePos[ind] = Keys[i].EePosOrig[ind] = _torso.Wrist[ind].position;
                Keys[i].RootForward = _torso.Root.forward;

            }
		    
        }
			
	
        animation.Stop(AnimName);

        */

	}
    
    
    
    public Vector3 ComputeInterpolatedBodyPos(Transform bodyPart, int keyInd, float lt) {

        int chainInd = _torso.BodyChain.IndexOf(bodyPart);

        
        if (keyInd + 1 > Keys.Length - 1)
            return Keys[keyInd].BodyPos[chainInd];

        return SIBody[chainInd].GetInterpolatedSplinePoint(lt, keyInd);

        //return Vector3.Lerp(Keys[keyInd].BodyPos[chainInd], Keys[keyInd + 1].BodyPos[chainInd], lt);

    }


    public Quaternion ComputeInterpolatedBodyRot(Transform bodyPart, int keyInd, float lt) {

        int chainInd = BodyChainTorso.IndexOf(bodyPart);

        if (keyInd + 1 > Keys.Length - 1)
            return Keys[keyInd].BodyRot[chainInd];

        return Quaternion.Slerp(Keys[keyInd].BodyRot[chainInd], Keys[keyInd + 1].BodyRot[chainInd], lt);

    }

    public Vector3 ComputeInterpolatedBodyLocalPos(Transform bodyPart, int keyInd, float lt) {

        int chainInd = BodyChainTorso.IndexOf(bodyPart);
        

        if (keyInd + 1 > Keys.Length - 1)
            return Keys[keyInd].BodyLocalPos[chainInd];

        return SIBodyLocal[chainInd].GetInterpolatedSplinePoint(lt, keyInd);
       // return Vector3.Lerp(Keys[keyInd].BodyLocalPos[chainInd], Keys[keyInd + 1].BodyLocalPos[chainInd], lt);

    }


    public Quaternion ComputeInterpolatedBodyLocalRot(Transform bodyPart, int keyInd, float lt) {

        int chainInd = BodyChainTorso.IndexOf(bodyPart);
        
        if (keyInd + 1 > Keys.Length - 1)
            return Keys[keyInd].BodyLocalRot[chainInd];

        return Quaternion.Slerp(Keys[keyInd].BodyLocalRot[chainInd], Keys[keyInd + 1].BodyLocalRot[chainInd], lt);

    }


    public void InterpolateWholeBody() {
        int keyInd = CurrKeyInd;
        float lt = LocalT;
        float t = GlobalT;
     
        Vector3 pivot = Vector3.zero;
        Quaternion pivotRot = Quaternion.identity;
        if(keyInd + 1 > Keys.Length - 1) {
            for (int i = 0; i < BodyChainTorso.Count; i++) {
                BodyChainTorso[i].transform.localPosition = Keys[keyInd].BodyLocalPos[i];//were all local pos rot
                BodyChainTorso[i].transform.localRotation = Keys[keyInd].BodyLocalRot[i];
            }
     
        }
        else {


            for (int i = 0; i < BodyChainTorso.Count; i++) {

                if (t < 0) {
                    pivot = Keys[0].BodyLocalPos[i]; 
                }
                else if (t > 1) {
                    pivot = Keys[keyInd - 1].BodyLocalPos[i];
                }

                Vector3 pos = SIBodyLocal[i].GetInterpolatedSplinePoint(lt, keyInd);
                Quaternion rot = Quaternion.Slerp(Keys[keyInd].BodyLocalRot[i], Keys[keyInd + 1].BodyLocalRot[i], lt);
                

                if ((t < 0 || t > 1)){                    
                    BodyChainTorso[i].transform.localPosition = 2 * pivot - pos;                    
                    BodyChainTorso[i].transform.localRotation = rot; 
                    }
                else {
                    BodyChainTorso[i].transform.localPosition = pos;                           
                    BodyChainTorso[i].transform.localRotation = rot;
                }
            }          
          
        }
   
    }


  
    
	
	public void InitInterpolators (float tension, float continuity, float bias) {
	    SIBody = new TCBSpline[_torso.BodyChain.Count];
        SIBodyLocal = new TCBSpline[_torso.BodyChain.Count];
        

		for(int arm = 0; arm < 2; arm++) {

            
        
            //End effector
            ControlPoint[] controlPointsEE = new ControlPoint[Keys.Length];
            for(int i = 0; i< Keys.Length; i++) {
                controlPointsEE[i] = new ControlPoint();
                controlPointsEE[i].Point = Keys[i].EePos[arm];                
                controlPointsEE[i].TangentI = Vector3.zero;
                controlPointsEE[i].TangentO = Vector3.zero;                
                controlPointsEE[i].Time = Keys[i].Time;
            }

            SIee[arm] = new TCBSpline(controlPointsEE, tension, continuity, bias);            
		}


        for (int c = 0; c < _torso.BodyChain.Count; c++) {

            ControlPoint[] controlPointsBody = new ControlPoint[Keys.Length];
            ControlPoint[] controlPointsBodyLocal = new ControlPoint[Keys.Length];
            for (int i = 0; i < Keys.Length; i++) {
                controlPointsBody[i] = new ControlPoint();
                controlPointsBody[i].Point = Keys[i].BodyPos[c];
                controlPointsBody[i].TangentI = Vector3.zero;
                controlPointsBody[i].TangentO = Vector3.zero;
                controlPointsBody[i].Time = Keys[i].Time;

                controlPointsBodyLocal[i] = new ControlPoint();
                controlPointsBodyLocal[i].Point = Keys[i].BodyLocalPos[c];
                controlPointsBodyLocal[i].TangentI = Vector3.zero;
                controlPointsBodyLocal[i].TangentO = Vector3.zero;
                controlPointsBodyLocal[i].Time = Keys[i].Time;
            }
            //SIBody[c] = new TCBSpline(controlPointsBody, tension, continuity, bias);        
            SIBody[c] = new TCBSpline(controlPointsBody, tension, continuity, bias);
            SIBodyLocal[c] = new TCBSpline(controlPointsBodyLocal, tension, continuity, bias);     
            
        }
	}

/*
    public float ComputeInterpolatedTime(Vector3 point, int p) {
        return SIKeyTime.FindDistanceOnSegment(point, p);
    }
    */
	public Vector3 ComputeInterpolatedTarget(float lt, int p,  int arm) {
        return SIee[arm].GetInterpolatedSplinePoint(lt, p);			
	}

    
    public void UpdateInterpolators() {
        float tp;
        float[] newKeyTimes = new float[Keys.Length];
        float[] originalKeyTimes = new float[Keys.Length];
        for (int i = 0; i < Keys.Length; i++) {

            int prevGoalKeyInd = FindPrevGoalAtTime(Keys[i].Time);
            int nextGoalKeyInd = prevGoalKeyInd + 1;
            if (nextGoalKeyInd > GoalKeys.Count - 1)
                nextGoalKeyInd = GoalKeys.Count - 1;

            int prevGoal = GoalKeys[prevGoalKeyInd];
            int nextGoal = GoalKeys[nextGoalKeyInd];


            if (Keys[i].Time <= Keys[prevGoal].Time)
                tp = 0;
            else if (Keys[nextGoal].Time == Keys[prevGoal].Time)
                tp = 1;
            else {
                tp = (Keys[i].Time - Keys[prevGoal].Time) / (Keys[nextGoal].Time - Keys[prevGoal].Time);
            }

            float t0, t1,ti, v0, v1;
            
            
             //no anticipation or overshoot except first and last keys
            if (prevGoalKeyInd == 0) {
                t0 = T0;
                v0 = V0;
                
            }
            else { //should shift ti as well
                t0 = 0;
                v0 = 0;
                               
            }

            if (prevGoalKeyInd + 1 == GoalKeys.Count - 1) {
                t1 = T1;
                v1 = V1;
            }
            else {
                t1 = 1;
                v1 = 0;
            }

            //should shift ti as well
            ti = Ti - T0/2f + (1 - T1)/2f;
                  
            // anticipation and overshoot for all keyframes

            v0 -= GoalSpeeds[prevGoalKeyInd];
            v1 -= GoalSpeeds[nextGoalKeyInd];
            float s = TimingControl(t0,t1, ti, v0, v1, tp);

            //map s into the whole spline        		   
            float t = (s * (Keys[nextGoal].Time - Keys[prevGoal].Time) + Keys[prevGoal].Time);
            newKeyTimes[i] = t;


        }

        //Record original keytimes
        for (int i = 0; i < Keys.Length; i++) {
            originalKeyTimes[i] = Keys[i].Time;
            Keys[i].Time = newKeyTimes[i];
        }

        //Update interpolators
        InitInterpolators(Tval, Continuity, 0);
        //Reset key times back
        for (int i = 0; i < Keys.Length; i++)
            Keys[i].Time = originalKeyTimes[i];

    }

    
    //t is between 0 and 1
    //Find the corresponding previous keyframe number at t
    //TODO: binary search
    public int FindKeyNumberAtNormalizedTime(float t) {
        if (t < 0 || t > 1) {
            Debug.Log("Incorrect time coefficient");
            return -1;
        }
      
        
        

        float appTime = t * AnimLength;
        for(int i = 0; i < Keys.Length-1; i++) {
            if (Keys[i].Time <= appTime && Keys[i + 1].Time > appTime)
                return i;
        }
        return Keys.Length - 1;
      
    /*    if (t == 1)
            return Keys.Length - 1;

        return Mathf.FloorToInt(t * Keys.Length);
    */
    }

    public int FindKeyNumberAtTime(float t) {

        
        for (int i = 0; i < Keys.Length - 1; i++) {
            if (Keys[i].Time <= t && Keys[i + 1].Time > t)
                return i;
        }
        return Keys.Length - 1;

    }


      public int FindFrameNumberAtTime(float t) {
        if (t < 0 || t > 1) {
            Debug.Log("Incorrect time coefficient");
            return -1;
        }

        //Compute it as in spline interpolation
        return (int) (t * FrameCnt);        
    }
      
    //index of the previous goal
    public int FindPrevGoalAtTime(float t) {
        for (int i = 0; i < GoalKeys.Count - 1; i++) {
            if (Keys[GoalKeys[i]].Time >= Keys[GoalKeys[GoalKeys.Count - 1]].Time)
                return GoalKeys.Count - 1;
            if (Keys[GoalKeys[i]].Time <= t && Keys[GoalKeys[i + 1]].Time > t)
                return i;
        }

        //return Keys.Length - 1;
        return GoalKeys.Count - 1;
     
    }

    public void UpdateKeypointsByShape(int arm) {
        

        bool passedCurves = false;
        for(int i = 0; i < Keys.Length ; i++) {
            if (UseCurveKeys == 1 && Keys[i].IsCurve) {
                //curve keys are fixed
                if (i > 0) {
                    passedCurves = true;
                }
                continue;
                
            }
            KeyInfo k = Keys[i];
            //initialize to original positions
            k.EePos[arm] = k.EePosOrig[arm];
            k.ShoulderPos[arm] = k.ShoulderPosOrig[arm];
            //Give shoulder position as a reference

            if(passedCurves) { //change direction after key point
                Hor = -Hor;
                Ver = -Ver;
                Sag = -Sag;
                passedCurves = false;
            }

            k.EePos[arm] = ArmShape(arm, k.EePos[arm], k.ShoulderPos[arm], k.RootForward);


        }

        InitInterpolators(Tval, Continuity, 0);
    }


    /// <summary>
    /// Update Arm Shape
    /// target: Keypoint to modify
    /// Returns modified keypoint
    /// </summary>
    Vector3 ArmShape(int arm, Vector3 target, Vector3 shoulderPos, Vector3 rootForward) {
        float rotTheta = 0f;
        Vector3 centerEllipse;
        //Transform target from world space to local EMOTE coordinates
        //	targetLocal = transform.InverseTransformPoint(target);	
        //Translate to world

        
        target = target - shoulderPos;

        Quaternion rot = Quaternion.FromToRotation(rootForward, Vector3.forward);
        //Rotate to world forward direction
        Vector3 targetLocal = rot * target;

        


        
        // Convert to Emote coordinate system
        targetLocal = new Vector3(targetLocal.y, -targetLocal.z, targetLocal.x);

        //hor				
        float theta = Mathf.Atan(Abratio * targetLocal.y / -targetLocal.z);

        if (-targetLocal.z < 0)
            theta += Mathf.PI;
        if (theta < 0)
            theta += 2 * Mathf.PI;

        float a = -targetLocal.z / Mathf.Cos(theta);




        if (Hor == 0) {
            // WRONG! rotTheta = 0f;
            rotTheta = theta;
        }
          
        else if (Hor < 0f && 0 < theta && theta <= Mathf.PI) {
            rotTheta = Mathf.Min(theta - Hor * MaxdTheta, Mathf.PI);
        }
        else if (Hor < 0f && Mathf.PI < theta && theta <= 2 * Mathf.PI) {
            rotTheta = Mathf.Max(theta + Hor * MaxdTheta, Mathf.PI);
        }
        else if (Hor > 0f && 0 < theta && theta <= Mathf.PI) {
            rotTheta = Mathf.Max(theta - Hor * MaxdTheta, 0);
        }
        else if (Hor > 0f && Mathf.PI < theta && theta <= 2 * Mathf.PI) {
            rotTheta = Mathf.Min(theta + Hor * MaxdTheta, 2 * Mathf.PI);
        }
        
         
        float hdz = -(a * Mathf.Cos(rotTheta)) - targetLocal.z;
        float  hdy = (a * Mathf.Sin(rotTheta) / Abratio) - targetLocal.y;
            
        




        //sag
        theta = Mathf.Atan(Abratio * targetLocal.x / -targetLocal.y);


        if (targetLocal.y < 0)
            theta += Mathf.PI;
        if (theta < 0)
            theta += 2 * Mathf.PI;



        a = targetLocal.y / Mathf.Cos(theta);

        if (Sag == 0) {
            // WRONG! rotTheta = 0f;
            rotTheta = theta;
        }
        else if (Sag < 0f && 0 < theta && theta <= Mathf.PI) {
            rotTheta = Mathf.Min(theta - Sag * MaxdTheta, Mathf.PI);
        }
        else if (Sag < 0f && Mathf.PI < theta && theta <= 2 * Mathf.PI) {
            rotTheta = Mathf.Max(theta + Sag * MaxdTheta, Mathf.PI);
        }
        else if (Sag > 0f && 0 < theta && theta <= Mathf.PI) {
            rotTheta = Mathf.Max(theta - Sag * MaxdTheta, 0);
        }
        else if (Sag > 0f && Mathf.PI < theta && theta <= 2 * Mathf.PI) {
            rotTheta = Mathf.Min(theta + Sag * MaxdTheta, 2 * Mathf.PI);
        }


        float sdx = -(a * Mathf.Sin(rotTheta) / Abratio) - targetLocal.x;
        float sdy = (a * Mathf.Cos(rotTheta)) - targetLocal.y;


        //ver
        theta = Mathf.Atan(-Abratio * targetLocal.z / -targetLocal.x);
        if (-targetLocal.x < 0)
            theta += Mathf.PI;
        if (theta < 0)
            theta += 2 * Mathf.PI;

        a = -targetLocal.x / Mathf.Cos(theta);


        if (Ver == 0) {
            // WRONG! rotTheta = 0f;
            rotTheta = theta;
        }
        else if (Ver < 0f && 0 < theta && theta <= Mathf.PI) {
            rotTheta = Mathf.Min(theta - Ver * MaxdTheta, Mathf.PI);
        }
        else if (Ver < 0f && Mathf.PI < theta && theta <= 2 * Mathf.PI) {
            rotTheta = Mathf.Max(theta + Ver * MaxdTheta, Mathf.PI);
        }
        else if (Ver > 0f && 0 < theta && theta <= Mathf.PI) {
            rotTheta = Mathf.Max(theta - Ver * MaxdTheta, 0);
        }
        else if (Ver > 0f && Mathf.PI < theta && theta <= 2 * Mathf.PI) {
            rotTheta = Mathf.Min(theta + Ver * MaxdTheta, 2 * Mathf.PI);
        }


        float vdx = -(a * Mathf.Cos(rotTheta)) - targetLocal.x;
        float vdz = -(a * Mathf.Sin(rotTheta) / Abratio) - targetLocal.z;

        if (Mathf.Abs(sdx) < 0.0001f) sdx = 0f;
        if (Mathf.Abs(sdy) < 0.0001f) sdy = 0f;
        if (Mathf.Abs(vdx) < 0.0001f) vdx = 0f;
        if (Mathf.Abs(vdz) < 0.0001f) vdz = 0f;
        if (Mathf.Abs(hdy) < 0.0001f) hdy = 0f;
        if (Mathf.Abs(hdz) < 0.0001f) hdz = 0f;


        //Update keypoint position

        if (arm == 1) {
            sdx = -sdx;
        }

        targetLocal.x += sdx + vdx;
        targetLocal.y += sdy + hdy;
        targetLocal.z += hdz + vdz;


        //Transform target from local EMOTE space to world coordinates

        //	target = transform.TransformPoint(targetLocal);

        //Convert back to unity coordinate system
        targetLocal = new Vector3(targetLocal.z, targetLocal.x, -targetLocal.y);


        //Translate back to world coordinate
        //Rotate to world forward direction
        rot = Quaternion.FromToRotation(Vector3.forward, rootForward);
        target = rot * targetLocal;
        
        target = target + shoulderPos;
        //Rotate to world forward direction
        

        return target;

    }
	
   

	

    float TimingControl(float t0, float t1, float ti, float v0, float v1, float tp) {

        float area1 = 0f, area2 = 0f, area3 = 0f;
        float vel;


        
        float tpp = Mathf.Pow(tp, Texp);
        
        float tpp2 = tpp * tpp;
        float t02 = t0 * t0;
        float t12 = t1 * t1;
        float ti2 = ti * ti;
        float s = 0f;


        if (t0 == t1)
            _vi = 0f;
            //  _vi = (2f + 2f * v1 * t1 - v1 + v0 * ti - v1 * ti) / (t1 - t0);
        else
            _vi = (2f + v1 + v0 * ti - v1 * ti) / (t1 - t0);
     


        area1 = -0.5f * v0 * t0;
        if (t0 == ti)
            area2 = area1;
        else
            area2 = area1 + (-0.5f * (v0 + _vi) * ti2 + (v0 * ti + t0 * _vi) * ti - (-0.5f * (v0 + _vi) * t02 + (v0 * ti + t0 * _vi) * t0)) / (t0 - ti);

        if (t1 == ti)
            area3 = area2;
        else
            area3 = area2 + (-0.5f * (v1 + _vi) * t12 + (v1 * ti + t1 * _vi) * t1 - (-0.5f * (v1 + _vi) * ti2 + (v1 * ti + t1 * _vi) * ti)) / (t1 - ti);


        //Compute s
        if (tpp >= 0f && tpp < t0) {
            vel = (-v0 / t0) * tpp;
            s = 0.5f * (-v0 / t0) * tpp2;
        }
        else if (tpp >= t0 && tpp < ti) {
            vel = (-(v0 + _vi) * tpp + v0 * ti + t0 * _vi) / (t0 - ti);
            s = area1 + (-(v0 + _vi) * tpp2 * 0.5f + (v0 * ti + t0 *   _vi) * tpp - (-(v0 + _vi) * t02 * 0.5f + (v0 * ti + t0 * _vi) * t0)) / (t0 - ti);
        }
        else if (tpp >= ti && tpp < t1) {
            vel = (-(v1 + _vi) * tpp + v1 * ti + t1 * _vi) / (t1 - ti);
            s = area2 + (-(v1 + _vi) * tpp2 * 0.5f + (v1 * ti + t1 * _vi) * tpp - (-(v1 + _vi) * ti2 * 0.5f + (v1 * ti + t1 * _vi) * ti)) / (t1 - ti);
        }
        else if (tpp >= t1 && tpp < 1f) {
            vel = (-v1 * tpp + v1) / (t1 - 1f);
            s = area3 + (-v1 * tpp2 * 0.5f + v1 * tpp - (-v1 * t12 * 0.5f + v1 * t1)) / (t1 - 1f);

        }
        else if (tpp == 1f) {
            s = area3;
            vel = 0f;
        }

        else
            vel = s = 0f;


        _velArr.Add(vel);
        _tppArr.Add(tpp);

        
        return s;



    }

    
    public void ComputeUpdatedTimingParameters() {

        float t0, t1, ti, v0, v1;
        
        //No anticipation or overshoot except the first and the last keyframes
        if (PrevGoalKeyInd == 0 &&  (!IsContinuous || LoopPlayCount == 0)  ) {
            t0 = T0;
            v0 = V0;
        }
        else {
            t0 = 0;
            v0 = 0;
        }

        if ((PrevGoalKeyInd + 1 == GoalKeys.Count - 1) && (!IsContinuous ||  LoopPlayCount == MaxLoopPlayCount)) {
            t1 = T1;
            v1 = V1;
        }
        else {
            t1 = 1;
            v1 = 0;
        }
        //should shift Ti as well
        ti = Ti - T0 / 2f + (1 - T1) / 2f;

     
        /*

        // anticipation and overshoot for all keyframes
        t0 = T0;
        t1 = T1;
        v0 = V0;
        v1 = V1;
        */

        /*
        v0 -= GoalSpeeds[PrevGoalKeyInd];


        
        if (PrevGoalKeyInd + 1 < GoalKeys.Count)
            v1 -= GoalSpeeds[PrevGoalKeyInd + 1];
        
   */

        if (GetComponent<IKAnimator>().LockHand && NormalizedTime > GetComponent<IKAnimator>().LockBegin && NormalizedTime < GetComponent<IKAnimator>().LockEnd) {

            if(AnimName.ToUpper().Contains("HANDSHAKE")){
                v0 -= 1f;
                v1 -= 1f;
            }
 
                
        }
        else {
            v0 -= GoalThreshold;
            v1 -= GoalThreshold;
        }

        float s = TimingControl(t0, t1, ti, v0, v1, Tp);
        
        //map s into the whole spline        		   
        GlobalT = (s * (NextGoal - PrevGoal) + PrevGoal) / AnimLength;

        
        if (NextGoal == PrevGoal)
            GlobalT = 1f;

        if (GlobalT < 0) {
            CurrKeyInd = FindKeyNumberAtNormalizedTime(-GlobalT); //find an imaginary key before the start of keyframes         
            if (CurrKeyInd + 1 < Keys.Length)
                LocalT = (float)(-GlobalT * AnimLength - Keys[CurrKeyInd].Time) / (Keys[CurrKeyInd + 1].Time - Keys[CurrKeyInd].Time);
            else
                LocalT = 0;
        }
        else if (GlobalT > 1) {
            CurrKeyInd = FindKeyNumberAtNormalizedTime(2 - GlobalT); //find an imaginary key beyond the keyframes   1 - ( t - 1)       
            if (CurrKeyInd + 1 < Keys.Length)
                LocalT = (float)((2 - GlobalT) * AnimLength - Keys[CurrKeyInd].Time) / (Keys[CurrKeyInd + 1].Time - Keys[CurrKeyInd].Time);
            else
                LocalT = 0;
        }
        else {
            CurrKeyInd = FindKeyNumberAtNormalizedTime(GlobalT); //including via keys
            if (CurrKeyInd + 1 < Keys.Length) {
                LocalT = (float)(GlobalT * AnimLength - Keys[CurrKeyInd].Time) / (Keys[CurrKeyInd + 1].Time - Keys[CurrKeyInd].Time);             
            }
            else
                LocalT = 0;
        }
        

        
    }


    //todo: was fixedupdate, but fixedupdate is called after ikanimator.lateupdate????
    void LateUpdate() {

        // for debug: silinecek
        NormalizedTime = NormalizedT; 
        Speed = AnimSpeed;
        //silinecek end

       
        if (_prevNormalizedT > 0.9 && NormalizedT < 0.1) {
            LoopPlayCount++;

        }
        _prevNormalizedT = NormalizedT;

        if (!GetComponent<Animation>().isPlaying)
            return;


        ComputeUpdatedTimingParameters();


        if (Tp == 0) {
            _velArr.Clear();
            _tppArr.Clear();
        }
        //Current velocity curve
        GameObject velCurveCurr = GameObject.Find("VelCurveCurr");
        if (velCurveCurr == null) {
            return;
        }
        velCurveCurr.GetComponent<LineRenderer>().SetVertexCount(_velArr.Count);
        for (int i = 0; i < _velArr.Count; i++) {
            velCurveCurr.GetComponent<LineRenderer>().SetPosition(i, new Vector3(_tppArr[i], _velArr[i], 0));
        }
        //General velocity curve as in EMOTE
        GameObject velCurveGen = GameObject.Find("VelCurveGeneral");
        velCurveGen.GetComponent<LineRenderer>().SetVertexCount(5);

        velCurveGen.GetComponent<LineRenderer>().SetPosition(0, new Vector3(0, 0, 0));
        velCurveGen.GetComponent<LineRenderer>().SetPosition(1, new Vector3(T0, -V0, 0));
        velCurveGen.GetComponent<LineRenderer>().SetPosition(2, new Vector3(Ti, _vi, 0));
        velCurveGen.GetComponent<LineRenderer>().SetPosition(3, new Vector3(T1, -V1, 0));
        velCurveGen.GetComponent<LineRenderer>().SetPosition(4, new Vector3(1, 0, 0));

        
    }



    BBox ComputeBoundingBox(List<Vector3> posList) {
       Vector3 min = posList[0];
        Vector3 max = posList[0];
        BBox bb = new BBox();

        foreach (Vector3 v in posList) {
            if (v.x < min.x)
                min.x = v.x;
            if (v.y < min.y)
                min.y = v.y;
            if (v.z < min.z)
                min.z = v.z;


            if (v.x > max.x)
                max.x = v.x;
            if (v.y > max.y)
                max.y = v.y;
            if (v.z > max.z)
                max.z = v.z;
        }


        bb.Min = min;
        bb.Max = max;

        return bb;
        
    }
}

