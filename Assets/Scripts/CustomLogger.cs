using UnityEngine;
using System;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

public class CustomLogger : MonoBehaviour
{
    private static string logFilePath;
    private static StringBuilder logBuffer = new StringBuilder();
    private static bool isInitialized = false;
    private static object lockObject = new object();

    // 当脚本实例被加载时调用
    void Awake()
    {
        Initialize();
    }
    
    // 检查日志系统是否已初始化
    public static bool IsInitialized()
    {
        return isInitialized;
    }

    // 初始化日志系统
    public static void Initialize()
    {
        try
        {
            if (isInitialized) return;

            // 首先尝试在项目根目录下创建日志文件作为备份
            string rootLogPath = Path.Combine(Application.dataPath, "..", "root_log.log");
            File.WriteAllText(rootLogPath, $"=== 开始尝试初始化日志系统: {DateTime.Now} ===\n");
            
            // 设置日志文件路径为Assets/Log/log.log
            string logDirectory = Path.Combine(Application.dataPath, "Log");
            
            File.AppendAllText(rootLogPath, $"日志目录路径: {logDirectory}\n");
            
            // 确保日志目录存在
            if (!Directory.Exists(logDirectory))
            {
                try 
                {
                    Directory.CreateDirectory(logDirectory);
                    File.AppendAllText(rootLogPath, $"创建日志目录成功\n");
                }
                catch (Exception e)
                {
                    File.AppendAllText(rootLogPath, $"创建日志目录失败: {e.Message}\n{e.StackTrace}\n");
                    // 如果无法在Assets/Log下创建，则使用临时目录
                    logDirectory = Path.Combine(Application.temporaryCachePath, "Logs");
                    Directory.CreateDirectory(logDirectory);
                    File.AppendAllText(rootLogPath, $"使用临时目录: {logDirectory}\n");
                }
            }
            
            logFilePath = Path.Combine(logDirectory, "log.log");
            File.AppendAllText(rootLogPath, $"日志文件路径: {logFilePath}\n");

            // 清空或创建日志文件
            try
            {
                File.WriteAllText(logFilePath, $"=== 日志会话开始: {DateTime.Now} ===\n");
                File.AppendAllText(rootLogPath, $"成功创建日志文件\n");
            }
            catch (Exception e)
            {
                File.AppendAllText(rootLogPath, $"创建日志文件失败: {e.Message}\n{e.StackTrace}\n");
                // 如果无法在预定目录创建文件，改为持久化数据路径
                logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
                Directory.CreateDirectory(logDirectory);
                logFilePath = Path.Combine(logDirectory, "log.log");
                File.WriteAllText(logFilePath, $"=== 日志会话开始(备用位置): {DateTime.Now} ===\n");
                File.AppendAllText(rootLogPath, $"使用备用日志位置: {logFilePath}\n");
            }

            // 添加日志回调
            Application.logMessageReceived += HandleLog;
            
            Debug.Log($"自定义日志系统已初始化，日志文件路径: {logFilePath}");
            File.AppendAllText(rootLogPath, $"日志系统初始化完成\n");
            isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"初始化日志系统时发生错误: {ex.Message}\n{ex.StackTrace}");
            try
            {
                File.WriteAllText(Path.Combine(Application.dataPath, "..", "log_error.log"), 
                    $"初始化日志系统时发生错误: {ex.Message}\n{ex.StackTrace}");
            }
            catch
            {
                // 最后的尝试，无能为力了
            }
        }
    }

    // 处理Unity的日志消息
    private static void HandleLog(string logString, string stackTrace, LogType type)
    {
        try
        {
            // 构造完整日志消息
            string fullLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {logString}";
            if (type == LogType.Error || type == LogType.Exception)
            {
                fullLog += $"\n{stackTrace}";
            }

            // 写入日志文件
            lock (lockObject)
            {
                try
                {
                    File.AppendAllText(logFilePath, fullLog + "\n");
                }
                catch (Exception e)
                {
                    Debug.LogError($"写入日志文件失败: {e.Message}");
                    // 尝试重新初始化日志系统
                    isInitialized = false;
                    Initialize();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理日志消息时发生错误: {ex.Message}");
        }
    }

    // 应用退出时调用
    void OnApplicationQuit()
    {
        if (isInitialized)
        {
            Application.logMessageReceived -= HandleLog;
            try
            {
                File.AppendAllText(logFilePath, $"=== 日志会话结束: {DateTime.Now} ===\n");
            }
            catch
            {
                Debug.LogError("关闭日志会话时发生错误");
            }
            isInitialized = false;
        }
    }

    // 确保在编辑器中也能记录日志
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    static void EditorInitialize()
    {
        Initialize();
    }

    // 编译错误处理（仅在编辑器中有效）
    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnScriptsReloaded()
    {
        if (!isInitialized)
        {
            Initialize();
        }
        
        // 检查编译错误
        if (UnityEditor.EditorUtility.scriptCompilationFailed)
        {
            string errorMessage = "脚本编译失败，详细错误请查看Unity控制台";
            HandleLog(errorMessage, "", LogType.Error);
        }
        else
        {
            HandleLog("脚本编译成功", "", LogType.Log);
        }
    }
    #endif
} 