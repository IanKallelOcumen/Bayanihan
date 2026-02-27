using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CircleCollider2D))]
public class Coin : MonoBehaviour
{
	[SerializeField] string playerTag = "Player";
	[SerializeField] int value = 5;
	[SerializeField] UnityEvent OnPickupEvent;

	GameManager gameManager;

	void Start()
	{
		gameManager = GameManager.Instance;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (!collision.CompareTag(playerTag)) return;
		OnPickupEvent.Invoke();
		if (gameManager != null)
			gameManager.AddCoins(value);
		Destroy(gameObject, 2f);
	}
}
