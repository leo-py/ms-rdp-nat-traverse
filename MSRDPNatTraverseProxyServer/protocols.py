#!/user/bin/python3
# coding: utf-8
# 文件：protocols.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：定制的客户端与代理服务器之间的交互协议的处理函数
# 参数：无
# 版本：0.1
# 备注：该文件中的函数适用于协议0.2版本
# 日志：无

from json import loads, dumps
from utils import *
from computer import *

# 计算机组,用来存放多个计算机信息
COMPUTER_GROUP = ComputerGroup(0, 'work', 100)

"""
具体的处理函数
"""


def get_computer_id(content=None):
    """
    请求得到一个合法分配到的编号
    :param content: 请求的具体内容
    :return: id
    """
    LOG.debug('调用: get_computer_id')
    if not isinstance(content, dict):
        LOG.error("消息内容格式不正确: {}".format(content))
        return -1

    # 不断产生随机数,保证随机数不能和其他计算机的id重复
    while True:
        rand_id = get_rand_num(10000, 40000)
        if not COMPUTER_GROUP.has_member(rand_id):
            return rand_id


def post_computer_info(content):
    """
    接收来自客户端上传过来的计算机信息
    :param content: 具体内容
    :return: True/False
    """
    LOG.debug('调用: post_computer_info')
    if not isinstance(content, dict):
        LOG.error("消息内容格式不正确: {}".format(content))
        return False

    if COMPUTER_GROUP.has_member(content['id']):
        LOG.error('存在相同的计算机信息,无法添加.')
        return False
    LOG.debug('已经有的成员个数: {}'.format(COMPUTER_GROUP.count()))
    return COMPUTER_GROUP.append(Computer(content['id'], 0, content['value']['name'],
                                          content['value']['description']))


def get_control_request(content):
    """
    请求查询指定id的control_request标志位,即有没有远程控制请求
    :param content: 具体内容
    :return: True/False
    """
    LOG.debug('调用: get_control_request')
    if not isinstance(content, dict):
        LOG.error("消息内容格式不正确: {}".format(content))
        return False

    if COMPUTER_GROUP.has_member(content['id']):
        return COMPUTER_GROUP.get_member(content['id']).control_request
    else:
        LOG.error("分组中不存在的id: {}".format(content['id']))
        return False


def post_control_request(content):
    """
    设置指定计算机id的control_request标志
    :param content: 具体内容
    :return: True/False
    """
    LOG.debug('调用: post_control_request')
    if not isinstance(content, dict):
        LOG.error("消息内容格式不正确: {}".format(content))
        return False

    if COMPUTER_GROUP.has_member(content['id']):
        COMPUTER_GROUP.get_member(content['id']).control_request = content['value']
        return True
    else:
        LOG.error("分组中不存在的id: {}".format(content['id']))
        return False


def get_tunnel_port(content):
    """
    请求获得一个合法的隧道端口号
    :param content: 具体内容
    :return: 端口号
    """
    LOG.debug('调用: get_tunnel_port')
    if not isinstance(content, dict):
        LOG.error("消息内容格式不正确: {}".format(content))
        return -1

    if COMPUTER_GROUP.has_member(content['id']):
        while True:
            ports = get_all_listen_ports()
            LOG.debug('正在监听的端口号: {}'.format(ports))
            port = get_rand_num(10000, 20000)
            if port not in ports:
                COMPUTER_GROUP.get_member(content['id']).tunnel_port = port
                LOG.debug('生成合法的端口号: {}'.format(port))
                return port
    else:
        LOG.error("分组中不存在的id: {}".format(content['id']))
        return -1


def post_tunnel_port(content):
    """
    请求设置指定id的计算机的隧道端口号
    :param content: 具体内容
    :return: True/False
    """
    LOG.debug('调用: post_tunnel_port')
    if not isinstance(content, dict):
        LOG.error("消息内容格式不正确: {}".format(content))
        return False

    if COMPUTER_GROUP.has_member(content['id']):
        COMPUTER_GROUP.get_member(content['id']).tunnel_port = content['value']
        return True
    else:
        LOG.error("分组中不存在的id: {}".format(content['id']))
        return False


def get_is_under_control(content):
    """
    获得指定id计算机是否正在被远程控制
    :param content: 具体内容
    :return: True/False
    """
    LOG.debug('调用: get_is_under_control')
    if not isinstance(content, dict):
        LOG.error("消息内容格式不正确: {}".format(content))
        return False

    if COMPUTER_GROUP.has_member(content['id']):
        return COMPUTER_GROUP.get_member(content['id']).is_under_control
    else:
        LOG.error("分组中不存在的id: {}".format(content['id']))
        return False


def post_is_under_control(content):
    """
    设置指定id计算机是否正在被被的计算机控制的状态
    :param content: 具体内容
    :return: True/False
    """
    LOG.debug('调用: post_is_under_control')
    if not isinstance(content, dict):
        LOG.error("消息内容格式不正确: {}".format(content))
        return False

    if COMPUTER_GROUP.has_member(content['id']):
        COMPUTER_GROUP.get_member(content['id']).is_under_control = content['value']
        return True
    else:
        LOG.error("分组中不存在的id: {}".format(content['id']))
        return False


