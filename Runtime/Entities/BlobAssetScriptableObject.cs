using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

public struct AssetLookup : IComponentData  // Workaround for WeakObjectReference<Object> not available in blobs yet
{
    public Hash128 AssetHash;
    public UntypedWeakReferenceId AssetReference;

    public static UntypedWeakReferenceId FindAssetReference()
    {
        throw new System.NotImplementedException();
    }
}

public abstract class BlobAssetScriptableObject<T> : ScriptableObject where T : unmanaged
{
    protected abstract void PopulateBlob(IBaker baker, BlobBuilder builder, ref T blobData);

    protected static Hash128 RegisterAssetReference(IBaker baker, Object asset) // Workaround for WeakObjectReference<Object> not available in blobs yet
    {
        Hash128 assetHash =  new Hash128((uint)asset.GetHashCode());
        Entity assetRef = baker.CreateAdditionalEntity(TransformUsageFlags.None, entityName: "AssetLookup_"+asset.name); // May end up with duplicates?
        baker.AddComponent(assetRef,
            new AssetLookup()
            {
                AssetHash = assetHash,
                AssetReference = UntypedWeakReferenceId.CreateFromObjectInstance(asset)
            }
        );
        return assetHash;
    }
        
    public BlobAssetReference<T> GetBlobDataReference(IBaker baker)
    {
        Hash128 hashCode = new Hash128((uint)GetHashCode());
        if (!baker.TryGetBlobAssetReference(hashCode, out BlobAssetReference<T> result))
        {
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            
            ref T blobData = ref builder.ConstructRoot<T>();
            PopulateBlob(baker, builder, ref blobData);
            result = builder.CreateBlobAssetReference<T>(Allocator.Persistent);
            
            builder.Dispose();
            
            baker.AddBlobAssetWithCustomHash(ref result, hashCode);
        }
        return result;
    }
}