using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 世界生成器，负责在场景中生成、管理和销毁方块对象
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    // 方块位置和纹理的字典（用于记录每个方块的位置及其对应的纹理）
    private Dictionary<Vector3, Texture2D> blockDictionary = new Dictionary<Vector3, Texture2D>();

    // 方块位置和游戏对象的字典（用于记录每个方块的位置及其对应的GameObject实例）
    private Dictionary<Vector3, GameObject> blockDictionary_GameObject = new Dictionary<Vector3, GameObject>();
    
    // 方块位置和公钥的字典（用于记录每个方块的位置及其对应的公钥）
    private Dictionary<Vector3, byte[]> blockDictionary_PubKey = new Dictionary<Vector3, byte[]>();
    
    // 材质管理系统
    private Dictionary<string, Texture2D> materialDictionary = new Dictionary<string, Texture2D>();
    private bool materialsInitialized = false;
    
    // 系统管理器引用（用于与主系统交互）
    private SYSManager sysManager;

    // 全局方块计数器（用于生成唯一的方块名称）
    private static long blockCounter = 0;

    // 方块基础颜色数组（用于生成不同类型的方块纹理）
    private Color[] blockColors = new Color[]
    {
        new Color(0.76f, 0.69f, 0.5f), // 沙子
        new Color(0.47f, 0.36f, 0.22f), // 木头
        new Color(0.53f, 0.53f, 0.53f), // 石头
        new Color(0.18f, 0.38f, 0.18f), // 草地
        new Color(0.54f, 0.27f, 0.07f), // 红棕色
        new Color(0.1f, 0.1f, 0.1f), // 深灰色
    };

    // 所有方块的父对象（用于层级管理）
    private GameObject blockRoot;

    /// <summary>
    /// 初始化世界生成器，绑定主系统管理器并重置方块字典
    /// </summary>
    /// <param name="manager">主系统管理器</param>
    public void Init(SYSManager manager)
    {
        this.sysManager = manager;
        blockDictionary = new Dictionary<Vector3, Texture2D>();
        blockDictionary_GameObject = new Dictionary<Vector3, GameObject>();
        blockDictionary_PubKey = new Dictionary<Vector3, byte[]>();
        CreateBlockRoot();
        InitializeMaterials();
    }
    
    /// <summary>
    /// 初始化材质字典
    /// </summary>
    private void InitializeMaterials()
    {
        if (materialsInitialized) return;
        
        // 根据WOOLS表生成对应的材质
        materialDictionary["RED"] = CreateSolidColorTexture(new Color(0.8f, 0.2f, 0.2f)); // 红色羊毛
        materialDictionary["WHITE"] = CreateSolidColorTexture(new Color(0.9f, 0.9f, 0.9f)); // 白色羊毛
        materialDictionary["PURPLE"] = CreateSolidColorTexture(new Color(0.6f, 0.2f, 0.8f)); // 紫色羊毛
        materialDictionary["YELLOW"] = CreateSolidColorTexture(new Color(0.9f, 0.9f, 0.2f)); // 黄色羊毛
        materialDictionary["PINK"] = CreateSolidColorTexture(new Color(0.9f, 0.6f, 0.8f)); // 粉色羊毛
        materialDictionary["ORANGE"] = CreateSolidColorTexture(new Color(0.9f, 0.6f, 0.2f)); // 橙色羊毛
        materialDictionary["BLUE"] = CreateSolidColorTexture(new Color(0.2f, 0.4f, 0.8f)); // 蓝色羊毛
        materialDictionary["BROWN"] = CreateSolidColorTexture(new Color(0.6f, 0.4f, 0.2f)); // 棕色羊毛
        materialDictionary["CYAN"] = CreateSolidColorTexture(new Color(0.2f, 0.8f, 0.8f)); // 青色羊毛
        materialDictionary["LIME"] = CreateSolidColorTexture(new Color(0.6f, 0.9f, 0.2f)); // 黄绿色羊毛
        materialDictionary["MAGENTA"] = CreateSolidColorTexture(new Color(0.8f, 0.2f, 0.8f)); // 品红色羊毛
        materialDictionary["GRAY"] = CreateSolidColorTexture(new Color(0.5f, 0.5f, 0.5f)); // 灰色羊毛
        materialDictionary["LIGHT_GRAY"] = CreateSolidColorTexture(new Color(0.7f, 0.7f, 0.7f)); // 浅灰色羊毛
        materialDictionary["LIGHT_BLUE"] = CreateSolidColorTexture(new Color(0.6f, 0.8f, 0.9f)); // 浅蓝色羊毛
        materialDictionary["GREEN"] = CreateSolidColorTexture(new Color(0.2f, 0.6f, 0.2f)); // 绿色羊毛
        materialDictionary["BLACK"] = CreateSolidColorTexture(new Color(0.1f, 0.1f, 0.1f)); // 黑色羊毛
        
        // 添加一些默认材质
        materialDictionary["RANDOM"] = CreateRandomTexture(); // 随机材质
        materialDictionary["STONE"] = CreateSolidColorTexture(new Color(0.5f, 0.5f, 0.5f)); // 石头
        materialDictionary["DIRT"] = CreateSolidColorTexture(new Color(0.6f, 0.4f, 0.2f)); // 泥土
        materialDictionary["GRASS"] = CreateSolidColorTexture(new Color(0.3f, 0.7f, 0.3f)); // 草地
        
        // 注意：AIR类型不在此字典中，它会被特殊处理为删除方块
        
        materialsInitialized = true;
        Debug.Log($"材质字典初始化完成，共加载 {materialDictionary.Count} 种材质");
    }
    
    /// <summary>
    /// 根据block_info获取对应的材质
    /// </summary>
    /// <param name="blockInfo">方块信息</param>
    /// <returns>对应的材质纹理，如果返回null则表示需要删除方块</returns>
    public Texture2D GetMaterialByBlockInfo(BlockInfoData blockInfo)
    {
        if (blockInfo == null || string.IsNullOrEmpty(blockInfo.type_id))
        {
            Debug.LogWarning("blockInfo为空或type_id为空，使用默认材质");
            return GetDefaultMaterial();
        }
        
        string typeId = blockInfo.type_id.ToUpper();
        
        // 特殊处理：air类型需要删除方块
        if (typeId == "AIR")
        {
            Debug.Log("检测到AIR类型方块，需要删除");
            return null; // 返回null表示需要删除方块
        }
        
        // 尝试从材质字典中获取
        if (materialDictionary.TryGetValue(typeId, out Texture2D material))
        {
            Debug.Log($"找到材质: {typeId}");
            return material;
        }
        
        // 如果没有找到，尝试解析数字ID
        if (int.TryParse(typeId, out int numericId))
        {
            return GetMaterialByNumericId(numericId);
        }
        
        Debug.LogWarning($"未找到材质: {typeId}，使用默认材质");
        return GetDefaultMaterial();
    }
    
    /// <summary>
    /// 根据数字ID获取材质
    /// </summary>
    /// <param name="numericId">数字ID</param>
    /// <returns>对应的材质纹理</returns>
    private Texture2D GetMaterialByNumericId(int numericId)
    {
        switch (numericId)
        {
            case 1: return materialDictionary["RANDOM"];
            case 2: return materialDictionary["RED"];
            case 3: return materialDictionary["BLUE"];
            case 4: return materialDictionary["GREEN"];
            case 5: return materialDictionary["YELLOW"];
            case 6: return materialDictionary["PURPLE"];
            case 7: return materialDictionary["ORANGE"];
            case 8: return materialDictionary["PINK"];
            case 9: return materialDictionary["CYAN"];
            case 10: return materialDictionary["LIME"];
            case 11: return materialDictionary["MAGENTA"];
            case 12: return materialDictionary["GRAY"];
            case 13: return materialDictionary["LIGHT_GRAY"];
            case 14: return materialDictionary["LIGHT_BLUE"];
            case 15: return materialDictionary["BROWN"];
            case 16: return materialDictionary["BLACK"];
            case 17: return materialDictionary["WHITE"];
            case 18: return materialDictionary["STONE"];
            case 19: return materialDictionary["DIRT"];
            case 20: return materialDictionary["GRASS"];
            default: return GetDefaultMaterial();
        }
    }
    
    /// <summary>
    /// 获取默认材质
    /// </summary>
    /// <returns>默认材质纹理</returns>
    private Texture2D GetDefaultMaterial()
    {
        return materialDictionary["RANDOM"];
    }

    private void CreateBlockRoot()
    {
        // 创建一个空的父对象用于管理所有方块，便于统一操作和清理
        blockRoot = new GameObject("BlockRoot");
        blockRoot.transform.position = Vector3.zero;
    }

    /// <summary>
    /// 在指定位置生成或删除方块（如果传入纹理为null则删除该位置的方块，否则生成/替换）
    /// </summary>
    /// <param name="position">方块位置</param>
    /// <param name="texture">方块纹理（可选）</param>
    public void Main(Vector3 position, Texture2D texture = null, byte[] pubKey = null)
    {
        if (texture == null && pubKey == null)
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
        CreateBlock(position, texture, pubKey);
    }

    /// <summary>
    /// 批量生成或删除方块（支持批量操作，纹理数组可选）
    /// </summary>
    /// <param name="positions">方块位置数组</param>
    /// <param name="textures">方块纹理数组（可选）</param>
    public void Main(Vector3[] positions, Texture2D[] textures = null, byte[][] pubKey = null)
        {
        if (positions.Length != textures.Length && positions.Length != pubKey.Length)
        {
            Debug.LogError("位置数组和纹理数组和pubKey数组长度不匹配");
            return;
        }

        for (int i = 0; i < positions.Length; i++)
        {
            if (textures == null || pubKey == null)
            {
                Main(positions[i], null, null);
            }
            else
            {
                Main(positions[i], textures[i], pubKey[i]);
            }

        }
    }

    /// <summary>
    /// 判断指定位置是否存在方块
    /// </summary>
    /// <param name="position">方块位置</param>
    /// <returns>是否存在方块</returns>
    public bool HasBlockAt(Vector3 position)
    {
        return blockDictionary_GameObject.ContainsKey(position);
    }

    /// <summary>
    /// 删除指定位置的方块（如果存在）
    /// </summary>
    /// <param name="position">方块位置</param>
    private void DeleteBlock(Vector3 position)
    {
        if (blockDictionary_GameObject.TryGetValue(position, out GameObject blockObject))
        {
            Destroy(blockObject);
            blockDictionary_GameObject.Remove(position);
            blockDictionary.Remove(position);
            blockDictionary_PubKey.Remove(position); // 删除公钥记录
            Debug.Log("已删除位置 " + position + " 的方块");
        }
        else
        {
            Debug.LogWarning("找不到位置 " + position + " 的方块以删除");
        }
    }

    /// <summary>
    /// 创建一个新的方块对象并设置其纹理和材质
    /// </summary>
    /// <param name="position">方块位置</param>
    /// <param name="texture">方块纹理</param>
    public void CreateBlock(Vector3 position, Texture2D texture, byte[] pubKey)
    {
        blockCounter++;
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one;
        cube.transform.SetParent(blockRoot.transform);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
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
        cube.name = string.Format("Block_{0}_{1}_{2}_{3}", blockCounter, position.x.ToString("F0"),
            position.y.ToString("F0"), position.z.ToString("F0"));
        blockDictionary_GameObject[position] = cube;
        blockDictionary_PubKey[position] = pubKey; // 存储公钥
        Debug.Log("已创建方块: " + cube.name + ", 纹理大小: " + texture.width + "x" + texture.height + ", 着色器: " +
                  material.shader.name);
    }

    /// <summary>
    /// 更新指定位置方块的纹理（如果存在）
    /// </summary>
    /// <param name="position">方块位置</param>
    /// <param name="texture">新的纹理</param>
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

    /// <summary>
    /// 创建一个带有细节的随机纹理（用于生成不同风格的方块）
    /// </summary>
    /// <returns>生成的随机纹理</returns>
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
            Debug.Log("已创建随机纹理 - 大小: " + texture.width + "x" + texture.height + ", 格式: " + texture.format + ", 基础颜色: " +
                      baseColor);
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

    /// <summary>
    /// 创建一个带有轻微噪声的纯色纹理
    /// </summary>
    /// <param name="color">基础颜色</param>
    /// <returns>生成的纯色纹理</returns>
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
            Debug.Log("已创建纯色纹理 - 大小: " + texture.width + "x" + texture.height + ", 格式: " + texture.format + ", 基础颜色: " +
                      color);
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
    /// 清空所有方块（用于同步世界状态时重置）
    /// </summary>
    public void ClearAllBlocks()
    {
        foreach (var pos in new List<Vector3>(blockDictionary_GameObject.Keys))
        {
            DeleteBlock(pos);
        }
        blockDictionary.Clear();
        blockDictionary_GameObject.Clear();
        blockDictionary_PubKey.Clear(); // 清空公钥字典
    }

    /// <summary>
    /// 设置方块及其公钥
    /// </summary>
    /// <param name="blockData">方块数据</param>
    /// <param name="pubKey">公钥</param>
    public void SetBlock(BlockData blockData, byte[] pubKey)
    {
        if (blockData?.point == null) return;
        
        Vector3 position = new Vector3(blockData.point.x, blockData.point.y, blockData.point.z);
        
        // 根据block_info获取对应的材质
        Texture2D texture = GetMaterialByBlockInfo(blockData.block_info);
        
        // 如果texture为null，表示需要删除方块（如AIR类型）
        if (texture == null)
        {
            if (blockDictionary_GameObject.ContainsKey(position))
            {
                Debug.Log($"SetBlock: 删除位置 {position} 的方块（AIR类型）");
                DeleteBlock(position);
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
        CreateBlock(position, texture, pubKey);
        
        Debug.Log($"SetBlock: 在位置 {position} 创建方块，类型: {blockData.block_info?.type_id ?? "未知"}");
    }

    /// <summary>
    /// 获取指定位置方块的公钥
    /// </summary>
    /// <param name="position">方块位置</param>
    /// <returns>公钥，如果不存在则返回null</returns>
    public byte[] GetPubKey(Vector3 position)
    {
        if (blockDictionary_PubKey.TryGetValue(position, out byte[] pubKey))
        {
            return pubKey;
        }
        return null;
    }

    /// <summary>
    /// 获取方块中心点
    /// </summary>
    public Vector3 GetBlockCenter(Vector3 hitPoint, Vector3 hitNormal)
    {
        Vector3 placePosition = hitPoint + hitNormal * 0.5f;
        return new Vector3(
            Mathf.Round(placePosition.x),
            Mathf.Round(placePosition.y),
            Mathf.Round(placePosition.z)
        );
    }
}