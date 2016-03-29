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

from threading import Thread
from twisted.internet import reactor
from protocol import *
from config import *
from utils import config_logging, LOG
import logging
from time import sleep

# 全局的配置
PROGRAM_CONFIG = {
    "listen_port": 8088,
    "log_level": 'DEBUG',
    "log_name": "debug.log"
}

# 检查在线状况的时间间隔, 单位为秒
CHECK_INTERVAL = 2


def check_alive_thread():
    while True:
        # LOG.debug('检查客户端是否在线...')
        # 不断地对keep_alive_count做递减的操作,当递减到零后,会认为客户端下线了
        from protocol import CLIENT_GROUP

        # 记录下线的客户端的id
        offline_id_list = list()

        for client in CLIENT_GROUP.get_members().values():
            client.keep_alive_count -= 1
            if client.keep_alive_count == 0:
                offline_id_list.append(client.id)

        # 移除那些离线的客户端
        for c_id in offline_id_list:
            if CLIENT_GROUP.remove(c_id):
                LOG.debug('id: {}的客户端已经离线'.format(c_id))

        sleep(CHECK_INTERVAL)


class ProxyServer:
    """
    代理服务器
    """
    def __init__(self, port):
        self._port = port

    def start(self):
        """
        启动代理服务器并监听
        :return:
        """
        print('启动代理服务器, 监听端口: {}'.format(self._port))
        reactor.listenTCP(self._port, NatTraverseFactory())
        reactor.run()

    def stop(self):
        """
        停止代理服务器
        :return:
        """
        print('停止监听端口{}, 停止代理服务器'.format(self._port))
        reactor.stop()


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

    server = ProxyServer(PROGRAM_CONFIG['listen_port'])

    # 用于检查客户端在线状态的线程
    check_thread = Thread(target=check_alive_thread)

    try:
        print('启动检查客户端在线状态的线程')
        check_thread.start()

        server.start()
    except Exception as err:
        LOG.error(str(err))
    finally:
        print('写入配置信息')
        save_config(PROGRAM_CONFIG)
        print('退出程序')

if __name__ == '__main__':
    main()
