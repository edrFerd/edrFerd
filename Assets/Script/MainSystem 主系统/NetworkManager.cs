using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 网络管理器，负责与本地HTTP服务器通信，获取公钥和世界状态。
/// </summary>
public class NetworkManager : MonoBehaviour
{
    private const string ServerUrl = "http://10.233.1.4:1416";
    private SYSManager sysManager;
    private WorldGenerator worldGenerator;
    private static readonly HttpClient httpClient = new HttpClient();

    // 公钥响应结构
    [Serializable]
    public class PubkeyResponse
    {
        public List<int> pubkey;
    }

    // 世界状态响应结构
    [Serializable]
    public class KnownWorldStateResponse
    {
        public List<BlockState> known_world_state;
    }

    [Serializable]
    public class BlockState
    {
        public Block block;
        public List<int> pub_key;
    }

    [Serializable]
    public class Block
    {
        public Point point;
        public BlockInfo block_info;
    }

    [Serializable]
    public class Point
    {
        public int x;
        public int y;
        public int z;
    }

    [Serializable]
    public class BlockInfo
    {
        public string type_id;
    }

    // 初始化，获取SYSManager和WorldGenerator引用
    public void Init(SYSManager sysManager)
    {
        this.sysManager = sysManager;
        this.worldGenerator = sysManager.worldGenerator;
        // 启动协程请求数据
        StartCoroutine(RequestPubkey());
        StartCoroutine(RequestKnownWorldState());
    }

    /// <summary>
    /// 请求公钥端点
    /// </summary>
    private IEnumerator RequestPubkey()
    {
        string url = ServerUrl + "/pubkey";
        var task = Task.Run(async () =>
        {
            try
            {
                var response = await httpClient.GetStringAsync(url);
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkManager] 获取公钥失败: {ex.Message}");
                return null;
            }
        });

        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.Result != null)
        {
            string json = task.Result;
            // 兼容直接数组格式
            if (json.TrimStart().StartsWith("["))
            {
                PubkeyResponse resp = new PubkeyResponse { pubkey = JsonUtility.FromJson<IntArrayWrapper>("{\"array\":" + json + "}").array };
                Debug.Log($"[NetworkManager] 公钥: {string.Join(",", resp.pubkey)}");
            }
            else
            {
                PubkeyResponse resp = JsonUtility.FromJson<PubkeyResponse>(json);
                Debug.Log($"[NetworkManager] 公钥: {string.Join(",", resp.pubkey)}");
            }
        }
    }

    /// <summary>
    /// 请求世界状态端点
    /// </summary>
    private IEnumerator RequestKnownWorldState()
    {
        string url = ServerUrl + "/known_world_state";
        var task = Task.Run(async () =>
        {
            try
            {
                var response = await httpClient.GetStringAsync(url);
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkManager] 获取世界状态失败: {ex.Message}");
                return null;
            }
        });

        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.Result != null)
        {
            string json = task.Result;
            // 兼容直接数组格式
            if (json.TrimStart().StartsWith("["))
            {
                KnownWorldStateResponse resp = new KnownWorldStateResponse { known_world_state = JsonUtility.FromJson<BlockStateArrayWrapper>("{\"array\":" + json + "}").array };
                ApplyWorldState(resp);
            }
            else
            {
                KnownWorldStateResponse resp = JsonUtility.FromJson<KnownWorldStateResponse>(json);
                ApplyWorldState(resp);
            }
        }
    }

    /// <summary>
    /// 应用世界状态到WorldGenerator
    /// </summary>
    private void ApplyWorldState(KnownWorldStateResponse resp)
    {
        if (resp == null || resp.known_world_state == null) return;
        // 清空现有方块
        worldGenerator.ClearAllBlocks();
        foreach (var blockState in resp.known_world_state)
        {
            var pos = new Vector3(blockState.block.point.x, blockState.block.point.y, blockState.block.point.z);
            Texture2D tex = GetTextureByTypeId(blockState.block.block_info.type_id);
            worldGenerator.Main(pos, tex);
        }
    }

    /// <summary>
    /// 根据type_id获取材质
    /// </summary>
    private Texture2D GetTextureByTypeId(string typeId)
    {
        // 1: 随机方块 2: 第二种方块 3: 第三种方块
        if (typeId == "1") return worldGenerator.CreateRandomTexture();
        if (typeId == "2") return worldGenerator.CreateSolidColorTexture(Color.green);
        if (typeId == "3") return worldGenerator.CreateSolidColorTexture(Color.gray);
        // 其他类型默认随机
        return worldGenerator.CreateRandomTexture();
    }

    /// <summary>
    /// 向指定HTTP端点发送JSON数据（POST）
    /// </summary>
    /// <param name="ip">服务器IP</param>
    /// <param name="endpoint">端点路径</param>
    /// <param name="jsonString">要发送的JSON字符串</param>
    public void SendDataToEndpoint(string ip, string endpoint, string jsonString)
    {
        StartCoroutine(PostJsonCoroutine(ip, endpoint, jsonString));
    }

    private IEnumerator PostJsonCoroutine(string ip, string endpoint, string jsonString)
    {
        string url = $"http://{ip}{endpoint}";
        var task = Task.Run(async () =>
        {
            try
            {
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                return new { Success = response.IsSuccessStatusCode, Response = responseString };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkManager] POST到{url}失败: {ex.Message}");
                return new { Success = false, Response = ex.Message };
            }
        });

        while (!task.IsCompleted)
        {
            yield return null;
        }

        var result = task.Result;
        if (result.Success)
        {
            Debug.Log($"[NetworkManager] POST到{url}成功，响应: {result.Response}");
        }
        else
        {
            Debug.LogError($"[NetworkManager] POST到{url}失败: {result.Response}");
        }
    }

    // 用于JsonUtility解析数组
    [Serializable]
    private class IntArrayWrapper { public List<int> array; }
    [Serializable]
    private class BlockStateArrayWrapper { public List<BlockState> array; }
} 