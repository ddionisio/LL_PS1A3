using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEndController : MonoBehaviour {
    public M8.Animator.AnimatorData animator;
    public string takePlay = "end";

    public Button completeButton; //disable if we are ending

    public float autoEndDelay = 10f;

    public GameObject[] stars;

	// Use this for initialization
	IEnumerator Start () {
        //wait for scene to load
        while(M8.SceneManager.instance.isLoading)
            yield return null;

        //determine which stars to activate
        if(GameData.isInstantiated) {
            float score = GameData.instance.currentScore;
            float maxScore = GameData.instance.scorePerLevel * 8.0f; //TODO: need actual level count

            int index = maxScore > 0f ? Mathf.RoundToInt((stars.Length - 1) * Mathf.Clamp01(score / maxScore)) : 0;
            if(index >= stars.Length)
                index = stars.Length - 1;

            stars[index].SetActive(true);
        }
        else
            stars[Random.Range(0, stars.Length)].SetActive(true);

        //play
        animator.Play(takePlay);

        do {
            yield return null;
        } while(animator.isPlaying);

        //auto end
        if(autoEndDelay > 0f) {
            yield return new WaitForSeconds(autoEndDelay);

            completeButton.interactable = false;

            GameFlowController.Complete();
        }
    }
	
}
