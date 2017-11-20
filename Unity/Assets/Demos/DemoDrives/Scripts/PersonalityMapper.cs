
using System;
using Meta.Numerics.Statistics;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public enum MotionCoef {
    Speed,
    V0,
    V1,
    Ti,
    Texp,
    TVal,
    T0,
    T1,
    HrMag,
    HSign,
    HfMag,
    SquashMag,
    WbMag,
    WxMag,
    WfMag,
    WtMag,
    EtMag,
    EfMag,
    DMag,
    TrMag,
    TfMag,
    Continuity,
    EncSpr0,
    SinRis0,
    RetAdv0,
    EncSpr1,
    SinRis1,
    RetAdv1,
    EncSpr2,
    SinRis2,
    RetAdv2,
    Arm0X,
    Arm0Y,
    Arm0Z,
    Arm1X,
    Arm1Y,
    Arm1Z,
    ShapeTi,
    ExtraGoal,
    UseCurveKeys,
    FixedTarget,
    SquashF,
    GoalThreshold

}


public class PersonalityMapper {

    private MotionCoef _minMotionCoefs;
    private MotionCoef _maxMotionCoefs;

 
    //Weighted
    private float[][] _significantEfforts = new float[4][] {
        new float[] {-0.92f, 0.93f, -0.89f, 0, -1}, //space
        new float[] {0, 0, 0, -1, 0}, //weight        
        new float[] {0, -0.85f, 0.99f, -1, 0.97f}, //time
        new float[] {-0.931f, 0.94f, -1, 0, -0.761f}, //flow

    };

    /*
    private float[][] _significantEfforts = new float[4][] {
        new float[] {-1, 1, -1, 0, -1}, //space
        new float[] {0, 0, 0, -1, 0}, //weight        
        new float[] {0, -1, 1, -1, 1}, //time
        new float[] {-1, 1, -1, 0, -1}, //flow

    };
    */


    private float[][] _motionEffortCoefs = new float[5][];

    


    //x range for regression --> Effort values for each Drive
    private double[][] _xRange = new double[32][] {
        new double[] {-1, -1, -1, 0}, new double[] {-1, -1, 1, 0}, new double[] {-1, 1, -1, 0},
        new double[] {-1, 1, 1, 0}, new double[] {1, -1, -1, 0}, new double[] {1, -1, 1, 0}, new double[] {1, 1, -1, 0},
        new double[] {1, 1, 1, 0},
        new double[] {-1, -1, 0, -1}, new double[] {-1, -1, 0, 1}, new double[] {-1, 1, 0, -1},
        new double[] {-1, 1, 0, 1}, new double[] {1, -1, 0, -1}, new double[] {1, -1, 0, 1}, new double[] {1, 1, 0, -1},
        new double[] {1, 1, 0, 1},
        new double[] {-1, 0, -1, -1}, new double[] {-1, 0, -1, 1}, new double[] {-1, 0, 1, -1},
        new double[] {-1, 0, 1, 1}, new double[] {1, 0, -1, -1}, new double[] {1, 0, -1, 1}, new double[] {1, 0, 1, -1},
        new double[] {1, 0, 1, 1},
        new double[] {0, -1, -1, -1}, new double[] {0, -1, -1, 1}, new double[] {0, -1, 1, -1},
        new double[] {0, -1, 1, 1}, new double[] {0, 1, -1, -1}, new double[] {0, 1, -1, 1}, new double[] {0, 1, 1, -1},
        new double[] {0, 1, 1, 1}
    };




    public PersonalityMapper() {

        for (int i = 0; i < 5; i++)
            _motionEffortCoefs[i] = new float[43];

    }


    private int ComputeIndex(int o, int c, int e, int a, int n) {
        return (o + 1) + 3*(c + 1) + 9*(e + 1) + 27*(a + 1) + 81*(n + 1);

    }

    public void MapAnimSpeeds(AnimationInfo[] agentScripts, float minDesiredSpeed, float maxDesiredSpeed) {
       
            float min = 10000;
            float max = -10000;
            foreach (AnimationInfo t in agentScripts) {
                if (t.AnimSpeed < min)
                    min = t.AnimSpeed;
                if (t.AnimSpeed > max)
                    max = t.AnimSpeed;
            }
            if (max == min)
                return;
            foreach (AnimationInfo t in agentScripts) {

                t.AnimSpeed = minDesiredSpeed + (t.AnimSpeed - min) * (maxDesiredSpeed - minDesiredSpeed) / (max - min);
         
            }
    }


