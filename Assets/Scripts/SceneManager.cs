using System.Collections;
using UnityEngine;
using sm = UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
	public static SceneManager Instance { get; private set; }

	[SerializeField] Animation transition;
	[SerializeField] float transitionTime = 0.1f;

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	void OnDestroy()
	{
		if (Instance == this) Instance = null;
	}

	public void ResetLevel()
	{
		int currentSceneIndex = sm.SceneManager.GetActiveScene().buildIndex;
		StartCoroutine(LoadScene(currentSceneIndex));
	}

	public void PreviousScene()
	{
		int previousSceneIndex = sm.SceneManager.GetActiveScene().buildIndex - 1;
		StartCoroutine(LoadScene(previousSceneIndex));
	}

	public void NextScene()
	{
		int nextSceneIndex = sm.SceneManager.GetActiveScene().buildIndex + 1;
		StartCoroutine(LoadScene(nextSceneIndex));
	}

	public IEnumerator LoadScene(int sceneIndex)
	{
		if (transition != null)
			transition.PlayQueued("Fade_Start");
		yield return new WaitForSeconds(transitionTime);
		sm.SceneManager.LoadScene(sceneIndex);
	}
}
