using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour {
    public GameObject loadingGO;
    public GameObject readyGO;

    void Awake() {
        loadingGO.SetActive(true);
        readyGO.SetActive(false);
    }

    IEnumerator Start () {
        //wait for LoL to load/initialize
        while(!LoLManager.instance.isReady)
            yield return null;

        loadingGO.SetActive(false);
        readyGO.SetActive(true);
    }
}
