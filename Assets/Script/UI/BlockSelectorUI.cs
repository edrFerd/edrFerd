using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// 方块选择器UI，类似我的世界的下边框选择器
/// </summary>
public class BlockSelectorUI : BaseUI
{
    // 方块选择器主面板
    public GameObject selectorPanel;
    
    // 方块槽位UI组件列表
    private List<BlockSlotUI> blockSlots = new List<BlockSlotUI>();
    
    // 当前选中槽位索引
    private int selectedSlotIndex = 0;
    
    // 槽位数量，默认为9个（类似我的世界的物品栏）
    private const int SLOT_COUNT = 9;
    
    // 方块预览渲染器
    private BlockPreviewRenderer previewRenderer;
    
    public override void Init(UIManager manager)
    {
        base.Init(manager);
        
        // 创建方块预览渲染器
        previewRenderer = new BlockPreviewRenderer();
        
        // 创建选择器面板
        CreateSelectorPanel();
        
        // 创建初始槽位
        InitializeBlockSlots();
        
        // 默认选中第一个槽位
        SelectSlot(0);
    }
    
    /// <summary>
    /// 创建选择器面板
    /// </summary>
    private void CreateSelectorPanel()
    {
        // 创建主面板
        selectorPanel = new GameObject("SelectorPanel");
        selectorPanel.transform.SetParent(this.transform);
        
        // 添加Canvas组件
        Canvas canvas = selectorPanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // 添加Canvas Scaler组件以适应不同分辨率
        CanvasScaler scaler = selectorPanel.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // 添加GraphicRaycaster用于UI交互
        selectorPanel.AddComponent<GraphicRaycaster>();
    }
    
    /// <summary>
    /// 初始化方块槽位
    /// </summary>
    private void InitializeBlockSlots()
    {
        // 创建方块槽位的容器
        GameObject slotsContainer = new GameObject("SlotsContainer");
        slotsContainer.transform.SetParent(selectorPanel.transform, false);
        
        // 添加水平布局组件
        HorizontalLayoutGroup layoutGroup = slotsContainer.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.LowerCenter;
        layoutGroup.spacing = 10;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        
        // 设置RectTransform
        RectTransform containerRect = slotsContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.sizeDelta = new Vector2(0, 100);
        containerRect.anchoredPosition = new Vector2(0, 10);
        
        // 创建9个方块槽位
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            // 创建槽位GameObject
            GameObject slotObj = new GameObject($"BlockSlot_{i}");
            slotObj.transform.SetParent(slotsContainer.transform, false);
            
            // 添加Image组件作为槽位背景
            Image slotImage = slotObj.AddComponent<Image>();
            slotImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            
            // 设置RectTransform
            RectTransform slotRect = slotObj.GetComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(80, 80);
            
            // 创建并初始化槽位UI组件
            BlockSlotUI slotUI = slotObj.AddComponent<BlockSlotUI>();
            slotUI.Init(i, previewRenderer);
            
            // 添加到槽位列表
            blockSlots.Add(slotUI);
        }
        
