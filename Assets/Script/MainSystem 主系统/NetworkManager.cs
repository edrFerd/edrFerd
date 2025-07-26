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
    public float tick_update_interval = 1f/60f;

    private const string ServerUrl = "http://localhost:1416";
    private SYSManager sysManager;
    private WorldGenerator worldGenerator;
    private static readonly HttpClient httpClient = new HttpClient();

    // 初始化 HttpClient 配置
    static NetworkManager()
    {
        httpClient.Timeout = TimeSpan.FromSeconds(10); // 设置10秒超时
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Unity-Client/1.0");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

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

    // 适配实际 JSON 结构
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
        StartCoroutine(RequestTickUpdates());
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
                Debug.LogError($"[NetworkManager] 获取公钥失败: {ex.ToString()}");
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
        Debug.Log($"[NetworkManager] 开始请求世界状态: {url}");
        
        int retryCount = 0;
        const int maxRetries = 3;
        
        while (retryCount < maxRetries)
        {
            retryCount++;
            Debug.Log($"[NetworkManager] 尝试第 {retryCount} 次请求世界状态");
            
         var task = Task.Run(async () =>
         {
             try
             {
                Debug.Log($"[NetworkManager] 发送HTTP请求到: {url}");
                var response = await httpClient.GetStringAsync(url);
                Debug.Log($"[NetworkManager] 收到响应，长度: {response?.Length ?? 0}");
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkManager] 第 {retryCount} 次请求世界状态失败: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.LogError($"[NetworkManager] 内部异常: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
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
            Debug.Log($"[NetworkManager] 收到世界状态JSON: {json}");
            // 兼容直接数组格式
            if (json.TrimStart().StartsWith("["))
            {
                KnownWorldStateResponse resp = new KnownWorldStateResponse { known_world_state = JsonUtility.FromJson<BlockStateArrayWrapper>("{\"array\": " + json + "}").array };
                ApplyWorldState(resp);
                Debug.Log($"[NetworkManager] 成功应用世界状态，方块数量: {resp.known_world_state?.Count ?? 0}");
                yield break; // 成功，退出重试循环
            }
            else
            {
                KnownWorldStateResponse resp = JsonUtility.FromJson<KnownWorldStateResponse>(json);
                ApplyWorldState(resp);
                Debug.Log($"[NetworkManager] 成功应用世界状态，方块数量: {resp.known_world_state?.Count ?? 0}");
                yield break; // 成功，退出重试循环
            }
        }
        else
        {
            Debug.LogWarning($"[NetworkManager] 第 {retryCount} 次请求返回空结果");
            if (retryCount < maxRetries)
            {
                Debug.Log($"[NetworkManager] 等待 2 秒后重试...");
                yield return new WaitForSeconds(2f);
            }
        }
        }
        
        if (retryCount >= maxRetries)
        {
            Debug.LogError($"[NetworkManager] 世界状态请求失败，已重试 {maxRetries} 次");
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
    /// 应用增量世界更新
    /// </summary>
    private void ApplyTickUpdate(KnownWorldStateResponse resp)
    {
        if (resp == null || resp.known_world_state == null || resp.known_world_state.Count == 0) return;
        
        foreach (var blockState in resp.known_world_state)
        {
            var pos = new Vector3(blockState.block.point.x, blockState.block.point.y, blockState.block.point.z);
            Texture2D tex = GetTextureByTypeId(blockState.block.block_info.type_id);
            worldGenerator.Main(pos, tex);
        }
    }

    /// <summary>
    /// 定期请求增量更新
    /// </summary>
    private IEnumerator RequestTickUpdates()
    {
        string url = ServerUrl + "/tick_update_vec";
        while (true)
        {
            // 每秒请求一次
            yield return new WaitForSeconds(tick_update_interval);

            var task = Task.Run(async () =>
            {
                try
                {
                    var response = await httpClient.GetStringAsync(url);
                    return response;
                }
                catch (Exception ex)
                {
                    // 增量更新失败通常不视为关键错误，只在控制台打印日志
                    Debug.Log($"[NetworkManager] 获取增量更新失败: {ex.Message}");
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
                if (string.IsNullOrEmpty(json) || json == "[]")
                {
                    continue; // 如果返回空或空数组，则跳过本次更新
                }

                Debug.Log($"[NetworkManager] 收到增量更新JSON: {json}");
                // 兼容直接数组格式
                if (json.TrimStart().StartsWith("["))
                {
                    KnownWorldStateResponse resp = new KnownWorldStateResponse { known_world_state = JsonUtility.FromJson<BlockStateArrayWrapper>("{\"array\": " + json + "}").array };
                    ApplyTickUpdate(resp);
                }
                else
                {
                    KnownWorldStateResponse resp = JsonUtility.FromJson<KnownWorldStateResponse>(json);
                    ApplyTickUpdate(resp);
                }
            }
        }
    }

    void OnDestroy()
    {
        // 清理HTTP客户端资源
        httpClient?.Dispose();
    }

    void OnApplicationQuit()
    {
        // 清理HTTP客户端资源
        httpClient?.Dispose();
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