using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI管理器，负责管理所有UI组件
/// </summary>
public class UIManager : MonoBehaviour
{
    // UI根对象
    private GameObject uiRoot;
    
    // 方块选择器UI
    public BlockSelectorUI blockSelectorUI;
    
    // UI组件字典，用于快速访问
    private Dictionary<string, BaseUI> uiComponents = new Dictionary<string, BaseUI>();
    
    // 系统管理器引用
    private SYSManager sysManager;
    
    // 用户意图管理器引用
    private UserIntentManager userIntentManager;
    
    /// <summary>
    /// 初始化UI管理器
    /// </summary>
    public void Init(SYSManager manager)
    {
        this.sysManager = manager;
        
        // 创建UI根对象
        CreateUIRoot();
        
        // 初始化方块选择器UI
        InitializeBlockSelectorUI();
    }
    
    /// <summary>
    /// 设置用户意图管理器引用
    /// </summary>
    public void SetUserIntentManager(UserIntentManager manager)
    {
        this.userIntentManager = manager;
    }
    
    /// <summary>
    /// 创建UI根对象
    /// </summary>
    private void CreateUIRoot()
    {
        // 创建UI根对象
        uiRoot = new GameObject("UIRoot");
        
        // 确保UI根对象不会被销毁
        DontDestroyOnLoad(uiRoot);
    }
    
    /// <summary>
    /// 初始化方块选择器UI
    /// </summary>
    private void InitializeBlockSelectorUI()
    {
        // 创建方块选择器GameObject
        GameObject blockSelectorObj = new GameObject("BlockSelector");
        blockSelectorObj.transform.SetParent(uiRoot.transform);
        
        // 添加BlockSelectorUI组件
        blockSelectorUI = blockSelectorObj.AddComponent<BlockSelectorUI>();
        blockSelectorUI.Init(this);
        
        // 将方块选择器UI添加到字典中
        uiComponents.Add("BlockSelector", blockSelectorUI);
    }
    
    /// <summary>
    /// 处理数字键输入
    /// </summary>
    public void HandleNumKeyInput(int keyNumber)
    {
        if (keyNumber >= 1 && keyNumber <= 9)
        {
            blockSelectorUI.HandleNumKeyInput(keyNumber);
        }
    }
    
    /// <summary>
    /// 通知槽位选择变更
    /// </summary>
    public void NotifySlotSelected(string blockType)
    {
        if (userIntentManager != null)
        {
            userIntentManager.SetSelectedBlockType(blockType);
        }
    }
    
    /// <summary>
    /// 获取UI组件
    /// </summary>
    public T GetUIComponent<T>(string name) where T : BaseUI
    {
        if (uiComponents.TryGetValue(name, out BaseUI ui))
        {
            return ui as T;
        }
        return null;
    }
    
    /// <summary>
    /// 添加新UI组件
    /// </summary>
    public T AddUIComponent<T>(string name) where T : BaseUI
    {
        // 检查是否已存在同名组件
        if (uiComponents.ContainsKey(name))
        {
            Debug.LogWarning($"UI组件'{name}'已存在，无法添加");
            return null;
        }
        
        // 创建新的GameObject
        GameObject uiObj = new GameObject(name);
        uiObj.transform.SetParent(uiRoot.transform);
        
        // 添加组件
        T component = uiObj.AddComponent<T>();
        component.Init(this);
        
        // 将组件添加到字典中
        uiComponents.Add(name, component);
        
        return component;
    }
    
    /// <summary>
    /// 更新所有UI
    /// </summary>
    public void UpdateAllUI()
    {
        foreach (var ui in uiComponents.Values)
        {
            ui.UpdateUI();
        }
    }
    
    /// <summary>
    /// 显示指定UI
    /// </summary>
    public void ShowUI(string name)
    {
        if (uiComponents.TryGetValue(name, out BaseUI ui))
        {
            ui.Show();
        }
    }
    
    /// <summary>
    /// 隐藏指定UI
    /// </summary>
    public void HideUI(string name)
    {
        if (uiComponents.TryGetValue(name, out BaseUI ui))
        {
            ui.Hide();
        }
    }
} 