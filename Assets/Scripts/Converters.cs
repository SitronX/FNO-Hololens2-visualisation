using UnityEngine;

public class Converters : MonoBehaviour
{
    public static TransformSave ConvertTransform(Transform transform)
    {
        TransformSave save;
        save.LocalPosition = transform.localPosition;
        save.LocalRotationEuler = transform.localRotation.eulerAngles;
        save.LocalScale = transform.localScale;

        return save;
    }
    public static void UpdateTransform(Transform transform, TransformSave save)
    {
        transform.localPosition = save.LocalPosition;
        transform.localRotation = Quaternion.Euler(save.LocalRotationEuler);
        transform.localScale = save.LocalScale;      
    }
}

public struct TransformSave
{
    public Vector3 LocalPosition;
    public Vector3 LocalRotationEuler;
    public Vector3 LocalScale;
}

