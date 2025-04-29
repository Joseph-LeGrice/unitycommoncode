using System;
using UnityEngine;

public class OrbitZoomCamera : MonoBehaviour
{
    [SerializeField]
    private float m_orbitSpeed;
    [SerializeField]
    private Transform m_orbitTransform;
    [SerializeField]
    private bool m_invertX;
    [SerializeField]
    private bool m_invertY;
    
    [SerializeField]
    private Transform m_zoomTransform;
    [SerializeField]
    private float m_zoomSpeed;
    [SerializeField]
    private float m_minZoom;
    [SerializeField]
    private float m_maxZoom;

    private float m_currentPitch;
    private float m_currentYaw;
    private float m_currentZoom;

    private void Awake()
    {
        m_currentZoom = m_minZoom + 0.5f * (m_maxZoom - m_minZoom);
    }

    public void Move(Vector2 delta, float dt)
    {
        float zoomDist = Mathf.Max(Mathf.Abs(m_zoomTransform.localPosition.z), 1.0f);
        float yawDelta = (m_invertX ? -1.0f : 1.0f) * m_orbitSpeed * dt * Mathf.Rad2Deg * Mathf.Atan2(delta.x, zoomDist);
        float pitchDelta = (m_invertY ? 1.0f : -1.0f) * m_orbitSpeed * dt * Mathf.Rad2Deg * Mathf.Atan2(delta.y, zoomDist);

        m_currentPitch = Mathf.Clamp(m_currentPitch + pitchDelta, -0.0f, 89.0f);
        m_currentYaw = (m_currentYaw + yawDelta) % 360f;
        m_orbitTransform.localRotation = Quaternion.Euler(m_currentPitch, m_currentYaw, 0.0f);
    }

    public void Zoom(float delta, float dt)
    {
        delta = m_zoomSpeed * dt * -Mathf.Sign(delta) * Mathf.Clamp01(Mathf.Abs(delta));
        m_currentZoom = Mathf.Clamp(m_currentZoom + delta, Mathf.Abs(m_minZoom), Mathf.Abs(m_maxZoom));
        m_zoomTransform.localPosition = new Vector3(0.0f, 0.0f, -m_currentZoom);
    }
}
