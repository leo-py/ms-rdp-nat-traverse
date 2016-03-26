#!/user/bin/python3
# coding: utf-8
# 文件：server.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：代理服务器主要的功能集成
# 参数：无
# 版本：0.1
# 备注：无
# 日志：无
import socket
from threading import Thread
from protocols import *
from config import *
from time import sleep

# 全局的配置
PROGRAM_CONFIG = {
    "listen_port": 8088,
    "log_level": 'INFO',
    "log_name": "debug.log"
}

# 检查在线状况的时间间隔, 单位为秒
CHECK_INTERVAL = 6


def check_alive_thread():
    while True:
        # LOG.debug('检查计算机是否在线...')
        # 不断地对keep_alive_count做递减的操作,当递减到零后,会认为计算机下线了
        from protocols import COMPUTER_GROUP

        # 记录下线的计算机的id
        offline_id_list = list()

        for computer in COMPUTER_GROUP.get_members().values():
            computer.keep_alive_count -= 1
            if computer.keep_alive_count == 0:
                offline_id_list.append(computer.id)

        # 移除那些离线的计算机
        for c_id in offline_id_list:
            if COMPUTER_GROUP.remove(c_id):
                LOG.debug('id: {}的计算机已经离线'.format(c_id))

        sleep(CHECK_INTERVAL)


def connection_thread(connection):
    """
    处理具体的连接请求,并在请求处理后关闭连接,释放资源
    :param connection: 连接
    :return: None
    """
    if isinstance(connection, socket.socket):
        recv_request = connection.recv(1024)
        LOG.debug('接收: {}'.format(recv_request.decode('utf-8')))
        res = handle_request(recv_request.decode('utf-8'))
        LOG.debug('响应: {}'.format(res))
        # 响应后关闭连接
        if res:
            connection.send(res.encode('utf-8'))
        connection.close()
    else:
        return None


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

    def run(self):
        """
        监听来自客户端发起的连接,和接收消息并启动新的线程处理消息.
        """
        while self.__can_run:
            conn, add = self.__listen_server.accept()
            LOG.debug('收到连接请求,来自: {}'.format(add))
            thread = Thread(target=connection_thread, args=(conn,))
            thread.setDaemon(True)
            thread.start()

    def start(self):
        """
        启动监听线程
        """
        LOG.debug('启动监听线程, 监听地址: {0}:{1}'.format(self._listen_ip, self._listen_port))
        self.__listen_server.listen()
        self.__can_run = True

        LOG.debug('启动检查计算机是否在线的线程')
        ck_alive = Thread(target=check_alive_thread, name='check_alive')
        ck_alive.setDaemon(True)
        ck_alive.start()
        self.run()

    def stop(self):
        """
        停止监听线程
        """
        LOG.debug('停止监听线程')
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
        LOG.debug('启动代理服务器')
        if self._listen_thread is None:
            self._listen_thread = ListenThread(self._listen_ip,
                                               self._listen_port,
                                               'listen_thread')
            self._listen_thread.setDaemon(True)
            self._listen_thread.start()
        LOG.debug('已经成功启动')

    def stop(self):
        """
        停止代理服务器的服务.
        """
        LOG.debug('停止代理服务器')
        if self._listen_thread:
            self._listen_thread.stop()


def main():
    global PROGRAM_CONFIG

    # 加载配置
    config = load_config()
    if config:
        PROGRAM_CONFIG = config

    # 获取日志等级
    if PROGRAM_CONFIG['log_level'] == 'DEBUG':
        config_logging(logging.DEBUG, PROGRAM_CONFIG['log_name'])
    elif PROGRAM_CONFIG['log_level'] == 'INFO':
        config_logging(logging.debug, PROGRAM_CONFIG['log_name'])
    elif PROGRAM_CONFIG['log_level'] == 'ERROR':
        config_logging(logging.ERROR, PROGRAM_CONFIG['log_name'])
    elif PROGRAM_CONFIG['log_level'] == 'WARN':
        config_logging(logging.WARN, PROGRAM_CONFIG['log_name'])
    else:
        config_logging()

    server = ProxyServer('0.0.0.0', PROGRAM_CONFIG['listen_port'])
    try:
        server.start()
    except Exception as err:
        LOG.error(str(err))
    finally:
        server.stop()
        LOG.debug('写入配置信息')
        save_config(PROGRAM_CONFIG)
        LOG.debug('退出程序')

if __name__ == '__main__':
    main()
