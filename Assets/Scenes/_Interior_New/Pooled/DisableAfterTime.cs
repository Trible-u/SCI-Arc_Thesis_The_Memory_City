using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterTime : MonoBehaviour {
	public float lifeTime = 4f;
	// Use this for initialization
	void OnEnable () {
		StartCoroutine(Disabler());
	}
	private IEnumerator Disabler()
	{
		yield return new WaitForSeconds(lifeTime);
		gameObject.SetActive(false);
	}

}
