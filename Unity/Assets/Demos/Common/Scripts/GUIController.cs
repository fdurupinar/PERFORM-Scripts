using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class GUIController:MonoBehaviour {
    public GUIText[] AgentText; //each agent has an associated text
   

    protected void InitText(int id, Font myFont) {
        AgentText[id] = new GUIText();
        AgentText[id] = GameObject.Find("GUI Text" + id).GetComponent<GUIText>();
        AgentText[id].gameObject.AddComponent<ObjectLabel>();
        AgentText[id].gameObject.GetComponent<ObjectLabel>().Target = AgentText[id].gameObject.GetComponent<TorsoController>().Root;
        AgentText[id].fontSize = 12;
        AgentText[id].anchor = TextAnchor.LowerCenter;
        AgentText[id].text = "";
        AgentText[id].font = myFont;
    }

    protected void Reset(AnimationInfo agent) {
        
        ResetComponents(agent);
        StopAtFirstFrame(agent);


    }

    protected void ResetComponents(AnimationInfo agent) {

        agent.GetComponent<Animation>().Stop();
        agent.GetComponent<FlourishAnimator>().Reset();
        agent.GetComponent<TorsoController>().Reset();
        agent.Reset(agent.AnimName);
        agent.GetComponent<IKAnimator>().Reset();



    }
    protected void StopAtFirstFrame(AnimationInfo agent) {
        if (!agent.GetComponent<Animation>().isPlaying)
            agent.GetComponent<Animation>().Play(agent.AnimName);

        agent.GetComponent<Animation>().clip.SampleAnimation(agent.gameObject, 0); //instead of rewind
        agent.GetComponent<Animation>().Stop();

    }
    protected void Play(AnimationInfo agent) {
        //agent.animation[agent.AnimName].wrapMode = WrapMode.ClampForever;
        agent.GetComponent<Animation>()[agent.AnimName].wrapMode = WrapMode.Loop;
        agent.GetComponent<Animation>().Play(agent.AnimName);
        agent.GetComponent<Animation>().enabled = true;

    }


    protected void StopAnimations(AnimationInfo agent) {

        StopAtFirstFrame(agent);
        agent.GetComponent<TorsoController>().Reset();


        StopAtFirstFrame(agent);
        PlayAnim(agent); //start the next animation
        StopAtFirstFrame(agent);
    }


    protected void InitAgent(AnimationInfo agent) {

        agent.Reset(agent.AnimName);
        agent.GetComponent<IKAnimator>().Reset();
        agent.GetComponent<Animation>().enabled = true;
        agent.GetComponent<Animation>().Play(agent.AnimName);

    }


    protected void PlayAnim(AnimationInfo agent) {

        StopAtFirstFrame(agent); //stop first
        InitAgent(agent);


        agent.GetComponent<Animation>()[agent.AnimName].wrapMode = agent.IsContinuous ? WrapMode.Loop : WrapMode.ClampForever;

    }

}
