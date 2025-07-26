using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
///   <para>内部工具类</para>
/// </summary>
public static class Tools
{
    //--------字符串格式化--------
    //--------字符串格式化--------
    //--------字符串格式化--------
    public static string Displacement(Vector3 inV3)
    {
        string s = inV3.ToString("F9");


        return s;
    }





    //--------交互处理--------
    //--------交互处理--------
    //--------交互处理--------


    /// <summary>
    ///   <para>某一轴上的指针位移量</para>
    /// </summary>
    /// <param name="pointerCurrentPosition">指针当前位置</param>
    ///<param name="pointerLastPosition">指针上一次位置</param>
    public static float Displacement(float pointerCurrentPosition, float pointerLastPosition)
    {
        return pointerCurrentPosition - pointerLastPosition;
    }


    //--------数值处理--------
    //--------数值处理--------
    //--------数值处理--------

    /// <summary>
    ///   <para>指针是否越出屏幕边界</para>
    /// </summary>
    /// <param name="boundaryExtension">视角边界模糊判断区域的延伸长度,避免全屏时正好落在边界数值</param>
    public static bool PointerOutOfBounds(float boundaryExtension)
    {
        if (!new Rect(0 + boundaryExtension, 0 + boundaryExtension, Screen.width - boundaryExtension, Screen.height - boundaryExtension)
            .Contains(Input.mousePosition))
        {// 指针越界
            return true;
        }
        else
        {// 指针未越界，在屏幕显示范围内 
            return false;
        }
    }


    /// <summary>
    ///   <para>坐标是否越出屏幕边界</para>
    /// </summary>
    /// <param name="coordinate">待判断的坐标</param>
    /// <param name="boundaryExtension">视角边界模糊判断区域的延伸长度,避免全屏时正好落在边界数值</param>
    /// 越界 T
    /// 未越界 F
    public static bool CoordinateOutOfBounds(Vector2 coordinate, float boundaryExtension)
    {
        // 调整Rect的定义，左下角减去边界延伸，右上角增加边界延伸
        Rect extendedRect = new Rect(
            0 - boundaryExtension, // 左边缘扩展
            0 - boundaryExtension, // 下边缘扩展
            Screen.width + 2 * boundaryExtension,  // 宽度增加两倍扩展量
            Screen.height + 2 * boundaryExtension  // 高度增加两倍扩展量
        );

        if (!extendedRect.Contains(new Vector3(coordinate.x, coordinate.y, 0)))
        {// 指针越界
            return true;
        }
        else
        {// 指针未越界，在屏幕显示范围内 
            return false;
        }
    }



