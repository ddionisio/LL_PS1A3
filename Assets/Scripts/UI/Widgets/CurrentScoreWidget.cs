using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentScoreWidget : MonoBehaviour {
    public Text scoreLabel;

	void Start () {
        scoreLabel.text = GameData.instance.currentScore.ToString();
	}
}
