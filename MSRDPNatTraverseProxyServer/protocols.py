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
from utils import LOG


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
    return request.values[0]


def key_equals(request, key):
    """
    判断请求的内容中的key是不是指定的key,从而调用相应的方法处理
    :param request: 请求消息(dict)类型的
    :param key: key的字符串内容
    :return: True/False
    """
    if isinstance(request, dict):
        return key in request.values[0]
    else:
        return False


def validate_callbacks(callbacks):
    """
    验证一下回调函数是否是合法的,否则将无法调用
    :param callbacks:
    :return:
    """
    if isinstance(callbacks, dict):
        if len(callbacks) > 0:
            return True
    return False


def handle_request(request, callbacks):
    """
    处理请求的函数. 该函数会识别出来是何种请求, 并根据不同的请求类型,调用不同的函数,返回响应信息
    :param request: 完整的请求内容字符串
    :param callbacks: 一组回调函数字典,这些回调函数是在主程序代码中定义的.
    callbacks的格式:{"get": {"key_str": function}, "post": {"key_str": function}}
    :return: 响应消息字符串
    """
    if validate_callbacks(callbacks):
        LOG.error('callbacaks不是类型不合法')
        return response('false')

    if isinstance(request, str):
        # 将请求转换成一个字典类型
        request = parse(request)

        # 判断请求的类型
        if is_get_request(request):
            # 判断是何种get请求,通过key来判读
            if key_equals(request, 'id'):
                pass
            elif key_equals(request, 'control_request'):
                pass
            elif key_equals(request, 'tunnel_port'):
                pass
            elif key_equals(request, 'is_under_control'):
                pass
            elif key_equals(request, 'peered_remote_id'):
                pass
            elif key_equals(request, 'keep_alive_count'):
                pass
            else:
                pass
        elif is_post_request(request):
            # 判断是何种post请求
            if key_equals(request, 'computer_info'):
                pass
            elif key_equals(request, 'control_request'):
                pass
            elif key_equals(request, 'tunnel_port'):
                pass
            elif key_equals(request, 'is_under_control'):
                pass
            elif key_equals(request, 'keep_alive_count'):
                pass
            else:
                pass
    else:
        return response("false")
