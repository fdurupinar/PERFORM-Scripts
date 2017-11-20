#define DEBUGMODE
using System;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;




public class IKAnimator : MonoBehaviour {

    //public GameObject trajectory;
    private Vector3 _targetL, _targetR; //current target


    //Torso
    public float[] EncSpr;
    public float[] SinRis;
    public float[] RetAdv;
    public float ShapeTi; //inflection time for shape changes

    public int FixedTarget; //if look at target is fixed
    public float HrMag; //Head rotation magnitude
    public float HfMag; //Head rotation frequency


    public float SquashMag;
    public float SquashF; //Squash frequency


    private AnimationInfo _animInfo;
    private bool _stopHeadRotation;

    private float _kneeDist = -1f;
    private float _feetDist = -1f;
    private float _initFeetDist = -1f;

    private GameObject _handCurve;

    public Transform HeadTarget;

    public bool LockHand;
    public float LockBegin;//handshake 0.2f;
    public float LockEnd;//handshake 0.6f;
    

    //public Transform TargetLeftWrist, TargetRightWrist;

#if DEBUGMODE
    private List<Vector3> _targetLPrev, _targetRPrev;
    private List<Vector3> _handRPrev;
    private List<float> _handRTime;


#endif


    private IKSolver _ikSolver;
    private FullBodyBipedIK _fbIk;
    private LookAtIK _laIk;

    //private BipedIK _bipIk;



    private Vector3 _bodyTargetPos;
    private Vector3 _footLTargetPos;
    private Vector3 _footRTargetPos;
    private Vector3 _shoulderLTargetPos;
    private Vector3 _shoulderRTargetPos;

    private Vector3 _elbowLTargetPos;
    private Vector3 _elbowRTargetPos;

    private Vector3 _kneeLTargetPos;
    private Vector3 _kneeRTargetPos;


    private Vector3 _thighLTargetPos;
    private Vector3 _thighRTargetPos;


    private float _currentLerpTime;
    private TorsoController _torso;


    private float _encSprAngleLast;
    //body targets


    private bool _stopRotating;


    public bool DrawGizmos = false;



    
    public float T;
    

    private void Awake() {

#if DEBUGMODE
        _targetRPrev = new List<Vector3>();
        _targetLPrev = new List<Vector3>();

        //_handRPrev = new List<Vector3>();
        //_handRTime = new List<float>();

        //  _handCurve = GameObject.Find("HandCurve");

#endif



        _animInfo = GetComponent<AnimationInfo>();

        _fbIk = GetComponent<FullBodyBipedIK>();
        _laIk = GetComponentInChildren<LookAtIK>();


        EncSpr = new float[3];
        SinRis = new float[3];
        RetAdv = new float[3];

        ShapeTi = 0f;


        _torso = GetComponent<TorsoController>();




        Reset();



    }

    public void Reset() {

        

#if DEBUGMODE
        _targetRPrev.Clear();
        _targetLPrev.Clear();
        GetComponent<FlourishAnimator>().HandLPrev.Clear();
        GetComponent<FlourishAnimator>().HandRPrev.Clear();

#endif




        _fbIk.solver.Initiate(_fbIk.solver.rootNode);
        _laIk.solver.Initiate(_laIk.solver.GetRoot());


        EnableIK();


        _laIk.solver.IKPosition = _laIk.solver.head.transform.position + _laIk.solver.head.transform.forward*2f;

        _stopHeadRotation = false; //= FixedTarget == 1;



        _stopRotating = false;


        _fbIk.solver.leftArmChain.bendConstraint.weight = 0f;
        _fbIk.solver.rightArmChain.bendConstraint.weight = 0f;

        _encSprAngleLast = 0;



        _initFeetDist = Vector3.Distance(_torso.Toe[0].position, _torso.Toe[1].position) - 0.1f; //0.1 = foot width


        if(_animInfo.AnimName.ToUpper().Contains("HAND")) {
            LockBegin = 0.2f;
            LockEnd = 0.6f;
        }
        else if (_animInfo.AnimName.ToUpper().Contains("FIVE")) {
            LockBegin = 0.5f;
            LockEnd = 0.8f;
        }


    }

