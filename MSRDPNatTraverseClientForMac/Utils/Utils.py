#!/user/bin/python3
# 文件：.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：
# 参数：
# 版本：0.1
# 备注：无
# 日志：

import logging

LOGGER = logging.getLogger('')


def config_logging(log_level=logging.ERROR, log_name='debug.log'):
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
