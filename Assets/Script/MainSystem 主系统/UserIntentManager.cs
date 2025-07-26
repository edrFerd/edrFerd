using UnityEngine;

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
    public void CreateBlock(Vector3 position)
    {
        if (sysManager.worldGenerator == null)
            return;
            
        // 根据选中的方块类型创建不同的方块
        switch (selectedBlockType)
        {
            case "随机":
                // 创建随机材质方块，复用WorldGenerator中的方法
                Texture2D randomTexture = sysManager.worldGenerator.CreateRandomTexture();
                sysManager.worldGenerator.Main(position, randomTexture);
                break;
                
            case "红色":
                // 创建红色方块
                Texture2D redTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.9f, 0.3f, 0.3f));
                sysManager.worldGenerator.Main(position, redTexture);
                break;
                
            case "蓝色":
                // 创建蓝色方块
                Texture2D blueTexture = sysManager.worldGenerator.CreateSolidColorTexture(new Color(0.3f, 0.5f, 0.9f));
                sysManager.worldGenerator.Main(position, blueTexture);
                break;
                
            default:
                // 默认创建随机方块
                Texture2D defaultTexture = sysManager.worldGenerator.CreateRandomTexture();
                sysManager.worldGenerator.Main(position, defaultTexture);
                break;
        }
    }
    
    /// <summary>
    /// 删除方块
    /// </summary>
    public void DeleteBlock(Vector3 position)
    {
        if (sysManager != null && sysManager.worldGenerator != null)
        {
            sysManager.worldGenerator.Main(position, null);
        }
    }

} 