using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 网络管理器，负责与本地HTTP服务器通信，获取公钥和世界状态。
/// </summary>
public class NetworkManager : MonoBehaviour
{
    public float tickUpdateInterval = 1f / 20f;

    private const string ServerUrl = "http://localhost:1416";

}