        // 设置前三个槽位的方块类型
        blockSlots[0].SetBlockType("随机", new Color(0.8f, 0.8f, 0.8f, 1));
        blockSlots[1].SetBlockType("红色", new Color(0.9f, 0.3f, 0.3f, 1));
        blockSlots[2].SetBlockType("蓝色", new Color(0.3f, 0.5f, 0.9f, 1));
    }
    
    /// <summary>
    /// 选择指定索引的槽位
    /// </summary>
    public void SelectSlot(int index)
    {
        // 确保索引在有效范围内
        if (index < 0 || index >= blockSlots.Count)
            return;
        
        // 检查该槽位是否为空
        if (string.IsNullOrEmpty(blockSlots[index].GetBlockType()))
        {
            Debug.Log($"槽位 {index} 为空，无法选择");
            return;
        }
        
        // 取消当前选中槽位的高亮
        blockSlots[selectedSlotIndex].SetSelected(false);
        
        // 更新选中索引
        selectedSlotIndex = index;
        
        // 高亮新选中的槽位
        blockSlots[selectedSlotIndex].SetSelected(true);
        
        // 通知用户意图管理器槽位已更改
        uiManager.NotifySlotSelected(GetSelectedBlockType());
        
        Debug.Log($"选择了槽位 {index}，方块类型: {GetSelectedBlockType()}");
    }
    
    /// <summary>
    /// 获取当前选中槽位的方块类型
    /// </summary>
    public string GetSelectedBlockType()
    {
        return blockSlots[selectedSlotIndex].GetBlockType();
    }
    
    /// <summary>
    /// 处理数字键输入
    /// </summary>
    public void HandleNumKeyInput(int keyNumber)
    {
        // 数字键1-9对应索引0-8
        int index = keyNumber - 1;
        if (index >= 0 && index < SLOT_COUNT)
        {
            SelectSlot(index);
        }
    }
    
    /// <summary>
    /// 销毁时清理资源
    /// </summary>
    private void OnDestroy()
    {
        // 清理预览渲染器资源
        if (previewRenderer != null)
        {
            previewRenderer.Cleanup();
        }
    }
}

/// <summary>
/// 方块槽位UI组件
/// </summary>
public class BlockSlotUI : MonoBehaviour
{
    // 槽位索引
    private int slotIndex;
    
    // 方块类型
    private string blockType = "";
    
    // 槽位背景图像
    private Image backgroundImage;
    
    // 方块图像
    private RawImage blockPreviewImage;
    
    // 文本标签
    private Text blockLabel;
    
    // 方块预览渲染器引用
    private BlockPreviewRenderer previewRenderer;
    
    // 当前方块的纹理
    private Texture2D blockTexture;
    
    /// <summary>
    /// 初始化槽位
    /// </summary>
    public void Init(int index, BlockPreviewRenderer renderer)
    {
        slotIndex = index;
        previewRenderer = renderer;
        backgroundImage = GetComponent<Image>();
        
        // 创建方块预览图像
        GameObject blockImageObj = new GameObject("BlockPreview");
        blockImageObj.transform.SetParent(transform, false);
        blockPreviewImage = blockImageObj.AddComponent<RawImage>();
        
        RectTransform blockRect = blockPreviewImage.rectTransform;
        blockRect.anchorMin = new Vector2(0.1f, 0.2f);
        blockRect.anchorMax = new Vector2(0.9f, 0.9f);
        blockRect.sizeDelta = Vector2.zero;
        blockRect.anchoredPosition = Vector2.zero;
        blockRect.pivot = new Vector2(0.5f, 0.5f);
        // 强制正方形布局（宽高取最小值）
        blockImageObj.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        blockImageObj.GetComponent<AspectRatioFitter>().aspectRatio = 1f;
        
        // 创建文本标签
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(transform, false);
        blockLabel = labelObj.AddComponent<Text>();
        blockLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        blockLabel.fontSize = 14;
        blockLabel.alignment = TextAnchor.LowerCenter;
        blockLabel.color = Color.white;
        
        RectTransform labelRect = blockLabel.rectTransform;
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 0.2f);
        labelRect.sizeDelta = Vector2.zero;
        