    public void EnableIK() {
        _fbIk.enabled = true;
        _laIk.enabled = true;
    }

    public void DisableIK() {
        _fbIk.enabled = false;
        _laIk.enabled = false;
    }

    private void Update() {



        //if called continuously, this causes slowing down, but if not called, the motion stops abruptly
        //DEMO DRIVES
        if (!GetComponent<Animation>().isPlaying) {
//|| animation[_animInfo.AnimName].normalizedTime > 1f) {   //Call in update to prevent it from calling lateupdate
            DisableIK();
            return;
        }


        Vector3 kneeL = _torso.Knee[0].position;
        Vector3 kneeR = _torso.Knee[1].position;
        _kneeDist = Vector3.Distance(kneeL, kneeR);

        Vector3 toeL = _torso.Toe[0].position;
        Vector3 toeR = _torso.Toe[1].position;

        _feetDist = Vector3.Distance(toeL, toeR);

       

    }



    //Has to be lateupdate because we overwrite the transforms
    private void LateUpdate() {
        
        //correct breathing scales to ensure fbik computations are correct
        _torso.Spine1.transform.localScale = new Vector3(1, 1, 1);
        for (int i = 0; i < _torso.Spine1.GetChildCount(); i++)
            _torso.Spine1.GetChild(i).localScale = new Vector3(1, 1, 1); //correct child


       

        if (_animInfo.DisableLMA) {
            _animInfo.GetComponent<Animation>()[_animInfo.AnimName].speed = 1;
            DisableIK();

            return;
        }


        Interpolate();


        if (!GetComponent<Animation>().isPlaying || _animInfo.NormalizedT > 1f)
            return;


        EnableIK();

        if (_animInfo.NormalizedT > 1f)
            _stopHeadRotation = true;




        int keyInd = _animInfo.CurrKeyInd;
        float lt = _animInfo.LocalT;
        T = _animInfo.GlobalT;




        ComputeTargets(keyInd, lt, T);


        if (_animInfo.AnimName.ToUpper().Contains("AIM") || _animInfo.AnimName.ToUpper().Contains("GUARD") || _animInfo.AnimName.ToUpper().Contains("CUSTOMER"))
            BendKnees();

        if (_animInfo.CharacterName.ToUpper().Contains("CUSTOMER") || _animInfo.CharacterName.ToUpper().Contains("CARL"))
            BendKnees();


        if (_animInfo.AnimName.ToUpper().Contains("GUARD") || _animInfo.AnimName.ToUpper().Contains("CUSTOMER") ||
            _animInfo.AnimName.ToUpper().Contains("ROBBER") || _animInfo.AnimName.ToUpper().Contains("TELLER") || _animInfo.AnimName.ToUpper().Contains("CONVERS") || _animInfo.AnimName.ToUpper().Contains("FOOTBALL")) {

            if (HrMag < 0.1f)
                _stopHeadRotation = true;

          

            if (_stopHeadRotation) {
                //_laIk.solver.target = HeadTarget;
                _torso.Head.rotation = _torso.Spine.rotation;
                _laIk.solver.headWeight = 0;
            }

            else {
                _laIk.solver.headWeight = 1;
                //_laIk.solver.target = null;
            }

         //   UpdateLook(_animInfo.Tp + _animInfo.PrevGoalKeyInd);
       //     UpdateBodyIK(_animInfo.Tp + _animInfo.PrevGoalKeyInd);
            //UpdateBodyIK(_animInfo.NormalizedT);

        }
        //else {
            UpdateLook(_animInfo.NormalizedT);
            UpdateBodyIK(_animInfo.NormalizedT);


        //}


       

    }

    private void Interpolate() {
        if (!GetComponent<Animation>().isPlaying)
            return;

        _animInfo.InterpolateWholeBody();

    }

