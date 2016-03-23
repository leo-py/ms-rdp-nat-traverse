#!/user/bin/python3
# 文件：ProxyServer.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：监听服务器
# 参数：无
# 版本：0.1
# 备注：无
# 日志：无

import os
import sys
import subprocess
from json import loads, dumps
import socket
from threading import Thread
import logging

"""
全局的日志实例
"""
LOG = logging.getLogger()


"""
与代理服务器结合的函数
"""


def generate_machine_id():
    """
    该函数用于产生一个ID, 并且该ID可以唯一标识与该代理服务器连接的PC机
    :return: 随机产生的但是合法的ID
    """
    return 10000


def get_remote_control_machine_address():
    """
    该函数用于获取已经要求建立远程连接的机器所建立成功后的隧道IP地址和端口, 以供控制端连接.
    :return: 远程控制的机器的地址
    """
    return '{}:{}'.format('192.168.0.1', 10000)


def get_available_port():
    """
    该函数用于分配一个合法的隧道端口给请求建立反向隧道的PC
    :return: 合法的隧道端口
    """
    return '10000'


def get_tunnel_status():
    """
    该函数用于查询隧道建立的状态, 通过查询机器的配置文件即可得到隧道的建立状态是否ok
    :return: connected/disconnected
    """
    return 'connected'


def get_remote_control_request():
    """
    该函数用来查询本机有没有收到来自其他机器的远程控制请求
    :return: True/False
    """
    return True

"""
下面几个handle函数用来处理不同请求功能. 这些方法将会返回json字符串作为回复内容
"""


def build_response_str(content):
    """
    构建应答消息:json格式的字符串
    :param content: object用于包装在json格式的回复消息中
    :return: json格式的字符串
    """
    response = dict()
    response['response'] = content
    return dumps(response)


def handle_get_function(request, with_id=True):
    """
    专门用于处理get功能的请求
    :param request: 详细的请求消息
    :param with_id: 是否带有id的请求
    :return: json格式的回复消息
    """
    content = request['content'].strip()
    if with_id:
        id = int(request['id'])
        if content == "remote_machine_address":
            return build_response_str(get_remote_control_machine_address())
        elif content == "available_port":
            return build_response_str(get_available_port())
        else:
            pass
    else:
        if content == "machine_id":
            return build_response_str(generate_machine_id())
        else:
            pass
    return None


def handle_connect_remote_function(request):
    """
    专门用于处理connect_remote功能
    :param request: 请求的详细消息
    :return: json格式的回复消息
    """
    return build_response_str('success')


def handle_query_function(request):
    """
    专门用于处理query功能
    :param request: 请求的详细消息
    :return: json格式的回复消息
    """
    content = request['content'].strip()
    id = int(request['id'])
    if content == "tunnel_status":
        return build_response_str(get_tunnel_status())
    elif content == "remote_control_request":
        return build_response_str(get_remote_control_request())
    elif content == "keep-alive":
        return build_response_str('ok')
    else:
        return None


def handle_upload_function(request):
    """
    专门处理upload功能
    :param request: 请求的详细消息
    :return: json格式的回复消息
    """
    print(request['content'])
    return build_response_str('ok')


def analyze_message(conn):
    """
    处理接收到的客户端消息, 该函数作为后台线程运行时调用, 处理完消息后会自动关闭连接
    :param conn: 与客户端的连接
    :return: None
    """
    assert isinstance(conn, socket.socket)

    try:
        recv_msg = conn.recv(1024)
        LOG.info('接收到来自客户端的消息:\n{}'.format(recv_msg.decode('utf-8')))

        # 对消息内容进行分析
        request = loads(recv_msg.decode('utf-8'))
        if type(request) is not dict:
            conn.close()
            return

        # 将会返回的消息内容
        response = ""
        # 再看有几个键值
        if len(request.keys()) == 2:
            # 说明这是不带有machine_id的请求消息
            LOG.info('不带有ID的请求消息')
            if request['function'] == 'get':
                response = handle_get_function(request, False)
            elif request['function'] == 'upload':
                response = handle_upload_function(request)
        elif len(request.keys()) == 3:
            # 说明这是带有machine_id的请求消息
            LOG.info('带有ID的请求消息')
            if request['function'] == 'connect_remote':
                response = handle_connect_remote_function(request)
            elif request['function'] == 'get':
                response = handle_get_function(request)
            elif request['function'] == 'query':
                response = handle_query_function(request)
        else:
            # 说明这个是非法的请求
            LOG.info('收到非法的请求消息: {}'.format(request))
        if response is not None and response != "":
            conn.send(response.encode('utf-8'))
        conn.close()
    except Exception as e:
        conn.close()
        LOG.info(str(e))


