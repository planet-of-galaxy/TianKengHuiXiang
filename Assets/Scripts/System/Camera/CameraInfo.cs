using UnityEngine;

public struct CameraInfo
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float FOV;

    public CameraInfo(Vector3 position, Quaternion rotation, float fov)
    {
        Position = position;
        Rotation = rotation;
        FOV = fov;
    }

    public CameraInfo(Vector3 position, Vector3 eulerAngles, float fov)
    {
        Position = position;
        Rotation = Quaternion.Euler(eulerAngles);
        FOV = fov;
    }
}
