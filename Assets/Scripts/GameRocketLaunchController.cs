using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRocketLaunchController : M8.SingletonBehaviour<GameRocketLaunchController> {
    public M8.Animator.AnimatorData animator;

    public Transform rocketRoot;

    [Header("Pumping")]
    public string takePumping;

    [Header("Lift Off")]
    public string takeLiftoff;
        
    public bool isPumping { get; private set; }
    public bool isLiftingOff { get; private set; }
    
    public void StartPump() {
        isPumping = true;
        animator.Play(takePumping);
    }

    public void StartIgnition() {
        //move active blocks to rocket
        var pool = M8.PoolController.GetPool(BlockInfo.poolGroup);

        var paletteData = GameMapController.instance.mapData.initialPalette;
        for(int i = 0; i < paletteData.Length; i++) {
            var activeSpawns = pool.GetActiveList(paletteData[i].blockName);
            if(activeSpawns != null) {
                for(int j = 0; j < activeSpawns.Count; j++) {
                    activeSpawns[j].transform.SetParent(rocketRoot, true);

                    var blockWidgetStatic = activeSpawns[j].GetComponent<BlockWidgetStatic>();
                    if(blockWidgetStatic && blockWidgetStatic.rootConnectActiveGO)
                        blockWidgetStatic.rootConnectActiveGO.SetActive(true);
                }
            }
        }
    }

    public void LiftOff() {
        isLiftingOff = true;
        animator.Play(takeLiftoff);
    }

    void OnDestroy() {
        if(animator)
            animator.takeCompleteCallback -= OnAnimatorTakeFinish;
    }

    void Awake() {
        animator.takeCompleteCallback += OnAnimatorTakeFinish;        
    }

    void Start() {
        var rocketHUDGO = HUD.instance.GetMiscHUD("rocketControl");
        rocketHUDGO.SetActive(true);
    }

    void OnAnimatorTakeFinish(M8.Animator.AnimatorData anim, M8.Animator.AMTakeData take) {
        if(take.name == takePumping)
            isPumping = false;
        else if(take.name == takeLiftoff)
            isLiftingOff = false;
    }
}
