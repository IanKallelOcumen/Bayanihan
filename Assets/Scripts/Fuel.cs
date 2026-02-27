using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Fuel : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
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
			gameManager.Refuel();
		Destroy(gameObject, 2f);
	}
}
