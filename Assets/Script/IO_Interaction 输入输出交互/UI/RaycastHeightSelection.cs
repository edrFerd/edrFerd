using UnityEngine;

/// <summary>
/// 获取光标在矩形碰撞器指定高度的内部平面坐标 外部射线结果输入版
/// </summary>
public class RaycastHeightSelection : MonoBehaviour
{
    /// <summary>
    /// 获取光标在矩形碰撞器指定高度的内部平面坐标(世界坐标)
    /// </summary>
    /// <param name="hit">应该从Camera.main.ScreenPointToRay(Input.mousePosition)获取</param>
    /// <param name="targetHeight">目标高度（相对于世界坐标）</param>
    /// <returns>光标不在选定层上则返回Vector3.zero</returns>
    public Vector3 Main(RaycastHit hit, float targetHeight)
    {
        // 获取碰撞点
        Vector3 hitPoint = hit.point; 
        // 创建从碰撞点到平面的方向向量
        Ray ray = new Ray(hitPoint, Vector3.up);
        // 在目标高度创建一个平面（平行于XZ平面）
        Plane heightPlane = new Plane(Vector3.up, new Vector3(0, targetHeight, 0));

        // 检查射线是否与平面相交
        if (heightPlane.Raycast(ray, out float enter))
        {
            // 计算射线与平面的交点
            Vector3 intersectionPoint = ray.GetPoint(enter);
            
            // 检查交点是否在BoxCollider内部
            BoxCollider boxCollider = hit.collider as BoxCollider;
            if (IsPointInsideBoxCollider(intersectionPoint, boxCollider))
            {
                Debug.Log("Intersection Point inside BoxCollider: " + intersectionPoint);
                return intersectionPoint;
            }
            else
            {
                Debug.Log("Intersection Point is outside BoxCollider.");
                return Vector3.zero;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 获取光标在矩形碰撞器指定高度的内部平面坐标(世界坐标)
    /// </summary>
    /// <param name="boxCollider">目标BoxCollider对象</param>
    /// <param name="targetHeight">目标高度（相对于世界坐标）</param>
    /// <returns>光标不在选定层上则返回Vector3.zero</returns>
    public Vector3 Main(BoxCollider boxCollider, float targetHeight)
    {
        // 获取鼠标在屏幕上的位置
        Vector3 mousePosition = Input.mousePosition;
        // 从相机发射一条射线
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        // 在目标高度创建一个平面（平行于XZ平面）
        Plane heightPlane = new Plane(Vector3.up, new Vector3(0, targetHeight, 0));

        // 检查射线是否与平面相交
        if (heightPlane.Raycast(ray, out float enter))
        {
            // 计算射线与平面的交点
            Vector3 intersectionPoint = ray.GetPoint(enter);

            // 检查交点是否在BoxCollider内部
            if (IsPointInsideBoxCollider(intersectionPoint, boxCollider))
            {
                Debug.Log("Intersection Point inside BoxCollider: " + intersectionPoint);
                return intersectionPoint;
            }
            else
            {
                Debug.Log("Intersection Point is outside BoxCollider.");
                return Vector3.zero;
            }
        }

        return Vector3.zero;
    }

    // 检查一个点是否在BoxCollider的范围内
    bool IsPointInsideBoxCollider(Vector3 point, BoxCollider boxCollider)
    {
        // 获取BoxCollider的大小和中心点
        Vector3 colliderMin = boxCollider.bounds.min;
        Vector3 colliderMax = boxCollider.bounds.max;

        // 检查点是否在BoxCollider的范围内
        return (point.x >= colliderMin.x && point.x <= colliderMax.x) &&
               (point.y >= colliderMin.y && point.y <= colliderMax.y) &&
               (point.z >= colliderMin.z && point.z <= colliderMax.z);
    }

    // 在指定位置创建一个小方块
    void CreateCubeAtPosition(Vector3 position)
    {
        // 创建一个新的GameObject
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;
        cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); // 设置小方块的尺寸
        cube.GetComponent<Renderer>().material.color = Color.red; // 设置小方块的颜色
    }
}
