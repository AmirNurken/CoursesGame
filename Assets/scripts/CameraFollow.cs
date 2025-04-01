using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // Цель, за которой следует камера (персонаж)
    public float height = 10f; // Высота камеры
    public float angle = 10f; // Угол наклона камеры
    public float distance = 10f; // Расстояние от персонажа 

    private void LateUpdate()
    {
        if (target == null) return;

        FollowTarget();
    }

    private void FollowTarget()
    {
        // Вычисляем позицию камеры
        Quaternion rotation = Quaternion.Euler(angle, 0f, 0f); // Фиксированный наклон
        Vector3 offset = rotation * new Vector3(0, height, -distance);
        Vector3 desiredPosition = target.position + offset;

        // Камера всегда смотрит на персонажа
        transform.position = desiredPosition;
        transform.LookAt(target);
    }
}
