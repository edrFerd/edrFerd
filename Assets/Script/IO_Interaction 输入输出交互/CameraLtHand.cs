using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using cakeslice;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static VisibleGrid;

/// <summary>
///   <para>摄像机之手 主要外部操作工具</para>
/// </summary>
public class CameraLtHand : MonoBehaviour
{
	public float defaultMoveSpeed = 15; // 默认摄像机旋转速度
	private float moveSpeed; // 移动速度
	public float rotationSpeed = 80f; // 当前摄像机旋转速度
	public float defaultVerticalMoveSpeed = 10; // 默认上下移动速度
	private float verticalMoveSpeed; // 上下移动速度
	public float boundaryExtension = 10f;//可移动视角的视角边界延伸长度,避免全屏时正好落在边界数值而无法移动视角
    
    // 创建方块的前方距离参数，可以在Unity编辑器中修改
    public float blockPlacementDistance = 2.0f;
    
    // 记录当前指向的位置坐标
    private Vector3 currentTargetPosition;
    // 记录上一次创建方块的位置
    private Vector3 lastBlockPosition;
    // 鼠标左键是否被按住
    private bool isMouseLeftHeld = false;
    
    // 光标控制
    public bool hideCursor = true; // 是否隐藏光标
    private bool isCursorHidden = false; // 光标当前是否隐藏

	public bool cameraMove = true; //相机移动
	public bool pointerMove = true; //指针移动

	public Transform pointer;             //指针
	public Transform pointer_2;           //指针
	private GameObject focusObject;       //焦点物体 需要被描边的物体 本次所选的功能块焦点对象
	private RaycastHit[] raycastHit;//本次指针路径上的对象列表

	//SYS
	public SYSManager sYSManager;

	// 鼠标点击检测相关
	private bool leftMousePressed = false;

    // 可视化相关
    private GameObject visualIndicator; // 可视化标识父物体
    private GameObject wireframeBlock; // 线框方块
    private Material wireframeMaterial; // 线框材质

    // 自定义垂直移动输入
    private float verticalInput = 0f;
    private float verticalVelocity = 0f;
    private float verticalSmoothTime = 0.1f; // 平滑时间，控制缓动速度

