using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorPlayAfterSceneLoad : MonoBehaviour {
    public M8.Animator.AnimatorData animator;
    public string take;

    private IEnumerator Start() {
        if(!animator)
            animator = GetComponent<M8.Animator.AnimatorData>();

        while(M8.SceneManager.instance.isLoading)
            yield return null;

        animator.Play(take);
    }
}
