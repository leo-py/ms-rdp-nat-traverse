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
import random
from RegisteredMachine import RegisteredMachine
from json import loads, dumps, load, dump
import socket
from threading import Thread
import logging

"""
全局变量
"""
LOG = logging.getLogger()

# 存放注册的机器信息,方便用于查找等操作
# 键取值为machine_id, 从而确保唯一性;
# 值为RegisteredMachine类型
REGISTERED_MACHINES = dict()

"""
相关函数:处理注册机器信息等的函数
"""


def append_machine(machine):
    """
    该函数用于添加一个新的注册机器
    :param machine: 该机器的详细信息参数
    :return: bool类型
    """
    global REGISTERED_MACHINES
    if isinstance(machine, RegisteredMachine):
        if machine.machine_id in REGISTERED_MACHINES.keys():
            LOG.info('无法添加相同ID的机器信息: {}'.format(str(machine)))
            return False
        else:
            LOG.info('添加主机信息: {}'.format(machine.machine_id))
            REGISTERED_MACHINES[machine.machine_id] = machine
            return True
    else:
        return False


def remove_machine(machine_id):
    """
    从已注册的表中删除一个已经注册的机器信息
    :param machine_id: 机器的ID
    :return: True/False
    """
    global REGISTERED_MACHINES
    if isinstance(machine_id, int):
        if machine_id not in REGISTERED_MACHINES.keys():
            LOG.info("尝试删除一个不存在的机器信息: {}".format(machine_id))
            return False
        else:
            REGISTERED_MACHINES.pop(machine_id)
            return True
    else:
        return False


def save_machines():
    """
    保存所有的已成功注册的机器信息到磁盘上
    :return: True/False
    """
    with open('machines.json', 'w') as w_file:
        try:
            dump(REGISTERED_MACHINES, w_file)
            return True
        except Exception as err:
            LOG.error(str(err))
            return False


def read_machines():
    """
    从磁盘的配置文件中读取已经存放的machines信息
    :return: True/False
    """
    global REGISTERED_MACHINES
    if os.path.exists('machines.json'):
        try:
            REGISTERED_MACHINES = load('machines.json')
            return True
        except Exception as err:
            LOG.error(str(err))
            return False


def generate_machine_id():
    """
    该函数用于产生一个ID, 并且该ID可以唯一标识与该代理服务器连接的PC机
    :return: 随机产生的但是合法的ID,ID范围取值为(1000, 2000)
    """
    LOG.info('generate_machine_id')
    rand = random.Random()

    while True:
        machine_id = rand.randrange(1000, 2001)
        if machine_id not in REGISTERED_MACHINES.keys():
            return machine_id


def get_remote_control_machine_address(remote_id):
    """
    该函数用于获取已经要求建立远程连接的机器所建立成功后的隧道IP地址和端口, 以供控制端连接.
    :param remote_id: 远程主机ID
    :return: 远程控制的机器的地址
    """
    LOG.info('get_remote_control_machine_address')
    hostname = socket.gethostname()
    if hostname != '':
        server_ip = socket.gethostbyname(hostname)
        if remote_id in REGISTERED_MACHINES.keys():
            return '{}:{}'.format(server_ip, (REGISTERED_MACHINES[remote_id]).tunnel_port)

    return '{}.{}'.format('', -1)


def get_all_listen_ports():
    """
    使用系统命令,查询当前所有的监听端口,使用netstat命令即可得到
    """
    # 获取所有的输出信息
    sub = subprocess.Popen(('netstat', '-nat'), stdout=subprocess.PIPE)
    sub.wait()

    # 过滤得到只有LISTEN关键字的行
    sub = subprocess.Popen(('grep', 'LISTEN'), stdin=sub.stdout, stdout=subprocess.PIPE)
    sub.wait()

    # 接下来会把所有端口提取出来
    listen_ports = set()
    for row in sub.stdout.read().decode('utf-8').split('\n'):
        column = [x for x in row.split(' ') if x != '']
        # 出错判断,否则会导致访问不存在的元素
        if len(column) == 6:
            listen_ports.add(int(column[3].split(':')[-1]))

    return listen_ports


