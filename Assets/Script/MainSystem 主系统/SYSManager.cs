using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 系统管理器
/// </summary>
public class SYSManager : MonoBehaviour
{
    public WorldMachine worldMachine;
    public EventSystem eventSystem;
    public ResourceManager resourceManager;
    public CameraLtHand cameraLtHand;
    public WorldGenerator worldGenerator;
    public NetworkManager networkManager;
    public UIManager uiManager;
    public UserIntentManager userIntentManager;

    /// <summary>
    ///   <para>初始化</para>
    /// </summary>
    public void Init()
    {
        this.worldMachine = this.gameObject.GetComponent<WorldMachine>();

        this.eventSystem = GameObject.Find("WorldMachine").GetComponent<EventSystem>();

        this.resourceManager = this.gameObject.AddComponent<ResourceManager>();
        resourceManager.Init();

        this.cameraLtHand = GameObject.Find("Main Camera").AddComponent<CameraLtHand>();
        cameraLtHand.Init(this);
        
        this.worldGenerator = this.gameObject.AddComponent<WorldGenerator>();
        worldGenerator.Init(this);

        this.networkManager = this.gameObject.AddComponent<NetworkManager>();
        networkManager.Init();
        
        // 初始化UI管理器
        this.uiManager = this.gameObject.AddComponent<UIManager>();
        uiManager.Init(this);
        
        // 初始化用户意图管理器
        this.userIntentManager = this.gameObject.AddComponent<UserIntentManager>();
        userIntentManager.Init(this);
    }
    
    /// <summary>
    /// 主循环更新
    /// </summary>
    public void Update()
    {
        // 更新用户意图管理器
        if (userIntentManager != null)
        {
            userIntentManager.Main();
        }
    }
}