def get_peered_remote_id(content):
    """
    获得指定id计算机配对的远程计算机的id
    :param content: 具体内容
    :return: id
    """
    LOG.debug('调用: get_peered_remote_id')
    if not isinstance(content, dict):
        LOG.error("消息内容格式不正确: {}".format(content))
        return -1

    if COMPUTER_GROUP.has_member(content['id']):
        return COMPUTER_GROUP.get_member(content['id']).peered_remote_id
    else:
        LOG.error("分组中不存在的id: {}".format(content['id']))
        return -1


def get_keep_alive_count(content):
    """
    获取指定id的计算机的keep-alive计数值
    :param content: 具体内容
    :return: 数值
    """
    LOG.debug('调用: get_keep_alive_count')
    if not isinstance(content, dict):
        LOG.error("消息内容格式不正确: {}".format(content))
        return -1

    if COMPUTER_GROUP.has_member(content['id']):
        return COMPUTER_GROUP.get_member(content['id']).keep_alive_count
    else:
        LOG.error("分组中不存在的id: {}".format(content['id']))
        return -1


def post_keep_alive_count(content):
    """
    客户端设置指定id计算机的keep-alive值
    :param content: 具体内容
    :return: True/False
    """
    LOG.debug('调用: post_keep_alive_count')
    if not isinstance(content, dict):
        LOG.error("消息内容格式不正确: {}".format(content))
        return False

    if COMPUTER_GROUP.has_member(content['id']):
        COMPUTER_GROUP.get_member(content['id']).keep_alive_count = content['value']
        return True
    else:
        LOG.error("分组中不存在的id: {}".format(content['id']))
        return False

'''
处理协议解析部分函数
'''


def response(content):
    """
    自动将内容打包成响应的消息格式
    :param content: 响应内容
    :return: json格式的响应消息
    """
    resp = dict()
    resp["response"] = content
    return dumps(content)


def parse(request):
    """
    将请求的json格式的消息字符串转换成一个字典格式的实例,便于访问
    :param request: 请求的消息字符串
    :return: 字典格式的消息请求或者None
    """
    try:
        req = loads(request, encoding='utf-8')
        if isinstance(req, dict) and len(req) == 1:
            return req
        else:
            return None
    except:
        return None


def is_get_request(request):
    """
    判断是否是get请求
    :param request: 已经转换成dict类型的请求
    :return: True/False
    """
    if request:
        return 'get' in request.keys()
    else:
        return False


def is_post_request(request):
    """
    判断是否是post请求
    :param request: 已经转换成dict类型的请求
    :return: True/False
    """
    if request:
        return 'post' in request.keys()
    else:
        return False


def get_request_content(request):
    """
    获得request的请求内容
    :param request: 已经被转换成dict类型的request请求
    :return: None/dict类型的请求内容
    """
    pass


def key_equals(content, key):
    """
    判断请求的内容中的key是不是指定的key,从而调用相应的方法处理
    :param content: 请求消息(dict)类型的内容
    :param key: key的字符串内容
    :return: True/False
    """
    if isinstance(content, dict):
        return key in content.values()
    else:
        return False


def handle_request(request):
    """
    处理请求的函数. 该函数会识别出来是何种请求, 并根据不同的请求类型,调用不同的函数,返回响应信息
    :param request: 完整的请求内容字符串
    :return: 响应消息字符串
    """
    if isinstance(request, str):
        # 将请求转换成一个字典类型
        request = parse(request)

        LOG.debug(request)
        # 判断请求的类型
        if is_get_request(request):
            # 判断是何种get请求,通过key来判读
            content = request['get']
            if key_equals(content, 'id'):
                return response(get_computer_id(content))
            elif key_equals(content, 'control_request'):
                return response(get_control_request(content))
            elif key_equals(content, 'tunnel_port'):
                return response(get_tunnel_port(content))
            elif key_equals(content, 'is_under_control'):
                return response(get_is_under_control(content))
            elif key_equals(content, 'peered_remote_id'):
                return response(get_peered_remote_id(content))
            elif key_equals(content, 'keep_alive_count'):
                return response(get_keep_alive_count(content))
            else:
                return response(False)
        elif is_post_request(request):
            # 判断是何种post请求
            content = request['post']
            if key_equals(content, 'computer_info'):
                return response(post_computer_info(content))
            elif key_equals(content, 'control_request'):
                return response(post_control_request(content))
            elif key_equals(content, 'tunnel_port'):
                return response(post_tunnel_port(content))
            elif key_equals(content, 'is_under_control'):
                return response(post_is_under_control(content))
            elif key_equals(content, 'keep_alive_count'):
                return response(post_keep_alive_count(content))
            else:
                return response(False)
    else:
        return response(False)
