using FPSControllerLPFP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 적 발사체 제어
public class EnemyProjectile : MonoBehaviour
{
    private MovementTransform movement;
    [Tooltip("발새체 최대 발사 거리"), SerializeField]
    private float projectileDistance = 30;
    [Tooltip("몬스터 데미지"), SerializeField]
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
