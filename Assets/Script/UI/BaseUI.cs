using UnityEngine;

/// <summary>
/// UI基类，所有UI组件都应继承此类
/// </summary>
public abstract class BaseUI : MonoBehaviour
{
    protected UIManager uiManager;
    
    /// <summary>
    /// UI初始化
    /// </summary>
    /// <param name="manager">UI管理器引用</param>
    public virtual void Init(UIManager manager)
    {
        this.uiManager = manager;
    }
    
    /// <summary>
    /// 显示UI
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 隐藏UI
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 更新UI
    /// </summary>
    public virtual void UpdateUI()
    {
        // 子类根据需要重写此方法
    }
} 