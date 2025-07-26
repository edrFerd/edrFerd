using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 下拉框
/// </summary>
public class DropdownExample : MonoBehaviour
{
    public TMP_Dropdown dropdown; // 拖入你的Dropdown UI组件
    public Action<Enum> onEnumSelected; // 委托，参数为具体的枚举类型

    private Type enumType; // 枚举的类型

    /// <summary>
    /// 初始化Dropdown，传入枚举类型和选择后的回调委托
    /// </summary>
    public void Init(Type enumType, Action<Enum> onEnumSelected)
    {
        if (!enumType.IsEnum)
        {
            Debug.LogError("传入的类型不是枚举类型");
            return;
        }

        this.enumType = enumType;
        this.onEnumSelected = onEnumSelected;
        dropdown = this.GetComponent<TMP_Dropdown>();
        dropdown.options.Clear(); // 清空旧选项
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged); // 注册Dropdown值变化事件
        SetupDropdownOptions();
    }

    /// <summary>
    /// 当用户选择发生变化时调用
    /// </summary>
    public void OnDropdownValueChanged(int index)
    {
        // 获取枚举的数量
        int enumLength = Enum.GetValues(enumType).Length;

        // 检查index是否在合法范围内
        if (index < 0 || index >= enumLength)
        {
            Debug.LogError("选择的索引超出了枚举范围");
            return;
        }

        // 将index转换为对应的枚举
        Enum selectedEnum = (Enum)Enum.ToObject(enumType, index);

        // 调用委托并传递转换后的枚举
        onEnumSelected?.Invoke(selectedEnum);
    }

    /// <summary>
    /// 设置Dropdown的选项
    /// </summary>
    public void SetupDropdownOptions()
    {
        dropdown.options.Clear();

        // 获取枚举的所有值，并将它们作为选项添加到Dropdown
        foreach (Enum value in Enum.GetValues(enumType))
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(value.ToString()));
        }

        // 刷新Dropdown显示
        dropdown.RefreshShownValue();
    }

    /// <summary>
    /// 设置当前选中的枚举值
    /// </summary>
    public void SetSelectedEnum(Enum selectedEnum)
    {
        // 确保传入的枚举类型正确
        if (selectedEnum.GetType() != enumType)
        {
            Debug.LogError("传入的枚举类型与Dropdown的枚举类型不匹配");
            return;
        }

        // 获取枚举的值列表并找到与传入枚举相匹配的索引
        int index = Array.IndexOf(Enum.GetValues(enumType), selectedEnum);

        // 检查索引是否有效
        if (index >= 0 && index < dropdown.options.Count)
        {
            dropdown.SetValueWithoutNotify(index); // 设置当前选中的值
            dropdown.RefreshShownValue(); // 刷新Dropdown显示
        }
        else
        {
            Debug.LogError("未找到匹配的枚举值");
        }
    }
}