using UnityEngine;

public class MovementTransform : MonoBehaviour
{
    [Tooltip("몬스터 이동 속도"), SerializeField]
    private float moveSpeed = 0.0f;
    [Tooltip("몬스터 이동 방향"), SerializeField]
    private Vector3 moveDirection = Vector3.zero;


    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    public void MoveTo(Vector3 direction)
    {
        moveDirection = direction;
    }
}
