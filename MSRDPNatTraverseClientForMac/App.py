#!/user/bin/python3
# 文件：App.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：应用主入口
# 参数：
# 版本：0.1
# 备注：无
# 日志：

from MainForm.MainForm import *
from PyQt5.QtWidgets import QApplication
from Utils.Utils import *
import sys


def main():
    config_logging()
    app = QApplication(sys.argv)

    # 给应用设置一个图标
    app.setWindowIcon(QIcon('Icons/main.ico'))
    mainForm = MainForm()
    mainForm.show()
    sys.exit(app.exec_())


if __name__ == '__main__':
    main()