    //only compute after drives are achieved
    public void ComputeMotionEffortCoefs(DriveParams[] driveParams) {
        //using (StreamWriter sw = new StreamWriter("regressionCoefs.txt")) {
            double param = 0;
            for (int coefInd = 0; coefInd < _motionEffortCoefs[0].Length; coefInd++) {
                MultivariateSample efforts = new MultivariateSample(5); //4 efforts + 1 coefficient            

                for (int i = 0; i < 32; i++) { 
                    if (coefInd == (int) MotionCoef.Speed)
                        param = driveParams[i].Speed;
                    else if (coefInd == (int) MotionCoef.V0)
                        param = driveParams[i].V0;
                    else if (coefInd == (int) MotionCoef.V1)
                        param = driveParams[i].V1;
                    else if (coefInd == (int) MotionCoef.Ti)
                        param = driveParams[i].Ti;
                    else if (coefInd == (int) MotionCoef.Texp)
                        param = driveParams[i].Texp;
                    else if (coefInd == (int) MotionCoef.TVal)
                        param = driveParams[i].Tval;
                    else if (coefInd == (int) MotionCoef.T0)
                        param = driveParams[i].T0;
                    else if (coefInd == (int) MotionCoef.T1)
                        param = driveParams[i].T1;
                    else if (coefInd == (int) MotionCoef.HrMag)
                        param = Mathf.Abs(driveParams[i].HrMag);
                    else if (coefInd == (int) MotionCoef.HSign)
                        param = driveParams[i].HSign;
                    else if (coefInd == (int) MotionCoef.HfMag)
                        param = driveParams[i].HfMag;
                    else if (coefInd == (int) MotionCoef.SquashMag)
                        param = driveParams[i].SquashMag;
                    else if (coefInd == (int) MotionCoef.WbMag)
                        param = driveParams[i].WbMag;
                    else if (coefInd == (int) MotionCoef.WxMag)
                        param = driveParams[i].WxMag;
                    else if (coefInd == (int) MotionCoef.WtMag)
                        param = driveParams[i].WtMag;
                    else if (coefInd == (int) MotionCoef.WfMag)
                        param = driveParams[i].WfMag;
                    else if (coefInd == (int) MotionCoef.EtMag)
                        param = driveParams[i].EtMag;
                    else if (coefInd == (int) MotionCoef.EfMag)
                        param = driveParams[i].EfMag;
                    else if (coefInd == (int) MotionCoef.DMag)
                        param = driveParams[i].DMag;
                    else if (coefInd == (int) MotionCoef.TrMag)
                        param = driveParams[i].TrMag;
                    else if (coefInd == (int) MotionCoef.TfMag)
                        param = driveParams[i].TfMag;
                    else if (coefInd == (int) MotionCoef.EncSpr0)
                        param = driveParams[i].EncSpr0;
                    else if (coefInd == (int) MotionCoef.SinRis0)
                        param = driveParams[i].SinRis0;
                    else if (coefInd == (int) MotionCoef.RetAdv0)
                        param = driveParams[i].RetAdv0;
                    else if (coefInd == (int) MotionCoef.EncSpr1)
                        param = driveParams[i].EncSpr1;
                    else if (coefInd == (int) MotionCoef.SinRis1)
                        param = driveParams[i].SinRis1;
                    else if (coefInd == (int) MotionCoef.RetAdv1)
                        param = driveParams[i].RetAdv1;
                    else if (coefInd == (int) MotionCoef.EncSpr2)
                        param = driveParams[i].EncSpr2;
                    else if (coefInd == (int) MotionCoef.SinRis2)
                        param = driveParams[i].SinRis2;
                    else if (coefInd == (int) MotionCoef.RetAdv2)
                        param = driveParams[i].RetAdv2;
                    else if (coefInd == (int) MotionCoef.Continuity)
                        param = driveParams[i].Continuity;
                    else if (coefInd == (int) MotionCoef.Arm0X)
                        param = driveParams[i].Arm[0].x;
                    else if (coefInd == (int) MotionCoef.Arm0Y)
                        param = driveParams[i].Arm[0].y;
                    else if (coefInd == (int) MotionCoef.Arm0Z)
                        param = driveParams[i].Arm[0].z;
                    else if (coefInd == (int) MotionCoef.Arm1X)
                        param = driveParams[i].Arm[1].x;
                    else if (coefInd == (int) MotionCoef.Arm1Y)
                        param = driveParams[i].Arm[1].y;
                    else if (coefInd == (int) MotionCoef.Arm1Z)
                        param = driveParams[i].Arm[1].z;
                    else if (coefInd == (int) MotionCoef.ShapeTi)
                        param = driveParams[i].ShapeTi;
                    else if (coefInd == (int) MotionCoef.ExtraGoal)
                        param = driveParams[i].ExtraGoal;
                    else if (coefInd == (int) MotionCoef.UseCurveKeys)
                        param = driveParams[i].UseCurveKeys;
                    else if (coefInd == (int) MotionCoef.FixedTarget)
                        param = driveParams[i].FixedTarget;
                    else if (coefInd == (int) MotionCoef.SquashF)
                        param = driveParams[i].SquashF;
                    else if (coefInd == (int)MotionCoef.GoalThreshold)
                        param = driveParams[i].GoalThreshold;


                    efforts.Add(param, _xRange[i][0], _xRange[i][1], _xRange[i][2], _xRange[i][3]);
                }



                FitResult regression = efforts.LinearRegression(0); //keep motion parameter fixed
                //intercept, space, time, weight, flow coefficients

                for (int i = 0; i < 5; i++) {
                    _motionEffortCoefs[i][coefInd] = (float) regression.Parameter(i).Value;
                }
       //         sw.WriteLine(_motionEffortCoefs[0][coefInd] + "\t" + _motionEffortCoefs[1][coefInd] + "\t" +
         //                    _motionEffortCoefs[2][coefInd] + "\t" + _motionEffortCoefs[3][coefInd] + "\t" +
           //                  _motionEffortCoefs[4][coefInd]);


           // }
        }
    


}



