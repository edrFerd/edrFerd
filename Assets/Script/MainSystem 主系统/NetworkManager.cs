using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Concurrent; // 新增

/// <summary>
/// 网络管理器，负责HTTP服务器接收数据
/// </summary>
public class NetworkManager : MonoBehaviour
{
    private WorldGenerator worldGenerator;
    private SYSManager sysManager;
    private bool isInitialized = false;
    
    // HTTP服务器配置
    private const int HTTP_PORT = 8766;  // Unity服务器端口
    private HttpListener httpListener;
    private bool serverRunning = false;
    private Thread serverThread;

    // 用于主线程处理的队列
    private ConcurrentQueue<(string, bool)> worldStateQueue = new ConcurrentQueue<(string, bool)>();

    void Start()
    {
        Init();
    }

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
        
        // 启动HTTP服务器
        StartHttpServer();
    }

    /// <summary>
    /// 启动HTTP服务器
    /// </summary>
    private void StartHttpServer()
    {
        try
        {
            // 先尝试停止可能存在的旧服务器
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Stop();
                httpListener.Close();
    }

            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://localhost:{HTTP_PORT}/");
            httpListener.Start();
            serverRunning = true;
            
            Debug.Log($"Unity HTTP服务器已启动，监听端口: {HTTP_PORT}");
        
            // 在后台线程中处理请求
            serverThread = new Thread(HandleHttpRequests);
            serverThread.IsBackground = true;
            serverThread.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError($"启动HTTP服务器失败: {ex.Message}");
            // 尝试使用备用端口
            TryAlternativePort();
        }
    }

    /// <summary>
    /// 尝试使用备用端口
    /// </summary>
    private void TryAlternativePort()
    {
        for (int port = 8767; port <= 8770; port++)
    {
            try
            {
                if (httpListener != null && httpListener.IsListening)
                {
                    httpListener.Stop();
                    httpListener.Close();
                }
                
                httpListener = new HttpListener();
                httpListener.Prefixes.Add($"http://localhost:{port}/");
                httpListener.Start();
                serverRunning = true;
                
                Debug.Log($"Unity HTTP服务器已启动，使用备用端口: {port}");
                
                // 在后台线程中处理请求
                serverThread = new Thread(HandleHttpRequests);
                serverThread.IsBackground = true;
                serverThread.Start();
                return;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"尝试端口 {port} 失败: {ex.Message}");
            }
        }
        
        Debug.LogError("所有端口都无法使用，HTTP服务器启动失败");
    }

    /// <summary>
    /// 处理HTTP请求的后台线程方法
    /// </summary>
    private void HandleHttpRequests()
    {
        while (serverRunning && httpListener != null && httpListener.IsListening)
            {
            try
            {
                HttpListenerContext context = httpListener.GetContext();
                ProcessRequest(context);
            }
            catch (Exception ex)
            {
                if (serverRunning)
            {
                    Debug.LogError($"处理HTTP请求时出错: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 处理单个HTTP请求（只入队数据，不直接处理）
    /// </summary>
    private void ProcessRequest(HttpListenerContext context)
    {
        try
        {
            string path = context.Request.Url.AbsolutePath;
            Debug.Log($"收到HTTP请求: {path}");

            // 设置响应头
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            if (context.Request.HttpMethod == "OPTIONS")
        {
                // 处理预检请求
                context.Response.StatusCode = 200;
                context.Response.Close();
                return;
            }

            if (path == "/world_update" && context.Request.HttpMethod == "POST")
            {
                using (var reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    string jsonData = reader.ReadToEnd();
                    worldStateQueue.Enqueue((jsonData, false)); // 入队
                }

                // 返回成功响应
                string response = "{\"status\":\"success\"}";
                byte[] buffer = Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else if (path == "/full_world_state" && context.Request.HttpMethod == "POST")
            {
                using (var reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    string jsonData = reader.ReadToEnd();
                    worldStateQueue.Enqueue((jsonData, true)); // 入队
                }

                // 返回成功响应
                string response = "{\"status\":\"success\"}";
                byte[] buffer = Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                // 未知端点
                context.Response.StatusCode = 404;
                string response = "{\"error\":\"Unknown endpoint\"}";
                byte[] buffer = Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }

            context.Response.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理HTTP请求时出错: {ex.Message}");
            try
            {
                context.Response.StatusCode = 500;
                string response = "{\"error\":\"Internal server error\"}";
                byte[] buffer = Encoding.UTF8.GetBytes(response);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.Close();
            }
            catch
            {
                // 忽略关闭响应时的错误
            }
        }
    }

    // 在主线程Update里处理队列
    void Update()
    {
        while (worldStateQueue.TryDequeue(out var item))
        {
            ProcessWorldState(item.Item1, item.Item2);
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
                Debug.Log($"接收到{(isFullState ? "完整世界状态" : "世界更新")}，包含 {response.Blocks?.Length ?? 0} 个方块");
                worldGenerator.UpdateFromBlockList(response, isFullState);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理世界状态数据失败: {ex.Message}");
        }
    }

    void OnDestroy()
    {
        StopHttpServer();
    }

    void OnApplicationQuit()
    {
        StopHttpServer();
    }

    /// <summary>
    /// 停止HTTP服务器
    /// </summary>
    private void StopHttpServer()
    {
        if (serverRunning)
        {
            serverRunning = false;
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Stop();
                httpListener.Close();
            }
            Debug.Log("Unity HTTP服务器已停止");
        }
    }

    /// <summary>
    /// 发送消息（兼容方法，当前不发送数据）
    /// </summary>
    /// <param name="message">要发送的消息</param>
    public void SendMessage(string message)
    {
        Debug.Log($"SendMessage被调用，但当前使用HTTP服务器模式，消息未发送: {message.Substring(0, Math.Min(message.Length, 100))}");
    }

    /// <summary>
    /// 向指定端点发送数据（兼容方法，当前不发送数据）
    /// </summary>
    /// <param name="ip">服务器IP</param>
    /// <param name="endpoint">端点路径</param>
    /// <param name="data">要发送的数据</param>
    public void SendDataToEndpoint(string ip, string endpoint, string data)
    {
        Debug.Log($"SendDataToEndpoint被调用，但当前使用HTTP服务器模式，消息未发送: {endpoint}, 数据: {data.Substring(0, Math.Min(data.Length, 100))}");
    }
} 