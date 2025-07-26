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
                    worldGenerator.SetBlock(item.block);
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
                    worldGenerator.SetBlock(item.block.block);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GetTickUpdateAsync error: {e}");
        }
    }

    public async Task set_backend_block(BlockData block, int duration)
    {
        var url = $"{ServerUrl}/set_block";

    }
}