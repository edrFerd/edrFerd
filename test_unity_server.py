import requests
import json
import time

def test_unity_server():
    """
    测试Unity HTTP服务器是否正常工作
    """
    # 尝试多个可能的端口
    unity_url = None
    for port in range(8766, 8771):
        try:
            test_url = f"http://localhost:{port}/"
            response = requests.get(test_url, timeout=2)
            if response.status_code == 404:  # 404表示服务器运行但端点不存在，这是正常的
                unity_url = f"http://localhost:{port}"
                print(f"检测到Unity服务器运行在端口 {port}")
                break
        except requests.exceptions.RequestException:
            continue
    
    if unity_url is None:
        print("未检测到Unity HTTP服务器运行")
        return
    
    # 测试数据
    test_data = {
        "Blocks": [
            {
                "Position": [10, 5, 15],
                "Texture": "dGVzdHRleHR1cmVkYXRhZm9ydGVzdGluZ3B1cnBvc2Vz",
                "PublicKey": "746573747075626c69636b657964617461666f7274657374696e67",
                "Difficulty": 42
            }
        ]
    }
    
    print("测试Unity HTTP服务器...")
    
    # 测试完整世界状态端点
    try:
        print("1. 测试 /full_world_state 端点...")
        response = requests.post(
            f"{unity_url}/full_world_state",
            data=json.dumps(test_data),
            headers={'Content-Type': 'application/json'},
            timeout=5
        )
        print(f"   状态码: {response.status_code}")
        print(f"   响应: {response.text}")
    except requests.exceptions.RequestException as e:
        print(f"   错误: {e}")
    
    time.sleep(1)
    
    # 测试世界更新端点
    try:
        print("2. 测试 /world_update 端点...")
        response = requests.post(
            f"{unity_url}/world_update",
            data=json.dumps(test_data),
            headers={'Content-Type': 'application/json'},
            timeout=5
        )
        print(f"   状态码: {response.status_code}")
        print(f"   响应: {response.text}")
    except requests.exceptions.RequestException as e:
        print(f"   错误: {e}")
    
    time.sleep(1)
    
    # 测试未知端点
    try:
        print("3. 测试未知端点...")
        response = requests.get(f"{unity_url}/unknown", timeout=5)
        print(f"   状态码: {response.status_code}")
        print(f"   响应: {response.text}")
    except requests.exceptions.RequestException as e:
        print(f"   错误: {e}")

if __name__ == "__main__":
    test_unity_server() 