    /// <summary>
    ///   <para>获取主摄像机的屏幕空间中心位置->前向延长线与物体交汇点->对应的世界坐标。未命中则返回(0,0,0) </para>
    /// </summary>
    public static GameObject CenterPositionToWorldCoordinates() //中心位置到世界坐标
    {
        RaycastHit hit; //存储击中停留处的ray数据
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)); //生成待发射的ray
        if (Physics.Raycast(ray, out hit))                                                           //发射ray，结果存入hit
        {//如果命中
            return hit.collider.gameObject;
        }

        //未命中
        return null;
    }



    /// <summary>
    ///   <para>获取主摄像机的屏幕空间中鼠标当前位置->前向延长线与物体交汇点->对应的世界坐标。未命中则返回(0,0,0) </para>
    /// </summary>
    public static Vector3 MousePositionToWorldCoordinates() //鼠标位置到世界坐标
    {
        RaycastHit hit; //存储击中停留处的ray数据
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //生成待发射的ray
        if (Physics.Raycast(ray, out hit))                           //发射ray，结果存入hit
        {//如果命中
            return hit.point;
        }

        //未命中
        return Vector3.zero;
    }


    /// <summary>
    ///   <para>获取指定摄像机的屏幕空间中鼠标当前位置->前向延长线与物体交汇点->对应的世界坐标。未命中则返回(0,0,0)</para>
    /// </summary>
    public static Vector3 MousePositionToWorldCoordinates(Camera inCamera)
    {
        RaycastHit hit; //存储击中停留处的ray数据
        Ray ray = inCamera.ScreenPointToRay(Input.mousePosition); //生成待发射的ray
        if (Physics.Raycast(ray, out hit))                        //发射ray，结果存入hit
        {//如果命中
            return hit.point;
        }

        //未命中
        return Vector3.zero;
    }


    /// <summary>
    ///   <para>获取主摄像机的屏幕空间中鼠标当前位置->前向延长线与指定标签物体交汇点->对应的世界坐标。未命中或标签不符则返回(0,0,0) </para>
    /// </summary>
    public static Vector3 MousePositionToWorldCoordinates(string Tag)
    {
        RaycastHit hit; //存储击中停留处的ray数据
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //生成待发射的ray
        if (Physics.Raycast(ray, out hit))                           //发射ray，结果存入hit
        {//如果命中
            //检查被命中物体的标签
            if (hit.collider.tag == Tag)
            {//标签相符
                return hit.point;
            }
        }

        //未命中或标签不符
        return Vector3.zero;
    }


    /// <summary>
    ///   <para>获取指定摄像机的屏幕空间中鼠标当前位置->前向延长线与指定标签物体交汇点->对应的世界坐标。未命中或标签不符则返回(0,0,0)</para>
    /// </summary>
    public static Vector3 MousePositionToWorldCoordinates(string Tag, Camera inCamera)
    {
        RaycastHit hit; //存储击中停留处的ray数据
        Ray ray = inCamera.ScreenPointToRay(Input.mousePosition); //生成待发射的ray
        if (Physics.Raycast(ray, out hit))                        //发射ray，结果存入hit
        {//如果命中
            //检查被命中物体的标签
            if (hit.collider.tag == Tag)
            {//标签相符
                return hit.point;
            }
        }

        //未命中或标签不符
        return Vector3.zero;
    }


    /// <summary>
    ///   <para>获取“主摄像机的屏幕空间中鼠标当前位置->前向延长线与首个物体交汇点上对应的物体” 未交汇则返回null。可响应设置为运动学和触发器的碰撞器</para>
    /// </summary>
    public static GameObject MousePositionToGameObject()
    {
        RaycastHit hit; //存储击中停留处的ray数据
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //生成待发射的ray
        if (Physics.Raycast(ray, out hit))                           //发射ray，结果存入hit
        {//如果命中
            return hit.collider.gameObject;
        }

        //未命中
        return null;
    }
    

    /// <summary>
    ///   <para>获取“主摄像机的屏幕空间中鼠标当前位置->前向延长线与首个物体交汇点上对应的物体” 未交汇则返回null。可响应设置为运动学和触发器的碰撞器</para>
    ///   <para>并返回其中距离发射源最近的带有指定脚本的对象</para>
    /// </summary>
    public static RaycastHit[] MousePositionToGameObject_2()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 生成待发射的ray
        return Physics.RaycastAll(ray);// 使用Physics.RaycastAll获取所有碰撞体
    }


    //--------deBUG--------
    //--------deBUG--------
    //--------deBUG--------

    public static void FUTdebug(string s)
    {
        Debug.Log(Time.fixedUnscaledTime + "<b> SSSP-SI </b>即将超出数组容量");
    }




    //--------可视化--------
    //--------可视化--------
    //--------可视化--------

    /// <summary>
    ///   <para>可视化球体</para>
    /// </summary>
    public static void VisualizationSphere(Vector3 position, Transform parentObject, float diameter, Material material)
    {
        GameObject Visualization = GameObject.CreatePrimitive(PrimitiveType.Sphere); //创建球体
        Visualization.transform.position = position;                                 //物体中心为本物体位置
        Visualization.transform.parent = parentObject.transform;                     //设置父物体
        Visualization.transform.localScale = new Vector3(diameter, diameter, diameter); //缩放比例

        MonoBehaviour.Destroy(Visualization.GetComponent<SphereCollider>()); //移除固有的碰撞器
        Visualization.GetComponent<Renderer>().material = material;          //附加材质
    }

    /// <summary>
    ///   <para>可视化球体</para>
    /// </summary>
    public static void VisualizationSphere(Vector3 position, GameObject parentObject, float diameter)
    {
        GameObject Visualization = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 创建球体
        Visualization.transform.position = position;     // 物体中心为本物体位置
        Visualization.transform.parent = parentObject.transform; // 设置父物体
        Visualization.transform.localScale = new Vector3(diameter, diameter, diameter); // 缩放比例

        MonoBehaviour.Destroy(Visualization.GetComponent<SphereCollider>()); // 移除固有的碰撞器
        Visualization.GetComponent<Renderer>().material = Resources.Load<Material>("Material/VisualizationFlag/VisualizationSphere");
    }

    /// <summary>
    ///   <para>可视化球体</para>
    /// </summary>
    public static void VisualizationSphere(Vector3 position, GameObject parentObject)
    {
        GameObject Visualization = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 创建球体
        Visualization.transform.position = position;     // 物体中心为本物体位置
        Visualization.transform.parent = parentObject.transform; // 设置父物体
        Visualization.transform.localScale = new Vector3(1, 1, 1); // 缩放比例

        MonoBehaviour.Destroy(Visualization.GetComponent<SphereCollider>()); // 移除固有的碰撞器
        Visualization.GetComponent<Renderer>().material = Resources.Load<Material>("Material/VisualizationFlag/VisualizationSphere");
    }

    /// <summary>
    ///   <para>可视化球体</para>
    /// </summary>
    public static void VisualizationSphere(Vector3 position, float diameter)
    {
        GameObject Visualization = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 创建球体
        Visualization.transform.position = position;     // 物体中心为本物体位置
        Visualization.transform.localScale = new Vector3(diameter, diameter, diameter); // 缩放比例

        MonoBehaviour.Destroy(Visualization.GetComponent<SphereCollider>()); // 移除固有的碰撞器
        Visualization.GetComponent<Renderer>().material = Resources.Load<Material>("Material/VisualizationFlag/VisualizationSphere");
    }

    /// <summary>
    ///   <para>可视化球体</para>
    /// </summary>
    public static void VisualizationSphere(Vector3 position)
    {
        GameObject Visualization = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 创建球体
        Visualization.transform.position = position;     // 物体中心为本物体位置
        Visualization.transform.localScale = new Vector3(1, 1, 1); // 缩放比例

        MonoBehaviour.Destroy(Visualization.GetComponent<SphereCollider>()); // 移除固有的碰撞器
        Visualization.GetComponent<Renderer>().material = Resources.Load<Material>("Material/VisualizationFlag/VisualizationSphere");
    }



    //--------数组处理--------
    //--------数组处理--------
    //--------数组处理--------
    /// <summary>
    ///   <para>打印数组-double （FUTime时间输出+String+数组依次打印）</para>
    /// </summary>
    public static void printArr(string inString, double[] inArr)
    {
        for (int i = 0; i < inArr.Length; i++)
        {
            Debug.Log(Time.fixedUnscaledTime + " " + inString + " " + i + ": " + inArr[i]);
        }
    }

    /// <summary>
    ///   <para>打印数组-float （FUTime时间输出+String+数组依次打印）</para>
    /// </summary>
    public static void printArr(string inString, float[] inArr)
    {
        for (int i = 0; i < inArr.Length; i++)
        {
            Debug.Log(Time.fixedUnscaledTime + " " + inString + " " + i + ": " + inArr[i]);
        }
    }

    /// <summary>
    ///   <para>打印数组-string （FUTime时间输出+String数组依次打印）</para>
    /// </summary>
    public static void printArr(string inString, string[] inArr)
    {
        for (int i = 0; i < inArr.Length; i++)
        {
            Debug.Log(Time.fixedUnscaledTime + " " + inString + ": " + inArr[i]);
        }
    }

    /// <summary>
    ///   <para>打印数组-Transform （FUTime时间输出+String+数组依次打印）</para>
    /// </summary>
    public static void printArr(string inString, Transform[] inArr)
    {
        for (int i = 0; i < inArr.Length; i++)
        {
            Debug.Log(Time.fixedUnscaledTime + " " + inString + " " + i + ": " + inArr[i]);
        }
    }

    /// <summary>
    ///   <para>格式化数组-double</para>
    /// </summary>
    public static void formatArr(double[] inArr)
    {
        for (int i = 0; i < inArr.Length; i++)
        {
            inArr[i] = 0;
        }
    }

    /// <summary>
    ///   <para>格式化数组-float</para>
    /// </summary>
    public static void formatArr(float[] inArr)
    {
        for (int i = 0; i < inArr.Length; i++)
        {
            inArr[i] = 0;
        }
    }

    /// <summary>
    ///   <para>格式化数组-Transform</para>
    /// </summary>
    public static void formatArr(Transform[] inArr)
    {
        for (int i = 0; i < inArr.Length; i++)
        {
            inArr[i] = null;
        }
    }


    /// <summary>
    ///   <para>数组类型转换</para>
    /// </summary>
    public static float[] ArrTypeConversion(double[] inArr)
    {
        float[] floatArray = new float[inArr.Length];
        for (int i = 0; i < inArr.Length; i++)
        {
            floatArray[i] = (float)inArr[i];
        }

        return floatArray;
    }


    /// <summary>
    ///   <para>数组类型转换</para>
    /// </summary>
    public static float[] ArrTypeConversion(decimal[] inArr)
    {
        float[] floatArray = new float[inArr.Length];
        for (int i = 0; i < inArr.Length; i++)
        {
            floatArray[i] = (float)inArr[i];
        }

        return floatArray;
    }



    /// <summary>
    ///   <para>数组类型转换</para>
    /// </summary>
    public static double[] ArrTypeConversion(float[] inArr)
    {
        double[] floatArray = new double[inArr.Length];
        for (int i = 0; i < inArr.Length; i++)
        {
            floatArray[i] = inArr[i];
        }

        return floatArray;
    }



    // /// <summary>
    // ///   <para>单维数组转为多维数组</para>
    // /// </summary>
    // /// <param name="singleArr">单维数组</param>
    // /// <param name="indeX">X索引 横轴</param>
    // /// <param name="indeY">Y索引 竖轴</param>
    // ///
    // /// 把{1, 1, 1, 1, 1, 1, 1, 1, 1, }转换为
    // /// {
    // /// { 1, 1, 1 },
    // /// { 1, 1, 1 },
    // /// { 1, 1, 1 },
    // /// }
    // public static TerrainUnit[,] SingleDimensional_to_MultidimensionalArr(TerrainUnit[] singleArr, int indeX, int indeY)
    // {
    //     TerrainUnit[,] terrainArr = new TerrainUnit[indeX, indeY];

    //     int i = 0;
    //     int x = 0; //x横 y竖
    //     int y = 0;
    //     while (true)
    //     {
    //         if (x == indeX)
    //         {//列遍历完毕 结束
    //             return terrainArr;
    //         }

    //         //Debug.Log("XYI " + x + " , " + y + " , " + i);
    //         terrainArr[x, y] = singleArr[i]; //行内赋值
    //         y++;
    //         i++;

    //         if (y == indeY)
    //         {//本行遍历完毕 换行 否则继续本行
    //             y = 0;
    //             x++;
    //             continue; //换行重启循环
    //         }
    //     }
    // }


    /// <summary>
    ///   <para>从多维数组获取索引 若不存在则返回(-1, -1)</para>
    /// </summary>
    /// https://stackoverflow.com/questions/3260935/finding-position-of-an-element-in-a-two-dimensional-array
    public static Tuple<int, int> CoordinatesOf<T>(this T[,] matrix, T value)
    {
        int w = matrix.GetLength(0); // width
        int h = matrix.GetLength(1); // height

        for (int x = 0; x < w; ++x)
        {
            for (int y = 0; y < h; ++y)
            {
                if (matrix[x, y] != null && matrix[x, y] != null)
                {//本位操作数非空
                    if (matrix[x, y].Equals(value))
                        return Tuple.Create(x, y);
                }
            }
        }

        return Tuple.Create(-1, -1);
    }


        /// <summary>
        ///   <para>打印List</para>
        /// </summary>
        public static void printList<T>(List<T> value)
    {
        if (value == null)
        {
            Debug.Log("NULL");
            return;
        }

        foreach (var item in value)
        {
            Debug.Log(item);
        }
    }
    //----------------
    //----------------
    //----------------
}
