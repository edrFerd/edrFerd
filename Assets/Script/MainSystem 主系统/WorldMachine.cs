using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///   <para>世界机器</para>
/// </summary>
public class WorldMachine : MonoBehaviour
{
    /// <summary>
    ///   <para>世界随机性种子</para>
    /// </summary>
    public int worldRandomSeed = 1;
    /// <summary>
    ///   <para>全局计数，自第一次运行到现在经过的时间片总数，假设以60Hz运行可记录十亿年</para>
    /// </summary>
    public long GlobalCount_Physical { get => globalCount_Physical; } //不可写,外部只读
    private long globalCount_Physical = 1;         //依据FixedUpdate更新的GlobalCount，迎合完整时间精度要求的连续性判断
    /// <summary>
    ///   <para>全局计数_迎合外部观测的不完整低性能版,自第一次运行到现在经过的时间片总数</para>
    /// </summary>
    public long GlobalCount_Update { get => globalCount_Update; } //不可写,外部只读
    private long globalCount_Update = 1; //依据Update更新的GlobalCount，迎合低性能的低时间精度要求的连续性判断

    public long newObjectCount;//新建对象计数

    public SYSManager sYSManager;
    private bool isInitialized = false;

    private void Awake()
    {
        Debug.Log("hello, world\n");

        //子系统管理器初始化
        sYSManager = this.gameObject.AddComponent<SYSManager>();
        sYSManager.Init();
        isInitialized = true;
        //Debug.unityLogger.logEnabled = false; //关闭debug输出 可极大提高性能
    }

    private void FixedUpdate()
    {
        // Debug.Log("---" + globalCount_Physical + "--- （内部方法 FixedUpdate） " + Time.fixedUnscaledTime);     // 输出时间片序号（内部方法）
        // Debug.Log("---" + Time.frameCount + "--- （外部方法 FixedUpdate） " + Time.fixedUnscaledTime); // 输出时间片序号（外部方法）

        //--------------------------------------世界环境更新开始--------------------------------------
        //CODE;
        //--------------------------------------世界环境更新结束--------------------------------------

        //--------------------------------------外部操作开始--------------------------------------
        //cameraLtHand.Main();
        // directorSystem.Main(); //导演系统应最后更新
        //--------------------------------------外部操作结束--------------------------------------

        globalCount_Physical += 1; // 全局计数++
        // OutTime();                 // 输出时间
    }


    void Update()
    {
        // Debug.Log("---" + globalCount_Update + "--- （内部方法 Update） " + Time.deltaTime);     // 输出时间片序号（内部方法）

        //--------------------------------------操作开始--------------------------------------
        // 检查所有组件是否已正确初始化
        if (isInitialized && sYSManager != null && sYSManager.cameraLtHand != null && 
            sYSManager.worldGenerator != null)
        {
            sYSManager.cameraLtHand.Main(); //为避免无规律操作失效而放在这里 推测是卡在Update里而无法运行
        }
        else if (isInitialized && sYSManager != null)
        {
            // 如果SYSManager已初始化但其他组件未初始化，尝试重新初始化
            Debug.LogWarning("检测到组件未初始化，重新尝试初始化...");
            sYSManager.Init();
        }
        //--------------------------------------操作开始--------------------------------------

        globalCount_Update += 1; // 全局计数_Update++
    }


    /// <summary>
    ///   <para>输出时间 打印所有时间参数</para>
    /// </summary>
    public void OutTime()
    {
        Debug.Log(
            "时间输出\n" +
            "01. " + "captureFramerate         " + Time.captureFramerate + "\n" +
            "02. " + "deltaTime                " + Time.deltaTime + "\n" +
            "03. " + "fixedDeltaTime           " + Time.fixedDeltaTime + "\n" +
            "04. " + "fixedTime                " + Time.fixedTime + "\n" +
            "05. " + "fixedUnscaledDeltaTime   " + Time.fixedUnscaledDeltaTime + "\n" +
            "06. " + "fixedUnscaledTime        " + Time.fixedUnscaledTime + "\n" +
            "07. " + "frameCount               " + Time.frameCount + "\n" +
            "08. " + "inFixedTimeStep          " + Time.inFixedTimeStep + "\n" +
            "09. " + "maximumDeltaTime         " + Time.maximumDeltaTime + "\n" +
            "10. " + "maximumParticleDeltaTime " + Time.maximumParticleDeltaTime + "\n" +
            "11. " + "realtimeSinceStartup     " + Time.realtimeSinceStartup + "\n" +
            "12. " + "smoothDeltaTime          " + Time.smoothDeltaTime + "\n" +
            "13. " + "time                     " + Time.time + "\n" +
            "14. " + "timeScale                " + Time.timeScale + "\n" +
            "15. " + "timeSinceLevelLoad       " + Time.timeSinceLevelLoad + "\n" +
            "16. " + "unscaledDeltaTime        " + Time.unscaledDeltaTime + "\n" +
            "17. " + "unscaledTime             " + Time.unscaledTime + "\n"
        );
    }
}