        // 初始状态为空
        SetEmpty();
    }
    
    /// <summary>
    /// 设置槽位为空
    /// </summary>
    public void SetEmpty()
    {
        blockType = "";
        blockPreviewImage.enabled = false;
        blockLabel.text = "";
        blockTexture = null;
    }
    
    /// <summary>
    /// 设置方块类型
    /// </summary>
    public void SetBlockType(string type)
    {
        SetBlockType(type, new Color(0.5f, 0.5f, 0.5f, 1));
    }
    
    /// <summary>
    /// 设置方块类型和颜色
    /// </summary>
    public void SetBlockType(string type, Color color)
    {
        blockType = type;
        
        // 更新UI显示
        blockPreviewImage.enabled = true;
        
        // 设置方块文本
        blockLabel.text = type;
        
        // 获取WorldGenerator引用
        WorldGenerator worldGenerator = GameObject.FindObjectOfType<SYSManager>()?.worldGenerator;
        
        if (worldGenerator != null)
        {
            // 根据类型生成方块纹理，复用WorldGenerator中的方法
            if (type == "随机")
            {
                // 随机方块使用随机纹理
                blockTexture = worldGenerator.CreateRandomTexture();
            }
            else
            {
                // 对于纯色方块，使用WorldGenerator中的方法并传入颜色
                blockTexture = worldGenerator.CreateSolidColorTexture(color);
            }
            
                    // 使用预览渲染器渲染方块
        if (previewRenderer != null)
        {
            Texture renderTexture = previewRenderer.GetBlockPreview(blockTexture);
            blockPreviewImage.texture = renderTexture;
            
            // 设置RawImage的颜色为白色，确保正确显示纹理
            blockPreviewImage.color = Color.white;
            
            // 设置适当的UV矩形确保纹理正确显示
            blockPreviewImage.uvRect = new Rect(0, 0, 1, 1);
        }
        }
    }
    
    /// <summary>
    /// 获取方块类型
    /// </summary>
    public string GetBlockType()
    {
        return blockType;
    }
    
    /// <summary>
    /// 设置选中状态
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selected)
        {
            // 高亮显示
            backgroundImage.color = new Color(0.8f, 0.8f, 0.3f, 0.8f);
        }
        else
        {
            // 正常显示
            backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        }
    }
    

    

}

/// <summary>
/// 方块预览渲染器，用于渲染方块的3D预览
/// </summary>
public class BlockPreviewRenderer
{
    // 渲染纹理
    private RenderTexture renderTexture;
    
    // 预览相机
    private Camera previewCamera;
    
    // 预览方块
    private GameObject previewCube;
    
    // 预览场景
    private GameObject previewScene;
    
    // 缓存的预览图像
    private Dictionary<int, Texture2D> previewCache = new Dictionary<int, Texture2D>();
    
    // 构造函数
    public BlockPreviewRenderer()
    {
        // 创建预览场景
        CreatePreviewScene();
    }
    
    /// <summary>
    /// 创建预览场景
    /// </summary>
    private void CreatePreviewScene()
    {
        // 创建预览场景根物体
        previewScene = new GameObject("BlockPreviewScene");
        previewScene.hideFlags = HideFlags.HideAndDontSave;
        previewScene.layer = LayerMask.NameToLayer("Ignore Raycast");
        // 初始将预览场景移动到世界中心之外的远处，防止影响主世界
        previewScene.transform.position = new Vector3(1000000f, 1000000f, 1000000f);
        
        // 创建预览相机
        GameObject cameraObj = new GameObject("PreviewCamera");
        cameraObj.transform.SetParent(previewScene.transform);
        cameraObj.hideFlags = HideFlags.HideAndDontSave;
        cameraObj.layer = LayerMask.NameToLayer("Ignore Raycast");
        previewCamera = cameraObj.AddComponent<Camera>();
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0);
        previewCamera.orthographic = false;
        previewCamera.fieldOfView = 60;
        previewCamera.nearClipPlane = 0.01f;
        previewCamera.farClipPlane = 100f;
        previewCamera.enabled = false;
        // 将相机设为相对于预览场景根的本地偏移
        cameraObj.transform.localPosition = new Vector3(1.2f, 1.2f, 1.2f);
        // 使相机朝向预览方块所在的场景根位置
        previewCamera.transform.LookAt(previewScene.transform.position);
        previewCamera.renderingPath = RenderingPath.Forward;
        
