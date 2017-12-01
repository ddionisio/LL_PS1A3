using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEditGrid : MonoBehaviour {
    public GameObject gridGO;
    public float fadeDelay = 0.3f;

    private SpriteRenderer mGridSpriteRender;
    private bool mGridIsShown = false;
    private bool mIsFade;
    private float mLastFadeTime;
    private Coroutine mGridUpdateRout;
    private Color mCurColor;
    private Color mDefaultColor;

    void OnDestroy() {
        if(GameMapController.instance)
            GameMapController.instance.modeChangeCallback -= OnGameModeChanged;
    }

    void Awake() {
        mGridSpriteRender = gridGO.GetComponent<SpriteRenderer>();
        gridGO.SetActive(false);

        mDefaultColor = mGridSpriteRender.color;
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
            mCurColor = mGridSpriteRender.color;
        }
    }
    
    IEnumerator DoGridUpdate() {
        var gameCam = GameCamera.instance;
        Vector2 lastCameraPos = new Vector2(float.MaxValue, float.MaxValue);

        while(true) {
            var curCameraPos = gameCam.position;
            if(lastCameraPos != curCameraPos) {
                //compute grid size
                var camBounds = gameCam.cameraViewBounds;
                camBounds.center = gameCam.transform.localToWorldMatrix.MultiplyPoint3x4(camBounds.center);

                var mapData = GameMapController.instance.mapData;

                //grab min and max cell indices
                var cellMin = mapData.GetCellIndex(camBounds.min);
                var cellMax = mapData.GetCellIndex(camBounds.max);

                //convert back to world space
                var blockHalfSize = GameData.instance.blockSize * 0.5f;

                var cellMinPos = mapData.GetPositionFromCell(cellMin) - blockHalfSize;
                var cellMaxPos = mapData.GetPositionFromCell(cellMax) + GameData.instance.blockSize;

                if(mGridSpriteRender)
                    mGridSpriteRender.size = cellMaxPos - cellMinPos;

                gridGO.transform.position = Vector2.Lerp(cellMinPos, cellMaxPos, 0.5f);

                lastCameraPos = curCameraPos;
            }

            if(mIsFade) {
                float time = Time.realtimeSinceStartup;
                float curFadeTime = time - mLastFadeTime;
                if(curFadeTime > fadeDelay) {
                    curFadeTime = fadeDelay;
                    mIsFade = false;
                }

                float t = curFadeTime / fadeDelay;

                if(mGridIsShown) {
                    mGridSpriteRender.color = Color.Lerp(mCurColor, mDefaultColor, t);
                }
                else {
                    mGridSpriteRender.color = Color.Lerp(mCurColor, Color.clear, t);

                    if(!mIsFade)
                        break;
                }
            }

            yield return null;
        }

        gridGO.SetActive(true);
    }
}
