using UnityEngine;

public class MovementTransform : MonoBehaviour
{
    [Tooltip("���� �̵� �ӵ�"), SerializeField]
    private float moveSpeed = 0.0f;
    [Tooltip("���� �̵� ����"), SerializeField]
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
