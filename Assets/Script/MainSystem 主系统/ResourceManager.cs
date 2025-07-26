using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
///   <para>资源管理器</para>
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public Material infrastructure;
    public Material supportStructure;//支撑结构
    public Material envelopEnclosure;
    public Material functionEnclosure;
    public Material conveyingStructure;
    public Material autonomousEntity;
    public Material conceptualObject;
    public Material load;

    public Material completelyTransparent;
    public Material explosion;

    //terrainMaterial = Resources.Load<Material>("Art/Material/UniversalBlock");

    public TMP_FontAsset fontAsset; //中文字体

    public Camera MainCamera;


    public void Init()
    {
        infrastructure = new Material(Shader.Find("Standard"));// 创建一个新的材质实例，这里使用 Unity 内置的标准着色器
        infrastructure.SetFloat("_Glossiness", 0f); // 设置光滑度（Glossiness）为 0，没有高光反射
        infrastructure.SetFloat("_Metallic", 0f); // 设置金属度（Metallic）为 0
        infrastructure.color = new Color(
            20 / 255f,
            20 / 255f,
            20 / 255f,
            255 / 255f);

        supportStructure = new Material(Shader.Find("Standard"));// 创建一个新的材质实例，这里使用 Unity 内置的标准着色器
        supportStructure.SetFloat("_Glossiness", 0f); // 设置光滑度（Glossiness）为 0，没有高光反射
        supportStructure.SetFloat("_Metallic", 0f); // 设置金属度（Metallic）为 0
        supportStructure.color = new Color(
            32 / 255f,
            32 / 255f,
            32 / 255f,
            255 / 255f);

        envelopEnclosure = new Material(Shader.Find("Standard"));// 创建一个新的材质实例，这里使用 Unity 内置的标准着色器
        envelopEnclosure.SetFloat("_Glossiness", 0f); // 设置光滑度（Glossiness）为 0，没有高光反射
        envelopEnclosure.SetFloat("_Metallic", 0f); // 设置金属度（Metallic）为 0
        envelopEnclosure.color = new Color(
            50 / 255f,
            50 / 255f,
            50 / 255f,
            255 / 255f);

        functionEnclosure = new Material(Shader.Find("Standard"));// 创建一个新的材质实例，这里使用 Unity 内置的标准着色器
        functionEnclosure.SetFloat("_Glossiness", 0f); // 设置光滑度（Glossiness）为 0，没有高光反射
        functionEnclosure.SetFloat("_Metallic", 0f); // 设置金属度（Metallic）为 0
        functionEnclosure.color = new Color(
            90 / 255f,
            90 / 255f,
            90 / 255f,
            255 / 255f);

        conveyingStructure = new Material(Shader.Find("Standard"));// 创建一个新的材质实例，这里使用 Unity 内置的标准着色器
        conveyingStructure.SetFloat("_Glossiness", 0f); // 设置光滑度（Glossiness）为 0，没有高光反射
        conveyingStructure.SetFloat("_Metallic", 0f); // 设置金属度（Metallic）为 0
        conveyingStructure.color = new Color(
            0 / 255f,
            0 / 255f,
            0 / 255f,
            255 / 255f);

        autonomousEntity = new Material(Shader.Find("Standard"));// 创建一个新的材质实例，这里使用 Unity 内置的标准着色器
        autonomousEntity.SetFloat("_Glossiness", 0f); // 设置光滑度（Glossiness）为 0，没有高光反射
        autonomousEntity.SetFloat("_Metallic", 0f); // 设置金属度（Metallic）为 0
        autonomousEntity.color = new Color(
            150 / 255f,
            150 / 255f,
            150 / 255f,
            100 / 255f);

        conceptualObject = new Material(Shader.Find("Standard"));// 创建一个新的材质实例，这里使用 Unity 内置的标准着色器
        conceptualObject.SetFloat("_Glossiness", 0f); // 设置光滑度（Glossiness）为 0，没有高光反射
        conceptualObject.SetFloat("_Metallic", 0f); // 设置金属度（Metallic）为 0
        conceptualObject.color = new Color(
            200 / 255f,
            200 / 255f,
            200 / 255f,
            100 / 255f);


        load = new Material(Shader.Find("Standard"));// 创建一个新的材质实例，这里使用 Unity 内置的标准着色器
        load.SetFloat("_Glossiness", 0f); // 设置光滑度（Glossiness）为 0，没有高光反射
        load.SetFloat("_Metallic", 0f); // 设置金属度（Metallic）为 0
        load.color = new Color(
            0 / 255f,
            0 / 255f,
            0 / 255f,
            255 / 255f);


        explosion = new Material(Shader.Find("Standard"));// 创建一个新的材质实例，这里使用 Unity 内置的标准着色器
        explosion.SetFloat("_Glossiness", 0f); // 设置光滑度（Glossiness）为 0，没有高光反射
        explosion.SetFloat("_Metallic", 0f); // 设置金属度（Metallic）为 0
        explosion.color = new Color(
            255 / 255f,
            0 / 255f,
            0 / 255f,
            0 / 255f);

        completelyTransparent = new Material(Shader.Find("Standard"));//透明材质
        completelyTransparent.SetFloat("_Mode", 3); // 3 表示透明模式
        completelyTransparent.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        completelyTransparent.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        completelyTransparent.SetInt("_ZWrite", 0);
        completelyTransparent.DisableKeyword("_ALPHATEST_ON");
        completelyTransparent.EnableKeyword("_ALPHABLEND_ON");
        completelyTransparent.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        completelyTransparent.renderQueue = 3000;

        completelyTransparent.color = new Color(0, 0, 0, 0);//完全透明

        fontAsset = Resources.Load<TMP_FontAsset>("Art/Font/STHeiti Light SDF");
        MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }
}
