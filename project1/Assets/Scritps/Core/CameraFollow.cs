using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;

    private GameObject player;

    private void LateUpdate()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        target = player != null ? player.transform : null;
        if (target == null) return;

        Vector3 newPosition = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            newPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}
