using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using System;
using System.IO;

[InitializeOnLoad]
public class LoggerEditor
{
    // 构造函数会在编辑器启动时调用
    static LoggerEditor()
    {
        try
        {
            // 初始化日志系统前确保不重复初始化
            if (!CustomLogger.IsInitialized())
            {
                CustomLogger.Initialize();
            }

            // 添加编译器消息处理
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            
            // 添加编辑器日志回调
            Application.logMessageReceived += HandleEditorLog;
            
            Debug.Log("编辑器日志系统已初始化");
        }
        catch (Exception ex)
        {
            Debug.LogError($"初始化编辑器日志系统时发生错误: {ex.Message}");
            try
            {
                File.WriteAllText(Path.Combine(Application.dataPath, "..", "editor_log_error.log"), 
                    $"初始化编辑器日志系统时发生错误: {ex.Message}\n{ex.StackTrace}");
            }
            catch
            {
                // 无能为力了
            }
        }
    }

    // 编译开始时的处理
    private static void OnCompilationStarted(object context)
    {
        try
        {
            // 直接使用CustomLogger进行日志记录
            Debug.Log("[编译] 开始编译脚本");
        }
        catch (Exception e)
        {
            Debug.LogError($"记录编译开始信息失败: {e.Message}");
        }
    }

    // 编译结束时的处理
    private static void OnCompilationFinished(object context)
    {
        try
        {
            string status = EditorUtility.scriptCompilationFailed ? "失败" : "成功";
            Debug.Log($"[编译] 编译脚本{status}");
            
            // 如果编译失败，记录更详细的信息
            if (EditorUtility.scriptCompilationFailed)
            {
                Debug.LogError("[编译] 编译失败，请查看Unity控制台获取详细错误信息");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"记录编译完成信息失败: {e.Message}");
        }
    }
    
    // 处理编辑器日志（确保编译错误也被记录）
    private static void HandleEditorLog(string logString, string stackTrace, LogType type)
    {
        // 在CustomLogger中已经处理了这些日志，这里只是为了确保编辑器特定日志也被捕获
    }
} 