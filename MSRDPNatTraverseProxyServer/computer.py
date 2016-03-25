#!/user/bin/python3
# coding: utf-8
# 文件：computer.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：
# 参数：
# 版本：0.1
# 备注：无
# 日志：


class Computer(object):
    """
    描述一台在线的计算机信息的类
    """
    def __init__(self, c_id=0, g_id=0, name=None, description=None):
        """
        构造函数
        :param c_id: 计算机分配到的id
        :param g_id: 计算机所在的分组g_id
        :param name: 计算机的名称
        :param description: 计算机的详细描述
        :return: None
        """
        self._id = c_id
        self._g_id = g_id
        self._name = name
        self._description = description
        self._control_request = False
        self._tunnel_port = -1
        self._is_under_control = False
        self._peered_remote_id = -1
        self._keep_alive_count = 10

    @property
    def id(self):
        """
        返回计算机的id
        :return: id
        """
        return self._id

    @property
    def g_id(self):
        """
        返回该计算机所在的分组g_id
        :return: g_id
        """
        return self._g_id

    @g_id.setter
    def g_id(self, value):
        """
        设置该计算机的分组g_id
        :param value: 分组号
        :return: 无
        """
        if isinstance(value, int):
            self._g_id = value
        else:
            raise TypeError("设置的值不是int类型")

    @property
    def name(self):
        """
        返回计算机的名称
        :return: name
        """
        return self._name

    @name.setter
    def name(self, value):
        """
        设置计算机的名称
        :param value: 名称
        :return: 无
        """
        if isinstance(value, str):
            self._name = value
        else:
            raise TypeError('设置的值不是str类型')

    @property
    def description(self):
        """
        获得计算机的描述信息
        :return: 描述信息
        """
        return self._description

    @description.setter
    def description(self, value):
        """
        设置计算机的描述信息
        :param value: 描述信息
        :return: 无
        """
        if isinstance(value, str):
            self._description = value
        else:
            raise TypeError('设置的值不是str类型')

    @property
    def control_request(self):
        """
        返回有无控制请求
        :return: True表示有控制请求; False表示没有
        """
        return self._control_request

    @control_request.setter
    def control_request(self, value):
        """
        设置控制请求状态
        :param value: True/False
        :return: 无
        """
        if isinstance(value, bool):
            self._control_request = value
        else:
            raise TypeError("设置的值不是bool类型")

    @property
    def tunnel_port(self):
        """
        获取该计算机获得的隧道端口号
        :return: 端口号
        """
        return self._tunnel_port

    @tunnel_port.setter
    def tunnel_port(self, value):
        """
        设置该计算机分配到的隧道端口号
        :param value: 端口号
        :return: 无
        """
        if isinstance(value, int):
            self._tunnel_port = value
        else:
            raise TypeError("设置的值不是int类型")

    @property
    def is_under_control(self):
        """
        获取当前计算机是否正在被控制
        :return: True/False
        """
        return self._is_under_control

    @is_under_control.setter
    def is_under_control(self, value):
        """
        设置此计算机是否正在被控制的状态
        :param value: True/False
        :return: 无
        """
        if isinstance(value, bool):
            self._is_under_control = value
        else:
            raise TypeError("设置的值不是bool类型")

    @property
    def peered_remote_id(self):
        """
        获取被远程控制的计算机的id
        :return: remote_id
        """
        return self._peered_remote_id

    @peered_remote_id.setter
    def peered_remote_id(self, value):
        """
        设置配对的计算机的id
        :param value: 远程计算机的id
        :return: 无
        """
        if isinstance(value, int):
            self._peered_remote_id = value
        else:
            raise TypeError("设置的值不是int类型")

    @property
    def keep_alive_count(self):
        """
        获得存活状态值
        :return: 状态值
        """
        return self._keep_alive_count

    @keep_alive_count.setter
    def keep_alive_count(self, value):
        """
        设置存活状态值
        :param value: 任意一个数字
        :return: 无
        """
        if isinstance(value, int):
            self._keep_alive_count = value
        else:
            raise TypeError("设置的值不是int类型")

    def __str__(self):
        return 'id: {0}, ' \
               'gid: {1}, ' \
               'name: {2}, ' \
               'description: {3}, ' \
               'control_request: {4}, ' \
               'tunnel_port: {5}, ' \
               'is_under_control: {6}, ' \
               'peered_remote_id: {7}, ' \
               'keep_alive_count: {8}'.format(self._id, self._g_id,
                                              self._name, self._description,
                                              self._control_request, self._tunnel_port,
                                              self._is_under_control, self._peered_remote_id,
                                              self._keep_alive_count)

    def __repr__(self):
        return self.__str__()


class ComputerGroup:
    """
    该类用来存放多个计算机的组
    """
    def __init__(self, g_id=0, name=None, capacity=50):
        """
        构造函数
        :param g_id: 该分组的id
        :param name: 该分组的名称
        :param capacity: 该分组最多可容纳的计算机个数
        :return: 无
        """
        self._id = g_id
        self._name = name
        self._capacity = capacity
        self._members = dict()  # 分组中包含的成员存放在一个字典中

    @property
    def name(self):
        """
        获取分组的名称
        :return: 分组名称
        """
        return self._name

    @name.setter
    def name(self, value):
        """
        设置分组的名称
        :param value: 分组名称
        :return: 无
        """
        if isinstance(value, str):
            self._name = value
        else:
            raise TypeError("设置的值不是str类型")

    @property
    def capacity(self):
        """
        获得分组最多可以容纳的计算机个数
        :return: 个数
        """
        return self._capacity

    @capacity.setter
    def capacity(self, value):
        """
        设置分组可以容纳的计算机个数
        :param value: 最多的成员个数
        :return: 无
        """
        if isinstance(value, int):
            self._capacity = value
        else:
            raise TypeError("设置的值不是int类型")

    def has_member(self, computer_id):
        """
        通过使用成员的id,即计算机的id来检查是否存在于分组中
        :param computer_id: 计算机的id
        :return: True/False
        """
        return computer_id in self._members.keys()

    def append(self, computer):
        """
        向分组中添加计算机成员
        :param computer: Computer类型的实例
        :return: True/False, 指示是否添加成功
        """
        if isinstance(computer, Computer):
            # 首先需要检查是否到达最大的容量上限
            if len(self._members) == self._capacity:
                return False
            else:
                # 检查是否有该成员存在
                if self.has_member(computer.id):
                    return False
                else:
                    self._members[computer.id] = computer
                    return True
        else:
            raise TypeError("添加的类型不是Computer类型")

    def remove(self, computer_id):
        """
        从分组中移除计算机成员
        :param computer_id: 指定计算机的id
        :return: True/False
        """
        if self.has_member(computer_id):
            self._members.pop(computer_id)
            return True
        else:
            return False

    def get_member(self, computer_id):
        """
        从成员列表中获得指定的id的计算机实例
        :param computer_id: 计算机的id
        :return: None/Computer类型的对象实例
        """
        if self.has_member(computer_id):
            return self._members[computer_id]
        else:
            return None

    def set_member(self, computer):
        """
        设置分组中的成员实例,通过此接口可以更改一个成员
        :param computer: Computer类型的对象实例
        :return: True/False
        """
        if self.remove(computer.id):
            return self.append(computer)
        else:
            return False


