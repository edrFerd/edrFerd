using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// 用户意图管理器，管理用户当前意图和状态
/// </summary>
public class UserIntentManager : MonoBehaviour
{
    // 系统管理器引用
    private SYSManager sysManager;
    
    // UI管理器引用
    private UIManager uiManager;
    
    // 当前选中的方块类型
    private string selectedBlockType = "随机";
    
    /// <summary>
    /// 初始化用户意图管理器
    /// </summary>
    public void Init(SYSManager manager)
    {
        this.sysManager = manager;
        this.uiManager = manager.uiManager;
        
        // 设置UI管理器的用户意图管理器引用
        if (uiManager != null)
        {
            uiManager.SetUserIntentManager(this);
        }
        else
        {
            Debug.LogError("UIManager未初始化");
        }
    }
    
    /// <summary>
    /// 主循环处理
    /// </summary>
    public void Main()
    {
        // 处理数字键输入（1-6）
        HandleNumericInput();
    }
    
    /// <summary>
    /// 处理数字键输入
    /// </summary>
    private void HandleNumericInput()
    {
        // 检测数字键1-6
        for (int i = 1; i <= 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i - 1) || Input.GetKeyDown(KeyCode.Keypad1 + i - 1))
            {
                // 通知UI管理器处理数字键输入
                uiManager.HandleNumKeyInput(i);
                
                // 记录日志
                Debug.Log($"选择了方块槽位 {i}，方块类型: {selectedBlockType}");
                
                break;
            }
        }
    }
    
    /// <summary>
    /// 设置选中的方块类型
    /// </summary>
    public void SetSelectedBlockType(string blockType)
    {
        this.selectedBlockType = blockType;
        Debug.Log($"方块类型已更改为: {blockType}");
    }
    
    /// <summary>
    /// 获取当前选中的方块类型
    /// </summary>
    public string GetSelectedBlockType()
    {
        return selectedBlockType;
    }
    
    /// <summary>
    /// 创建方块
    /// </summary>
    /// <param name="position">方块位置</param>
    public async void CreateBlock(Vector3 position)
    {
        Debug.Log($"UserIntentManager: 开始创建方块，位置: {position}, 类型: {selectedBlockType}");
        
        if (sysManager.worldGenerator == null)
        {
            Debug.LogError("UserIntentManager: worldGenerator为空，无法创建方块");
            return;
        }
            
        // 根据选中的方块类型创建不同的方块
        switch (selectedBlockType)
        {
            case "随机":
                // 创建随机材质方块，复用WorldGenerator中的方法
                Texture2D randomTexture = sysManager.worldGenerator.CreateRandomTexture();
                sysManager.worldGenerator.Main(position, randomTexture, new byte[0]);
                break;
                
            case "红色":
                // 创建红色方块
                Texture2D redTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.8f, 0.2f, 0.2f));
                sysManager.worldGenerator.Main(position, redTexture, new byte[0]);
                break;
                
            case "蓝色":
                // 创建蓝色方块
                Texture2D blueTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.2f, 0.4f, 0.8f));
                sysManager.worldGenerator.Main(position, blueTexture, new byte[0]);
                break;
                
            case "绿色":
                // 创建绿色方块
                Texture2D greenTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.2f, 0.6f, 0.2f));
                sysManager.worldGenerator.Main(position, greenTexture, new byte[0]);
                break;
                
            case "黄色":
                // 创建黄色方块
                Texture2D yellowTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.9f, 0.9f, 0.2f));
                sysManager.worldGenerator.Main(position, yellowTexture, new byte[0]);
                break;
                
            case "紫色":
                // 创建紫色方块
                Texture2D purpleTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.6f, 0.2f, 0.8f));
                sysManager.worldGenerator.Main(position, purpleTexture, new byte[0]);
                break;
                
            case "橙色":
                // 创建橙色方块
                Texture2D orangeTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.9f, 0.6f, 0.2f));
                sysManager.worldGenerator.Main(position, orangeTexture, new byte[0]);
                break;
                
            case "粉色":
                // 创建粉色方块
                Texture2D pinkTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.9f, 0.6f, 0.8f));
                sysManager.worldGenerator.Main(position, pinkTexture, new byte[0]);
                break;
                
            case "青色":
                // 创建青色方块
                Texture2D cyanTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.2f, 0.8f, 0.8f));
                sysManager.worldGenerator.Main(position, cyanTexture, new byte[0]);
                break;
                
            case "黄绿色":
                // 创建黄绿色方块
                Texture2D limeTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.6f, 0.9f, 0.2f));
                sysManager.worldGenerator.Main(position, limeTexture, new byte[0]);
                break;
                
            case "品红色":
                // 创建品红色方块
                Texture2D magentaTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.8f, 0.2f, 0.8f));
                sysManager.worldGenerator.Main(position, magentaTexture, new byte[0]);
                break;
                
            case "灰色":
                // 创建灰色方块
                Texture2D grayTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.5f, 0.5f, 0.5f));
                sysManager.worldGenerator.Main(position, grayTexture, new byte[0]);
                break;
                
            case "浅灰色":
                // 创建浅灰色方块
                Texture2D lightGrayTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.7f, 0.7f, 0.7f));
                sysManager.worldGenerator.Main(position, lightGrayTexture, new byte[0]);
                break;
                
            case "浅蓝色":
                // 创建浅蓝色方块
                Texture2D lightBlueTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.6f, 0.8f, 0.9f));
                sysManager.worldGenerator.Main(position, lightBlueTexture, new byte[0]);
                break;
                
            case "棕色":
                // 创建棕色方块
                Texture2D brownTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.6f, 0.4f, 0.2f));
                sysManager.worldGenerator.Main(position, brownTexture, new byte[0]);
                break;
                
            case "黑色":
                // 创建黑色方块
                Texture2D blackTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));
                sysManager.worldGenerator.Main(position, blackTexture, new byte[0]);
                break;
                
            case "白色":
                // 创建白色方块
                Texture2D whiteTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.9f, 0.9f, 0.9f));
                sysManager.worldGenerator.Main(position, whiteTexture, new byte[0]);
                break;
                
            case "石头":
                // 创建石头方块
                Texture2D stoneTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.5f, 0.5f, 0.5f));
                sysManager.worldGenerator.Main(position, stoneTexture, new byte[0]);
                break;
                
            case "泥土":
                // 创建泥土方块
                Texture2D dirtTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.6f, 0.4f, 0.2f));
                sysManager.worldGenerator.Main(position, dirtTexture, new byte[0]);
                break;
                
            case "草地":
                // 创建草地方块
                Texture2D grassTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.3f, 0.7f, 0.3f));
                sysManager.worldGenerator.Main(position, grassTexture, new byte[0]);
                break;
                
            default:
                // 默认创建随机方块
                Texture2D defaultTexture = sysManager.worldGenerator.CreateRandomTexture();
                sysManager.worldGenerator.Main(position, defaultTexture, new byte[0]);
                break;
        }
        
        Debug.Log($"UserIntentManager: 本地方块创建完成，准备向服务器发送请求");
        
        // 向服务器声明方块创建
        if (sysManager.networkManager != null)
        {
            var blockData = new BlockData
            {
                point = new PointData
                {
                    x = (int)position.x,
                    y = (int)position.y,
                    z = (int)position.z
                },
                block_info = new BlockInfoData
                {
                    type_id = GetBlockTypeId(selectedBlockType)
                }
            };
            
            Debug.Log($"UserIntentManager: 调用set_backend_block，位置: ({blockData.point.x}, {blockData.point.y}, {blockData.point.z}), 类型: {blockData.block_info.type_id}");
            await sysManager.networkManager.set_backend_block(blockData, 0);
            Debug.Log($"UserIntentManager: set_backend_block调用完成");
        }
        else
        {
            Debug.LogError("UserIntentManager: networkManager为空，无法向服务器发送请求");
        }
    }
    
    /// <summary>
    /// 删除方块
    /// </summary>
    public async void DeleteBlock(Vector3 position)
    {
        Debug.Log($"UserIntentManager: 开始删除方块，位置: {position}");
        
        if (sysManager != null && sysManager.worldGenerator != null)
        {
            sysManager.worldGenerator.Main(position, null, null);
            Debug.Log($"UserIntentManager: 本地方块删除完成，准备向服务器发送请求");
            
            // 向服务器声明方块删除
            if (sysManager.networkManager != null)
            {
                var pointData = new PointData
                {
                    x = (int)position.x,
                    y = (int)position.y,
                    z = (int)position.z
                };
                
                Debug.Log($"UserIntentManager: 调用remove_block，位置: ({pointData.x}, {pointData.y}, {pointData.z})");
                await sysManager.networkManager.remove_block(pointData);
                Debug.Log($"UserIntentManager: remove_block调用完成");
            }
            else
            {
                Debug.LogError("UserIntentManager: networkManager为空，无法向服务器发送删除请求");
            }
        }
        else
        {
            Debug.LogError("UserIntentManager: sysManager或worldGenerator为空，无法删除方块");
        }
    }
    
    /// <summary>
    /// 根据方块类型获取类型ID
    /// </summary>
    /// <param name="blockType">方块类型</param>
    /// <returns>类型ID</returns>
    private string GetBlockTypeId(string blockType)
    {
        switch (blockType)
        {
            case "随机":
                return "RANDOM";
            case "红色":
                return "RED";
            case "蓝色":
                return "BLUE";
            case "绿色":
                return "GREEN";
            case "黄色":
                return "YELLOW";
            case "紫色":
                return "PURPLE";
            case "橙色":
                return "ORANGE";
            case "粉色":
                return "PINK";
            case "青色":
                return "CYAN";
            case "黄绿色":
                return "LIME";
            case "品红色":
                return "MAGENTA";
            case "灰色":
                return "GRAY";
            case "浅灰色":
                return "LIGHT_GRAY";
            case "浅蓝色":
                return "LIGHT_BLUE";
            case "棕色":
                return "BROWN";
            case "黑色":
                return "BLACK";
            case "白色":
                return "WHITE";
            case "石头":
                return "STONE";
            case "泥土":
                return "DIRT";
            case "草地":
                return "GRASS";
            case "空气":
            case "AIR":
                return "AIR";
            default:
                return "RANDOM";
        }
    }

} 