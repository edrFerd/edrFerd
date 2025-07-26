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
        CreateBlockRoot();
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

    /// <summary>
    /// 批量生成或删除方块（支持批量操作，纹理数组可选）
    /// </summary>
    /// <param name="positions">方块位置数组</param>
    /// <param name="textures">方块纹理数组（可选）</param>
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
    public void CreateBlock(Vector3 position, Texture2D texture)
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
        cube.name = string.Format("Block_{0}_{1}_{2}_{3}", blockCounter, position.x.ToString("F0"),
            position.y.ToString("F0"), position.z.ToString("F0"));
        blockDictionary_GameObject[position] = cube;
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

    /// <summary>
    ///  调用这个函数，来修改世界中的方块
    /// </summary>
    /// <param name="block"></param>
    public void SetBlock(BlockData block, byte[] pubKey)
    {
        var point = block.point;
        var id = block.block_info.type_id;
        Debug.Log($"设置坐标为({point.x}, {point.y}, {point.z})的方块为类型{id}");
        // TODO 把这个函数完成
        Main(new Vector3(block.point.x,block.point.y,block.point.z), new Texture2D(1,1));
    }
}