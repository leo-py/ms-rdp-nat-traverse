#!/user/bin/python3
# 文件：Protocol.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：完成协议规定格式的请求, 并对响应消息进行解析
# 参数：无
# 版本：0.1
# 备注：无
# 日志：无

import socket
import json
from Utils.Utils import LOGGER, config_logging


def buildRequestStr(method, key, clientId, value):
    """
    用于构建标准兼容格式的请求消息字符串
    :param clientId:
    :param method:
    :param key: 请求的关键字
    :param id: 客户端id
    :param value: 实际请求的参数等内容
    :return: 字符串
    """
    if not isinstance(method, str):
        raise TypeError('{} is not str type.'.format(key))

    if not isinstance(key, str):
        raise TypeError('{} is not str type.'.format(key))

    if not isinstance(clientId, int):
        raise TypeError('{} is not int type.'.format(clientId))

    request = dict()
    content = dict()

    content['key'] = key
    content['id'] = clientId
    content['value'] = value

    request[method] = content

    return json.dumps(request, ensure_ascii=False)


def sendProtocolRequest(host, port, request):
    """
    按照协议要求发送消息 并等待响应结果
    :param host: 主机地址
    :param port: 主机端口
    :param request: 请求消息
    :return: 响应内容
    """
    client = socket.socket()

    try:
        client.connect((host, port))
        client.send(request.encode('utf-8'))
        data = client.recv(1024)
        data = data.decode('utf-8')
        client.close()
        resp = json.loads(data)
        return resp
    except Exception as e:
        client.close()
        return None


def getClientId(host, port):
    """
    获取一个合法分配的客户端ID
    :param host: 主机地址
    :param port: 主机端口
    :return: 合法的ID或者-1无效ID值
    """
    # 构建要发送的消息
    request = buildRequestStr('get', 'id', -1, None)

    # 发送消息并等待响应
    response = sendProtocolRequest(host, port, request)

    if isinstance(response, dict):
        clientId = response['response']
        LOGGER.debug('getClientId: {}'.format(clientId))
        return clientId
    else:
        return -1


def postClientInformation(host, port, clientId, clientInfo):
    """
    上传本机相关信息
    :param clientInfo: 客户端详细信息 一个字典对象即可
    :param clientId: 客户端id
    :param host: 主机地址
    :param port: 主机端口
    :return: True/False
    """
    # 构建要发送的消息
    request = buildRequestStr('post', 'client_info', clientId, clientInfo)

    # 发送消息并等待响应
    response = sendProtocolRequest(host, port, request)

    if isinstance(response, dict):
        result = response['response']
        LOGGER.debug('postClientInformation: {}'.format(result))
        return result
    else:
        return False


def getControlRequest(host, port, clientId):
    """
    获取指定ID的控制请求状态
    :param clientId: 请求的id
    :param host: 主机地址
    :param port: 主机端口
    :return: True/False
    """
    # 构建要发送的消息
    request = buildRequestStr('get', 'control_request', clientId, None)

    # 发送消息并等待响应
    response = sendProtocolRequest(host, port, request)

    if isinstance(response, dict):
        result = response['response']
        LOGGER.debug('getControlRequest: {}'.format(result))
        return result
    else:
        return False


def postControlRequest(host, port, clientId, value):
    """
    获取指定ID的控制请求状态
    :param value: 要设置的值, True/False
    :param clientId: 请求的id
    :param host: 主机地址
    :param port: 主机端口
    :return: True/False
    """
    # 构建要发送的消息
    request = buildRequestStr('post', 'control_request', clientId, value)

    # 发送消息并等待响应
    response = sendProtocolRequest(host, port, request)

    if isinstance(response, dict):
        result = response['response']
        LOGGER.debug('postControlRequest: {}'.format(result))
        return result
    else:
        return False


def getTunnelPort(host, port, clientId):
    """
    :param clientId: 请求的id
    :param host: 主机地址
    :param port: 主机端口
    :return: True/False
    """
    # 构建要发送的消息
    request = buildRequestStr('get', 'tunnel_port', clientId, False)

    # 发送消息并等待响应
    response = sendProtocolRequest(host, port, request)

    if isinstance(response, dict):
        result = response['response']
        LOGGER.debug('getTunnelPort: {}'.format(result))
        return result
    else:
        return -1


def getIsUnderControl(host, port, clientId):
    """
    获取指定id的客户端是否正在被控制中
    :param host: 主机地址
    :param port: 主机端口
    :param clientId: 指定的id
    :return: True/False
    """
    request = buildRequestStr('get', 'is_under_control', clientId, None)

    response = sendProtocolRequest(host, port, request)

    if isinstance(response, dict):
        result = response['response']
        LOGGER.debug('getIsUnderControl: {}'.format(result))
        return result
    else:
        return False


