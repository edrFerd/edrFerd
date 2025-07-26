using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WebSocket工具类，用于处理JSON数据的发送和接收
/// </summary>
public static class WebSocketUtility
{
    /// <summary>
    /// 将对象序列化为JSON并通过网络管理器发送
    /// </summary>
    /// <param name="networkManager">网络管理器实例</param>
    /// <param name="obj">要发送的对象</param>
    /// <returns>是否成功发送</returns>
    public static bool SendJson(NetworkManager networkManager, object obj)
    {
        if (networkManager == null)
        {
            Debug.LogError("网络管理器为空，无法发送数据");
            return false;
        }

        try
        {
            string jsonString = JsonUtility.ToJson(obj);
            networkManager.SendMessage(jsonString);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("序列化对象或发送数据失败: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 将JSON字符串解析为指定类型的对象
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="jsonString">JSON字符串</param>
    /// <returns>解析后的对象</returns>
    public static T ParseJson<T>(string jsonString)
    {
        try
        {
            return JsonUtility.FromJson<T>(jsonString);
        }
        catch (Exception ex)
        {
            Debug.LogError($"解析JSON到{typeof(T).Name}类型失败: " + ex.Message);
            return default(T);
        }
    }

    /// <summary>
    /// 向指定端点发送JSON对象
    /// </summary>
    /// <param name="networkManager">网络管理器实例</param>
    /// <param name="ip">服务器IP</param>
    /// <param name="endpoint">端点路径</param>
    /// <param name="obj">要发送的对象</param>
    /// <returns>是否成功发送</returns>
    public static bool SendJsonToEndpoint(NetworkManager networkManager, string ip, string endpoint, object obj)
    {
        if (networkManager == null)
        {
            Debug.LogError("网络管理器为空，无法发送数据");
            return false;
        }

        try
        {
            string jsonString = JsonUtility.ToJson(obj);
            networkManager.SendDataToEndpoint(ip, endpoint, jsonString);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("序列化对象或发送数据到端点失败: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 尝试将JSON字符串解析为指定类型，如果失败则返回默认值
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="jsonString">JSON字符串</param>
    /// <param name="defaultValue">解析失败时的默认值</param>
    /// <returns>解析后的对象或默认值</returns>
    public static T TryParseJson<T>(string jsonString, T defaultValue)
    {
        try
        {
            return JsonUtility.FromJson<T>(jsonString);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 检查JSON字符串是否有效
    /// </summary>
    /// <param name="jsonString">JSON字符串</param>
    /// <returns>是否为有效的JSON</returns>
    public static bool IsValidJson(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
        {
            return false;
        }

        jsonString = jsonString.Trim();
        if ((jsonString.StartsWith("{") && jsonString.EndsWith("}")) || 
            (jsonString.StartsWith("[") && jsonString.EndsWith("]")))
        {
            try
            {
                // 尝试解析为通用类，验证JSON格式是否有效
                var obj = JsonUtility.FromJson<object>(jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        return false;
    }

    /// <summary>
    /// 创建一个简单的JSON消息对象
    /// </summary>
    /// <param name="type">消息类型</param>
    /// <param name="content">消息内容</param>
    /// <returns>JSON字符串</returns>
    public static string CreateJsonMessage(string type, string content)
    {
        SimpleMessage message = new SimpleMessage
        {
            messageType = type,
            content = content,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        return JsonUtility.ToJson(message);
    }

    /// <summary>
    /// 简单消息类，用于创建基本的消息结构
    /// </summary>
    [Serializable]
    public class SimpleMessage
    {
        public string messageType;
        public string content;
        public string timestamp;
    }
} 