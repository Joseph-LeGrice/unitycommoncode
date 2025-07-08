using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;


public static class PhysicsHelpers
{
    public static float3 GetAngularVelocityFromToRotation(float deltaTime, float maxSpeed, in PhysicsMass physicsMass, quaternion from, quaternion to)
    {
        float3 fromForward = new float3(0.0f, 0.0f, 1.0f);
        fromForward = math.mul(from, fromForward);
        float3 fromUp = new float3(0.0f, 1.0f, 0.0f);
        fromUp = math.mul(from, fromUp);
        
        float3 toForward = new float3(0.0f, 0.0f, 1.0f);
        toForward = math.mul(to, toForward);
        float3 toUp = new float3(0.0f, 1.0f, 0.0f);
        toUp = math.mul(to, toUp);
        
        quaternion forwardRotation = MathHelpers.GetFromToRotation(fromForward, toForward);
        quaternion upRotation = MathHelpers.GetFromToRotation(fromUp, toUp);
        quaternion finalRotation = math.mul(forwardRotation, upRotation);
            
        float3 angularVelocity = float3.zero;
        float rotationAngle = 2.0f * math.acos(finalRotation.value.w);
        if (rotationAngle != 0.0f)
        {
            float3 rotationAxis = math.normalize(finalRotation.value.xyz);
            float rotationSpeed = maxSpeed * math.clamp(math.degrees(rotationAngle) / 90.0f, 0, 1);
            angularVelocity = rotationSpeed * deltaTime * rotationAxis;
            
            quaternion inertiaOrientationInWorldSpace = math.mul(from, physicsMass.InertiaOrientation);
            angularVelocity = math.rotate(math.inverse(inertiaOrientationInWorldSpace), angularVelocity);
        }

        return angularVelocity;
    }
}
