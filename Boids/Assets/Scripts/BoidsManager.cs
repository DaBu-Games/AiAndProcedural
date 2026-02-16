using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BoidsManager : MonoBehaviour
{
    [Header("Boid settings")]
    [SerializeField] private GameObject boidPrefab;
    [FormerlySerializedAs("boidCount")] [SerializeField] private int boidsCount = 50;
    [SerializeField] private Bounds boidBounds = new Bounds(Vector3.zero, new Vector3(20, 20, 20));
    [SerializeField] private float minRadius = 0.5f;
    [SerializeField] private float maxRadius = 1.5f;
    [SerializeField] private float cellSize = 2f;
    
    [Header("Query settings")]
    [SerializeField] private Transform querySphere;
    [SerializeField] private float queryRadius = 5f;
    [SerializeField] private bool showGird = false;
    
    private GameObject[] _boidsInstances;
    private Renderer[] _boidsRenderers;
    
    NativeArray<Boid> _boidsNative;
    NativeArray<HashAndIndex> _hashAndIndices;
    private NativeList<int> _resultIndices; 

    struct Boid{
        public float3 Position;
        public float3 Velocity;
        public float Radius;
    }

    struct HashAndIndex : IComparable<HashAndIndex>{
        public int Hash;
        public int Index;

        public int CompareTo(HashAndIndex other)
        {
            return Hash.CompareTo(other.Hash);
        }
    }

    static int Hash(int3 gridPos){
        unchecked {
            return gridPos.x * 73856093 ^ gridPos.y * 19349663 ^ gridPos.z * 83492791;
        }
    }
    
    static int3 GridPosition(float3 position, float cellSize){
        return new int3(math.floor(position / cellSize));
    }
    
    void Start()
    {
        InitializeBoids();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        if (querySphere != null)
            Gizmos.DrawWireSphere(querySphere.position, queryRadius);
        
        if (showGird)
            DrawSpatialGrid();
    }

    private void DrawSpatialGrid()
    {
        int gridCountX = Mathf.CeilToInt(boidBounds.size.x / cellSize);
        int gridCountY = Mathf.CeilToInt(boidBounds.size.y / cellSize);
        int gridCountZ = Mathf.CeilToInt(boidBounds.size.z / cellSize);

        for (int x = 0; x < gridCountX; x++)
        {
            for (int y = 0; y < gridCountY; y++)
            {
                for (int z = 0; z < gridCountZ; z++)
                {
                    Vector3 cellCenter = boidBounds.min + new Vector3(x, y, z) * cellSize + Vector3.one * (cellSize / 2f);
                    Gizmos.DrawWireCube(cellCenter, Vector3.one * cellSize);
                }
            }
        }
    }

    private void InitializeBoids()
    {
        _boidsInstances = new GameObject[boidsCount];
        _boidsRenderers = new Renderer[boidsCount];
        _boidsNative = new NativeArray<Boid>(boidsCount, Allocator.Persistent);
        _hashAndIndices = new NativeArray<HashAndIndex>(boidsCount, Allocator.Persistent);

        for (int i = 0; i < boidsCount; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(boidBounds.min.x, boidBounds.max.x),
                Random.Range(boidBounds.min.y, boidBounds.max.y),
                Random.Range(boidBounds.min.z, boidBounds.max.z)
            );
            float radius = Random.Range(minRadius, maxRadius);

            Vector3 velocity = new Vector3(
                Random.Range(-1f, 1f), 
                Random.Range(-1f, 1f), 
                Random.Range(-1f, 1f)
            );

            _boidsNative[i] = new Boid {
                Position = position,
                Velocity = velocity,
                Radius = radius
            };
            
            GameObject boid = Instantiate(boidPrefab, position, Quaternion.identity);
            boid.transform.localScale = Vector3.one * radius * 2f;
            boid.transform.SetParent(transform);
            _boidsInstances[i] = boid;
            _boidsRenderers[i] = boid.GetComponent<Renderer>();
        }
    }

    private void Update()
    {
        if(!_boidsNative.IsCreated) 
            return;

        UpdateBoidsJob updateJob = new UpdateBoidsJob
        {
            Boids = _boidsNative,
            BoundsMin = boidBounds.min,
            BoundsMax = boidBounds.max,
            DeltaTime = Time.deltaTime
        };
        
        JobHandle updateJobHandle = updateJob.Schedule(_boidsNative.Length, 64);

        HashBoidsJob hashJob = new HashBoidsJob {
            Boids = _boidsNative,
            CellSize = cellSize,
            HashAndIndices = _hashAndIndices,
        };
        
        JobHandle hashJobHandle = hashJob.Schedule(_boidsNative.Length, 64, updateJobHandle);

        SortHashCodesJob sortJob = new SortHashCodesJob
        {
            HashAndIndices = _hashAndIndices
        };
        
        JobHandle sortJobHandle = sortJob.Schedule(hashJobHandle);

        QueryJob queryJob = new QueryJob
        {
            Boids = _boidsNative,
            HashAndIndices = _hashAndIndices,
            QueryPosition = querySphere.position,
            QueryRadius = queryRadius,
            CellSize = cellSize,
            ResultIndices = new NativeList<int>(Allocator.TempJob)
        };
        
        JobHandle queryJobHandle = queryJob.Schedule(sortJobHandle);
        queryJobHandle.Complete();
        
        if(_resultIndices.IsCreated) 
            _resultIndices.Dispose();

        _resultIndices = queryJob.ResultIndices;

        foreach (Renderer render in _boidsRenderers)
        {
            render.material.color = Color.white;
        }

        foreach (int index in _resultIndices)
        {
            _boidsRenderers[index].material.color = Color.red;
        }

        for (int i = 0; i < _boidsNative.Length; i++)
            _boidsInstances[i].transform.position = _boidsNative[i].Position;
    }

    private void OnDestroy()
    {
        if(_boidsNative.IsCreated)
            _boidsNative.Dispose();
        
        if(_hashAndIndices.IsCreated)
            _hashAndIndices.Dispose();
        
        if(_resultIndices.IsCreated)
            _resultIndices.Dispose();
    }
    
    
    [BurstCompile]
    struct UpdateBoidsJob : IJobParallelFor {
        public NativeArray<Boid> Boids;
        public float3 BoundsMin;
        public float3 BoundsMax;
        public float DeltaTime;
        
        public void Execute(int index) {
            Boid boid = Boids[index];
            boid.Position += boid.Velocity * DeltaTime;
            
            // boid functions 
            
            Boids[index] = boid;
        }
    }

    [BurstCompile]
    struct HashBoidsJob : IJobParallelFor {
        [ReadOnly] public NativeArray<Boid> Boids;
        public NativeArray<HashAndIndex> HashAndIndices;
        public float CellSize;
        
        public void Execute(int index){
            Boid boid = Boids[index];
            int hash = Hash( GridPosition(boid.Position, CellSize));

            HashAndIndices[index] = new HashAndIndex { Hash = hash, Index = index };
        }
    }

    [BurstCompile]
    struct SortHashCodesJob : IJob{
        public NativeArray<HashAndIndex> HashAndIndices;

        public void Execute()
        {
            HashAndIndices.Sort();
        }
    }

    [BurstCompile]
    struct QueryJob : IJob {
        [ReadOnly] public NativeArray<Boid> Boids;
        [ReadOnly] public NativeArray<HashAndIndex> HashAndIndices;
        public float3 QueryPosition;
        public float QueryRadius;
        public float CellSize;
        public NativeList<int> ResultIndices;

        public void Execute()
        {
            float radiusSquared = QueryRadius * QueryRadius;
            int3 minGridPos = GridPosition(QueryPosition - QueryRadius, CellSize);
            int3 maxGridPos = GridPosition(QueryPosition + QueryRadius, CellSize);

            for (int x = minGridPos.x; x <= maxGridPos.x; x++)
            {
                for (int y = minGridPos.y; y <= maxGridPos.y; y++)
                {
                    for (int z = minGridPos.z; z <= maxGridPos.z; z++) 
                    {
                        int3 gridPos = new int3(x, y, z);
                        int hash = Hash(gridPos);
                        
                        int startIndex = BinarySearch(HashAndIndices, hash);
                        
                        if(startIndex < 0 )
                            continue;

                        for (int i = startIndex; i < HashAndIndices.Length && HashAndIndices[i].Hash == hash; i++) {
                            int boidIndex = HashAndIndices[i].Index;
                            Boid boid = Boids[boidIndex];
                            float3 toBoid = boid.Position - QueryPosition;

                            if (math.lengthsq(toBoid) <= radiusSquared)
                            {
                                ResultIndices.Add(boidIndex);
                            }
                        }
                    }
                }
            }
        }

        int BinarySearch(NativeArray<HashAndIndex> array, int hash)
        {
            int left = 0;
            int right = array.Length - 1;
            int result = -1;

            while (left <= right)
            {
                int middle = (left + right) / 2;
                int midHash = array[middle].Hash;

                if (midHash == hash)
                {
                    result = middle;
                    right = middle - 1;
                } 
                else if (midHash < hash)
                {
                    left = middle + 1;
                }
                else
                {
                    right = middle - 1;
                }
            }
            
            return result;
        }
    }
}
