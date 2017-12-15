using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneController : MonoBehaviour {
    [System.Serializable]
    public struct Data {
        public GameObject root;
        public M8.Animator.AnimatorData animator;
        public string takeEnter;
        public string takeExit;
    }

    public M8.Animator.AnimatorData animator;
    public string takeStart; //prep background
    public string takeInteractEnter; //'next' button enter
    public string takeInteractExit; //'next' button exit

    public Data[] pages;

    private int mCurPageInd;

    public void NextPage() {
        StartCoroutine(DoGoNextPage());
    }

    void Awake() {
        for(int i = 0; i < pages.Length; i++) {
            if(pages[i].root)
                pages[i].root.SetActive(false);
        }
    }

    // Use this for initialization
    IEnumerator Start () {
        while(M8.SceneManager.instance.isLoading)
            yield return null;

        animator.Play(takeStart);
        while(animator.isPlaying)
            yield return null;

        //start up the first page
        mCurPageInd = 0;
        ShowCurrentPage();
    }

    void ShowCurrentPage() {
        StartCoroutine(DoShowCurrentPage());
    }

    IEnumerator DoShowCurrentPage() {
        if(mCurPageInd < pages.Length) {
            var page = pages[mCurPageInd];

            if(page.root)
                page.root.SetActive(true);

            if(page.animator) {
                page.animator.Play(page.takeEnter);
                while(page.animator.isPlaying)
                    yield return null;
            }
        }

        animator.Play(takeInteractEnter);
    }

    IEnumerator DoGoNextPage() {
        animator.Play(takeInteractExit);

        if(mCurPageInd < pages.Length) {
            var page = pages[mCurPageInd];

            if(page.animator) {
                page.animator.Play(page.takeExit);
                while(page.animator.isPlaying)
                    yield return null;
            }

            if(page.root)
                page.root.SetActive(false);
        }

        if(mCurPageInd < pages.Length - 1) {
            mCurPageInd++;
            ShowCurrentPage();
        }
        else //proceed to gameplay
            GameFlowController.LoadCurrentProgressScene();
    }
}
