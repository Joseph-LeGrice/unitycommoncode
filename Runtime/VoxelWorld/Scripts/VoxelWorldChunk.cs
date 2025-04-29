using UnityEngine;

public class VoxelWorldChunk : MonoBehaviour
{
    [SerializeField]
    private MeshFilter m_meshFilter;
    [SerializeField]
    private MeshCollider m_meshCollider;
    [SerializeField]
    private Mesh m_cachedMesh;
    [SerializeField]
    private Vector3Int m_chunkCoordinate;
    [SerializeField]
    private State m_state = State.Unloaded;

    public enum State
    {
        Unloaded,
        Loaded
    }
    
    public void FinaliseChunkLoad(VoxelWorldChunkBuilder builder)
    {
        m_cachedMesh = builder.GetMesh();
        m_meshFilter.mesh = m_cachedMesh;
        m_meshCollider.sharedMesh = m_cachedMesh;

        builder.Dispose();

        m_chunkCoordinate = builder.GetChunkCoordinate();
        m_state = State.Loaded;
    }
    
    public State GetState()
    {
        return m_state;
    }
    
    public Vector3Int GetLoadedChunkCoordinate()
    {
        return m_chunkCoordinate;
    }
}
