using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;

[Serializable]
public class PubKeyWrapper
{
    public byte[] pubkey;
}

[Serializable]
public class PointData
{
    public int x;
    public int y;
    public int z;
}

[Serializable]
public class BlockInfoData
{
    public string type_id;
}

[Serializable]
public class BlockData
{
    public PointData point;
    public BlockInfoData block_info;
}

[Serializable]
public class WorldStateItem
{
    public BlockData block;
    public byte[] pub_key;
}

[Serializable]
public class WorldStateWrapper
{
    public WorldStateItem[] items;
}

[Serializable]
public class TickUpdateItem
{
    public WorldStateItem block;
    public string timestamp;
}

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

    public void Init(SYSManager manager)
    {
        httpClient = new HttpClient();
        _ = GetPubKeyAsync();
        _ = GetWorldStateAsync();
    }

    /// <summary>从服务器获取公钥并反序列化为 byte[]</summary>
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
                foreach (var item in wrapper.items)
                {
                    Debug.Log(
                        $"方块: 坐标: x: {item.block.point.x}, y: {item.block.point.y}, z: {item.block.point.z} 类型: {item.block.block_info.type_id} 公钥长度: {item.pub_key.Length}");
                }
                // TODO 处理世界状态
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

    public async Task GetTickUpdateAsync()
    {
        var url = $"{ServerUrl}/tick_update_vec";
        try
        {
            Debug.Log("开始获取Tick更新");
            var jsonString = await httpClient.GetStringAsync(url);
            var wrappedJson = $"{{\"items\":{jsonString}}}";
            var wrapper = JsonUtility.FromJson<TickUpdateWrapper>(wrappedJson);

            if (wrapper != null && wrapper.items != null)
            {
                Debug.Log($"成功获取到 {wrapper.items.Length} 个Tick更新。");
                foreach (var item in wrapper.items)
                {
                    var blockData = item.block.block;
                    Debug.Log($"Tick Update at {item.timestamp}: Block at ({blockData.point.x}, {blockData.point.y}, {blockData.point.z}) with type {blockData.block_info.type_id}");
                }
            }
            else
            {
                Debug.LogWarning("获取到的Tick更新为空或格式不正确。");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"GetTickUpdateAsync error: {e}");
        }
    }
}