def get_available_port(machine_id):
    """
    该函数用于分配一个合法的隧道端口给请求建立反向隧道的PC
    :param machine_id: 机器的ID
    :return: 合法的隧道端口
    """
    # 随机生成一个端口号用于返回
    LOG.info('get_available_port')
    while True:
        rand_port = random.Random().randrange(10000, 20000)
        if rand_port not in get_all_listen_ports():
            LOG.info('生成随机端口号用于建立反向隧道建立: {}'.format(rand_port))
            REGISTERED_MACHINES[machine_id].tunnel_port = rand_port
            return REGISTERED_MACHINES[machine_id].tunnel_port


def get_tunnel_status(machine_id):
    """
    获取隧道建立的状态
    :param machine_id: 机器的ID
    :return: True/False
    """
    LOG.info('调用函数:查询隧道建立状态')
    if machine_id in REGISTERED_MACHINES.keys():
        LOG.info('机器存在于列表中')
        machine = REGISTERED_MACHINES[machine_id]
        if machine.tunnel_port in get_all_listen_ports():
            LOG.info('{} in {}'.format(machine.tunnel_port, get_all_listen_ports()))
            machine.tunnel_status = True
            REGISTERED_MACHINES[machine_id] = machine
            return True
        else:
            LOG.info('{} not in {}'.format(machine.tunnel_port, get_all_listen_ports()))
            machine.tunnel_status = False
            REGISTERED_MACHINES[machine_id] = machine
            return False
    else:
        LOG.info('不存在的ID')
        return False


def get_remote_control_request(machine_id):
    """
    该函数用来查询本机有没有收到来自其他机器的远程控制请求
    :param machine_id: 机器的ID
    :return: True/False
    """
    return REGISTERED_MACHINES[machine_id].remote_control_request


def get_machine_list(machine_id):
    """
    获取所有在线的机器信息,发回到客户端
    :param machine_id: 本机的ID
    :return: 一个字典类型的实例
    """
    LOG.info('get_machine_list')
    machine_list = dict()

    for machine in REGISTERED_MACHINES.values():
        if machine_id != machine.machine_id:
            machine_list[machine.machine_id] = machine.name

    return machine_list


def clear_remote_control_request(machine_id):
    """
    该函数用来清除远程连接请求,标记此时主机已经成功处理了该请求
    :param machine_id: 机器的ID
    :return: True/False
    """
    LOG.info('clear_remote_control_request')
    if machine_id in REGISTERED_MACHINES.keys():
        machine = REGISTERED_MACHINES[machine_id]
        machine.remote_control_request = False
        REGISTERED_MACHINES[machine_id] = machine
        LOG.info('{}'.format(machine))
        return True
    else:
        return False


def build_response_str(content):
    """
    构建应答消息:json格式的字符串
    :param content: object用于包装在json格式的回复消息中
    :return: json格式的字符串
    """
    response = dict()
    response['response'] = content
    # LOG.info('响应消息内容: {}'.format(dumps(response)))
    return dumps(response)


def handle_get_function(request, with_id=True):
    """
    专门用于处理get功能的请求
    :param request: 详细的请求消息
    :param with_id: 是否带有id的请求
    :return: json格式的回复消息
    """
    LOG.info('handle_get_function')
    content = request['content'].strip()
    if with_id:
        machine_id = int(request['id'])
        if content == "remote_machine_address":
            return get_remote_control_machine_address(machine_id)
        elif content == "available_port":
            return get_available_port(machine_id)
        elif content == "online_machine_list":
            return get_machine_list(machine_id)
        else:
            pass
    else:
        if content == "machine_id":
            return generate_machine_id()
        else:
            pass
    return None


