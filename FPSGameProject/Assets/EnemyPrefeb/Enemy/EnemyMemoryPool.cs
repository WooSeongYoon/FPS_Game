using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemyMemoryPool : MonoBehaviour
{
    [Header("몬스터 설정")]
    [Tooltip("적의 공격 목표 위치"), SerializeField]
    private Transform target;
    
    [Tooltip("적의 스폰 위치를 알려주는 프리팹"), SerializeField]
    private GameObject enemySpawnPointPrefab;
    [Tooltip("생성되는 적 프리팹"), SerializeField]
    private GameObject enemyPrefab;
    [Tooltip("적 생성 주기"), SerializeField]
    private float enemySpawnTime = 1;
    [Tooltip("타일 생성 후 적이 등장하기까지 대기 시간"), SerializeField]
    private float EnemySpawnLatency = 1;
    [Tooltip("적 스폰 위치를 알려주는 오브젝트 생성, 활성/비활성 관리"), SerializeField]
    private MemoryPool spawnPointMemoryPool;
    [Tooltip("적 생성, 활성/비활성 관리"), SerializeField]
    private MemoryPool enemyMemoryPool;
    [Tooltip("한번에 생성되는 적의 수"), SerializeField]
    private int numberOFEnemiesSpawnedAtOnce = 1;

    [Header("스테이지 설정")]
    [Tooltip("스테이지 크기"), SerializeField]
    private Vector2Int mapSize = new Vector2Int(100, 100);

    private void Awake()
    {
        spawnPointMemoryPool = new MemoryPool(enemySpawnPointPrefab);
        enemyMemoryPool = new MemoryPool(enemyPrefab);

        StartCoroutine("SpawnTile");
    }


    // 적 생성
    private IEnumerator SpawnTile()
    {
        int currentNumber = 0;
        int maximumNumber = 50;

        while ( true )
        {
            for (int i = 0; i < numberOFEnemiesSpawnedAtOnce; ++i) // numberOFEnemiesSpawnedAtOnce 만큼 적 생성
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
        
        // 적 오브젝트를 생성하고, 적의 위치를 스폰 위치로 이동
        GameObject item = enemyMemoryPool.ActivatePoolItem();
        item.transform.position = point.transform.position;

        // 적 오브젝트에게 타겟을 설정
        item.GetComponent<EnemyFSM>().Setup(target, this);

        // 타일 오브젝트 비활성화
        spawnPointMemoryPool.DeactivatePoolItem(point);


    }

    // 적 비활성화
    public void DeactivateEnemy(GameObject enemy)
    {
        enemyMemoryPool.DeactivatePoolItem(enemy);
    }

}
