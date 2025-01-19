using UnityEngine;
using System.Collections;

public class TargetScript : MonoBehaviour {

	float randomTime;
	bool routineStarted = false;
	public bool isHit = false;

	[Header("Customizable Options")]
	public float minTime;
	public float maxTime;

	[Header("Audio")]
	public AudioClip upSound;
	public AudioClip downSound;
	public AudioSource audioSource;
	
	private void Update (){

		randomTime = Random.Range (minTime, maxTime);

		if (isHit == true) 
		{
			if (routineStarted == false) 
			{
				// 총에 맞았을 때, target_down 애니메이션 실행
				gameObject.GetComponent<Animation> ().Play("target_down");

				// 다운 사운드를 현재 사운드로 설정하고 재생
				audioSource.GetComponent<AudioSource>().clip = downSound;
				audioSource.Play();

				StartCoroutine(DelayTimer());
				routineStarted = true;
			} 
		}
	}

	private IEnumerator DelayTimer () {
		// randomTime 만큼 대기 후에 target_up 애니메이션 실행
		yield return new WaitForSeconds(randomTime);
		gameObject.GetComponent<Animation> ().Play ("target_up");

		audioSource.GetComponent<AudioSource>().clip = upSound;
		audioSource.Play();

		isHit = false;
		routineStarted = false;
	}
}