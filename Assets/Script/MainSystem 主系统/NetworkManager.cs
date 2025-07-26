using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 网络管理器，负责HTTP和WebSocket通信
/// </summary>
public class NetworkManager : MonoBehaviour
{
    private string serverBaseUrl = "http://localhost:8765";
    private WorldGenerator worldGenerator;
    private SYSManager sysManager;
    private bool isInitialized = false;
    
    private const string KNOWN_WORLD_STATE_ENDPOINT = "/known_world_state";
    private const string TICK_UPDATE_ENDPOINT = "/tick_block_update_list";

    // 定时检查更新
    private float checkInterval = 5.0f;
    private float lastCheckTime = 0f;

    /// <summary>
    /// 初始化网络管理器
    /// </summary>
    public void Init()
    {
        Debug.Log("网络管理器初始化");
        sysManager = GetComponent<SYSManager>();
        worldGenerator = sysManager?.worldGenerator;
        
        if (worldGenerator == null)
        {
            Debug.LogError("初始化网络管理器失败：无法获取WorldGenerator引用");
            return;
        }
        
        isInitialized = true;
        
        // 立即开始获取世界状态
        StartCoroutine(GetFullWorldState());
        lastCheckTime = Time.time;
    }

    void Update()
    {
        if (!isInitialized) return;
        
        // 每隔一段时间检查更新
        if (Time.time - lastCheckTime > checkInterval)
        {
            StartCoroutine(GetTickUpdate());
            lastCheckTime = Time.time;
        }
    }

    /// <summary>
    /// 获取完整世界状态
    /// </summary>
    private IEnumerator GetFullWorldState()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverBaseUrl + KNOWN_WORLD_STATE_ENDPOINT))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                ProcessWorldState(jsonResponse, true);
            }
            else
            {
                Debug.LogError($"获取世界状态失败: {request.error}");
            }
        }
    }

    /// <summary>
    /// 获取Tick更新
    /// </summary>
    private IEnumerator GetTickUpdate()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverBaseUrl + TICK_UPDATE_ENDPOINT))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                ProcessWorldState(jsonResponse, false);
            }
            else
            {
                Debug.LogWarning($"获取Tick更新失败: {request.error}");
            }
        }
    }

    /// <summary>
    /// 处理从服务器接收到的世界状态数据
    /// </summary>
    /// <param name="jsonData">JSON数据</param>
    /// <param name="isFullState">是否为完整世界状态</param>
    private void ProcessWorldState(string jsonData, bool isFullState)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.LogWarning("接收到的世界状态数据为空");
            return;
        }

        try
        {
            BlockListResponse response = WebSocketUtility.ParseJson<BlockListResponse>(jsonData);
            if (response != null)
            {
                Debug.Log($"接收到{(isFullState ? "完整世界状态" : "Tick更新")}，包含 {response.Blocks?.Length ?? 0} 个方块");
                worldGenerator.UpdateFromBlockList(response, isFullState);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理世界状态数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 发送消息（WebSocket兼容方法）
    /// </summary>
    /// <param name="message">要发送的消息</param>
    public void SendMessage(string message)
    {
        Debug.Log($"SendMessage被调用，但当前使用HTTP通信，消息未发送: {message.Substring(0, Math.Min(message.Length, 100))}");
    }

    /// <summary>
    /// 向指定端点发送数据（WebSocket兼容方法）
    /// </summary>
    /// <param name="ip">服务器IP</param>
    /// <param name="endpoint">端点路径</param>
    /// <param name="data">要发送的数据</param>
    public void SendDataToEndpoint(string ip, string endpoint, string data)
    {
        Debug.Log($"SendDataToEndpoint被调用，但当前使用HTTP通信，消息未发送: {endpoint}, 数据: {data.Substring(0, Math.Min(data.Length, 100))}");
        // 可以实现HTTP POST如果需要
    }
} 