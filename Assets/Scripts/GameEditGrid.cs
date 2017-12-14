using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEditGrid : MonoBehaviour {
    public GameObject gridGO;
    public float fadeDelay = 0.3f;
        
    private bool mGridIsShown = false;
    private bool mIsFade;
    private float mLastFadeTime;
    private Coroutine mGridUpdateRout;

    private Material mMat;

    private int mMatColorId;

    private Color mCurColor;
    private Color mDefaultColor;

    void OnDestroy() {
        if(GameMapController.instance)
            GameMapController.instance.modeChangeCallback -= OnGameModeChanged;

        if(mMat)
            Destroy(mMat);
    }

    void Awake() {
        Renderer r = gridGO.GetComponent<Renderer>();
        mMat = r.material;

        //apply cam dimension
        var gameCam = GameCamera.instance;

        var camBounds = gameCam.cameraViewBounds;
        camBounds.center = gameCam.transform.localToWorldMatrix.MultiplyPoint3x4(camBounds.center);

        Transform gridT = gridGO.transform;
        gridT.position = new Vector3(camBounds.center.x, camBounds.center.y, 0f);
        gridT.localScale = camBounds.size;

        gridGO.SetActive(false);

        //setup color
        mMatColorId = Shader.PropertyToID("colorMod");

        mDefaultColor = mMat.GetColor(mMatColorId);
        mCurColor = Color.clear;

        GameMapController.instance.modeChangeCallback += OnGameModeChanged;
    }

    void OnGameModeChanged(GameMapController.Mode mode) {
        switch(mode) {
            case GameMapController.Mode.Play:
                ShowGrid(false);
                break;

            case GameMapController.Mode.Edit:
                ShowGrid(true);
                break;
        }
    }

    private void ShowGrid(bool show) {
        if(mGridIsShown != show) {
            mGridIsShown = show;

            if(mGridIsShown) {
                gridGO.SetActive(true);

                if(mGridUpdateRout != null)
                    StopCoroutine(mGridUpdateRout);
                
                mGridUpdateRout = StartCoroutine(DoGridUpdate());
            }

            mIsFade = true;
            mLastFadeTime = Time.realtimeSinceStartup;
            mCurColor = mMat.GetColor(mMatColorId);
        }
    }
    
    IEnumerator DoGridUpdate() {
        while(true) {
            if(mIsFade) {
                float time = Time.realtimeSinceStartup;
                float curFadeTime = time - mLastFadeTime;
                if(curFadeTime > fadeDelay) {
                    curFadeTime = fadeDelay;
                    mIsFade = false;
                }

                float t = curFadeTime / fadeDelay;

                if(mGridIsShown) {
                    mMat.SetColor(mMatColorId, Color.Lerp(mCurColor, mDefaultColor, t));
                }
                else {
                    mMat.SetColor(mMatColorId, Color.Lerp(mCurColor, Color.clear, t));

                    if(!mIsFade)
                        break;
                }
            }

            yield return null;
        }

        gridGO.SetActive(false);
    }
}