        // 创建预览方块
        previewCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        previewCube.transform.SetParent(previewScene.transform);
        previewCube.transform.localPosition = Vector3.zero;
        previewCube.transform.localRotation = Quaternion.Euler(20, 45, 0);
        previewCube.hideFlags = HideFlags.HideAndDontSave;
        previewCube.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        // 创建主光源
        GameObject lightObj = new GameObject("PreviewLight");
        lightObj.transform.SetParent(previewScene.transform);
        lightObj.hideFlags = HideFlags.HideAndDontSave;
        lightObj.layer = LayerMask.NameToLayer("Ignore Raycast");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = Color.white;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        
        // 添加环境光以确保方块各面都能看清
        GameObject fillLightObj = new GameObject("FillLight");
        fillLightObj.transform.SetParent(previewScene.transform);
        fillLightObj.hideFlags = HideFlags.HideAndDontSave;
        fillLightObj.layer = LayerMask.NameToLayer("Ignore Raycast");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.5f;
        fillLight.color = new Color(0.7f, 0.7f, 0.8f); // 稍微偏蓝的填充光
        fillLightObj.transform.rotation = Quaternion.Euler(-30, 60, 0);
        
        // 创建渲染纹理
        renderTexture = new RenderTexture(128, 128, 24, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 4; // 启用抗锯齿
        renderTexture.useMipMap = true; // 使用mipmap提高质量
        renderTexture.filterMode = FilterMode.Bilinear; // 双线性过滤
        renderTexture.Create();
    }
    
        /// <summary>
    /// 获取方块预览图像
    /// </summary>
    public Texture GetBlockPreview(Texture2D blockTexture)
    {
        try 
        {
            // 如果方块纹理为空，返回null
            if (blockTexture == null)
                return null;
                
            // 计算纹理的哈希码用于缓存
            int textureHash = blockTexture.GetHashCode();
            
            // 检查缓存中是否已有该预览
            if (previewCache.TryGetValue(textureHash, out Texture2D cachedPreview))
            {
                return cachedPreview;
            }
            
            // 创建材质 - 尝试使用无光照着色器确保正确显示纹理
            Material material;
            if (Shader.Find("Unlit/Texture") != null)
            {
                material = new Material(Shader.Find("Unlit/Texture"));
            }
            else
            {
                material = new Material(Shader.Find("Standard"));
                // 对于Standard着色器，关闭光泽反射
                material.SetFloat("_Glossiness", 0.0f);
                material.SetFloat("_Metallic", 0.0f);
            }
            
            // 设置纹理
            material.mainTexture = blockTexture;
            material.color = Color.white;
            
            // 应用材质到预览方块
            previewCube.GetComponent<Renderer>().material = material;
            
            // 旋转方块以便更好地展示
            previewCube.transform.rotation = Quaternion.Euler(20, 45, 0);
            
            // 渲染预览
            previewCamera.targetTexture = renderTexture;
            previewCamera.Render();
            
            // 创建结果纹理
            Texture2D resultTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;
            resultTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            resultTexture.Apply();
            RenderTexture.active = null;
            
            // 缓存结果
            previewCache[textureHash] = resultTexture;
            
            Debug.Log("成功渲染方块预览，纹理尺寸: " + resultTexture.width + "x" + resultTexture.height);
            
            return resultTexture;
        }
        catch (System.Exception e)
        {
            Debug.LogError("渲染方块预览时出错: " + e.Message);
            
            // 创建一个简单的备用纹理（紫色，表示渲染错误）
            Texture2D fallbackTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color fallbackColor = new Color(0.8f, 0.2f, 0.8f, 1f); // 亮紫色
            
            for (int x = 0; x < fallbackTexture.width; x++)
            {
                for (int y = 0; y < fallbackTexture.height; y++)
                {
                    fallbackTexture.SetPixel(x, y, fallbackColor);
                }
            }
            
            fallbackTexture.Apply();
            return fallbackTexture;
        }
    }
    
    /// <summary>
    /// 清理资源
    /// </summary>
    public void Cleanup()
    {
        // 清理渲染纹理
        if (renderTexture != null)
        {
            renderTexture.Release();
            Object.Destroy(renderTexture);
        }
        
        // 清理预览场景
        if (previewScene != null)
        {
            Object.Destroy(previewScene);
        }
        
        // 清理缓存的预览图像
        foreach (var preview in previewCache.Values)
        {
            Object.Destroy(preview);
        }
        previewCache.Clear();
    }
} 