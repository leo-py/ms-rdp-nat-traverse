#!/user/bin/python3
# coding: utf-8
# 文件：RegisteredMachine.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：描述已经"注册",即与服务器成功连接的机器描述信息
# 参数：无
# 版本：0.1
# 备注：无
# 日志：无
from json import dumps, loads


class RegisteredMachine(object):
    def __init__(self,
                 machine_id=10000,
                 name=None,
                 description=None,
                 tunnel_port=-1,
                 tunnel_status=False,
                 remote_control_request=False,
                 alive=True):
        registered_machine = dict()
        registered_machine['id'] = machine_id
        registered_machine['name'] = name
        registered_machine['description'] = description
        registered_machine['tunnel_port'] = tunnel_port
        registered_machine['tunnel_status'] = tunnel_status
        registered_machine['remote_control_request'] = remote_control_request
        registered_machine['alive'] = alive
        self.__registered_machine = registered_machine

    @property
    def instance(self):
        return self.__registered_machine

    @property
    def machine_id(self):
        return self.__registered_machine['id']

    @machine_id.setter
    def machine_id(self, value):
        if isinstance(value, int):
            self.__registered_machine['id'] = value

    @property
    def name(self):
        return self.__registered_machine['name']

    @name.setter
    def name(self, value):
        if isinstance(value, str):
            self.__registered_machine['name'] = value

    @property
    def description(self):
        return self.__registered_machine['description']

    @description.setter
    def description(self, value):
        if isinstance(value, str):
            self.__registered_machine['description'] = value;

    @property
    def tunnel_port(self):
        return self.__registered_machine['tunnel_port']

    @tunnel_port.setter
    def tunnel_port(self, value):
        if isinstance(value, int):
            self.__registered_machine['tunnel_port'] = value

    @property
    def tunnel_status(self):
        return self.__registered_machine['tunnel_status']

    @tunnel_status.setter
    def tunnel_status(self, value):
        if isinstance(value, str):
            if "connected" in value or "disconnected" in value:
                self.__registered_machine['tunnel_status'] = value

    @property
    def remote_control_request(self):
        return self.__registered_machine['remote_control_request']

    @remote_control_request.setter
    def remote_control_request(self, value):
        if isinstance(value, bool):
            self.__registered_machine['remote_control_request'] = value

    @property
    def alive(self):
        return self.__registered_machine['alive']

    @alive.setter
    def alive(self, value):
        if isinstance(value, bool):
            self.__registered_machine['alive'] = value

    def __str__(self):
        return dumps(self.__registered_machine)

    def __repr__(self):
        return dumps(self.__registered_machine)
