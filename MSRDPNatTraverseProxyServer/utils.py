#!/user/bin/python3
# coding: utf-8
# 文件：utils.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：一些工具类函数
# 参数：无
# 版本：0.1
# 备注：无
# 日志：无
import subprocess
import logging
import platform
from random import Random

# 全局使用的LOG
LOG = logging.getLogger()


def config_logging(log_level=logging.DEBUG, log_name='debug.log'):
    """
    配置日志模块,可以在同时将日志信息输出到终端和文件中
    :param log_level: 日志输出等级
    :param log_name: 生成的日志文件名称
    :return: 无
    """
    logging.basicConfig(
        level=log_level,
        format="[%(asctime)s][%(module)-6s][line %(lineno) -d][%(levelname)s]: %(message)s",
        datefmt='%m-%d %H:%M',
        filename=log_name,
        filemode='a')

    console = logging.StreamHandler()
    console.setLevel(log_level)
    formatter = logging.Formatter('[%(asctime)s][%(module)-6s][line %(lineno) -d][%(levelname)s]: %(message)s')
    console.setFormatter(formatter)
    logging.getLogger('').addHandler(console)


def get_system_type():
    """
    获得系统的类型
    :return: 字符串类型的系统类型
    """
    return platform.system()


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
            sys_type = get_system_type()
            if sys_type == "Linux":
                listen_ports.add(int(column[3].split(':')[-1]))
            elif sys_type == "Darwin":
                listen_ports.add(int(column[3].split('.')[-1]))
            else:
                LOG.debug('未能识别的操作系统类型')
    LOG.debug('listen_ports: {}'.format(listen_ports))
    return listen_ports


def get_rand_num(start=0, stop=1):
    """
    获得一个指定范围的随机数
    :param start: 起始值
    :param stop: 结束值
    :return: 指定范围的随机数
    """
    return Random().randrange(start, stop)