def postIsUnderControl(host, port, clientId, value):
    """
    设置指定id的客户端是否正在被控制中
    :param host: 主机地址
    :param port: 主机端口
    :param clientId: 指定的id
    :return: True/False
    """
    request = buildRequestStr('post', 'is_under_control', clientId, value)

    response = sendProtocolRequest(host, port, request)

    if isinstance(response, dict):
        result = response['response']
        LOGGER.debug('postIsUnderControl: {}'.format(result))
        return result
    else:
        return False


def getPeeredRemoteId(host, port, clientId):
    """
    获取与此客户端远程连接的客户端id
    :param host: 主机地址
    :param port: 主机端口
    :param clientId: 指定的id
    :return: 匹配的端口号或者无效端口-1
    """
    request = buildRequestStr('get', 'peered_remote_id', clientId, None)

    response = sendProtocolRequest(host, port, request)

    if isinstance(response, dict):
        result = response['response']
        LOGGER.debug('getPeeredRemoteId: {}'.format(result))
        return result
    else:
        return -1


def postPeeredRemoteId(host, port, clientId, value):
    """
    设置指定id客户端的配对远程id
    :param host: 主机地址
    :param port: 主机端口
    :param clientId: 客户端id
    :param value: 配对id
    :return: True/False
    """
    content = buildRequestStr('post', 'peered_remote_id', clientId, value)
    response = sendProtocolRequest(host, port, content)

    if isinstance(response, dict):
        result = response['response']
        LOGGER.debug('postPeeredRemoteId: {}'.format(result))
        return result
    else:
        return False


def postKeepAliveCount(host, port, clientId, value):
    """
    设置存活状态值
    :param host: 主机地址
    :param port: 主机端口
    :param clientId: 客户端id
    :param value: 要设置的值 一般为10 每隔10s更新一次即可保证基本不掉线了
    :return: True/False
    """
    content = buildRequestStr('post', 'keep_alive_count', clientId, value)
    response = sendProtocolRequest(host, port, content)

    if isinstance(response, dict):
        result = response['response']
        LOGGER.debug('postKeepAliveCount: {}'.format(result))
        return result
    else:
        return False


def getIsOnline(host, port, clientId):
    """
    获得指定id的客户端是否在线
    :param host: 主机地址
    :param port: 主机端口
    :param clientId: 客户端id
    :return: True/False
    """
    content = buildRequestStr('get', 'is_online', clientId, None)
    response = sendProtocolRequest(host, port, content)

    if isinstance(response, dict):
        result = response['response']
        LOGGER.debug('getIsOnline: {}'.format(result))
        return result
    else:
        return False


def getOnlineClientList(host, port, clientId):
    """
    获取在线的客户端列表
    :param host: 主机地址
    :param port: 主机端口
    :param clientId: 客户端id
    :return: 在线客户端列表
    """
    content = buildRequestStr('get', 'online_list', clientId, None)
    response = sendProtocolRequest(host, port, content)

    if isinstance(response, dict):
        result = response['response']
        LOGGER.debug('getOnlineClientList: {}'.format(result))
        return result
    else:
        return None


def testProtocolMethods():
    """
    测试上面的函数
    :return:
    """
    host = 'wealllink.com'
    port = 9001
    # 获取id测试
    clientId = getClientId(host, port)

    # 上传本机信息
    info = {'name': 'test_mac_os_x', 'description': 'This is a test client.' }
    postClientInformation(host, port, clientId, info)

    # 防止在调试过程中掉线了, 暂时测试下
    postKeepAliveCount(host, port, clientId, 1000)

    # 告诉代理服务器本地客户端不允许被控制
    postPeeredRemoteId(host, port, clientId, clientId)

    # 查询是否有控制请求
    # 其实在本客户端上无需这个查询功能, 不接受控制
    # 只是测试用
    getControlRequest(host, port, clientId)

    # 更改控制请求
    # 这个可以去更改别的id的控制标志
    postControlRequest(host, port, clientId, True)
    getControlRequest(host, port, clientId)

    # 查询指定端口的隧道
    getTunnelPort(host, port, clientId)

    # 查询本身是否真正被控制
    # 仅仅是测试用, 因为该客户端将不会检查自身的该标志
    getIsUnderControl(host, port, clientId)

    # 尝试修改控制标志
    postIsUnderControl(host, port, clientId, True)
    getIsUnderControl(host, port, clientId)

    # 获取配对的id
    getPeeredRemoteId(host, port, clientId)

    # 尝试修改配对id
    postPeeredRemoteId(host, port, clientId, 12212)
    getPeeredRemoteId(host, port, clientId)

    # 存活状态值修改
    postKeepAliveCount(host, port, clientId, 100)

    # 查询是否在线
    getIsOnline(host, port, clientId)

    # 查询所有在线用户列表
    getOnlineClientList(host, port, clientId)


def main():
    config_logging()
    testProtocolMethods()

if __name__ == '__main__':
    main()
