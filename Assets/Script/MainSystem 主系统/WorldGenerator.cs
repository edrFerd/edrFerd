using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 世界生成器
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    // 方块位置和纹理的字典
    private Dictionary<Vector3, Texture2D> blockDictionary = new Dictionary<Vector3, Texture2D>();
    // 方块位置和游戏对象的字典
    private Dictionary<Vector3, GameObject> blockDictionary_GameObject = new Dictionary<Vector3, GameObject>();
    private SYSManager sysManager;
    private static long blockCounter = 0;
    private Color[] blockColors = new Color[]
    {
        new Color(0.76f, 0.69f, 0.5f), // 沙子
        new Color(0.47f, 0.36f, 0.22f), // 木头
        new Color(0.53f, 0.53f, 0.53f), // 石头
        new Color(0.18f, 0.38f, 0.18f), // 草地
        new Color(0.54f, 0.27f, 0.07f), // 红棕色
        new Color(0.1f, 0.1f, 0.1f),    // 深灰色
    };
    private GameObject blockRoot;

    public void Init(SYSManager manager)
    {
        this.sysManager = manager;
        blockDictionary = new Dictionary<Vector3, Texture2D>();
        blockDictionary_GameObject = new Dictionary<Vector3, GameObject>();
        CreateBlockRoot();
    }

    private void CreateBlockRoot()
    {
        blockRoot = new GameObject("BlockRoot");
        blockRoot.transform.position = Vector3.zero;
    }

    public void Main(Vector3 position, Texture2D texture = null)
    {
        if (texture == null)
        {
            if (blockDictionary_GameObject.ContainsKey(position))
            {
                DeleteBlock(position);
                return;
            }
            return;
        }
        
        // 如果该位置已存在方块，先删除它
        if (blockDictionary_GameObject.ContainsKey(position))
        {
            DeleteBlock(position);
        }
        
        // 创建新方块
        blockDictionary.Add(position, texture);
        CreateBlock(position, texture);
    }

    public void Main(Vector3[] positions, Texture2D[] textures = null)
    {
        if (textures == null)
        {
            textures = new Texture2D[positions.Length];
            for (int i = 0; i < positions.Length; i++) textures[i] = null;
        }
        if (positions.Length != textures.Length)
        {
            Debug.LogError("位置数组和纹理数组长度不匹配");
            return;
        }
        for (int i = 0; i < positions.Length; i++)
        {
            Main(positions[i], textures[i]);
        }
    }

    public bool HasBlockAt(Vector3 position)
    {
        return blockDictionary_GameObject.ContainsKey(position);
    }

    private void DeleteBlock(Vector3 position)
    {
        if (blockDictionary_GameObject.TryGetValue(position, out GameObject blockObject))
        {
            Destroy(blockObject);
            blockDictionary_GameObject.Remove(position);
            blockDictionary.Remove(position);
            Debug.Log("已删除位置 " + position + " 的方块");
        }
        else
        {
            Debug.LogWarning("找不到位置 " + position + " 的方块以删除");
        }
    }

    private void CreateBlock(Vector3 position, Texture2D texture)
    {
        blockCounter++;
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one;
        cube.transform.SetParent(blockRoot.transform);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.Apply();
        Material material = null;
        if (Shader.Find("Unlit/Texture") != null)
        {
            material = new Material(Shader.Find("Unlit/Texture"));
        }
        else if (Shader.Find("Mobile/Unlit (Supports Lightmap)") != null)
        {
            material = new Material(Shader.Find("Mobile/Unlit (Supports Lightmap)"));
        }
        else
        {
            material = new Material(Shader.Find("Standard"));
            material.SetFloat("_Glossiness", 0.0f);
            material.SetFloat("_Metallic", 0.0f);
        }
        material.color = Color.white;
        material.mainTexture = texture;
        material.mainTextureScale = Vector3.one;
        Renderer renderer = cube.GetComponent<Renderer>();
        renderer.material = material;
        cube.name = string.Format("Block_{0}_{1}_{2}_{3}", blockCounter, position.x.ToString("F0"), position.y.ToString("F0"), position.z.ToString("F0"));
        blockDictionary_GameObject[position] = cube;
        Debug.Log("已创建方块: " + cube.name + ", 纹理大小: " + texture.width + "x" + texture.height + ", 着色器: " + material.shader.name);
    }

    private void UpdateBlock(Vector3 position, Texture2D texture)
    {
        if (blockDictionary_GameObject.TryGetValue(position, out GameObject targetBlock))
        {
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.Apply();
            Renderer renderer = targetBlock.GetComponent<Renderer>();
            Material material = renderer.material;
            material.color = Color.white;
            material.mainTexture = texture;
        }
        else
        {
            Debug.LogWarning("找不到位置 " + position + " 的方块以更新");
        }
    }

    public Texture2D CreateRandomTexture()
    {
        try
        {
            Color baseColor = blockColors[Random.Range(0, blockColors.Length)];
            Texture2D texture = CreateSolidColorTexture(baseColor);
            int numDetails = Random.Range(5, 15);
            for (int i = 0; i < numDetails; i++)
            {
                int detailX = Random.Range(0, 16);
                int detailY = Random.Range(0, 16);
                int detailSize = Random.Range(1, 4);
                Color detailColor = new Color(
                    baseColor.r * 0.8f,
                    baseColor.g * 0.8f,
                    baseColor.b * 0.8f,
                    1.0f);
                for (int x = detailX; x < detailX + detailSize && x < 16; x++)
                {
                    for (int y = detailY; y < detailY + detailSize && y < 16; y++)
                    {
                        texture.SetPixel(x, y, detailColor);
                    }
                }
            }
            texture.Apply();
            Debug.Log("已创建随机纹理 - 大小: " + texture.width + "x" + texture.height + ", 格式: " + texture.format + ", 基础颜色: " + baseColor);
            return texture;
        }
        catch (System.Exception e)
        {
            Debug.LogError("创建随机纹理时出错: " + e.Message);
            Texture2D fallbackTexture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color fallbackColor = Color.red;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    fallbackTexture.SetPixel(x, y, fallbackColor);
                }
            }
            fallbackTexture.Apply();
            return fallbackTexture;
        }
    }

    public Texture2D CreateSolidColorTexture(Color color)
    {
        try
        {
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Repeat;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    float noise = Random.Range(-0.05f, 0.05f);
                    Color pixelColor = new Color(
                        Mathf.Clamp01(color.r + noise),
                        Mathf.Clamp01(color.g + noise),
                        Mathf.Clamp01(color.b + noise),
                        1.0f);
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            texture.Apply();
            Debug.Log("已创建纯色纹理 - 大小: " + texture.width + "x" + texture.height + ", 格式: " + texture.format + ", 基础颜色: " + color);
            return texture;
        }
        catch (System.Exception e)
        {
            Debug.LogError("创建纯色纹理时出错: " + e.Message);
            Texture2D fallbackTexture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color fallbackColor = Color.red;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    fallbackTexture.SetPixel(x, y, fallbackColor);
                }
            }
            fallbackTexture.Apply();
            return fallbackTexture;
        }
    }

    /// <summary>
    /// BASE64字符串转换为Texture2D，对于相同的输入字符串总是返回相同的实例。
    /// 输入字符串应为有效的PNG或JPG BASE64编码。缓存确保相同输入返回相同实例。
    /// </summary>
    private static Dictionary<string, Texture2D> base64TextureCache = new Dictionary<string, Texture2D>();
    public static Texture2D TextureFromBase64(string base64)
    {
        if (string.IsNullOrEmpty(base64)) return null;
        if (base64TextureCache.TryGetValue(base64, out Texture2D cached))
        {
            return cached;
        }
        
        try
        {
            byte[] imageData = System.Convert.FromBase64String(base64);
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            bool loadSuccess = tex.LoadImage(imageData);
            
            if (!loadSuccess)
            {
                Debug.LogWarning($"无法加载Base64图像数据，使用默认纹理。数据长度: {imageData.Length}");
                // 创建默认纹理
                tex = CreateDefaultTexture();
            }
            else
            {
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Repeat;
                tex.Apply();
            }
            
            base64TextureCache[base64] = tex;
            return tex;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Base64图像数据解析失败，使用默认纹理。错误: {e.Message}");
            // 创建默认纹理
            Texture2D fallbackTex = CreateDefaultTexture();
            base64TextureCache[base64] = fallbackTex;
            return fallbackTex;
        }
    }
    
    /// <summary>
    /// 创建默认纹理作为加载失败时的备用方案
    /// </summary>
    private static Texture2D CreateDefaultTexture()
    {
        Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        Color defaultColor = new Color(0.5f, 0.5f, 0.5f, 1.0f); // 灰色
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                tex.SetPixel(x, y, defaultColor);
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// 从JSON响应更新世界（适用于完整和增量更新）
    /// </summary>
    /// <param name="response">解析的BlockListResponse</param>
    /// <param name="isFullState">这是否是一个完整的世界状态？</param>
    public void UpdateFromBlockList(BlockListResponse response, bool isFullState)
    {
        if (response == null || response.Blocks == null) return;
        if (isFullState)
        {
            // 完全同步：清除所有方块
            foreach (var pos in new List<Vector3>(blockDictionary_GameObject.Keys))
            {
                DeleteBlock(pos);
            }
            // TODO: 可以优化为只删除不在新列表中的方块
        }
        foreach (var block in response.Blocks)
        {
            if (block.Position == null || block.Position.Length != 3) continue;
            Vector3 pos = new Vector3(block.Position[0], block.Position[1], block.Position[2]);
            Texture2D tex = TextureFromBase64(block.Texture);
            Main(pos, tex); // Main会覆盖已存在的方块
        }
        // TODO: 对于增量更新，如有需要处理删除
    }
}