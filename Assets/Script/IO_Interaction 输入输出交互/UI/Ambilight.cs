using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///   <para>流光溢彩</para>
/// </summary>
public class Ambilight : MonoBehaviour
{
    public Image img;

    void Start()
    {
        img = this.GetComponent<Image>();
    }


    void FixedUpdate()
    {
        //img.color = Random.ColorHSV(); //赋予随机颜色
        img.color = new Color(64,64,64);
    }
}