def handle_connect_remote_function(request):
    """
    专门用于处理connect_remote功能
    :param request: 请求的详细消息
    :return: json格式的回复消息
    """
    LOG.info('handle_connect_remote_function')
    machine_id_a = request['id']
    machine_id_b = request['content']

    if machine_id_b in REGISTERED_MACHINES.keys():
        LOG.info("主机: {} 向主机: {}发送远程控制请求".format(machine_id_a, machine_id_b))
        machine = REGISTERED_MACHINES[machine_id_b]
        machine.remote_control_request = True
        REGISTERED_MACHINES[machine_id_b] = machine
        return True
    else:
        return False


def handle_query_function(request):
    """
    专门用于处理query功能
    :param request: 请求的详细消息
    :return: json格式的回复消息
    """
    # LOG.info('handle_query_function')
    content = request['content'].strip()
    machine_id = int(request['id'])
    if content == "tunnel_status":
        return get_tunnel_status(machine_id)
    elif content == "remote_control_request":
        return get_remote_control_request(machine_id)
    elif content == "keep-alive":
        return True
    else:
        return False


def handle_upload_function(request):
    """
    专门处理upload功能
    :param request: 请求的详细消息
    :return: json格式的回复消息
    """
    LOG.info('handle_upload_function')
    content = (request['content'])

    if content['ID'] not in REGISTERED_MACHINES:
        machine = RegisteredMachine(machine_id=content['ID'],
                                    name=content['Name'],
                                    description=content['Description'])
        append_machine(machine)

    return True


def handle_clear_function(request):
    """
    专门用于处理协议中的clear方法
    :param request: 请求详细信息
    :return: json响应消息
    """
    LOG.info("handle_clear_function")
    if request['content'] == 'remote_control_request':
        return clear_remote_control_request(int(request['id']))
    else:
        return False


def handle_reset_tunnel_port(request):
    """
    重置tunnel_port的值,从而间接地关闭隧道
    :param request: 请求详细信息
    :return:True/False
    """
    LOG.info('handle_reset_tunnel_port')
    if request['content'] == 'tunnel_port':
        if request['id'] in REGISTERED_MACHINES.keys():
            machine = REGISTERED_MACHINES[request['id']]
            machine.tunnel_port = -1
            REGISTERED_MACHINES[request['id']] = machine
            return True
        else:
            return False
    return False


def analyze_message(conn):
    """
    处理接收到的客户端消息, 该函数作为后台线程运行时调用, 处理完消息后会自动关闭连接
    :param conn: 与客户端的连接
    :return: None
    """
    assert isinstance(conn, socket.socket)

    try:
        recv_msg = conn.recv(1024)
        # LOG.info('接收到来自客户端的消息:\n{}'.format(recv_msg.decode('utf-8')))
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
            # LOG.info('不带有ID的请求消息')
            if request['function'] == 'get':
                response = handle_get_function(request, False)
            elif request['function'] == 'upload':
                response = handle_upload_function(request)
        elif len(request.keys()) == 3:
            # 说明这是带有machine_id的请求消息
            # LOG.info('带有ID的请求消息')
            if request['function'] == 'connect_remote':
                response = handle_connect_remote_function(request)
            elif request['function'] == 'get':
                response = handle_get_function(request)
            elif request['function'] == 'query':
                response = handle_query_function(request)
            elif request['function'] == 'reset':
                response = handle_reset_tunnel_port(request)
        else:
            # 说明这个是非法的请求
            LOG.info('收到非法的请求消息: {}'.format(request))
        if response is not None and response != "":
            conn.send(build_response_str(response).encode('utf-8'))
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
    server = ProxyServer('0.0.0.0', 9001)
    try:
        server.start()
    except Exception as e:
        LOG.error(str(e))
    finally:
        server.stop()


if __name__ == '__main__':
    main()
