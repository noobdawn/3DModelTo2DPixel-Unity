using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class RaySampleManager : MonoBehaviour {
    public bool IsExporting;

    public RaySample raySample;
    public InputField roleInput;
    public InputField animInput;
    public InputField frameInput;
    public InputField totalFrameInput;
    public Slider progress;
    public Transform modelRoot;
    public Dropdown dropDown;

    private ChildAnimatorState curState;
    private const float FrameRate = 240;

    public void OnClickExportCurrent()
    {
        raySample.ExportCurrent(roleInput.text, animInput.text + "-" + frameInput.text);
    }

    public void OnProgressBarChanged()
    {
        Animator animator = modelRoot.GetComponentInChildren<Animator>();
        animator.speed = 0f;
        animator.ForceStateNormalizedTime(progress.value * ((AnimationClip)curState.state.motion).length);
    }

    public void OnClickPlay()
    {
        Animator animator = modelRoot.GetComponentInChildren<Animator>();
        animator.speed = 1f;
        animator.Play(curState.state.name, -1, 0);
    }

    public void OnClickRefresh()
    {
        Animator animator = modelRoot.GetComponentInChildren<Animator>();
        AnimatorController animationController = animator.runtimeAnimatorController as AnimatorController;
        AnimatorStateMachine stateMachine = animationController.layers[0].stateMachine;
        dropDown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var s in stateMachine.states)
        {
            options.Add(s.state.name);
        }
        dropDown.AddOptions(options);
        OnDropDownChanged();
    }

    public void OnDropDownChanged()
    {
        string value = dropDown.options[dropDown.value].text;
        Animator animator = modelRoot.GetComponentInChildren<Animator>();
        AnimatorController animationController = animator.runtimeAnimatorController as AnimatorController;
        AnimatorStateMachine stateMachine = animationController.layers[0].stateMachine;
        foreach (var s in stateMachine.states)
        {
            if (s.state.name == value)
            { 
                curState = s;
            return;
            }
        }
    }

    public void OnClickExportAnimation()
    {
        StartCoroutine(StartExport(int.Parse(totalFrameInput.text)));
    }

    IEnumerator StartExport(int totalFrames)
    {
        Debug.Log("Start export!");
        Animator animator = modelRoot.GetComponentInChildren<Animator>();
        animator.speed = 0f;
        var length = ((AnimationClip)curState.state.motion).length;
        for(int i = 0; i < totalFrames; i++)
        {
            animator.ForceStateNormalizedTime(i / (float)totalFrames);
            raySample.ExportCurrent(roleInput.text + "_auto", animInput.text + "-" + (i+1).ToString());
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Export complete!");
    }
}