    //TODO: use torso instead of target pos etc.
    private void BendKnees() {
        Vector3 kneeDirR = Vector3.Cross(
            _footRTargetPos - _thighRTargetPos,
            Vector3.Cross(
                _footRTargetPos - _thighRTargetPos,
                _footRTargetPos - _kneeRTargetPos));

        _fbIk.solver.rightLegChain.bendConstraint.direction = kneeDirR;
        _fbIk.solver.rightLegChain.bendConstraint.weight = 1;



        Vector3 kneeDirL = Vector3.Cross(
            _footLTargetPos - _thighLTargetPos,
            Vector3.Cross(
                _footLTargetPos - _thighLTargetPos,
                _footLTargetPos - _kneeLTargetPos));

        _fbIk.solver.leftLegChain.bendConstraint.direction = kneeDirL;
        _fbIk.solver.leftLegChain.bendConstraint.weight = 1;

    }


    private void BendArms() {

        Vector3 elbowDirR = Vector3.Cross(
            _torso.Wrist[1].position - _torso.Shoulder[1].position,
            Vector3.Cross(
                _torso.Wrist[1].position - _torso.Shoulder[1].position,
                _torso.Wrist[1].position - _torso.Elbow[1].position)
            );

        //Now bend arms
        _fbIk.solver.rightArmChain.bendConstraint.direction = elbowDirR;
        _fbIk.solver.rightArmChain.bendConstraint.weight = 1;

        Vector3 elbowDirL = Vector3.Cross(
            _torso.Wrist[0].position - _torso.Shoulder[0].position,
            Vector3.Cross(
                _torso.Wrist[0].position - _torso.Shoulder[0].position,
                _torso.Wrist[0].position - _torso.Elbow[0].position)
            );

        _fbIk.solver.leftArmChain.bendConstraint.direction = elbowDirL;
        _fbIk.solver.leftArmChain.bendConstraint.weight = 1;
    }




    private void ComputeTargets(int keyInd, float lt, float t) {

        //update both arms
        for (int arm = 0; arm < 2; arm++) {

            //Actual target
            Vector3 target = _animInfo.ComputeInterpolatedTarget(lt, keyInd, arm);

            if (t < 0) {
                //globalTf
                // project target to a position before position at keyInd                              
                Vector3 pivot = _animInfo.ComputeInterpolatedTarget(0, 0, arm); //TCB interpolation for position
                target = 2*pivot - target;

            }
            else if (t > 1) {
                //globalT
                // project target to a position beyond keyInd

                Vector3 pivot = _animInfo.ComputeInterpolatedTarget(0, _animInfo.Keys.Length - 1, arm);
                    //TCB interpolation  for position           

                target = 2*pivot - target;
            }

            if (arm == 0)
                _targetL = target;

            else
                _targetR = target;




        }
        //update other body parts

        _bodyTargetPos = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.bodyEffector.bone.transform, keyInd, lt);

