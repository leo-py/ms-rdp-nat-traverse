#!/user/bin/python3
# 文件：protocol.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：协议解析及处理
# 参数：无
# 版本：0.1
# 备注：无
# 日志：无
from twisted.internet.protocol import Protocol
from twisted.internet.protocol import Factory, connectionDone
from client import ClientGroup, Client
from utils import *
from json import loads, dumps, load, dump


# 客户端组,用来存放多个客户端信息
CLIENT_GROUP = ClientGroup(0, 'work', 100)


class NatTraverseProtocol(Protocol):
    """
    内网穿透相关的协议解析和处理
    """
    def __init__(self):
        pass

    def connectionMade(self):
        """
        当TCP连接成功时触发的事件函数
        :return: 无
        """
        pass

    def connectionLost(self, reason=connectionDone):
        """
        当TCP连接断开后触发的事件函数
        :param reason: 原因
        :return: 无
        """
        pass

    def dataReceived(self, data):
        """
        当接收到来自客户端的数据时触发的事件函数
        :param data: 具体的数据,需要decode
        :return: 无
        """
        # 解析数据
        response = self.handle_request(data.decode('utf-8'))

        # 响应
        if response:
            self.transport.write(response.encode('utf-8'))
        self.transport.loseConnection()

    def handle_request(self, request):
        """
        解析接收到来自客户端的请求消息
        :param request: 具体的数据
        :return: json格式的响应消息
        """
        if isinstance(request, str):
            # 将请求转换成一个字典类型
            request = self.parse(request)

            LOG.debug(request)
            # 判断请求的类型
            if self.is_get_request(request):
                # 判断是何种get请求,通过key来判读
                content = request['get']
                if self.key_equals(content, 'id'):
                    return self.response(self.get_client_id(content))
                elif self.key_equals(content, 'online_list'):
                    return self.response(self.get_online_client_list(content))
                elif self.key_equals(content, 'control_request'):
                    return self.response(self.get_control_request(content))
                elif self.key_equals(content, 'tunnel_port'):
                    return self.response(self.get_tunnel_port(content))
                elif self.key_equals(content, 'is_under_control'):
                    return self.response(self.get_is_under_control(content))
                elif self.key_equals(content, 'peered_remote_id'):
                    return self.response(self.get_peered_remote_id(content))
                elif self.key_equals(content, 'keep_alive_count'):
                    return self.response(self.get_keep_alive_count(content))
                elif self.key_equals(content, 'tunnel_status'):
                    return self.response(self.get_tunnel_status(content))
                elif self.key_equals(content, 'is_online'):
                    return self.response(self.get_is_online(content))
                else:
                    return self.response(False)
            elif self.is_post_request(request):
                # 判断是何种post请求
                content = request['post']
                if self.key_equals(content, 'client_info'):
                    return self.response(self.post_client_info(content))
                elif self.key_equals(content, 'control_request'):
                    return self.response(self.post_control_request(content))
                elif self.key_equals(content, 'tunnel_port'):
                    return self.response(self.post_tunnel_port(content))
                elif self.key_equals(content, 'is_under_control'):
                    return self.response(self.post_is_under_control(content))
                elif self.key_equals(content, 'keep_alive_count'):
                    return self.response(self.post_keep_alive_count(content))
                elif self.key_equals(content, 'peered_remote_id'):
                    return self.response(self.post_peered_remote_id(content))
                else:
                    return self.response(False)
        else:
            return self.response(False)

    @staticmethod
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

    @staticmethod
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

    @staticmethod
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

    @staticmethod
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

    @staticmethod
    def response(content):
        """
        自动将内容打包成响应的消息格式
        :param content: 响应内容
        :return: json格式的响应消息
        """
        resp = dict()
        resp["response"] = content
        return dumps(resp)

    @staticmethod
    def get_client_id(content=None):
        """
        请求得到一个合法分配到的编号
        :param content: 请求的具体内容
        :return: id
        """
        LOG.debug('调用: get_client_id')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return -1

        # 不断产生随机数,保证随机数不能和其他客户端的id重复
        while True:
            rand_id = get_rand_num(10000, 40000)
            if not CLIENT_GROUP.has_member(rand_id):
                return rand_id

    @staticmethod
    def post_client_info(content):
        """
        接收来自客户端上传过来的客户端信息
        :param content: 具体内容
        :return: True/False
        """
        LOG.debug('调用: post_client_info')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return False

        if CLIENT_GROUP.has_member(content['id']):
            LOG.error('存在相同的客户端信息,无法添加.')
            return False
        LOG.debug('已经有的成员个数: {}'.format(CLIENT_GROUP.count()))
        return CLIENT_GROUP.append(Client(content['id'], 0, content['value']['name'],
                                              content['value']['description']))

    @staticmethod
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

        if CLIENT_GROUP.has_member(content['id']):
            return CLIENT_GROUP.get_member(content['id']).control_request
        else:
            LOG.error("分组中不存在的id: {}".format(content['id']))
            return False

    @staticmethod
    def post_control_request(content):
        """
        设置指定客户端id的control_request标志
        :param content: 具体内容
        :return: True/False
        """
        LOG.debug('调用: post_control_request')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return False

        if CLIENT_GROUP.has_member(content['id']):
            CLIENT_GROUP.get_member(content['id']).control_request = content['value']
            return True
        else:
            LOG.error("分组中不存在的id: {}".format(content['id']))
            return False

    @staticmethod
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

        if CLIENT_GROUP.has_member(content['id']):
            if content['value']:
                while True:
                    ports = get_all_listen_ports()
                    LOG.debug('正在监听的端口号: {}'.format(ports))
                    port = get_rand_num(10000, 20000)
                    if port not in ports:
                        CLIENT_GROUP.get_member(content['id']).tunnel_port = port
                        LOG.debug('生成合法的端口号: {}'.format(port))
                        return port
            else:
                port = CLIENT_GROUP.get_member(content['id']).tunnel_port
                LOG.info('读取tunnel_port: {}'.format(port))
                return port
        else:
            LOG.error("分组中不存在的id: {}".format(content['id']))
            return -1

    @staticmethod
    def post_tunnel_port(content):
        """
        请求设置指定id的客户端的隧道端口号
        :param content: 具体内容
        :return: True/False
        """
        LOG.debug('调用: post_tunnel_port')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return False

        if CLIENT_GROUP.has_member(content['id']):
            CLIENT_GROUP.get_member(content['id']).tunnel_port = content['value']
            return True
        else:
            LOG.error("分组中不存在的id: {}".format(content['id']))
            return False

    @staticmethod
    def get_tunnel_status(content):
        """
        请求查询指定id的客户端隧道状态,即要求建立的端口号有没有正在被监听
        :param content:
        :return:
        """
        LOG.debug('调用: get_tunnel_status')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return False

        if CLIENT_GROUP.has_member(content['id']):
            LOG.info('隧道端口号: {}'.format(CLIENT_GROUP.get_member(content['id']).tunnel_port))
            status = CLIENT_GROUP.get_member(content['id']).tunnel_port in get_all_listen_ports()
            LOG.debug('隧道状态: {}'.format(status))
            return status
        else:
            LOG.error('分组中不存在的id: {}'.format(content['id']))
            return False

    @staticmethod
    def get_is_under_control(content):
        """
        获得指定id客户端是否正在被远程控制
        :param content: 具体内容
        :return: True/False
        """
        LOG.debug('调用: get_is_under_control')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return False

        if CLIENT_GROUP.has_member(content['id']):
            return CLIENT_GROUP.get_member(content['id']).is_under_control
        else:
            LOG.error("分组中不存在的id: {}".format(content['id']))
            return False

    @staticmethod
    def post_is_under_control(content):
        """
        设置指定id客户端是否正在被被的客户端控制的状态
        :param content: 具体内容
        :return: True/False
        """
        LOG.debug('调用: post_is_under_control')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return False

        if CLIENT_GROUP.has_member(content['id']):
            CLIENT_GROUP.get_member(content['id']).is_under_control = content['value']
            return True
        else:
            LOG.error("分组中不存在的id: {}".format(content['id']))
            return False

    @staticmethod
    def get_peered_remote_id(content):
        """
        获得指定id客户端配对的远程客户端的id
        :param content: 具体内容
        :return: id
        """
        LOG.debug('调用: get_peered_remote_id')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return -1

        if CLIENT_GROUP.has_member(content['id']):
            return CLIENT_GROUP.get_member(content['id']).peered_remote_id
        else:
            LOG.error("分组中不存在的id: {}".format(content['id']))
            return -1

    @staticmethod
    def post_peered_remote_id(content):
        """
        设置指定id客户端配对的id
        :param content: 具体内容
        :return: True/False
        """
        LOG.debug('调用: post_peered_remote_id')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return -1

        if CLIENT_GROUP.has_member(content['id']):
            CLIENT_GROUP.get_member(content['id']).peered_remote_id = content['value']
            LOG.debug("修改客户端({})的配对客户端为{}".format(content['id'], content['value']))
            return True
        else:
            LOG.error("分组中不存在的id: {}".format(content['id']))
            return False

    @staticmethod
    def get_keep_alive_count(content):
        """
        获取指定id的客户端的keep-alive计数值
        :param content: 具体内容
        :return: 数值
        """
        LOG.debug('调用: get_keep_alive_count')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return -1

        if CLIENT_GROUP.has_member(content['id']):
            return CLIENT_GROUP.get_member(content['id']).keep_alive_count
        else:
            LOG.error("分组中不存在的id: {}".format(content['id']))
            return -1

    @staticmethod
    def post_keep_alive_count(content):
        """
        客户端设置指定id客户端的keep-alive值
        :param content: 具体内容
        :return: True/False
        """
        LOG.debug('调用: post_keep_alive_count')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return False

        if CLIENT_GROUP.has_member(content['id']):
            CLIENT_GROUP.get_member(content['id']).keep_alive_count = content['value']
            return True
        else:
            LOG.error("分组中不存在的id: {}".format(content['id']))
            return False

    @staticmethod
    def get_is_online(content):
        """
        查询指定的id的客户端是否在线
        :param content: 具体内容
        :return: True/False
        """
        LOG.debug('调用: get_is_online')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return False

        if CLIENT_GROUP.has_member(content['id']):
            is_online = CLIENT_GROUP.has_member(content['id'])
            LOG.debug('客户端({})在线状态: {}'.format(content['id'], is_online))
            return is_online
        else:
            LOG.error("分组中不存在的id: {}".format(content['id']))
            return False

    @staticmethod
    def get_online_client_list(content):
        """
        获取当前在线的计算列表
        :param content: 具体内容
        :return:列表
        """
        LOG.debug('调用: get_online_client_list')
        if not isinstance(content, dict):
            LOG.error("消息内容格式不正确: {}".format(content))
            return False

        online_list = dict()
        for item in CLIENT_GROUP.get_members().values():
            if isinstance(item, Client):
                # 必须要过滤掉申请查询者的信息,返回其他在线用户
                if item.id != content['id']:
                    online_list[item.id] = item.name

        return online_list


class NatTraverseFactory(Factory):
    """
    工厂类,创建一个协议解析和处理的实例
    """
    def __init__(self):
        pass

    def buildProtocol(self, addr):
        LOG.debug('客户端: {}'.format(addr))
        return NatTraverseProtocol()
