using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BodyPart {
    Root,
    Neck,
    Head,
    Spine,
    Spine1,
    Spine2,
    ClavicleL,
    ClavicleR,
    ShoulderL,
    ShoulderR,
    PelvisL,
    PelvisR,
    ElbowL,
    ElbowR,
    WristL,
    WristR,
    FootL,
    FootR,
    KneeL,
    KneeR,
    ToeL,
    ToeR,
    ToeEndL,
    ToeEndR,
    Hips
}




[System.Serializable]
public class TorsoController : MonoBehaviour {


    public List<Transform> BodyChain;

    public Transform Root;
    public Transform Neck;
    public Transform Head;
    public Transform Spine;
    public Transform Spine1;
    public Transform Spine2 = null;
    public Transform[] Clavicle = new Transform[2];
    public Transform[] Shoulder = new Transform[2];
    public Transform[] Pelvis = new Transform[2];
    public Transform[] Elbow = new Transform[2];
    public Transform[] Wrist = new Transform[2];
    public Transform[] Knee = new Transform[2];
    public Transform[] Foot = new Transform[2];
    public Transform[] Toe = new Transform[2];
    public Transform[] ToeEnd = new Transform[2];

    public Transform Hips;
    //	[HideInInspector] 


    public Quaternion[] InitRot;
    public Vector3[] InitPos;

    public Vector3 InitRootPos; //global root position
    public Vector3 InitFootPos; //global foot position
    public Vector3 InitToePos; //global foot position


    public List<Quaternion> BodyRot;
    public List<Vector3> BodyPos;
    public List<string> BodyPath;

    public List<Quaternion> BodyLocalRot;
    public List<Vector3> BodyLocalPos;




    // Use this for initialization

    //Assign initial positions and rotations  -- standing still positions as seen in scene view
    void Awake() {
        InitRot = new Quaternion[25];

        InitPos = new Vector3[25];

        AssignInitRootandFootPos();

        InitRot[(int)BodyPart.Root] = Root.rotation; //localrot idi
        InitPos[(int)BodyPart.Root] = Root.position;

        InitRot[(int)BodyPart.Head] = Head.rotation;
        InitPos[(int)BodyPart.Head] = Head.position;

        InitRot[(int)BodyPart.Neck] = Neck.rotation;
        InitPos[(int)BodyPart.Neck] = Neck.position;

        InitRot[(int)BodyPart.Spine] = Spine.rotation;
        InitPos[(int)BodyPart.Spine] = Spine.position;

        InitRot[(int)BodyPart.Spine1] = Spine1.rotation;
        InitPos[(int)BodyPart.Spine1] = Spine1.position;

        if (Spine2) {
            InitRot[(int)BodyPart.Spine2] = Spine2.rotation;
            InitPos[(int)BodyPart.Spine2] = Spine2.position;
        }
        else {
            Debug.Log("No spine2 found");
            InitRot[(int)BodyPart.Spine2] = Quaternion.identity;
            InitPos[(int)BodyPart.Spine2] = Vector3.zero;
        }

        InitRot[(int)BodyPart.ShoulderL] = Shoulder[0].rotation;
        InitPos[(int)BodyPart.ShoulderL] = Shoulder[0].position;

        InitRot[(int)BodyPart.ShoulderR] = Shoulder[1].rotation;
        InitPos[(int)BodyPart.ShoulderR] = Shoulder[1].position;


        InitRot[(int)BodyPart.ElbowL] = Elbow[0].rotation;
        InitPos[(int)BodyPart.ElbowL] = Elbow[0].position;

        InitRot[(int)BodyPart.ElbowR] = Elbow[1].rotation;
        InitPos[(int)BodyPart.ElbowR] = Elbow[1].position;

        InitRot[(int)BodyPart.WristL] = Wrist[0].rotation;
        InitPos[(int)BodyPart.WristL] = Wrist[0].position;

        InitRot[(int)BodyPart.WristR] = Wrist[1].rotation;
        InitPos[(int)BodyPart.WristR] = Wrist[1].position;


        InitRot[(int)BodyPart.ClavicleL] = Clavicle[0].rotation;
        InitPos[(int)BodyPart.ClavicleL] = Clavicle[0].position;

        InitRot[(int)BodyPart.ClavicleR] = Clavicle[1].rotation;
        InitPos[(int)BodyPart.ClavicleR] = Clavicle[1].position;

        InitRot[(int)BodyPart.PelvisL] = Pelvis[0].rotation;
        InitPos[(int)BodyPart.PelvisL] = Pelvis[0].position;

        InitRot[(int)BodyPart.PelvisR] = Pelvis[1].rotation;
        InitPos[(int)BodyPart.PelvisR] = Pelvis[1].position;



        InitRot[(int)BodyPart.FootL] = Foot[0].rotation;
        InitPos[(int)BodyPart.FootL] = Foot[0].position;

        InitRot[(int)BodyPart.FootR] = Foot[1].rotation;
        InitPos[(int)BodyPart.FootR] = Foot[1].position;



        InitRot[(int)BodyPart.KneeL] = Knee[0].rotation;
        InitPos[(int)BodyPart.KneeL] = Knee[0].position;


        InitRot[(int)BodyPart.KneeR] = Knee[1].rotation;
        InitPos[(int)BodyPart.KneeR] = Knee[1].position;


        InitRot[(int)BodyPart.Hips] = Hips.rotation;
        InitPos[(int)BodyPart.Hips] = Hips.position;

        InitRot[(int)BodyPart.ToeL] = Toe[0].rotation;
        InitPos[(int)BodyPart.ToeL] = Toe[0].position;

        InitRot[(int)BodyPart.ToeR] = Toe[1].rotation;
        InitPos[(int)BodyPart.ToeR] = Toe[1].position;

        if (ToeEnd[0]) {

            InitRot[(int) BodyPart.ToeEndL] = ToeEnd[0].rotation;
            InitPos[(int) BodyPart.ToeEndL] = ToeEnd[0].position;

            InitRot[(int) BodyPart.ToeEndR] = ToeEnd[1].rotation;
            InitPos[(int) BodyPart.ToeEndR] = ToeEnd[1].position;
        }
         else {
            Debug.Log("No toe end found");
            InitRot[(int)BodyPart.ToeEndL] = Quaternion.identity;
            InitPos[(int)BodyPart.ToeEndL] = Vector3.zero;

            InitRot[(int)BodyPart.ToeEndR] = Quaternion.identity;
            InitPos[(int)BodyPart.ToeEndR] = Vector3.zero;
        }

        BodyChain = BodyChainToArray(Root);
        BodyPath = BodyChainToPath(Root);

        BodyPos = BodyPosArr(BodyChain);
        BodyRot = BodyRotArr(BodyChain);


        BodyLocalPos = BodyLocalPosArr(BodyChain);
        BodyLocalRot = BodyLocalRotArr(BodyChain);




    }



