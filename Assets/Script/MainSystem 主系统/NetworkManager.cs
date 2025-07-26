using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;

/// <summary>用于反序列化公钥JSON数据的包装类。</summary>
[Serializable]
public class PubKeyWrapper
{
    public byte[] pubkey;
}

/// <summary>表示三维空间中的一个点。</summary>
[Serializable]
public class PointData
{
    public int x;
    public int y;
    public int z;
}

/// <summary>表示方块的类型信息。</summary>
[Serializable]
public class BlockInfoData
{
    public string type_id;
}

/// <summary>表示一个方块的完整数据，包括位置和类型。</summary>
[Serializable]
public class BlockData
{
    public PointData point;
    public BlockInfoData block_info;
}

/// <summary>表示世界状态中的一个条目，通常是一个方块及其所有者公钥。</summary>
[Serializable]
public class WorldStateItem
{
    public BlockData block;
    public byte[] pub_key;
}

/// <summary>用于反序列化完整世界状态JSON数组的包装类。</summary>
[Serializable]
public class WorldStateWrapper
{
    public WorldStateItem[] items;
}

/// <summary>表示一个Tick更新事件，包含一个方块状态和时间戳。</summary>
[Serializable]
public class TickUpdateItem
{
    public WorldStateItem block;
    public string timestamp;
}

/// <summary>用于反序列化Tick更新JSON数组的包装类。</summary>
[Serializable]
public class TickUpdateWrapper
{
    public TickUpdateItem[] items;
}

/// <summary>
/// 用于发送 set_block 请求的数据结构。
/// </summary>
[Serializable]
public class SetBlockRequestData
{
    public int duration;
    public int x;
    public int y;
    public int z;
    public BlockInfoData info;
}

/// <summary>
/// 网络管理器，负责与本地HTTP服务器通信，获取公钥和世界状态。
/// </summary>
public class NetworkManager : MonoBehaviour
{
    public float tickUpdateInterval = 1f / 20f;

    private const string ServerUrl = "http://localhost:1416";
    private HttpClient httpClient;
    private WorldGenerator worldGenerator;

    /// <summary>初始化网络管理器，获取必要数据并启动更新循环。</summary>
    public void Init(SYSManager manager, WorldGenerator worldGenerator)
    {
        this.worldGenerator = worldGenerator;
        httpClient = new HttpClient();
        Debug.Log("网络管理器初始化...");
        _ = GetPubKeyAsync();

        runCycle();
    }

    async void runCycle()
    {
        Debug.Log("启动更新周期，首先获取完整世界状态...");
        await GetWorldStateAsync();
        Debug.Log("获取完整世界状态成功，开始更新循环...");
        while (true)
        {
            await GetTickUpdateAsync();
            await Task.Delay((int)(tickUpdateInterval * 1000f));
        }
    }


    /// <summary>从服务器异步获取公钥并反序列化为 byte[]。</summary>
    public async Task GetPubKeyAsync()
    {
        var url = $"{ServerUrl}/pubkey";
        try
        {
            Debug.Log("开始获取公钥");
            var jsonString = await httpClient.GetStringAsync(url);
            var wrappedJson = $"{{\"pubkey\":{jsonString}}}";
            var wrapper = JsonUtility.FromJson<PubKeyWrapper>(wrappedJson);
            Debug.Log($"公钥获取成功，内容为: {JsonUtility.ToJson(wrapper)}");
        }
        catch (Exception e)
        {
            Debug.LogError($"GetPubKeyAsync error: {e}");
        }
    }

