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
import random
import socket
import subprocess

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
            listen_ports.add(int(column[3].split('.')[-1]))

    return listen_ports


def main():
    rand = random.Random()
    print(get_all_listen_ports())

if __name__ == '__main__':
    main()
