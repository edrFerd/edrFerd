using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 绘制网格
/// </summary>
public class VisibleGrid : MonoBehaviour
{
    // /// <summary>网格的尺寸</summary>
    // public int gridSize = 10;
    // /// <summary>每个网格单元的大小</summary>
    // public float cellSize = 1.0f;

    private List<List<GameObject>> gridLines = new List<List<GameObject>>(); // 存储所有网格线的列表
    private GameObject gridLineRoot; // 存储"GridLineRoot"空物体的引用

    public enum LineType
    {
        Default,
        VisualizationEntity
    }

    private void Start()
    {
        // 创建"GridLineRoot"空物体，并存储引用
        gridLineRoot = new GameObject("GridLineRoot");
    }


    /// <summary>
    /// 绘制网格
    /// </summary>
    /// <param name="center">网格中心位置</param>
    /// <param name="size">网格的尺寸（行列数）</param>
    /// <param name="cellSize">每个网格单元的大小</param>
    /// <param name="lineType">线的类型</param>
    /// <param name="isVertical">是否为垂直网格</param>
    public GameObject DrawGrid(Vector3 center, int gridSize, float cellSize, LineType lineType, bool isVertical)
    {
        GameObject gridLineParent = new GameObject("GridLine");
        gridLineParent.transform.SetParent(gridLineRoot.transform);
        List<GameObject> currentGridLines = new List<GameObject>();

        switch (lineType)
        {
            case LineType.Default:
            {
                Material lineMaterial = CreateLineMaterial(Color.white); // 网格线的颜色
                float widthMultiplier = 0.1f;//相对线宽
                DrawGrid(center, gridSize, cellSize, widthMultiplier, gridLineParent, currentGridLines, lineMaterial, isVertical);
                break;
            }
            case LineType.VisualizationEntity:
            {
                Material lineMaterial = CreateLineMaterial(Color.white); // 网格线的颜色
                float widthMultiplier = 0.1f;//相对线宽
                DrawGrid(center, gridSize, cellSize, widthMultiplier, gridLineParent, currentGridLines, lineMaterial, isVertical);
                break;
            }

            default:
                break;
        }
        return gridLineParent;
    }

    /// <summary>
    /// 绘制网格
    /// </summary>
    /// <param name="center">网格中心位置</param>
    /// <param name="size">网格的尺寸（行列数）</param>
    /// <param name="cellSize">每个网格单元的大小</param>
    /// <param name="widthMultiplier">线条宽度系数</param>
    /// <param name="gridLineParent">网格线的父对象</param>
    /// <param name="currentGridLines">当前网格线列表</param>
    /// <param name="lineMaterial">网格线材质</param>
    /// <param name="isVertical">是否为垂直网格</param>
    public void DrawGrid(Vector3 center, int size, float cellSize, float widthMultiplier, GameObject gridLineParent, List<GameObject> currentGridLines, Material lineMaterial, bool isVertical)
    {
        float halfSize = size * cellSize / 2;

        for (int i = 0; i <= size; i++)
        {
            // 根据isVertical参数来决定网格的方向
            Vector3 horizontalStart, horizontalEnd, verticalStart, verticalEnd;

            if (isVertical)
            {
                // 垂直网格: YZ 平面
                horizontalStart = new Vector3(center.x, center.y - halfSize, center.z + i * cellSize - halfSize);
                horizontalEnd = new Vector3(center.x, center.y + halfSize, center.z + i * cellSize - halfSize);

                verticalStart = new Vector3(center.x, center.y + i * cellSize - halfSize, center.z - halfSize);
                verticalEnd = new Vector3(center.x, center.y + i * cellSize - halfSize, center.z + halfSize);
            }
            else
            {
                // 水平网格: XY 平面
                horizontalStart = new Vector3(center.x - halfSize, center.y, center.z + i * cellSize - halfSize);
                horizontalEnd = new Vector3(center.x + halfSize, center.y, center.z + i * cellSize - halfSize);

                verticalStart = new Vector3(center.x + i * cellSize - halfSize, center.y, center.z - halfSize);
                verticalEnd = new Vector3(center.x + i * cellSize - halfSize, center.y, center.z + halfSize);
            }

            // 创建水平线
            GameObject hLine = CreateLine(horizontalStart, horizontalEnd, widthMultiplier, lineMaterial, gridLineParent);

            // 创建垂直线
            GameObject vLine = CreateLine(verticalStart, verticalEnd, widthMultiplier, lineMaterial, gridLineParent);

            // 将线对象存储到列表中
            currentGridLines.Add(hLine);
            currentGridLines.Add(vLine);
        }

        // 将当前网格线的列表添加到整体列表中
        gridLines.Add(currentGridLines);
    }