        _footLTargetPos = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.leftFootEffector.bone.transform, keyInd, lt);
        _footRTargetPos = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.rightFootEffector.bone.transform, keyInd, lt);

        _shoulderLTargetPos = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.leftShoulderEffector.bone.transform,
                                                                   keyInd, lt);
        _shoulderRTargetPos = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.rightShoulderEffector.bone.transform,
                                                                   keyInd, lt);


        _elbowLTargetPos = _animInfo.ComputeInterpolatedBodyPos(_torso.Elbow[0], keyInd, lt);
        _elbowRTargetPos = _animInfo.ComputeInterpolatedBodyPos(_torso.Elbow[1], keyInd, lt);


        _kneeLTargetPos = _animInfo.ComputeInterpolatedBodyPos(_torso.Knee[0], keyInd, lt);
        _kneeRTargetPos = _animInfo.ComputeInterpolatedBodyPos(_torso.Knee[1], keyInd, lt);




        _thighLTargetPos = _animInfo.ComputeInterpolatedBodyPos(_torso.Pelvis[0], keyInd, lt);
        _thighRTargetPos = _animInfo.ComputeInterpolatedBodyPos(_torso.Pelvis[1], keyInd, lt);



        //Update positions considering anticipation
        if (t < 0) {
            Vector3 pivot = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.bodyEffector.bone.transform, 0, 0);
            //if(_animInfo.CharacterName.Contains("CUSTOMER"))
            //   _bodyTargetPos = 2*pivot - _bodyTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.leftFootEffector.bone.transform, 0, 0);
            _footLTargetPos = 2*pivot - _footLTargetPos;


            pivot = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.rightFootEffector.bone.transform, 0, 0);
            _footRTargetPos = 2*pivot - _footRTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.leftShoulderEffector.bone.transform, 0, 0);
            _shoulderLTargetPos = 2*pivot - _shoulderLTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.rightShoulderEffector.bone.transform, 0, 0);
            _shoulderRTargetPos = 2*pivot - _shoulderRTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Elbow[0], 0, 0);
            _elbowLTargetPos = 2*pivot - _elbowLTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Elbow[1], 0, 0);
            _elbowRTargetPos = 2*pivot - _elbowRTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Knee[0], 0, 0);
            _kneeLTargetPos = 2*pivot - _kneeLTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Knee[1], 0, 0);
            _kneeRTargetPos = 2*pivot - _kneeRTargetPos;


            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Pelvis[0], 0, 0);
            _thighLTargetPos = 2*pivot - _thighLTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Pelvis[1], 0, 0);
            _thighRTargetPos = 2*pivot - _thighRTargetPos;
        }
            //Update positions considering overshoot
        else if (t > 1) {
            Vector3 pivot = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.bodyEffector.bone.transform,
                                                                 _animInfo.Keys.Length - 1, 0);

            pivot = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.leftFootEffector.bone.transform,
                                                         _animInfo.Keys.Length - 1, 0);
            _footLTargetPos = 2*pivot - _footLTargetPos;


            pivot = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.rightFootEffector.bone.transform,
                                                         _animInfo.Keys.Length - 1, 0);
            _footRTargetPos = 2*pivot - _footRTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.leftShoulderEffector.bone.transform,
                                                         _animInfo.Keys.Length - 1, 0);
            _shoulderLTargetPos = 2*pivot - _shoulderLTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_fbIk.solver.rightShoulderEffector.bone.transform,
                                                         _animInfo.Keys.Length - 1, 0);
            _shoulderRTargetPos = 2*pivot - _shoulderRTargetPos;



            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Elbow[0], _animInfo.Keys.Length - 1, 0);
            _elbowLTargetPos = 2*pivot - _elbowLTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Elbow[1], _animInfo.Keys.Length - 1, 0);
            _elbowRTargetPos = 2*pivot - _elbowRTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Knee[0], _animInfo.Keys.Length - 1, 0);
            _kneeLTargetPos = 2*pivot - _kneeLTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Knee[1], _animInfo.Keys.Length - 1, 0);
            _kneeRTargetPos = 2*pivot - _kneeRTargetPos;


            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Pelvis[0], _animInfo.Keys.Length - 1, 0);
            _thighLTargetPos = 2*pivot - _thighLTargetPos;

            pivot = _animInfo.ComputeInterpolatedBodyPos(_torso.Pelvis[1], _animInfo.Keys.Length - 1, 0);
            _thighRTargetPos = 2*pivot - _thighRTargetPos;

        }

    }


    private void UpdateLook(float t) {
        if (_stopHeadRotation)
            return;


        //seamless rotations
        float nT = _animInfo.NormalizedT + _animInfo.LoopPlayCount;
        
        float xTarget = (GetComponent<FlourishAnimator>().TrMag + HrMag)*Mathf.Cos(Mathf.PI + Mathf.PI*HfMag*nT);
        float sinVal = Mathf.Sin(Mathf.PI*HfMag*nT);
        float yTarget;


        xTarget = (HrMag)*Mathf.Cos(Mathf.PI + Mathf.PI*HfMag*nT);
        if (sinVal > 0)
            yTarget = 0.3f*HrMag*sinVal;
        else
            yTarget = 0.01f*HrMag*sinVal;

        _laIk.solver.IKPositionWeight = 1f;
        _laIk.solver.IKPosition = _laIk.solver.head.transform.position + _laIk.solver.head.transform.forward*2f +
                                  _laIk.solver.head.transform.right*xTarget + _laIk.solver.head.transform.up*yTarget;




    }


    private void UpdateBodyIK(float t) {

       
        Transform root = _fbIk.solver.GetRoot(); //bodyEffector.bone;//GetRoot();
        float groundY = _torso.InitFootPos.y;



    
        float shapeT = t; //lerp coefficient for shape changes -- changes throughout the animation
        float sinRisOffset;
        const float sinRisCoef = 0.03f;
        float encSprOffset;
        float footRetAdv;

        float minEncSprAngle = Mathf.PI / 4f; //Mathf.PI / 8f;
        float footLength = 0.5f; //todo: compute this value from bone positions        

        float encSprInit = footLength*Mathf.Sin(minEncSprAngle)*EncSpr[0]*_initFeetDist;
        if (EncSpr[0] > 0 && _animInfo.CharacterName.Contains("MIA") && _animInfo.AnimName.ToUpper().Contains("POINT"))
            encSprInit *= 20;

        if (_animInfo.IsContinuous)
            shapeT = (t+ _animInfo.LoopPlayCount)/_animInfo.MaxLoopPlayCount;

        float alpha;
        float encSprAngle;



        if (shapeT < ShapeTi) {

            shapeT = shapeT/ShapeTi;

            sinRisOffset = sinRisCoef*Mathf.Lerp(SinRis[0], SinRis[1], shapeT);

            footRetAdv = (RetAdv[0]*(1 - shapeT) + RetAdv[1]*shapeT);
            alpha = Mathf.PI*(footRetAdv - RetAdv[0])/(RetAdv[1] - RetAdv[0]);

            if (Math.Abs(RetAdv[1] - RetAdv[0]) < 0.00001f) //case when they are equal
                alpha = 0;


            if (EncSpr[1] < EncSpr[0] || (EncSpr[1] == EncSpr[0] && EncSpr[0] < 0)) {
                encSprOffset = 0.04f*Mathf.Lerp(EncSpr[0], EncSpr[1], shapeT);
            }
            else {
                //spreading
                encSprOffset = 0.12f*Mathf.Lerp(EncSpr[0], EncSpr[1], shapeT);


            }

            encSprAngle = minEncSprAngle*Mathf.Lerp(EncSpr[0], EncSpr[1], shapeT);
        }
        else {

            shapeT = (shapeT - ShapeTi)/(1 - ShapeTi);

            sinRisOffset = sinRisCoef*Mathf.Lerp(SinRis[1], SinRis[2], shapeT);
            footRetAdv = (RetAdv[1]*(1 - shapeT) + RetAdv[2]*shapeT);
            alpha = Mathf.PI*(footRetAdv - RetAdv[1])/(RetAdv[2] - RetAdv[1]);
            if (Math.Abs(RetAdv[2] - RetAdv[1]) < 0.00001f) //case when they are equal
                alpha = 0;


            if (EncSpr[2] < EncSpr[1] || (EncSpr[2] == EncSpr[1] && EncSpr[1] < 0)) {
                encSprOffset = 0.04f*Mathf.Lerp(EncSpr[1], EncSpr[2], shapeT);
            }
            else {
                //spreading
                encSprOffset = 0.12f*Mathf.Lerp(EncSpr[1], EncSpr[2], shapeT);

            }


            encSprAngle = minEncSprAngle*Mathf.Lerp(EncSpr[1], EncSpr[2], shapeT);
        }




        Vector3 zVal = footRetAdv*root.forward;
        Vector3 yVal = Mathf.Sin(alpha)*root.up;
        Vector3 retAdvOffset = 0.02f*Mathf.Abs(RetAdv[1] - RetAdv[0])*yVal + footLength*zVal;
        Vector3 bodyRetAdv = footLength*zVal;



        //Hands
        float encForward = 0f;

        _fbIk.solver.leftHandEffector.position = _targetL + 2*sinRisOffset*root.up - encSprOffset*root.right +
                                                 encForward*root.forward + 1.3f*bodyRetAdv;
        _fbIk.solver.leftHandEffector.positionWeight = 1f;

        if (_fbIk.solver.leftHandEffector.position.y < (groundY + 0.1f))
            _fbIk.solver.leftHandEffector.position.y = groundY + 0.1f;



        _fbIk.solver.rightHandEffector.position = _targetR + 2*sinRisOffset*root.up + encSprOffset*root.right +
                                                  encForward*root.forward + 1.3f*bodyRetAdv;
        _fbIk.solver.rightHandEffector.positionWeight = 1f;

       

        if (_fbIk.solver.rightHandEffector.position.y < groundY + 0.1f)
            _fbIk.solver.rightHandEffector.position.y = groundY + 0.1f;


        //Prevents incorrect arm swivels in pointing
        _fbIk.solver.leftHandEffector.rotationWeight = 0f;
        _fbIk.solver.rightHandEffector.rotationWeight = 0f;

        

        //Body


        //this is causing incorrect leg rotations
        _fbIk.solver.bodyEffector.position = _bodyTargetPos + sinRisOffset*root.up + 1.3f*bodyRetAdv;
        _fbIk.solver.bodyEffector.positionWeight = 1f;


        


        //Shoulders            
        Vector3 sinRisShoulders = Mathf.Abs(sinRisOffset)*root.up; //always rising

        _fbIk.solver.leftShoulderEffector.position = _shoulderLTargetPos + sinRisShoulders + 1.3f*bodyRetAdv;
            //sinris*2 because we raise twice (feet + shoulders)
        _fbIk.solver.leftShoulderEffector.positionWeight = 1f;


        _fbIk.solver.rightShoulderEffector.position = _shoulderRTargetPos + sinRisShoulders + 1.3f*bodyRetAdv;
        _fbIk.solver.rightShoulderEffector.positionWeight = 1f;

        
       
        //make sure hand does not get affected by rotations etc
        if (LockHand  ){//&& t > LockBegin&& t < LockEnd){

           
            _fbIk.solver.rightHandEffector.position = _fbIk.solver.rightHandEffector.bone.position; //animation position
            _fbIk.solver.rightHandEffector.rotation = _fbIk.solver.rightHandEffector.bone.rotation; //animation rotation

            _fbIk.solver.rightShoulderEffector.position = _fbIk.solver.rightShoulderEffector.bone.position; //animation position
            _fbIk.solver.rightShoulderEffector.rotation = _fbIk.solver.rightShoulderEffector.bone.rotation; //animation rotation



        }
        

        

        
        //_fbIk.solver.leftShoulderEffector.rotation = _fbIk.solver.leftShoulderEffector.bone.rotation;
       // _fbIk.solver.leftShoulderEffector.rotationWeight =0f;
       // _fbIk.solver.rightShoulderEffector.rotation = _fbIk.solver.rightShoulderEffector.bone.rotation;
       // _fbIk.solver.rightShoulderEffector.rotationWeight = 0f;
        

        if (sinRisOffset < 0)
            sinRisOffset = 0;

        //Feet
        _fbIk.solver.leftFootEffector.position = _footLTargetPos + sinRisOffset*root.up + retAdvOffset -
                                                 encSprInit*root.right;
        _fbIk.solver.leftFootEffector.positionWeight = 1f;

        _fbIk.solver.rightFootEffector.position = _footRTargetPos + sinRisOffset*root.up + encSprInit*root.right;
        _fbIk.solver.rightFootEffector.positionWeight = 1f;


        
        //Correct feet positions
        //Prevent collisions

        if (_fbIk.solver.leftFootEffector.position.y < groundY)
            _fbIk.solver.leftFootEffector.position.y = groundY;


        if (_fbIk.solver.rightFootEffector.position.y < groundY)
            _fbIk.solver.rightFootEffector.position.y = groundY;


        //do not update feet rotations while walking during overshoot        
        if ((T > 1)) {
            if (_animInfo.AnimName.ToUpper().Contains("WALK") || _animInfo.AnimName.ToUpper().Contains("SALSA"))
                return;
        }



        _fbIk.solver.leftFootEffector.rotationWeight = 1;
        _fbIk.solver.rightFootEffector.rotationWeight = 1;

        // _fbIk.solver.leftFootEffector.rotation = _fbIk.solver.leftFootEffector.bone.rotation;
        // _fbIk.solver.rightFootEffector.rotation = _fbIk.solver.rightFootEffector.bone.rotation;


        Quaternion footLRot1 = Quaternion.identity;
        Quaternion footRRot1 = Quaternion.identity;

        //Find what the new feetdist is gonna be

        _feetDist += 2f*encSprInit + 2f*Mathf.Sin(encSprAngle)*footLength;



        if (!_stopRotating && (_feetDist < 0.05f || _kneeDist < 0.05f) && EncSpr[1] < EncSpr[0]) {
            //enclosing
            //enclosing
            _stopRotating = true;
            

        }
        if (!_stopRotating) {
            footLRot1 = Quaternion.AngleAxis(-encSprAngle*Mathf.Rad2Deg, root.up);
            footRRot1 = Quaternion.AngleAxis(encSprAngle*Mathf.Rad2Deg, root.up);
            _encSprAngleLast = encSprAngle;
        }



        Quaternion footLRot2 = Quaternion.identity;
        Quaternion footRRot2 = Quaternion.identity;
        if (sinRisOffset >= 0) {
            //rising


            float footAngle = Mathf.Asin(sinRisOffset/(0.66f*footLength))*Mathf.Rad2Deg;
                //consider foot length from sole to heel: remove toe length


            //should determine foot rotation based on the model


            if (_animInfo.CharacterName.Contains("CHUCK") || _animInfo.CharacterName.Contains("ADAM") ||
                _animInfo.CharacterName.Contains("GUARD") || _animInfo.CharacterName.Contains("ROBBER") ||
                _animInfo.CharacterName.Contains("CUSTOMER")) //non-standard foot angles
                footAngle = -footAngle;

            footLRot2 = Quaternion.AngleAxis(footAngle, _fbIk.solver.leftFootEffector.bone.right);
            footRRot2 = Quaternion.AngleAxis(footAngle, _fbIk.solver.rightFootEffector.bone.right);

            //footLRot2 = Quaternion.AngleAxis(-footAngle,  root.right); 
            //footRRot2 = Quaternion.AngleAxis(-footAngle, root.right); 



        }

        //Correct feet

        if (!footLRot1.Equals(Quaternion.identity)) {
            if (!footLRot2.Equals(Quaternion.identity))
                _fbIk.solver.leftFootEffector.rotation = footLRot1*footLRot2*_fbIk.solver.leftFootEffector.bone.rotation;
            else {

                _fbIk.solver.leftFootEffector.rotation = footLRot1*_fbIk.solver.leftFootEffector.bone.rotation;
            }
        }
        else {
            if (!footLRot2.Equals(Quaternion.identity)) {
                //stopped rotating       
                footLRot1 = Quaternion.AngleAxis(-_encSprAngleLast*Mathf.Rad2Deg, root.up);
                    //for one last rotation                
                _fbIk.solver.leftFootEffector.rotation = footLRot1*footLRot2*_fbIk.solver.leftFootEffector.bone.rotation;
            }
            else {
                //maintain animation rotation
                footLRot1 = Quaternion.AngleAxis(-_encSprAngleLast*Mathf.Rad2Deg, root.up);
                    //for one last rotation                 
                _fbIk.solver.leftFootEffector.rotation = footLRot1*_fbIk.solver.leftFootEffector.bone.rotation;

            }
        }

        if (!footRRot1.Equals(Quaternion.identity)) {
            if (!footRRot2.Equals(Quaternion.identity))
                _fbIk.solver.rightFootEffector.rotation = footRRot1*footRRot2*
                                                          _fbIk.solver.rightFootEffector.bone.rotation;
            else
                _fbIk.solver.rightFootEffector.rotation = footRRot1*_fbIk.solver.rightFootEffector.bone.rotation;
        }
        else {
            if (!footRRot2.Equals(Quaternion.identity)) {
                footRRot1 = Quaternion.AngleAxis(_encSprAngleLast*Mathf.Rad2Deg, root.up); //for one last rotation
                _fbIk.solver.rightFootEffector.rotation = footRRot1*footRRot2*
                                                          _fbIk.solver.rightFootEffector.bone.rotation;
            }
            else {
//maintain animation rotation
                footRRot1 = Quaternion.AngleAxis(_encSprAngleLast*Mathf.Rad2Deg, root.up);
                    //for one last rotation                 
                _fbIk.solver.rightFootEffector.rotation = footRRot1*_fbIk.solver.rightFootEffector.bone.rotation;
            }
        }



        if (_animInfo.CharacterName.Contains("CUSTOMER")) {

            _torso.Toe[0].rotation = Quaternion.Inverse(footLRot2)*_torso.Toe[0].rotation;
            _torso.Toe[1].rotation = Quaternion.Inverse(footRRot2)*_torso.Toe[1].rotation;
        }


    }


    private void OnDrawGizmos() {
#if DEBUGMODE
        if (DrawGizmos) {
            

            //Gizmos.DrawSphere(_fbIk.solver.rightHandEffector.position, 0.001f);

            

            if (GetComponent<Animation>().isPlaying && _targetRPrev.Count > 1) {
                Gizmos.color = Color.red;

                for (int i = 0; i < _targetRPrev.Count ; i++) {

                    Gizmos.DrawSphere(_targetRPrev[i], 0.005f);


                }
            }
            
            

#endif
            if (_animInfo) {
                float size;
                for (int i = 0; i < _animInfo.Keys.Length; i++) {
                    if (_animInfo.Keys[i].IsGoal) {

                        if (GetComponent<Animation>().isPlaying && i == _animInfo.CurrKeyInd && _animInfo.Keys[_animInfo.CurrKeyInd].IsGoal) {
                            Gizmos.color = Color.yellow;
                            size = 0.04f;
                        }
                        else {
                            Gizmos.color = Color.blue;
                            size = 0.02f;
                        }


                        if (_animInfo.AnimName.ToUpper().Contains("SALSA") || _animInfo.AnimName.ToUpper().Contains("BALLET") || _animInfo.AnimName.ToUpper().Contains("WALK"))
                            Gizmos.DrawSphere(_animInfo.Keys[i].EePos[(int)EEType.RightFoot], size);
                        else
                            Gizmos.DrawSphere(_animInfo.Keys[i].EePos[(int)EEType.RightHand], size); 
                    }
                    else {
                        Gizmos.color = new Color((float) i/_animInfo.Keys.Length, 0, 0);
                        
                        size = 0.01f;
                        if (_animInfo.AnimName.ToUpper().Contains("SALSA") || _animInfo.AnimName.ToUpper().Contains("BALLET") || _animInfo.AnimName.ToUpper().Contains("WALK"))
                            Gizmos.DrawSphere(_animInfo.Keys[i].EePos[(int)EEType.RightFoot], size);
                        else
                            Gizmos.DrawSphere(_animInfo.Keys[i].EePos[(int)EEType.RightHand], size);
                    }
                    //Color.red;                    

                }

                
            }


        }


    }

}
