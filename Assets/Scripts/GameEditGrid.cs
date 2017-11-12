using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEditGrid : MonoBehaviour {
    public GameObject gridGO;

    private SpriteRenderer mGridSpriteRender;
    private bool mGridIsShown = false;

    void OnDestroy() {
        if(GameMapController.instance)
            GameMapController.instance.modeChangeCallback -= OnGameModeChanged;
    }

    void Awake() {
        mGridSpriteRender = gridGO.GetComponent<SpriteRenderer>();
        gridGO.SetActive(false);

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

                //compute grid size
                var camBounds = GameCamera.instance.cameraViewBounds;
                camBounds.center = GameCamera.instance.transform.localToWorldMatrix.MultiplyPoint3x4(camBounds.center);

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
            }
            else {
                gridGO.SetActive(false);
            }
        }
    }
}