    /// <summary>从服务器异步获取世界状态。</summary>
    public async Task GetWorldStateAsync()
    {
        var url = $"{ServerUrl}/known_world_state";
        try
        {
            Debug.Log("开始获取世界状态");
            var jsonString = await httpClient.GetStringAsync(url);
            // JsonUtility 不支持直接反序列化JSON数组，需要包装一下
            var wrappedJson = $"{{\"items\":{jsonString}}}";
            var wrapper = JsonUtility.FromJson<WorldStateWrapper>(wrappedJson);

            if (wrapper != null && wrapper.items != null)
            {
                Debug.Log($"成功获取到 {wrapper.items.Length} 个世界状态更新。");
                // 此处日志可能过多，暂时注释掉，需要时可打开
                foreach (var item in wrapper.items)
                {
                    worldGenerator.SetBlock(item.block, item.pub_key);
                }
            }
            else
            {
                Debug.LogWarning("获取到的世界状态为空或格式不正确。");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GetWorldStateAsync error: {e}");
        }
    }

    /// <summary>从服务器异步获取Tick更新。</summary>
    private async Task GetTickUpdateAsync()
    {
        var url = $"{ServerUrl}/tick_update_vec";
        try
        {
            var jsonString = await httpClient.GetStringAsync(url);
            var wrappedJson = $"{{\"items\":{jsonString}}}";
            var wrapper = JsonUtility.FromJson<TickUpdateWrapper>(wrappedJson);
            if (wrapper != null && wrapper.items != null && wrapper.items.Length > 0)
            {
                Debug.Log($"接收到 {wrapper.items.Length} 个Tick更新。");
                foreach (var item in wrapper.items)
                {
                    worldGenerator.SetBlock(item.block.block, item.block.pub_key);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GetTickUpdateAsync error: {e}");
        }
    }

    /// <summary>
    /// 对服务器声明一个方块的状态
    /// </summary>
    /// <param name="block"></param>
    /// <param name="duration"></param>
    public async Task set_backend_block(BlockData block, int duration)
    {
        // 1. 定义API端点
        var url = $"{ServerUrl}/set_block";

        // 2. 记录请求开始的日志，方便调试
        Debug.Log(
            $"开始向 {url} 发送 set_block 请求。方块位置: ({block.point.x}, {block.point.y}, {block.point.z}), 类型: {block.block_info.type_id}, 持续时间: {duration}");

        try
        {
            // 3. 创建请求体数据对象，以匹配服务器期望的JSON格式
            var requestData = new SetBlockRequestData
            {
                duration = duration,
                x = block.point.x,
                y = block.point.y,
                z = block.point.z,
                info = block.block_info
            };

            // 4. 将C#对象序列化为JSON字符串
            string jsonPayload = JsonUtility.ToJson(requestData);
            Debug.Log($"生成的JSON负载: {jsonPayload}");

            // 5. 创建HTTP请求内容 (HttpContent)
            //    - 使用 UTF8 编码
            //    - 设置 Content-Type 为 "application/json"
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // 6. 发送POST请求
            //    使用 httpClient 实例发送异步POST请求
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            // 7. 检查响应状态并记录日志
            if (response.IsSuccessStatusCode)
            {
                // 如果请求成功 (HTTP状态码 2xx)
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log($"set_block 请求成功。服务器响应: {responseBody}");
            }
            else
            {
                // 如果请求失败 (例如，HTTP状态码 4xx, 5xx)
                string errorBody = await response.Content.ReadAsStringAsync();
                Debug.LogError(
                    $"set_block 请求失败。状态码: {response.StatusCode}, 原因: {response.ReasonPhrase}, 响应体: {errorBody}");
            }
        }
        catch (HttpRequestException e)
        {
            // 捕获网络相关的异常 (例如，无法连接到服务器)
            Debug.LogError($"网络请求异常: {e.Message}");
        }
        catch (Exception e)
        {
            // 捕获其他所有可能的异常 (例如，序列化失败)
            Debug.LogError($"执行 set_backend_block 时发生未知错误: {e}");
        }
    }

    /// <summary>
    /// 向服务器发送请求，以去除对一个方块的声明。
    /// </summary>
    /// <param name="point">要移除声明的方块的坐标。</param>
    public async Task remove_block(PointData point)
    {
        // 1. 定义API端点
        var url = $"{ServerUrl}/remove_block";

        // 2. 记录请求开始的日志，方便调试
        Debug.Log($"开始向 {url} 发送 remove_block 请求。方块坐标: ({point.x}, {point.y}, {point.z})");

        try
        {
            // 3. 将 PointData 对象直接序列化为JSON字符串。
            //    它的结构 {x, y, z} 与API期望的负载完全匹配。
            string jsonPayload = JsonUtility.ToJson(point);
            Debug.Log($"生成的JSON负载: {jsonPayload}");

            // 4. 创建带有正确编码和MIME类型的HTTP请求内容
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // 5. 使用POST方法发送异步请求
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            // 6. 检查响应状态并记录结果
            if (response.IsSuccessStatusCode)
            {
                // 如果请求成功 (HTTP 2xx)
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log($"remove_block 请求成功。服务器响应: {responseBody}");
            }
            else
            {
                // 如果请求失败
                string errorBody = await response.Content.ReadAsStringAsync();
                Debug.LogError(
                    $"remove_block 请求失败。状态码: {response.StatusCode}, 原因: {response.ReasonPhrase}, 响应体: {errorBody}");
            }
        }
        catch (HttpRequestException e)
        {
            // 捕获网络连接或协议错误
            Debug.LogError($"网络请求异常: {e.Message}");
        }
        catch (Exception e)
        {
            // 捕获其他所有未预料的错误
            Debug.LogError($"执行 remove_block 时发生未知错误: {e}");
        }
    }
}