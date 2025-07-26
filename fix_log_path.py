import asyncio
import websockets
import json
import base64
import os
import random
import time
import sys
from http.server import HTTPServer, BaseHTTPRequestHandler
import threading

#############################################
# 方块世界服务器 - HTTP后端
# 
# 该脚本创建一个HTTP服务器，为Unity前端提供两个端点：
# 1. 完整世界状态 (/known_world_state)
# 2. 最近一个Tick的方块更新列表 (/tick_block_update_list)
#
# 数据以JSON格式传输，包含方块位置、贴图、公钥和难度信息
#############################################

# 配置参数
MAX_COORD = 100        # 方块坐标的最大值（0-99）
BLOCKS_PER_STATE = 10  # 完整世界状态包含的方块数量
BLOCKS_PER_TICK = 100  # 每个Tick更新的方块数量，改为100个方块
TEXTURE_BYTES = 32     # 贴图数据大小: 256位 = 32字节
PUBKEY_BYTES = 32      # 公钥数据大小: 256位 = 32字节

# API端点定义
KNOWN_WORLD_STATE_ENDPOINT = '/known_world_state'           # 获取完整世界状态的端点
TICK_BLOCK_UPDATE_LIST_ENDPOINT = '/tick_block_update_list' # 获取Tick更新的端点
HTTP_HOST = 'localhost'  # 服务器主机名
HTTP_PORT = 8765         # 服务器端口

# 全局变量
server_running = True     # 服务器运行状态标志
last_world_state = None   # 缓存的世界状态
tick_count = 0            # Tick计数器，记录服务器运行了多少个更新周期

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

class BlockDataHandler(BaseHTTPRequestHandler):
    """
    HTTP请求处理器，处理方块数据的请求
    
    实现两个主要端点:
    1. /known_world_state - 返回完整的世界状态
    2. /tick_block_update_list - 返回最新的Tick更新
    """
    def do_GET(self):
        """处理GET请求"""
        global last_world_state, tick_count
        
        print(f"[SERVER] Received request: {self.path}")
        
        # 设置CORS头信息，允许跨域请求
        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.end_headers()
        
        # 处理获取完整世界状态的请求
        if self.path == KNOWN_WORLD_STATE_ENDPOINT:
            # 如果尚未生成世界状态，则创建一个新的
            if last_world_state is None:
                last_world_state = make_state_json(BLOCKS_PER_STATE)
                print(f"[SERVER] Generated new world state with {BLOCKS_PER_STATE} blocks")
            
            # 返回完整的世界状态
            self.wfile.write(last_world_state.encode('utf-8'))
            print(f"[SERVER] Sent full world state: {last_world_state[:100]}...")
        
        # 处理获取Tick更新的请求    
        elif self.path == TICK_BLOCK_UPDATE_LIST_ENDPOINT:
            tick_count += 1
            # 为本次Tick生成新的方块更新
            tick_update = make_state_json(BLOCKS_PER_TICK)
            self.wfile.write(tick_update.encode('utf-8'))
            print(f"[SERVER] Sent tick update #{tick_count}: {tick_update[:100]}...")
        
        # 处理未知端点    
        else:
            response = json.dumps({
                "error": "Unknown endpoint",
                "path": self.path
            }, ensure_ascii=False)
            self.wfile.write(response.encode('utf-8'))
            print(f"[SERVER] Unknown endpoint: {self.path}")

    def log_message(self, format, *args):
        """
        覆盖默认的日志方法，避免控制台输出过多信息
        """
        # 不输出任何信息
        return

def run_http_server():
    """
    运行HTTP服务器的主函数
    
    创建并启动HTTP服务器，处理到来的请求直到服务器停止
    """
    try:
        # 创建并配置HTTP服务器
        server_address = (HTTP_HOST, HTTP_PORT)
        httpd = HTTPServer(server_address, BlockDataHandler)
        
        # 输出服务器启动信息
        print(f"[SERVER] HTTP server started at http://{HTTP_HOST}:{HTTP_PORT}/")
        print(f"[SERVER] Available endpoints:")
        print(f"[SERVER] - {KNOWN_WORLD_STATE_ENDPOINT} - Get full world state")
        print(f"[SERVER] - {TICK_BLOCK_UPDATE_LIST_ENDPOINT} - Get latest tick updates")
        
        # 开始服务器主循环
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("[SERVER] HTTP server stopped by user")
    except Exception as e:
        print(f"[SERVER] HTTP server error: {e}")
    finally:
        # 确保在服务器结束时更新全局状态
        global server_running
        server_running = False

def main():
    """
    程序主函数
    
    创建并启动HTTP服务器线程，然后保持主线程运行以便用户可以通过Ctrl+C终止程序
    """
    # 在单独的线程中启动HTTP服务器，以便主线程可以处理用户输入
    server_thread = threading.Thread(target=run_http_server)
    server_thread.daemon = True  # 设置为守护线程，这样主线程退出时，服务器线程也会退出
    server_thread.start()
    
    print("[MAIN] Server running. Press Ctrl+C to exit.")
    
    try:
        # 保持主线程运行，每秒检查一次服务器状态
        while server_running:
            time.sleep(1)
    except KeyboardInterrupt:
        # 处理用户通过Ctrl+C终止程序的情况
        print("\n[MAIN] Program terminated by user")
        sys.exit(0)
    except Exception as e:
        # 处理其他未捕获的异常
        print(f"[MAIN] Unhandled exception: {e}")
        sys.exit(1)

# 程序入口点
if __name__ == "__main__":
    main() 