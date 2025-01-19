using FPSControllerLPFP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �� �߻�ü ����
public class EnemyProjectile : MonoBehaviour
{
    private MovementTransform movement;
    [Tooltip("�߻�ü �ִ� �߻� �Ÿ�"), SerializeField]
    private float projectileDistance = 30;
    [Tooltip("���� ������"), SerializeField]
    private int damage = 20;

    public void Setup(Vector3 position)
    {
        movement = GetComponent<MovementTransform>();
        StartCoroutine("OnMove",position);
    }

    private IEnumerator OnMove(Vector3 targetPosition)
    {
        Vector3 start = transform.position;

        movement.MoveTo((targetPosition - transform.position).normalized);

        while (true)
        {
            if (Vector3.Distance(transform.position, start) >= projectileDistance)
            {
                Destroy(gameObject);

                yield break;
            }
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //Debug.Log("Player Hit");

            other.GetComponent<FpsControllerLPFP>().TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
