using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper Behaviour to hookup calls to GameFlowController
/// </summary>
public class GameFlowProxy : MonoBehaviour {
    public void ProgressStart() {
        GameFlowController.LoadCurrentProgressScene();
    }

    public void Progress() {
        GameFlowController.ProgressAndLoadNextScene();
    }
}
