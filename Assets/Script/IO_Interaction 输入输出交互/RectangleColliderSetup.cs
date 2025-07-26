using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RectangleColliderSetup : MonoBehaviour
{
    private BoxCollider thisTriggerCollider; // 自身已有的碰撞器
    private Collider[] overlappingColliders_1; // 存储重叠的物体

    public void Init()
    {
        thisTriggerCollider = this.GetComponent<BoxCollider>();
    }

    //可视化碰撞盒子
    private void OnDrawGizmos()
    {
        // 将 Gizmos 颜色设为红色
        Gizmos.color = Color.red;

        // 绘制盒子的边界
        Gizmos.matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, this.transform.localScale + new Vector3(1, 0, 0));
        Gizmos.DrawWireCube(Vector3.zero, this.transform.localScale + new Vector3(0, 1, 0));
        Gizmos.DrawWireCube(Vector3.zero, this.transform.localScale + new Vector3(0, 0, 1));

    }

    /// <summary>
    /// 检测创建物体与其他物体的重叠与重叠依赖关系
    /// </summary>
    /// <returns></returns>
    public (Collider[] overlappingColliders, bool overlapWithOtherObjects) Main()
    {
        this.overlappingColliders_1 = null;
        //排除边角的边缘外1unity单位重合
        var overlap1 = Physics.OverlapBox(this.transform.position, this.transform.localScale / 2 + (new Vector3(0.9f, -0.1f, -0.1f)), this.transform.rotation);
        var overlap2 = Physics.OverlapBox(this.transform.position, this.transform.localScale / 2 + (new Vector3(-0.1f, 0.9f, -0.1f)), this.transform.rotation);
        var overlap3 = Physics.OverlapBox(this.transform.position, this.transform.localScale / 2 + (new Vector3(-0.1f, -0.1f, 0.9f)), this.transform.rotation);
        //内在重合
        var overlap4 = Physics.OverlapBox(this.transform.position, this.transform.localScale / 2 + (new Vector3(-0.01f, -0.01f, -0.01f)), this.transform.rotation);

        // 使用 Concat 合并结果，并过滤掉 thisTriggerCollider
        overlappingColliders_1 = overlap1.Concat(overlap2).Concat(overlap3)
            .Where(c => c != thisTriggerCollider) // 过滤掉 thisTriggerCollider
            .Distinct() // 去重，避免重复碰撞
            .ToArray();

        var overlappingColliders_2 = overlap4
            .Where(c => c != thisTriggerCollider) // 过滤掉 thisTriggerCollider
            .Distinct() // 去重，避免重复碰撞
            .ToArray();

        // 输出调试信息
        foreach (var item in overlappingColliders_1)
        {
            Debug.Log("创建overlappingColliders_1: " + overlappingColliders_1.Length + " " + item);
        }
        foreach (var item in overlappingColliders_1)
        {
            Debug.Log("创建overlappingColliders_2: " + overlappingColliders_2.Length + " " + item);
        }
        
        return (overlappingColliders_1, (overlappingColliders_2.Length > 0));//检测到了与 自身碰撞器 的重合
    }

    /// <summary>
    /// 销毁脚本自身
    /// </summary>
    public void DestroySelf()
    {
        // 销毁脚本自身
        Destroy(this);
    }
}