    private void ComputeEffort(PersonalityComponent p) {
        for (int i = 0; i < 4; i++) {
            p.Effort[i] = 0;
            float maxP = 0;
            float minP = 0;
            for (int j = 0; j < 5; j++) {
                float val = _significantEfforts[i][j]*p.Personality[j];
                if ( val > maxP)
                    maxP = val;
                if ( val < minP)
                    minP = val;
            }
            
            p.Effort[i] = maxP + minP;            
        }
    }

    public void MapPersonalityToMotion(PersonalityComponent p) { //, DriveParams[] driveParams) {
       
        ComputeEffort(p);
  
        for (int i = 0; i < _motionEffortCoefs[0].Length; i++) {
            float param = _motionEffortCoefs[0][i] + p.Effort[0] * _motionEffortCoefs[1][i] +
                          p.Effort[1] * _motionEffortCoefs[2][i] + p.Effort[2] * _motionEffortCoefs[3][i] + p.Effort[3] * _motionEffortCoefs[4][i];
            
            //We multiply time-related parameters by animLength * fps /1.625 * 24  considering pointing animation's length and fps and current animations length and fps
            float timeScale = (p.GetComponent<AnimationInfo>().AnimLength * p.GetComponent<AnimationInfo>().Fps) / (1.625f * 24);
            
            float timeScale2 = p.GetComponent<AnimationInfo>().AnimLength /1.625f ;
            if(p.GetComponent<AnimationInfo>().AnimName.ToUpper().Contains("CONVERS"))
                timeScale2 /= 6f;
            //float timeScale2 = timeScale;
            //Apply constraints

            if (i == (int)MotionCoef.Speed) {
                if (p.GetComponent<AnimationInfo>().AnimName.ToUpper().Contains("CONVERS"))
                    p.GetComponent<AnimationInfo>().AnimSpeed = param.Constrain(0.7f, 0.8f);
                else if (p.GetComponent<AnimationInfo>().AnimName.ToUpper().Contains("FOOTBALL"))
                    p.GetComponent<AnimationInfo>().AnimSpeed = 0.7f;
                else if (p.GetComponent<AnimationInfo>().AnimName.ToUpper().Contains("WALK"))
                    p.GetComponent<AnimationInfo>().AnimSpeed = param;
                //  p.GetComponent<AnimationInfo>().AnimSpeed = param.Map(0.7f, 1.5f);
                else
                    p.GetComponent<AnimationInfo>().AnimSpeed = param.Constrain(0.5f, 1f);
                // p.GetComponent<AnimationInfo>().AnimSpeed = param.Constrain(0.7f, 0.8f);
                //p.GetComponent<AnimationInfo>().AnimSpeed = param.Constrain(0.7f, 0.8f);
            }

            else if (i == (int)MotionCoef.V0)
                p.GetComponent<AnimationInfo>().V0 = param.Constrain(0, 1);
            else if (i == (int)MotionCoef.V1)
                p.GetComponent<AnimationInfo>().V1 = param.Constrain(0, 1);
            else if (i == (int)MotionCoef.Ti)
                p.GetComponent<AnimationInfo>().Ti = (0.5f - (0.5f - param) / timeScale).Constrain(0, 1); //need to scale //param.Constrain(0, 1);
            else if (i == (int)MotionCoef.Texp)
                p.GetComponent<AnimationInfo>().Texp = param.Constrain(0);
            else if (i == (int)MotionCoef.GoalThreshold) {
                //determines goal frequency
                //Adjust with new scale

                p.GetComponent<AnimationInfo>().GoalThreshold = param.Constrain(0f, 1f);


                p.GetComponent<AnimationInfo>().InitKeyPoints();
            }

            else if (i == (int)MotionCoef.TVal) {
                p.GetComponent<AnimationInfo>().Tval = param.Constrain(-1, 1);
                p.GetComponent<AnimationInfo>().InitInterpolators(p.GetComponent<AnimationInfo>().Tval,
                                                                      p.GetComponent<AnimationInfo>().Continuity, 0);
            }

            else if (i == (int)MotionCoef.T0)
                p.GetComponent<AnimationInfo>().T0 = (param / timeScale).Constrain(0, p.GetComponent<AnimationInfo>().Ti);
            else if (i == (int)MotionCoef.T1)
                p.GetComponent<AnimationInfo>().T1 = (1 - (1 - param) / timeScale).Constrain(p.GetComponent<AnimationInfo>().Ti, 1); //need to scale 

            else if (i == (int)MotionCoef.HrMag)
                p.GetComponent<IKAnimator>().HrMag = param.Constrain(-0.8f, 0.8f);
            else if (i == (int)MotionCoef.HSign) {
                if (param < 0) param = -1;
                else if (param > 0) param = 1;
                p.GetComponent<IKAnimator>().HrMag = Mathf.Abs(p.GetComponent<IKAnimator>().HrMag) * param;
                //hrmag comes first                 
            }
            else if (i == (int)MotionCoef.HfMag)
                p.GetComponent<IKAnimator>().HfMag = (param * timeScale2).Constrain(0);
            else if (i == (int)MotionCoef.SquashMag)
                p.GetComponent<IKAnimator>().SquashMag = param.Constrain(0, 1);
            else if (i == (int)MotionCoef.WbMag)
                p.GetComponent<FlourishAnimator>().WbMag = param.Constrain(-1, 1);
            else if (i == (int)MotionCoef.WxMag)
                p.GetComponent<FlourishAnimator>().WxMag = param.Constrain(-1.8f, 1.8f);
            else if (i == (int)MotionCoef.WtMag)
                p.GetComponent<FlourishAnimator>().WtMag = param.Constrain(0, 1.4f);
            else if (i == (int)MotionCoef.WfMag)
                p.GetComponent<FlourishAnimator>().WfMag = (param * timeScale2).Constrain(0);
            else if (i == (int)MotionCoef.EtMag)
                p.GetComponent<FlourishAnimator>().EtMag = param.Constrain(0, 1.4f);
            else if (i == (int)MotionCoef.EfMag)
                p.GetComponent<FlourishAnimator>().EfMag = (param * timeScale2).Constrain(0);
            else if (i == (int)MotionCoef.DMag)
                p.GetComponent<FlourishAnimator>().DMag = param.Constrain(0, 1.4f);
            else if (i == (int)MotionCoef.TrMag)
                p.GetComponent<FlourishAnimator>().TrMag = param.Constrain(-1, 1);
            else if (i == (int)MotionCoef.TfMag)
                p.GetComponent<FlourishAnimator>().TfMag = (param * timeScale2).Constrain(0); //normalize to the animation range

            else if (i == (int)MotionCoef.EncSpr0)
                p.GetComponent<IKAnimator>().EncSpr[0] = param.Constrain(-1, 1);
            else if (i == (int)MotionCoef.SinRis0)
                p.GetComponent<IKAnimator>().SinRis[0] = param.Constrain(-1, 1);
            else if (i == (int)MotionCoef.RetAdv0)
                p.GetComponent<IKAnimator>().RetAdv[0] = param.Constrain(-1, 1);
            else if (i == (int)MotionCoef.EncSpr1)
                p.GetComponent<IKAnimator>().EncSpr[1] = param.Constrain(-1, 1);
            else if (i == (int)MotionCoef.SinRis1)
                p.GetComponent<IKAnimator>().SinRis[1] = param.Constrain(-1, 1);
            else if (i == (int)MotionCoef.RetAdv1)
                p.GetComponent<IKAnimator>().RetAdv[1] = param.Constrain(-1, 1);
            else if (i == (int)MotionCoef.EncSpr2)
                p.GetComponent<IKAnimator>().EncSpr[2] = param.Constrain(-1, 1);
            else if (i == (int)MotionCoef.SinRis2)
                p.GetComponent<IKAnimator>().SinRis[2] = param.Constrain(-1, 1);
            else if (i == (int)MotionCoef.RetAdv2)
                p.GetComponent<IKAnimator>().RetAdv[2] = param.Constrain(-1, 1);


            else if (i == (int)MotionCoef.Continuity) {
                p.GetComponent<AnimationInfo>().Continuity = param.Constrain(-1, 1);
                p.GetComponent<AnimationInfo>().InitInterpolators(p.GetComponent<AnimationInfo>().Tval,
                                                                      p.GetComponent<AnimationInfo>().Continuity, 0);
            }


            else if (i == (int)MotionCoef.Arm0X) {
                p.GetComponent<AnimationInfo>().Hor = param.Constrain(-1, 1);
                p.GetComponent<AnimationInfo>().UpdateKeypointsByShape(0); //Update keypoints                 
            }
            else if (i == (int)MotionCoef.Arm0Y) {
                p.GetComponent<AnimationInfo>().Ver = param.Constrain(-1, 1);
                p.GetComponent<AnimationInfo>().UpdateKeypointsByShape(0); //Update keypoints

            }
            else if (i == (int)MotionCoef.Arm0Z) {
                p.GetComponent<AnimationInfo>().Sag = param.Constrain(-1, 1);
                p.GetComponent<AnimationInfo>().UpdateKeypointsByShape(0); //Update keypoints

            }
            else if (i == (int)MotionCoef.Arm1X) {
                p.GetComponent<AnimationInfo>().Hor = param.Constrain(-1, 1);
                p.GetComponent<AnimationInfo>().UpdateKeypointsByShape(1); //Update keypoints
            }
            else if (i == (int)MotionCoef.Arm1Y) {
                p.GetComponent<AnimationInfo>().Ver = param.Constrain(-1, 1);
                p.GetComponent<AnimationInfo>().UpdateKeypointsByShape(1); //Update keypoints
            }

            else if (i == (int)MotionCoef.Arm1Z) {
                p.GetComponent<AnimationInfo>().Sag = param.Constrain(-1, 1);
                p.GetComponent<AnimationInfo>().UpdateKeypointsByShape(1); //Update keypoints

            }

            else if (i == (int)MotionCoef.ShapeTi) {
                p.GetComponent<IKAnimator>().ShapeTi = param.Constrain(0, 1);

            }

            else if (i == (int)MotionCoef.ExtraGoal) {
                p.GetComponent<AnimationInfo>().ExtraGoal = Mathf.RoundToInt(param);
                p.GetComponent<AnimationInfo>().InitKeyPoints();
            }
            else if (i == (int)MotionCoef.UseCurveKeys) {
                p.GetComponent<AnimationInfo>().UseCurveKeys = Mathf.RoundToInt(param);
                p.GetComponent<AnimationInfo>().InitKeyPoints();

            }
            else if (i == (int)MotionCoef.FixedTarget) {
                p.GetComponent<IKAnimator>().FixedTarget = Mathf.RoundToInt(param);


            }
            else if (i == (int)MotionCoef.SquashF) {
                p.GetComponent<IKAnimator>().SquashF = (param * timeScale2).Constrain(0);

            }
        }


    }



   



}


