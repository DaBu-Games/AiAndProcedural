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
    [SerializeField] private int boidsCount = 50;
    [SerializeField] float cohesionWeight = 0.01f;
    [SerializeField] float alignmentWeight = 0.05f;
    [SerializeField] float separationWeight = 0.1f;
    [SerializeField] float maxVelocity = 1f;
    [SerializeField] float minVelocity = 0.1f;

    [SerializeField] private Bounds boidBounds = new Bounds(Vector3.zero, new Vector3(20, 20, 20));
    [SerializeField] private float minRadius = 0.5f;
    [SerializeField] private float maxRadius = 1.5f;
    [SerializeField] private float cellSize = 2f;

    [Header("Query settings")]
    //[SerializeField] private Transform querySphere;
    [SerializeField]
    private float queryRadiusMultiplier = 1.5f;

    [SerializeField] private bool showGird = false;

    private GameObject[] _boidsInstances;
    private Renderer[] _boidsRenderers;

    NativeArray<Boid> _boidsNative;
    NativeArray<Boid> _boidsNext;
    NativeArray<HashAndIndex> _hashAndIndices;

    struct Boid
    {
        public float3 Position;
        public float3 Velocity;
        public float Radius;
    }

    struct HashAndIndex : IComparable<HashAndIndex>
    {
        public int Hash;
        public int Index;

        public int CompareTo(HashAndIndex other)
        {
            return Hash.CompareTo(other.Hash);
        }
    }

    static int Hash(int3 gridPos)
    {
        unchecked
        {
            return gridPos.x * 73856093 ^ gridPos.y * 19349663 ^ gridPos.z * 83492791;
        }
    }

    static int3 GridPosition(float3 position, float cellSize)
    {
        return new int3(math.floor(position / cellSize));
    }

    void Start()
    {
        InitializeBoids();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        //if (querySphere != null)
        //Gizmos.DrawWireSphere(querySphere.position, queryRadius);

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
                    Vector3 cellCenter = boidBounds.min + new Vector3(x, y, z) * cellSize +
                                         Vector3.one * (cellSize / 2f);
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
        _boidsNext = new NativeArray<Boid>(boidsCount, Allocator.Persistent);
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
                Random.Range(-maxVelocity, maxVelocity),
                Random.Range(-maxVelocity, maxVelocity),
                Random.Range(-maxVelocity, maxVelocity)
            );

            _boidsNative[i] = new Boid
            {
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
        if (!_boidsNative.IsCreated)
            return;

        HashBoidsJob hashJob = new HashBoidsJob
        {
            Boids = _boidsNative,
            CellSize = cellSize,
            HashAndIndices = _hashAndIndices,
        };

        JobHandle hashJobHandle = hashJob.Schedule(_boidsNative.Length, 64);

        SortHashCodesJob sortJob = new SortHashCodesJob
        {
            HashAndIndices = _hashAndIndices
        };

        JobHandle sortJobHandle = sortJob.Schedule(hashJobHandle);

        BoidsVelocityJob velocityJob = new BoidsVelocityJob
        {
            Boids = _boidsNative,
            BoidsNext = _boidsNext,
            HashAndIndices = _hashAndIndices,
            BoundsMin = boidBounds.min,
            BoundsMax = boidBounds.max,
            QueryRadiusMultiplier = queryRadiusMultiplier,
            CellSize = cellSize,
            MaxVelocity = maxVelocity,
            MinVelocity = minVelocity,
            CohesionWeight = cohesionWeight,
            AlignmentWeight = alignmentWeight,
            SeparationWeight = separationWeight,
        };

        JobHandle updateJobHandle = velocityJob.Schedule(_boidsNative.Length, 64, sortJobHandle);

        BoidsPositionJob positionJob = new BoidsPositionJob
        {
            BoidsRead = _boidsNext,
            BoidsWrite = _boidsNative,
            DeltaTime = Time.deltaTime,
        };

        JobHandle positionJobHandle = positionJob.Schedule(_boidsNative.Length, 64, updateJobHandle);

        positionJobHandle.Complete();

        //(_boidsNative, _boidsNext) = (_boidsNext, _boidsNative);

        for (int i = 0; i < _boidsNative.Length; i++)
            _boidsInstances[i].transform.position = _boidsNative[i].Position;
    }

    private void OnDestroy()
    {
        if (_boidsNative.IsCreated)
            _boidsNative.Dispose();
        
        if (_boidsNext.IsCreated)
            _boidsNext.Dispose();

        if (_hashAndIndices.IsCreated)
            _hashAndIndices.Dispose();
    }

    [BurstCompile]
    struct HashBoidsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Boid> Boids;
        public NativeArray<HashAndIndex> HashAndIndices;
        public float CellSize;

        public void Execute(int index)
        {
            Boid boid = Boids[index];
            int hash = Hash(GridPosition(boid.Position, CellSize));

            HashAndIndices[index] = new HashAndIndex { Hash = hash, Index = index };
        }
    }

    [BurstCompile]
    struct SortHashCodesJob : IJob
    {
        public NativeArray<HashAndIndex> HashAndIndices;

        public void Execute()
        {
            HashAndIndices.Sort();
        }
    }

    [BurstCompile]
    struct BoidsVelocityJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Boid> Boids;
        public NativeArray<Boid> BoidsNext;

        [ReadOnly] public NativeArray<HashAndIndex> HashAndIndices;
        public float3 BoundsMin;
        public float3 BoundsMax;
        public float QueryRadiusMultiplier;
        public float CellSize;
        public float MaxVelocity;
        public float MinVelocity;
        public float CohesionWeight;
        public float AlignmentWeight;
        public float SeparationWeight;

        public void Execute(int index)
        {
            Boid boid = Boids[index];

            float3 cohesion = float3.zero;
            float3 separation = float3.zero;
            float3 alignment = float3.zero;
            int neighborCount = 0;

            float queryRadius = boid.Radius * QueryRadiusMultiplier;
            float radiusSquared = queryRadius * queryRadius;
            int3 minGridPos = GridPosition(boid.Position - queryRadius, CellSize);
            int3 maxGridPos = GridPosition(boid.Position + queryRadius, CellSize);

            for (int x = minGridPos.x; x <= maxGridPos.x; x++)
            {
                for (int y = minGridPos.y; y <= maxGridPos.y; y++)
                {
                    for (int z = minGridPos.z; z <= maxGridPos.z; z++)
                    {
                        int3 gridPos = new int3(x, y, z);
                        int hash = Hash(gridPos);

                        int startIndex = BinarySearch(HashAndIndices, hash);

                        if (startIndex < 0)
                            continue;

                        for (int i = startIndex; i < HashAndIndices.Length && HashAndIndices[i].Hash == hash; i++)
                        {
                            int boidIndex = HashAndIndices[i].Index;
                            if(boidIndex == index)
                                continue;
                            
                            Boid boidTwo = Boids[boidIndex];
                            float3 toBoid = boidTwo.Position - boid.Position;

                            if (math.lengthsq(toBoid) <= radiusSquared)
                            {
                                neighborCount++;

                                cohesion += boidTwo.Position;
                                separation -= toBoid;

                                alignment += boidTwo.Velocity;
                            }
                        }
                    }
                }
            }

            if (neighborCount > 0)
            {
                cohesion /= neighborCount;
                alignment /= neighborCount;
                
                float3 cohesionDir = cohesion - boid.Position;
                float3 alignmentDir = alignment - boid.Velocity;

                boid.Velocity += cohesionDir * CohesionWeight + separation * SeparationWeight + alignmentDir * AlignmentWeight;
            }
            
            float3 pos = boid.Position;
            float margin = 1f;
            
            if (pos.x < BoundsMin.x + margin) boid.Velocity.x = math.abs(boid.Velocity.x);
            if (pos.y < BoundsMin.y + margin) boid.Velocity.y = math.abs(boid.Velocity.y);
            if (pos.z < BoundsMin.z + margin) boid.Velocity.z = math.abs(boid.Velocity.z);

            if (pos.x > BoundsMax.x - margin) boid.Velocity.x = -math.abs(boid.Velocity.x);
            if (pos.y > BoundsMax.y - margin) boid.Velocity.y = -math.abs(boid.Velocity.y);
            if (pos.z > BoundsMax.z - margin) boid.Velocity.z = -math.abs(boid.Velocity.z);
            
            float speed =  math.length(boid.Velocity);
            if (speed > MaxVelocity)
                boid.Velocity = boid.Velocity / speed * MaxVelocity;
            
            if(speed <= MinVelocity)
                boid.Velocity = boid.Velocity / speed * MinVelocity;

            BoidsNext[index] = boid;
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


    [BurstCompile]
    struct BoidsPositionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Boid> BoidsRead;
        public NativeArray<Boid> BoidsWrite;
        public float DeltaTime;

        public void Execute(int index)
        {
            Boid boid = BoidsRead[index];
            boid.Position += boid.Velocity * DeltaTime;
            BoidsWrite[index] = boid;
        }
    }
}
