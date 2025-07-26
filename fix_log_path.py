import asyncio
import websockets
import json
import base64
import os
import random
import time
import sys
import requests
import threading

#############################################
# 方块世界服务器 - 主动发送模式
# 
# 该脚本创建一个主动发送数据的服务器，定期向Unity客户端发送：
# 1. 完整世界状态 (/full_world_state)
# 2. 世界更新信息 (/world_update)
#
# 数据以JSON格式通过HTTP POST发送到Unity的HTTP服务器
#############################################

# 配置参数
MAX_COORD = 100        # 方块坐标的最大值（0-99）
BLOCKS_PER_STATE = 10  # 完整世界状态包含的方块数量
BLOCKS_PER_TICK = 100  # 每个Tick更新的方块数量，改为100个方块
TEXTURE_BYTES = 32     # 贴图数据大小: 256位 = 32字节
PUBKEY_BYTES = 32      # 公钥数据大小: 256位 = 32字节

# Unity客户端配置
UNITY_SERVER_URL = "http://localhost:8766"  # Unity HTTP服务器地址
FULL_WORLD_STATE_ENDPOINT = "/full_world_state"  # 完整世界状态端点
WORLD_UPDATE_ENDPOINT = "/world_update"     # 世界更新端点

# 全局变量
server_running = True     # 服务器运行状态标志
last_world_state = None   # 缓存的世界状态
tick_count = 0            # Tick计数器，记录服务器运行了多少个更新周期
unity_connected = False   # Unity客户端连接状态

def random_block():
    """
    生成随机方块信息
    
    生成的数据包括:
    - 随机坐标 (范围内的三维坐标)
    - 随机贴图 (256位BASE64编码字符串)
    - 随机公钥 (256位十六进制字符串)
    - 随机难度 (0-99整数)
    
    返回:
        dict: 包含方块信息的字典
    """
    # 生成范围内的随机三维坐标 [x,y,z]
    coord = [random.randint(0, MAX_COORD-1) for _ in range(3)]
    
    # 生成随机贴图数据并转换为BASE64编码
    texture_bytes = os.urandom(TEXTURE_BYTES)  # 使用操作系统提供的随机字节
    texture_b64 = base64.b64encode(texture_bytes).decode('utf-8')
    
    # 生成随机公钥数据并转换为十六进制字符串
    pubkey_bytes = os.urandom(PUBKEY_BYTES)
    pubkey_hex = pubkey_bytes.hex()
    
    # 生成随机难度值 (0-99)
    difficulty = random.randint(0, 99)
    
    # 返回符合API格式的方块数据
    return {
        "Position": coord,
        "Texture": texture_b64,
        "PublicKey": pubkey_hex,
        "Difficulty": difficulty
    }

def make_state_json(num_blocks):
    """
    创建包含指定数量方块的JSON字符串
    
    参数:
        num_blocks (int): 要生成的方块数量
        
    返回:
        str: 序列化后的JSON字符串，格式为 {"Blocks": [...方块列表...]}
    """
    return json.dumps({
        "Blocks": [random_block() for _ in range(num_blocks)]
    }, ensure_ascii=False)

def check_unity_connection():
    """
    检查Unity客户端是否连接
    
    返回:
        bool: Unity客户端是否可连接
    """
    # 尝试多个可能的端口
    for port in range(8766, 8771):
        try:
            url = f"http://localhost:{port}/"
            response = requests.get(url, timeout=2)
            if response.status_code == 404:  # 404表示服务器运行但端点不存在，这是正常的
                global UNITY_SERVER_URL
                UNITY_SERVER_URL = f"http://localhost:{port}"
                print(f"[SERVER] 检测到Unity服务器运行在端口 {port}")
                return True
        except requests.exceptions.RequestException:
            continue
    return False

def send_to_unity(endpoint, data):
    """
    向Unity客户端发送数据
    
    参数:
        endpoint (str): 端点路径
        data (str): JSON数据
        
    返回:
        bool: 发送是否成功
    """
    try:
        url = f"{UNITY_SERVER_URL}{endpoint}"
        headers = {'Content-Type': 'application/json'}
        response = requests.post(url, data=data, headers=headers, timeout=5)
        
        if response.status_code == 200:
            print(f"[SERVER] 成功发送数据到 {endpoint}")
            return True
        else:
            print(f"[SERVER] 发送数据到 {endpoint} 失败，状态码: {response.status_code}")
            return False
            
    except requests.exceptions.RequestException as e:
        print(f"[SERVER] 发送数据到 {endpoint} 时出错: {e}")
        return False

