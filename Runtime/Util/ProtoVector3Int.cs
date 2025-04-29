using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;

[ProtoContract]
public class ProtoVector3Int
{
    [ProtoMember(1)]
    private int x;
    [ProtoMember(2)]
    private int y;
    [ProtoMember(3)]
    private int z;

    public static implicit operator Vector3Int(ProtoVector3Int v)
    {
        return v == null ? Vector3Int.zero : new Vector3Int(v.x, v.y, v.z);
    }
    
    public static implicit operator ProtoVector3Int(Vector3Int v)
    {
        return new ProtoVector3Int() { x = v.x, y = v.y, z = v.z };
    }

    public static void Register()
    {
        if (!RuntimeTypeModel.Default.IsDefined(typeof(Vector3Int)))
        {
            RuntimeTypeModel.Default.Add(typeof(Vector3Int), false)
                .SetSurrogate(typeof(ProtoVector3Int));
        }
    }
}
