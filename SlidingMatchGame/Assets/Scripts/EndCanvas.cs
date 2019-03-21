using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndCanvas : MonoBehaviour {
	public Text scoreText;

	public void End(int score){
		scoreText.text = score.ToString();
	}
	public void Restart(){
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