class ListenThread(Thread):
    """
    后台监听线程. 当代理服务器配置后,会启动线程,监听来自客户端的消息请求.
    在监听线程中并不会对客户端请求直接处理,而是转交给单独的处理线程处理.
    """
    def __init__(self, listen_ip='', listen_port=8088, name=None):
        """
        对象初始化函数
        :param listen_ip: 指定要监听的IP
        :param listen_port: 指定监听的端口
        :param name: 指定线程的名称
        """
        super().__init__(name=name)
        self._listen_ip = listen_ip
        self._listen_port = listen_port
        self.__listen_server = socket.socket()
        self.__listen_server.bind((listen_ip, listen_port))
        self.__can_run = False

    def __run(self):
        """
        监听来自客户端发起的连接,和接收消息并启动新的线程处理消息.
        """
        while self.__can_run:
            conn, add = self.__listen_server.accept()
            analyze_msg_thread = Thread(target=analyze_message, args=(conn,))
            analyze_msg_thread.start()

    def start(self):
        """
        启动监听线程
        """
        LOG.info('启动监听线程, 监听地址: {0}:{1}'.format(self._listen_ip, self._listen_port))
        self.__listen_server.listen()
        self.__can_run = True
        self.__run()

    def stop(self):
        """
        停止监听线程
        """
        LOG.info('停止监听线程')
        self.__listen_server.close()
        self.__can_run = False


class ProxyServer(object):
    """
    代理服务器,负责监听指定的端口,并接收客户端发送的消息,完成消息的解析和响应等任务.
    """
    def __init__(self, listen_ip='', listen_port=8088):
        assert isinstance(listen_ip, str)
        assert isinstance(listen_port, int)
        self._listen_ip = listen_ip
        self._listen_port = listen_port
        self._listen_thread = None

    def start(self):
        """
        启动代理服务器. 只允许启动一个监听线程在单独的端口上进行监听和消息处理, 方便管理
        """
        LOG.info('启动代理服务器')
        if self._listen_thread is None:
            self._listen_thread = ListenThread(self._listen_ip,
                                               self._listen_port,
                                               'listen_thread')
            self._listen_thread.start()

    def stop(self):
        """
        停止代理服务器的服务.
        """
        LOG.info('停止代理服务器')
        if self._listen_thread:
            self._listen_thread.stop()


def config_logging():
    """
    配置日志模块,可以在同时将日志信息输出到终端和文件中
    """
    logging.basicConfig(
        level=logging.DEBUG,
        format="%(asctime)s - %(module)-6s - line %(lineno)-4d - %(levelname)s - %(message)s",
        datefmt='%m-%d %H:%M',
        filename='debug.log',
        filemode='a')

    console = logging.StreamHandler()
    console.setLevel(logging.INFO)
    formatter = logging.Formatter('%(asctime)s - %(module)-6s - line %(lineno)-4d - %(levelname)s - %(message)s')
    console.setFormatter(formatter)
    logging.getLogger('').addHandler(console)


def main():
    config_logging()
    server = ProxyServer('223.3.22.72', 9001)
    try:
        server.start()
    except Exception as e:
        LOG.error(str(e))
    finally:
        server.stop()


if __name__ == '__main__':
    main()
