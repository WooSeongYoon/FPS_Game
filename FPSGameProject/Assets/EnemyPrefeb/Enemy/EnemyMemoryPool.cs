using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyMemoryPool : MonoBehaviour
{
    [Header("���� ����")]
    [Tooltip("���� ���� ��ǥ ��ġ"), SerializeField]
    private Transform target;
    
    [Tooltip("���� ���� ��ġ�� �˷��ִ� ������"), SerializeField]
    private GameObject enemySpawnPointPrefab;
    [Tooltip("�����Ǵ� �� ������"), SerializeField]
    private GameObject enemyPrefab;
    [Tooltip("�� ���� �ֱ�"), SerializeField]
    private float enemySpawnTime = 1;
    [Tooltip("Ÿ�� ���� �� ���� �����ϱ���� ��� �ð�"), SerializeField]
    private float EnemySpawnLatency = 1;
    [Tooltip("�� ���� ��ġ�� �˷��ִ� ������Ʈ ����, Ȱ��/��Ȱ�� ����"), SerializeField]
    private MemoryPool spawnPointMemoryPool;
    [Tooltip("�� ����, Ȱ��/��Ȱ�� ����"), SerializeField]
    private MemoryPool enemyMemoryPool;
    [Tooltip("�ѹ��� �����Ǵ� ���� ��"), SerializeField]
    private int numberOFEnemiesSpawnedAtOnce = 1;

    [Header("�������� ����")]
    [Tooltip("�������� ũ��"), SerializeField]
    private Vector2Int mapSize = new Vector2Int(100, 100);

    private void Awake()
    {
        spawnPointMemoryPool = new MemoryPool(enemySpawnPointPrefab);
        enemyMemoryPool = new MemoryPool(enemyPrefab);

        StartCoroutine("SpawnTile");
    }


    // �� ����
    private IEnumerator SpawnTile()
    {
        int currentNumber = 0;
        int maximumNumber = 50;

        while ( true )
        {
            for (int i = 0; i < numberOFEnemiesSpawnedAtOnce; ++i) // numberOFEnemiesSpawnedAtOnce ��ŭ �� ����
            {
                GameObject item = spawnPointMemoryPool.ActivatePoolItem();

                item.transform.position = new Vector3(Random.Range(-mapSize.x * 0.49f, mapSize.x * 0.49f), 1,
                                                      Random.Range(-mapSize.y * 0.49f, mapSize.y * 0.49f));
                StartCoroutine("SpawnEnemy",item);
            }
            currentNumber++;

            if (currentNumber >= maximumNumber)
            {
                currentNumber = 0;
                numberOFEnemiesSpawnedAtOnce++;

            }
            yield return new WaitForSeconds(enemySpawnTime);
        }

    }

    private IEnumerator SpawnEnemy(GameObject point)
    {
        yield return new WaitForSeconds(EnemySpawnLatency);
        
        // �� ������Ʈ�� �����ϰ�, ���� ��ġ�� ���� ��ġ�� �̵�
        GameObject item = enemyMemoryPool.ActivatePoolItem();
        item.transform.position = point.transform.position;

        // �� ������Ʈ���� Ÿ���� ����
        item.GetComponent<EnemyFSM>().Setup(target, this);

        // Ÿ�� ������Ʈ ��Ȱ��ȭ
        spawnPointMemoryPool.DeactivatePoolItem(point);


    }

    // �� ��Ȱ��ȭ
    public void DeactivateEnemy(GameObject enemy)
    {
        enemyMemoryPool.DeactivatePoolItem(enemy);
    }

}