def send_full_world_state():
    """
    发送完整世界状态到Unity客户端
    """
    global last_world_state
    
    if not unity_connected:
        print("[SERVER] Unity客户端未连接，跳过发送完整世界状态")
        return
    
    # 生成新的完整世界状态
    last_world_state = make_state_json(BLOCKS_PER_STATE)
    
    # 发送到Unity
    if send_to_unity(FULL_WORLD_STATE_ENDPOINT, last_world_state):
        print(f"[SERVER] 已发送完整世界状态，包含 {BLOCKS_PER_STATE} 个方块")
    else:
        print("[SERVER] 发送完整世界状态失败")

def send_world_update():
    """
    发送世界更新到Unity客户端
    """
    global tick_count
    
    if not unity_connected:
        print("[SERVER] Unity客户端未连接，跳过发送世界更新")
        return
    
    tick_count += 1
    
    # 生成世界更新数据
    world_update = make_state_json(BLOCKS_PER_TICK)
    
    # 发送到Unity
    if send_to_unity(WORLD_UPDATE_ENDPOINT, world_update):
        print(f"[SERVER] 已发送世界更新 #{tick_count}，包含 {BLOCKS_PER_TICK} 个方块")
    else:
        print(f"[SERVER] 发送世界更新 #{tick_count} 失败")

def connection_monitor():
    """
    监控Unity客户端连接状态的后台线程
    """
    global unity_connected
    
    while server_running:
        try:
            current_connection = check_unity_connection()
            
            if current_connection != unity_connected:
                if current_connection:
                    print("[SERVER] 检测到Unity客户端连接")
                    unity_connected = True
                    # 立即发送完整世界状态
                    send_full_world_state()
                else:
                    print("[SERVER] Unity客户端断开连接")
                    unity_connected = False
            
            time.sleep(2)  # 每2秒检查一次连接状态
            
        except Exception as e:
            print(f"[SERVER] 连接监控出错: {e}")
            time.sleep(5)

def update_scheduler():
    """
    定期发送更新的调度器线程
    """
    while server_running:
        try:
            if unity_connected:
                send_world_update()
            time.sleep(10)  # 每10秒发送一次更新
            
        except Exception as e:
            print(f"[SERVER] 更新调度器出错: {e}")
            time.sleep(5)

def main():
    """
    程序主函数
    
    启动连接监控和更新调度器，然后保持主线程运行
    """
    global unity_connected, server_running
    
    print("[MAIN] 方块世界服务器启动")
    print(f"[MAIN] Unity服务器地址: {UNITY_SERVER_URL}")
    print(f"[MAIN] 完整世界状态端点: {FULL_WORLD_STATE_ENDPOINT}")
    print(f"[MAIN] 世界更新端点: {WORLD_UPDATE_ENDPOINT}")
    print("[MAIN] 更新间隔: 10秒")
    
    # 启动连接监控线程
    connection_thread = threading.Thread(target=connection_monitor)
    connection_thread.daemon = True
    connection_thread.start()
    
    # 启动更新调度器线程
    scheduler_thread = threading.Thread(target=update_scheduler)
    scheduler_thread.daemon = True
    scheduler_thread.start()
    
    print("[MAIN] 服务器运行中。按 Ctrl+C 退出。")
    
    try:
        # 保持主线程运行，每秒检查一次服务器状态
        while server_running:
            time.sleep(1)
    except KeyboardInterrupt:
        # 处理用户通过Ctrl+C终止程序的情况
        print("\n[MAIN] 程序被用户终止")
        server_running = False
        sys.exit(0)
    except Exception as e:
        # 处理其他未捕获的异常
        print(f"[MAIN] 未处理的异常: {e}")
        server_running = False
        sys.exit(1)

# 程序入口点
if __name__ == "__main__":
    main() 