    //Assign the initial root position for this animation
    //Call when animation is changed
    //y values usually don't change as model in on the ground but x and z values change
    public void AssignInitRootandFootPos() {
        InitRootPos = Root.position;
        if (Foot[0].position.y < Foot[1].position.y)
            InitFootPos = Foot[0].position;
        else
            InitFootPos = Foot[1].position;

        if (Toe[0].position.y < Toe[1].position.y)
            InitToePos = Toe[0].position;
        else
            InitToePos = Toe[1].position;

    }


    //Assign the initial root position for this animation
    public void ResetToInitRootPos() {
        Root.position = InitRootPos;

    }

    //Assign the initial root position for this animation
    public void ResetToGround() {
        Root.position = new Vector3(Root.position.x, InitRootPos.y, Root.position.z);

    }
    public void Reset() {
        //Reset to current animation's transforms


        for (int i = 0; i < BodyChain.Count; i++) {
            BodyChain[i].transform.position = BodyPos[i]; //was localpos and rot
            BodyChain[i].transform.rotation = BodyRot[i];


            BodyChain[i].transform.localPosition = BodyLocalPos[i]; //was localpos and rot
            BodyChain[i].transform.localRotation = BodyLocalRot[i];
        }

        Root.position = InitRootPos;



        //reset spine scales
        Spine1.transform.localScale = new Vector3(1, 1, 1);
        for (int i = 0; i < Spine1.GetChildCount(); i++)
            Spine1.GetChild(i).localScale = new Vector3(1, 1, 1); //correct child


    }
    //Add whole body path
    public List<string> BodyChainToPath(Transform root) {

        List<string> chain = new List<string>();

        if (!root.name.Contains("TARGET_") && !root.name.Contains("SNAP_") && !root.name.Contains("PROP_")) {
            chain.Add(root.name);

            for (int i = 0; i < root.childCount; i++) {
                List<string> childChain = BodyChainToPath(root.GetChild(i));
                foreach (string s in childChain)
                    chain.Add(root.name + "/" + s);
            }

        }
        return chain;
    }

    //Add whole body
    public List<Transform> BodyChainToArray(Transform root) {

        List<Transform> chain = new List<Transform>();


        if (!root.name.Contains("TARGET_") && !root.name.Contains("SNAP_") && !root.name.Contains("PROP_")) {
            chain.Add(root);

            for (int i = 0; i < root.childCount; i++)
                chain.AddRange(BodyChainToArray(root.GetChild(i)));
        }


        return chain;
    }

    public List<Vector3> BodyPosArr(List<Transform> bodyChain) {
        List<Vector3> bodyPos = new List<Vector3>();
        for (int i = 0; i < bodyChain.Count; i++)
            bodyPos.Add(bodyChain[i].position); //was localpos

        return bodyPos;
    }

    public List<Quaternion> BodyRotArr(List<Transform> bodyChain) {
        List<Quaternion> bodyRot = new List<Quaternion>();
        for (int i = 0; i < bodyChain.Count; i++)
            bodyRot.Add(bodyChain[i].rotation);//localrottu

        return bodyRot;
    }



    public List<Vector3> BodyLocalPosArr(List<Transform> bodyChain) {
        List<Vector3> bodyLocalPos = new List<Vector3>();
        for (int i = 0; i < bodyChain.Count; i++)
            bodyLocalPos.Add(bodyChain[i].localPosition); //localposdu

        return bodyLocalPos;
    }

    public List<Quaternion> BodyLocalRotArr(List<Transform> bodyChain) {
        List<Quaternion> bodyLocalRot = new List<Quaternion>();
        for (int i = 0; i < bodyChain.Count; i++)
            bodyLocalRot.Add(bodyChain[i].localRotation);//localrottu

        return bodyLocalRot;
    }
}

