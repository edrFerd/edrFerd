using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///   <para>LED显示屏</para>
/// </summary>
public class LEDScreen : MonoBehaviour
{
    public int refreshRate;                     //刷新率

    private string displayBuffer;               //显示缓冲区
    [SerializeField]
    private TMPro.TextMeshProUGUI textComponent;


    void Start()
    {
        textComponent = this.GetComponent<TMPro.TextMeshProUGUI>();
        textComponent.font = GameObject.Find("MainSYS").GetComponent<ResourceManager>().fontAsset; //修改字体
    }


    /// <summary>
    ///   <para>清屏</para>
    /// </summary>
    public void ClearText()
    {
        displayBuffer = "";
    }


    /// <summary>
    ///   <para>显示文字</para>
    /// </summary>
    public void AddText(string inText)
    {
        displayBuffer = inText;
        textComponent.text = displayBuffer;
    }
}
