using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;

#region Data Structures

/// <summary>
/// 用于存储整个 .vox 文件解析后的数据
/// </summary>
public class VoxData
{
    public int Version { get; set; }
    public List<VoxModel> Models { get; private set; } = new List<VoxModel>();
    public Color32[] Palette { get; set; } = new Color32[256];
}

/// <summary>
/// 代表一个独立的体素模型
/// </summary>
public class VoxModel
{
    public Vector3Int Size { get; set; }
    public List<Voxel> Voxels { get; private set; } = new List<Voxel>();
}

/// <summary>
/// 代表一个单独的体素（方块）
/// </summary>
public struct Voxel
{
    public byte X, Y, Z;
    public byte ColorIndex;

    public Voxel(byte x, byte y, byte z, byte colorIndex)
    {
        X = x;
        Y = y;
        Z = z;
        ColorIndex = colorIndex;
    }
}

#endregion

/// <summary>
/// .vox 文件解析器
/// </summary>
public static class VoxParser
{
    public static VoxData Parse(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[VoxParser] File not found: {filePath}");
            return null;
        }

        using (var reader = new BinaryReader(File.OpenRead(filePath)))
        {
            var voxData = new VoxData();

            // 1. 文件头
            string magic = new string(reader.ReadChars(4));
            if (magic != "VOX ")
            {
                Debug.LogError("[VoxParser] Invalid .vox file format.");
                return null;
            }
            voxData.Version = reader.ReadInt32();

            // 2. 主块 (MAIN)
            string mainId = new string(reader.ReadChars(4));
            if (mainId != "MAIN")
            {
                Debug.LogError("[VoxParser] MAIN chunk not found.");
                return null;
            }

            reader.ReadInt32(); // MAIN 内容字节数 (忽略)
            reader.ReadInt32(); // MAIN 子块字节数 (忽略)

            var currentModel = new VoxModel();

            // 3. 遍历子块
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                string chunkId = new string(reader.ReadChars(4));
                int contentSize = reader.ReadInt32();
                int childrenSize = reader.ReadInt32();
                long chunkEndPos = reader.BaseStream.Position + contentSize + childrenSize;

                switch (chunkId)
                {
                    case "PACK":
                        // 如果有PACK，表示有多个模型，这里简化处理，只读取第一个
                        break;

                    case "SIZE":
                        currentModel = new VoxModel();
                        int x = reader.ReadInt32();
                        int y = reader.ReadInt32();
                        int z = reader.ReadInt32();
                        currentModel.Size = new Vector3Int(x, y, z);
                        voxData.Models.Add(currentModel);
                        break;

                    case "XYZI":
                        int numVoxels = reader.ReadInt32();
                        for (int i = 0; i < numVoxels; i++)
                        {
                            byte vx = reader.ReadByte();
                            byte vy = reader.ReadByte();
                            byte vz = reader.ReadByte();
                            byte colorIndex = reader.ReadByte();
                            currentModel.Voxels.Add(new Voxel(vx, vy, vz, colorIndex));
                        }
                        break;

                    case "RGBA":
                        for (int i = 0; i < 255; i++) // 规范说256色，但索引0是空的
                        {
                            byte r = reader.ReadByte();
                            byte g = reader.ReadByte();
                            byte b = reader.ReadByte();
                            byte a = reader.ReadByte();
                            voxData.Palette[i + 1] = new Color32(r, g, b, a);
                        }
                        // 最后一个字节通常是填充或未使用
                        reader.ReadBytes(4);
                        break;

                    default:
                        // 跳过未知块
                        reader.BaseStream.Seek(contentSize + childrenSize, SeekOrigin.Current);
                        break;
                }
                // 确保我们移动到下一个块的开始位置
                reader.BaseStream.Position = chunkEndPos;
            }

            return voxData;
        }
    }
}

/// <summary>
/// 负责导入 .vox 模型并使用 WorldGenerator 在场景中重建
/// </summary>
public class VoxImporter : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    private Dictionary<byte, Texture2D> _materialCache = new Dictionary<byte, Texture2D>();

    void Start()
    {
        // 示例：在启动时加载一个文件
        // 您需要将 "model.vox" 放在 StreamingAssets 文件夹中
        string filePath = Path.Combine(Application.streamingAssetsPath, "model.vox");
        if (File.Exists(filePath))
        {
            ImportAndRebuildFromFile(filePath, Vector3Int.zero);
        }
        else
        {
            Debug.LogWarning($"[VoxImporter] Voxel model not found at {filePath}. Please place a .vox file there.");
        }
    }

    /// <summary>
    /// 从文件路径加载、解析并重建一个 .vox 模型
    /// </summary>
    /// <param name="filePath">.vox 文件的完整路径</param>
    /// <param name="origin">重建模型的起始世界坐标</param>
    public void ImportAndRebuildFromFile(string filePath, Vector3Int origin)
    {
        if (worldGenerator == null)
        {
            Debug.LogError("[VoxImporter] WorldGenerator reference is not set!");
            return;
        }

        Debug.Log($"[VoxImporter] Parsing file: {filePath}");
        VoxData voxData = VoxParser.Parse(filePath);

        if (voxData == null)
        {
            Debug.LogError("[VoxImporter] Failed to parse .vox file.");
            return;
        }

        RebuildModel(voxData, origin);
    }

    /// <summary>
    /// 使用解析出的数据在场景中重建模型
    /// </summary>
    /// <param name="voxData">已解析的 .vox 数据</param>
    /// <param name="origin">重建模型的起始世界坐标</param>
    private void RebuildModel(VoxData voxData, Vector3Int origin)
    {
        _materialCache.Clear();

        // 遍历所有模型（通常只有一个）
        foreach (var model in voxData.Models)
        {
            Debug.Log($"[VoxImporter] Rebuilding model of size {model.Size} with {model.Voxels.Count} voxels.");
            foreach (var voxel in model.Voxels)
            {
                Texture2D blockTexture = GetOrCreateTextureForVoxel(voxel.ColorIndex, voxData.Palette);
                
                int worldX = origin.x + voxel.X;
                int worldY = origin.y + voxel.Y;
                int worldZ = origin.z + voxel.Z;

                worldGenerator.CreateBlock(worldX, worldY, worldZ, blockTexture);
            }
        }
        Debug.Log("[VoxImporter] Model rebuild complete.");
    }

    /// <summary>
    /// 为体素颜色创建或获取缓存的 Texture2D
    /// </summary>
    private Texture2D GetOrCreateTextureForVoxel(byte colorIndex, Color32[] palette)
    {
        if (_materialCache.TryGetValue(colorIndex, out Texture2D cachedTex))
        {
            return cachedTex;
        }

        Color32 color = palette[colorIndex];
        Texture2D newTex = worldGenerator.CreateSolidColorTexture(color);
        _materialCache[colorIndex] = newTex;
        return newTex;
    }
}