    /// <summary>
    /// 创建一条网格线
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="end">终点</param>
    /// <param name="lineMawidthMultiplierterial">线条宽度系数</param>
    /// <param name="lineMaterial">线的材质</param>
    /// <param name="parent">父对象</param>
    /// <returns>创建的线对象</returns>
    private GameObject CreateLine(Vector3 start, Vector3 end, float widthMultiplier, Material lineMaterial, GameObject parent)
    {
        GameObject line = new GameObject("GridLineSegment"); // 创建新的GameObject作为线
        line.transform.SetParent(parent.transform); // 设置为父对象的子对象

        LineRenderer lr = line.AddComponent<LineRenderer>(); // 添加LineRenderer组件
        lr.material = lineMaterial; // 设置材质
        lr.useWorldSpace = true; // 使用世界坐标系
        lr.positionCount = 2; // 设置线段数
        lr.SetPosition(0, start); // 设置起点
        lr.SetPosition(1, end); // 设置终点
        lr.startWidth = 0.1f; // 初始设置一个默认线宽
        lr.endWidth = 0.1f;
        lr.widthMultiplier = widthMultiplier; // 线条宽度系数

        // 取消抗锯齿
        lr.numCapVertices = 0; // 顶点数量为0，这样可以减少抗锯齿效果
        lr.numCornerVertices = 0; // 角落顶点数量为0，同样减少抗锯齿效果

        return line;
    }

    /// <summary>
    /// 创建用于网格线的材质
    /// </summary>
    /// <param name="color">线的颜色</param>
    /// <returns>材质</returns>
    private Material CreateLineMaterial(Color color)
    {
        // 使用 Unity 内置的Unlit着色器，该着色器简单且容易调试
        Material material = new Material(Shader.Find("Unlit/Color"));
        material.color = color;
        material.SetFloat("_Glossiness", 0.0f); // 确保无光泽效果
        return material;
    }

    /// <summary>
    /// 平移所有网格线
    /// </summary>
    /// <param name="translation">平移的向量</param>
    public void TranslateAllGrid(Vector3 translation)
    {
        gridLineRoot.transform.position += translation;
    }

    /// <summary>
    /// 平移指定网格线
    /// </summary>
    /// <param name="gridParent">网格线指定的父物体</param>
    /// <param name="translation">平移的向量</param>
    public void TranslateGrid(GameObject gridParent, Vector3 translation)
    {
        // 遍历所有存储的网格线组
        foreach (var grid in gridLines)
        {
            // 检查每个网格线组的父物体是否匹配
            if (grid.Count > 0 && grid[0].transform.parent == gridParent.transform)
            {
                // 如果匹配，平移该网格线组的所有线
                foreach (GameObject line in grid)
                {
                    line.transform.position += translation;
                }
                break; // 找到目标网格线组后退出循环
            }
        }
    }

    /// <summary>
    /// 删除所有网格线
    /// </summary>
    public void DeleteAllGridLines()
    {
        foreach (var grid in gridLines)
        {
            foreach (GameObject line in grid)
            {
                Destroy(line);
            }
        }
        gridLines.Clear(); // 清空列表
    }

    /// <summary>
    /// 删除指定的网格线
    /// </summary>
    /// <param name="gridParent">要删除的网格线的父物体</param>
    public void RemoveGridLines(GameObject gridParent)
    {
        // 找到指定的网格线父物体在列表中的索引
        for (int i = 0; i < gridLines.Count; i++)
        {
            // 查找是否有子线段隶属于传入的gridParent
            if (gridLines[i].Count > 0 && gridLines[i][0].transform.parent == gridParent.transform)
            {
                // 销毁所有子线段
                foreach (GameObject line in gridLines[i])
                {
                    Destroy(line);
                }
                gridLines.RemoveAt(i); // 从列表中移除该项
                Destroy(gridParent); // 销毁父物体
                break; // 跳出循环，因为我们找到了要删除的父物体
            }
        }
    }
}
