using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEndController : MonoBehaviour {
    public M8.Animator.AnimatorData animator;
    public string takePlay = "end";

    public Button completeButton; //disable if we are ending

    public float autoEndDelay = 10f;

	// Use this for initialization
	IEnumerator Start () {
        //wait for scene to load
        while(M8.SceneManager.instance.isLoading)
            yield return null;

        //play
        animator.Play(takePlay);

        do {
            yield return null;
        } while(animator.isPlaying);

        //auto end
        yield return new WaitForSeconds(autoEndDelay);

        completeButton.interactable = false;

        GameFlowController.Complete();
    }
	
}
