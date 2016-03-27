#!/user/bin/python3
# coding: utf-8
# 文件：config.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：配置文件的加载
# 参数：无
# 版本：0.1
# 备注：无
# 日志：无

from json import load, dump
from os import path

# 指定配置文件存放的位置和名称
CONFIG_PATH = 'config.json'


def validate_config(config):
    """
    检查配置文件是否完好
    :param config: 配置内容
    :return: True/False
    """
    if isinstance(config, dict):
        for key in ('listen_port', 'log_level', 'log_name'):
            if key not in config:
                return False
        return True
    else:
        return False


def load_config():
    """
    从硬盘上加载配置文件
    :return: 配置文件(dict)
    """
    if not path.exists(CONFIG_PATH):
        return None
    config = load(open(CONFIG_PATH))
    if validate_config(config):
        return config
    else:
        print('配置文件已经损坏.')
        return None


def save_config(config):
    """
    保存配置文件
    :param config: 配置信息
    :return: 无
    """
    try:
        dump(config, open(CONFIG_PATH, 'w'))
    except Exception as err:
        print(str(err))