	public void Init(SYSManager sYSManager)
	{
		this.sYSManager = sYSManager;

		moveSpeed = defaultMoveSpeed;//存储速度
		verticalMoveSpeed = defaultVerticalMoveSpeed; // 存储默认上下移动速度

		OutlineEffect outlineEffect = sYSManager.resourceManager.MainCamera.gameObject.AddComponent<OutlineEffect>();//描边脚本
		outlineEffect.addLinesBetweenColors = true;
        
        // 创建线框可视化对象
        CreateWireframeVisualizer();
        
        // 初始化光标设置
        SetCursorState(hideCursor);
        
        // 初始化最后方块位置为无效值
        lastBlockPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);
	}
    
    /// <summary>
    /// 设置光标状态
    /// </summary>
    /// <param name="hide">是否隐藏光标</param>
    public void SetCursorState(bool hide)
    {
        if (hide)
        {
            // 隐藏光标并锁定在屏幕中心
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            isCursorHidden = true;
        }
        else
        {
            // 显示光标并解除锁定
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            isCursorHidden = false;
        }
    }
    
    /// <summary>
    /// 处理按键输入
    /// </summary>
    private void HandleKeyInput()
    {
        // 计算目标垂直输入值
        float targetVerticalInput = 0f;
        
        // 空格键按下时，向上移动（正值）
        if (Input.GetKey(KeyCode.Space))
        {
            targetVerticalInput += 1.0f;
        }
        
        // Shift键按下时，向下移动（负值）
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            targetVerticalInput -= 1.0f;
        }
        
        // 使用SmoothDamp实现缓动效果，类似于Input.GetAxis的效果
        verticalInput = Mathf.SmoothDamp(verticalInput, targetVerticalInput, ref verticalVelocity, verticalSmoothTime);
    }


	/// <summary>
	///   <para>主函数</para>
	/// </summary>
	public void Main()
	{
        // 处理按键输入
        HandleKeyInput();
        
		//---------------------------处理相机移动-----------------------------
		if (cameraMove)
		{//"相机移动许可已下达!"
			HandleMovement();
			HandleRotation();
		}
		//---------------------------处理相机移动-----------------------------


		//---------------------------处理指针移动-----------------------------
		if (pointerMove)
		{//"指针移动许可已下达!"
			if (!Tools.CoordinateOutOfBounds(Input.mousePosition, boundaryExtension))
			{//指针位置未越界
				bool selectionPoint = false;//选点

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 从相机发射一条射线
				this.raycastHit = Physics.RaycastAll(ray);// 使用Physics.RaycastAll获取所有碰撞体 // 存储所有击中停留处的ray数据
				Array.Sort(this.raycastHit, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));// 对结果按距离从近到远进行排序 因为原始结果未排序
				int i = 0;
				bool[] isExecutionCompleted = new bool[4];//执行完毕

				OutlineEffect.Instance?.ClearAllOutlines();//清理所有使用的Outline


				foreach (var hit in this.raycastHit)
				{// 遍历所有击中点
					Debug.Log("functionBlocks hit" + i + " " + hit.collider.gameObject.name + " " + this.raycastHit.Length);
					i++;
					if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) break;//点击在UI上就不再判断
				}
				// pointer.transform.position = Input.mousePosition; //更新指针位置
			}
			else
			{//指针位置越界 复位
				// pointer.transform.position = new Vector3(0, 0, 0);
			}
		}

		// 线框始终显示（无需按键）
		if (wireframeBlock != null)
		{
			wireframeBlock.SetActive(true);
		}
		
		// 更新线框方块位置，考虑碰撞检测
		UpdateWireframeBlockPosition();
		
		// 检测鼠标右键操作，处理方块创建
		HandleContinuousBlockCreation();
	}
	//---------------------------处理指针移动-----------------------------
    
    /// <summary>
    /// 创建线框可视化对象
    /// </summary>
    private void CreateWireframeVisualizer()
    {
        // 创建线框材质
        wireframeMaterial = new Material(Shader.Find("Standard"));
        wireframeMaterial.SetFloat("_Mode", 3); // 透明模式
        wireframeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        wireframeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        wireframeMaterial.SetInt("_ZWrite", 0);
        wireframeMaterial.DisableKeyword("_ALPHATEST_ON");
        wireframeMaterial.EnableKeyword("_ALPHABLEND_ON");
        wireframeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        wireframeMaterial.renderQueue = 3000;
        wireframeMaterial.color = new Color(1f, 1f, 1f, 0.8f); // 纯白色，较高透明度
        
        // 创建可视化标识父物体
        visualIndicator = new GameObject("可视化标识");
        
        // 创建线框方块
        wireframeBlock = new GameObject("线框方块");
        wireframeBlock.transform.SetParent(visualIndicator.transform);
        
        // 添加LineRenderer组件
        CreateCubeWithLineRenderer(wireframeBlock, 0.5f, 0.01f); // 将线宽从0.02减小到0.01
        
        // 初始状态设置为隐藏
        wireframeBlock.SetActive(false);
        
        Debug.Log("已创建线框可视化对象");
    }
    
    /// <summary>
    /// 使用LineRenderer创建立方体线框
    /// </summary>
    private void CreateCubeWithLineRenderer(GameObject parent, float size, float lineWidth)
    {
        // 创建12条独立的线条，每条线对应立方体的一条边
        // 这样可以避免LineRenderer自动连接所有点导致的额外对角线
        CreateEdgeLine(parent, new Vector3(-size, -size, -size), new Vector3(size, -size, -size), lineWidth, 0); // 底面 前边
        CreateEdgeLine(parent, new Vector3(size, -size, -size), new Vector3(size, -size, size), lineWidth, 1);   // 底面 右边
        CreateEdgeLine(parent, new Vector3(size, -size, size), new Vector3(-size, -size, size), lineWidth, 2);   // 底面 后边
        CreateEdgeLine(parent, new Vector3(-size, -size, size), new Vector3(-size, -size, -size), lineWidth, 3); // 底面 左边
        
        CreateEdgeLine(parent, new Vector3(-size, size, -size), new Vector3(size, size, -size), lineWidth, 4);   // 顶面 前边
        CreateEdgeLine(parent, new Vector3(size, size, -size), new Vector3(size, size, size), lineWidth, 5);     // 顶面 右边
        CreateEdgeLine(parent, new Vector3(size, size, size), new Vector3(-size, size, size), lineWidth, 6);     // 顶面 后边
        CreateEdgeLine(parent, new Vector3(-size, size, size), new Vector3(-size, size, -size), lineWidth, 7);   // 顶面 左边
        
        CreateEdgeLine(parent, new Vector3(-size, -size, -size), new Vector3(-size, size, -size), lineWidth, 8); // 左前 竖边
        CreateEdgeLine(parent, new Vector3(size, -size, -size), new Vector3(size, size, -size), lineWidth, 9);   // 右前 竖边
        CreateEdgeLine(parent, new Vector3(size, -size, size), new Vector3(size, size, size), lineWidth, 10);    // 右后 竖边
        CreateEdgeLine(parent, new Vector3(-size, -size, size), new Vector3(-size, size, size), lineWidth, 11);  // 左后 竖边
    }
    
    /// <summary>
    /// 创建单条边线
    /// </summary>
    private void CreateEdgeLine(GameObject parent, Vector3 start, Vector3 end, float width, int index)
    {
        // 为每条边创建一个单独的GameObject和LineRenderer
        GameObject edgeLine = new GameObject("EdgeLine_" + index);
        edgeLine.transform.SetParent(parent.transform);
        
        LineRenderer lineRenderer = edgeLine.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;  // 只有两个点
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        
        // 设置线条属性
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.material = wireframeMaterial;
        lineRenderer.startColor = new Color(1f, 1f, 1f, 0.8f); // 纯白色
        lineRenderer.endColor = new Color(1f, 1f, 1f, 0.8f);
        lineRenderer.useWorldSpace = false; // 使用局部坐标
    }
	
	/// <summary>
    /// 更新线框方块的位置到当前摄像机前方，考虑碰撞检测
    /// </summary>
    private void UpdateWireframeBlockPosition()
    {
        if (wireframeBlock == null) return;
        
        // 从摄像机位置发射射线
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        // 默认位置（无碰撞时）：摄像机前方指定距离的位置
        Vector3 position = transform.position + transform.forward * blockPlacementDistance;
        Vector3 roundedPosition;
        
        // 检测是否有碰撞
        if (Physics.Raycast(ray, out hit, blockPlacementDistance * 2))
        {
            // 有碰撞，获取碰撞点
            Vector3 hitPoint = hit.point;
            
            // 根据碰撞面的法线确定放置位置（将方块放在碰撞面的相邻位置）
            // 由于我们希望方块贴着碰撞面放置，所以从碰撞点沿法线方向稍微偏移一点
            Vector3 placePosition = hitPoint + hit.normal * 0.5f; // 偏移0.5个单位，刚好是一个方块的中心点
            
            // 将位置四舍五入到整数坐标
            roundedPosition = new Vector3(
                Mathf.Round(placePosition.x),
                Mathf.Round(placePosition.y),
                Mathf.Round(placePosition.z)
            );
            
            // Debug信息
            Debug.DrawRay(hit.point, hit.normal, Color.green, 0.1f);
            Debug.Log("检测到碰撞，调整位置到: " + roundedPosition);
        }
        else
        {
            // 无碰撞，使用默认位置
            roundedPosition = new Vector3(
                Mathf.Round(position.x),
                Mathf.Round(position.y),
                Mathf.Round(position.z)
            );
        }
        
        // 保存当前目标位置
        currentTargetPosition = roundedPosition;
        
        // 更新线框方块位置
        wireframeBlock.transform.position = roundedPosition;
    }
	
	/// <summary>
    /// 处理连续方块创建（按住鼠标右键拖动）
    /// </summary>
    private void HandleContinuousBlockCreation()
    {
        // 检测鼠标右键按下，开始创建方块
        if (Input.GetMouseButtonDown(1) && 
            (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()) && 
            !isMouseLeftHeld) // 确保上次操作已完成（右键已抬起）
        {
            // 鼠标右键按下，开始创建方块
            isMouseLeftHeld = true;
            // 立即创建一个方块
            TryCreateBlockAtCurrentPosition();
            
            // 更新上次创建的方块位置
            lastBlockPosition = currentTargetPosition;
        }
        
        // 检测鼠标右键抬起，停止创建方块
        if (Input.GetMouseButtonUp(1))
        {
            // 鼠标右键抬起，停止创建方块
            isMouseLeftHeld = false;
            // 重置上次创建的方块位置为一个不可能的值
            lastBlockPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        }
        
        // 移除此部分逻辑，不再在按住状态下创建多个方块
        /*
        // 如果鼠标右键被按住，检查是否需要创建新方块
        if (isMouseLeftHeld)
        {
            // 计算当前位置与上次位置的距离
            float distance = Vector3.Distance(currentTargetPosition, lastBlockPosition);
            
            // 如果距离足够远，说明移动到了一个新位置
            if (distance >= 0.1f) // 使用较小的阈值确保可以检测到位置变化
            {
                // 尝试在当前位置创建方块
                TryCreateBlockAtCurrentPosition();
                
                // 无论成功与否，都更新上次尝试位置，避免重复检测
                lastBlockPosition = currentTargetPosition;
            }
        }
        */
    }
    
    /// <summary>
    /// 尝试在当前位置创建方块（如果该位置没有方块或已有方块需要被替换）
    /// </summary>
    private void TryCreateBlockAtCurrentPosition()
    {
        // 确保系统管理器已初始化
        if (sYSManager == null || sYSManager.worldGenerator == null)
        {
            Debug.LogError("SYSManager或WorldGenerator未初始化");
            return;
        }
        
        // 无论该位置是否已有方块，都尝试创建新方块
        // 现有逻辑会先删除已有方块，然后创建新方块
        CreateBlockInFront();
    }
	
	/// <summary>
	/// 检测鼠标右键点击，创建随机位置和材质的方块
	/// </summary>
	private void CheckMouseInputForBlockCreation()
	{
		// 检测鼠标右键点击，并确保系统管理器和事件系统存在
		if (Input.GetMouseButtonDown(1) && 
		    (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
		{
			// 使用摄像机前方位置创建方块，而不是随机位置
			CreateBlockInFront();
		}
	}
	
	/// <summary>
	/// 在摄像机前方指定距离处创建方块
	/// </summary>
	private void CreateBlockInFront()
	{
		// 使用已经计算好的目标位置
        Vector3 roundedPosition = currentTargetPosition;
		
		// 10%的概率执行删除操作
		if (UnityEngine.Random.value < 0.1f)
		{
			// 删除操作时传入null材质
			Debug.Log("执行删除操作，位置：" + roundedPosition);
			sYSManager.worldGenerator.Main(roundedPosition, null);
		}
		else
		{
		    // 使用用户意图管理器创建方块（如果可用）
		    if (sYSManager.userIntentManager != null)
		    {
		        sYSManager.userIntentManager.CreateBlock(roundedPosition);
		        Debug.Log("在位置 " + roundedPosition + " 创建了一个新方块（通过用户意图管理器）");
		    }
		    else
		    {
			    // 使用旧方式创建方块（备用）
			Texture2D randomTexture = sYSManager.worldGenerator.CreateRandomTexture();
			sYSManager.worldGenerator.Main(roundedPosition, randomTexture);
			Debug.Log("在位置 " + roundedPosition + " 创建了一个新方块");
		    }
		}
	}
    
    /// <summary>
    /// 处理摄像机在水平面上的移动（忽略高度方向）
    /// </summary>
    private void HandleMovement()
    {
        // 获取水平和垂直输入
        float horizontalInput = Input.GetAxis("Horizontal"); // A, D or 左右箭头
        float forwardInput = Input.GetAxis("Vertical");     // W, S or 上下箭头

        // 如果按下Ctrl键，水平和垂直移动速度每帧递增1%，否则恢复默认速度
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            moveSpeed *= 1.01f;
            verticalMoveSpeed *= 1.01f; // 垂直速度也增加
        }
        else
        {
            moveSpeed = defaultMoveSpeed;
            verticalMoveSpeed = defaultVerticalMoveSpeed; // 恢复默认垂直速度
        }

        // 计算"水平面"上的前向向量（剔除 y 分量并归一化）
        Vector3 flatForward = transform.forward;
        flatForward.y = 0;
        flatForward.Normalize();

        // 计算"水平面"上的侧向向量（剔除 y 分量并归一化）
        Vector3 flatRight = transform.right;
        flatRight.y = 0;
        flatRight.Normalize();

        // 根据输入在水平面上移动
        Vector3 forwardMovement = flatForward * forwardInput * moveSpeed * Time.deltaTime;
        Vector3 strafeMovement  = flatRight   * horizontalInput * moveSpeed * Time.deltaTime;
        
        // 垂直移动：使用平滑处理后的verticalInput
        Vector3 verticalMovement = Vector3.up * verticalInput * verticalMoveSpeed * Time.deltaTime;

        // 应用水平和垂直移动
        transform.position += forwardMovement + strafeMovement + verticalMovement;
    }


	/// <summary>
    /// 处理摄像机旋转
    /// </summary>
    private void HandleRotation()
    {
        // 如果光标已锁定，使用鼠标移动来旋转摄像机
        if (isCursorHidden)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * 0.02f;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * 0.02f;

            // 当前的俯仰角度（绕X轴旋转）
            float currentPitch = transform.eulerAngles.x;
            if (currentPitch > 180) currentPitch -= 360; // 将范围从0-360转化为-180到180

            // 限制俯仰角度
            float newPitch = Mathf.Clamp(currentPitch - mouseY, -80f, 80f);

            // 应用旋转
            transform.eulerAngles = new Vector3(newPitch, transform.eulerAngles.y + mouseX, 0);
        }
    }
}