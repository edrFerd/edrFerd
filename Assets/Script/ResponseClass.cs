using UnityEngine;

/// <summary>
/// 用于JSON序列化/反序列化的方块信息
/// </summary>
[System.Serializable]
public class BlockInfo
{
    // 位置: [x, y, z]，每个值小于指定变量（例如，30）
    public float[] Position;
    // 纹理: 256位BASE64字符串
    public string Texture;
    // 公钥: 256位随机字符串
    public string PublicKey;
    // 难度: 0-99
    public int Difficulty;
}

/// <summary>
/// 用于两个端点（完整世界状态和时钟更新）
/// </summary>
[System.Serializable]
public class BlockListResponse
{
    public BlockInfo[] Blocks;
}