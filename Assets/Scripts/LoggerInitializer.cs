using UnityEngine;

[DefaultExecutionOrder(-9999)] // 确保此脚本在其他脚本之前执行
public class LoggerInitializer : MonoBehaviour
{
    private static GameObject loggerObject;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeLogger()
    {
        // 确保日志系统在运行时初始化
        if (!CustomLogger.IsInitialized())
        {
            CustomLogger.Initialize();
        }

        // 避免重复创建LoggerObject
        if (loggerObject != null) return;
        
        // 在运行时创建一个持久的GameObject来保存CustomLogger组件
        loggerObject = new GameObject("LoggerObject");
        loggerObject.AddComponent<CustomLogger>();
        DontDestroyOnLoad(loggerObject);

        Debug.Log("日志系统已通过初始化器启动");
    }
} 