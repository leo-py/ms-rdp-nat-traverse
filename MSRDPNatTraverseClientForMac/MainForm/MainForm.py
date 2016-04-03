#!/user/bin/python3
# 文件：MainForm.py
# 作者：Christopher Lee
# 博客：http://www.cnblogs.com/chriscabin
# 主页：https://www.github.com/chrisleegit
# 邮箱：christopherlee1993@outlook.com
# 功能：统一实现一些槽函数. 且最大可能性地与UI代码隔离, 方便统一管理和后期更改
# 参数：无
# 版本：0.1
# 备注：无
# 日志：无

from PyQt5.QtCore import *
from PyQt5.QtWidgets import *
from PyQt5.QtGui import *
from MainForm.MainFormUI import Ui_MainFormUI
from Utils.Utils import LOGGER
from Protocol.Protocol import *
from json import load, dump
import sys


class MainForm(QMainWindow):
    """
    继承自QMainWindow, 使用Qt Designer设计出来的主界面代码来设置主窗口
    同时, 该类中还会详细实现一些槽函数, 响应信号, 完成用户操作.
    """

    def __init__(self):
        super(MainForm, self).__init__()

        # 设置MainForm的UI, 显示设计出的UI
        self.ui = Ui_MainFormUI()
        self.ui.setupUi(self)
        self.bindSlots()
        self.loaded = False

        # 本地客户端的信息
        self.localClient = {'name': 'Mac', 'id': -1, 'description': '这是一台用于测试的Mac电脑.', 'peered_id': -1}

        # 保存代理服务器的信息
        self.proxyServer = {'name': '阿里云代理服务器', 'host': 'wealllink.com', 'port': 9001}

        # 加载配置文件
        self.loadConfig()

        # 更新代理服务器显示信息
        self.showProxyServerInfo()

        # 更新本机显示信息
        self.showLocalClientInfo()

        # 更新状态
        self.setConnectionStatus(False)

        # 需要一个定时器 每隔一段时间执行一次 主要是发送心跳包同时更新列表
        self.timer = QTimer(self)
        self.timer.setInterval(10 * 1000)
        self.timer.timeout.connect(self.timeoutEventHandler)

    def showEvent(self, event):
        if isinstance(event, QShowEvent):
            if self.loaded:
                event.ignore()
            else:
                self.loaded = True

    def closeEvent(self, event):
        if isinstance(event, QCloseEvent):
            # 在窗口即将关闭时,保存一些关键的信息
            self.saveConfig()

    def bindSlots(self):
        """
        将一些控件的信号绑定到相关的槽函数
        :return: 无
        """
        # 按钮相关
        self.ui.pushButtonStartService.clicked.connect(self.startService)
        self.ui.pushButtonStopService.clicked.connect(self.stopService)
        self.ui.pushButtonQuit.clicked.connect(self.quitProgram)
        self.ui.pushButtonUpdateRemoteList.clicked.connect(self.updateRemoteList)
        self.ui.pushButtonControlConnection.clicked.connect(self.onControlConnectionButtonClicked)

        # 菜单相关
        self.ui.actionEditServer.triggered.connect(self.editProxyServer)
        self.ui.actionChangeServer.triggered.connect(self.changeProxyServer)
        self.ui.actionEditLocalClient.triggered.connect(self.editLocalInformation)
        self.ui.actionSaveLocalClient.triggered.connect(self.saveLocalInformation)
        self.ui.actionAboutQt5.triggered.connect(self.aboutQt)
        self.ui.actionAboutProgram.triggered.connect(self.aboutProgram)

    def loadConfig(self):
        """
        加载配置文件
        :return: 无
        """
        try:
            conf = load(open('config.json'))
            if conf and isinstance(conf, dict):
                if 'client' in conf.keys() and conf['client']:
                    self.localClient = conf['client']
                if 'server' in conf.keys() and conf['server']:
                    self.proxyServer = conf['server']
        except Exception as err:
            LOGGER.error(str(err))

    def saveConfig(self):
        """
        保存配置文件
        :return: 无
        """
        config = {'client': self.localClient, 'server': self.proxyServer}
        try:
            dump(config, open('config.json', 'w'))
        except Exception as err:
            LOGGER.error(str(err))

    def startService(self):
        """
        启动相关服务
        :return: 无
        """
        # 首先尝试与代理服务器联系, 获取一个合法分配的id
        clientId = getClientId(self.proxyServer['host'], self.proxyServer['port'])

        # 更新显示控件
        self.localClient['id'] = clientId
        self.showLocalClientInfo()

        # 根据id判断是否连接成功了
        if clientId != -1:
            self.setConnectionStatus(True)
            self.timer.start()
            # 向代理服务器上传本地客户端的信息
            clientInfo = {'name': self.ui.lineEditLocalName.text(),
                          'description': self.ui.textEditLocalDescription.toPlainText()}

            if postClientInformation(self.proxyServer['host'], self.proxyServer['port'], clientId, clientInfo):
                # 如果上传成功, 那么就告诉代理服务器, 本地客户端不接受来自远程的请求控制功能
                if postPeeredRemoteId(self.proxyServer['host'], self.proxyServer['port'], clientId, clientId):
                    # 查询一下当前有哪些客户端在线, 获取并更新在线的客户端列表
                    self.updateRemoteList()
                else:
                    LOGGER.error('未能成功设置本机为不可被控制状态, 请确保网络连接正常!')
                    self.stopService()
                    QMessageBox.warning(self, '提示信息', '未能成功设置本机为不可被控制状态, 请确保网络连接正常!')
            else:
                LOGGER.error('未能成功上传本机信息, 请确保网络连接正常!')
                self.stopService()
                QMessageBox.warning(self, '提示信息', '未能成功上传本机信息, 请确保网络连接正常!')
        else:
            LOGGER.error('未能成功获取有效的客户端ID, 请确保网络连接正常!')
            self.stopService()
            QMessageBox.warning(self, '提示信息', '未能成功获取有效的客户端ID, 请确保网络连接正常!')

    def stopService(self):
        """
        停止相关服务
        :return: 无
        """
        self.timer.stop()
        self.setConnectionStatus(False)

    def quitProgram(self):
        """
        退出应用程序
        :return: 无
        """
        self.stopService()
        qApp.quit()

    def editProxyServer(self):
        """
        弹出新的对话框, 提示用户添加代理服务器
        :return: 无
        """
        QMessageBox.warning(self, '提示信息', '抱歉, 功能尚未实现!')

    def changeProxyServer(self):
        """
        弹出ComboBox对话框, 提供让用户可选择的代理服务器项目
        :return: 无
        """
        QMessageBox.warning(self, '提示信息', '抱歉, 功能尚未实现!')

    def editLocalInformation(self):
        """
        编辑本机信息, 需要让不可编辑的控件设置为可以编辑的状态
        :return: 无
        """
        # 只有在没有启动服务的情况下才可以修改本地信息
        if self.ui.pushButtonStopService.isEnabled():
            QMessageBox.warning(self, '警告消息', '在服务已经启动的情况下禁止修改本机信息, 您可以在停止服务后修改.', QMessageBox.Ok)
        else:
            self.ui.lineEditLocalName.setReadOnly(False)
            self.ui.textEditLocalDescription.setReadOnly(False)

    def saveLocalInformation(self):
        """
        该函数除了需要将相关控件设置为只读状态外, 还要及时更新存储的信息
        :return: 无
        """
        self.ui.lineEditLocalName.setReadOnly(True)
        self.ui.textEditLocalDescription.setReadOnly(True)
        self.localClient['name'] = self.ui.lineEditLocalName.text()
        self.localClient['description'] = self.ui.textEditLocalDescription.toPlainText()

    def aboutQt(self):
        """
        弹出或者直接打开Qt 5的网站, 给用户查看相关Qt5的信息
        :return: 无
        """
        QMessageBox.warning(self, '提示信息', '抱歉, 功能尚未实现!')

    def aboutProgram(self):
        """
        弹出关于本软件的相关信息
        :return: 无
        """
        QMessageBox.warning(self, '提示信息', '抱歉, 功能尚未实现!')

    def onControlConnectionButtonClicked(self):
        """
        处理来自控制连接的按钮事件
        :return: 无
        """
        btn = self.sender()
        if isinstance(btn, QPushButton):
            if btn.text() == "连接":
                # 做连接该做的事情
                self.setStatusTip('正在准备连接, 请耐心等候...')
                if self.connectToRemote():
                    btn.setText('断开')
            else:
                # 做断开该做的事情
                if self.disconnectFromRemote():
                    btn.setText('连接')

    def connectToRemote(self):
        """
        连接到远程主机
        :return: True/False
        """
        # 检查有没有被选中的项目, 如果没有就算了
        item = self.ui.listWidgetRemoteClients.currentItem()
        if item:
            self.ui.listWidgetRemoteClients.setCurrentItem(item)
            try:
                self.localClient['peered_id'] = int((item.text().split('[')[-1]).split(']')[0])

                # 检查对方是否接收控制
                if getPeeredRemoteId(self.proxyServer['host'], self.proxyServer['port'],
                                     self.localClient['peered_id']) == self.localClient['peered_id']:
                    LOGGER.error('对方{}不接收控制请求, 无法申请控制!'.format(self.localClient['peered_id']))
                    QMessageBox.warning(self, '提示信息', '客户端{}拒绝接收控制请求, 请确保对方允许被控制!'.format(self.localClient['peered_id']))
                    return False

                # 检查对方是否被控制中, 如果是被控制中则无法申请控制
                if getIsUnderControl(self.proxyServer['host'], self.proxyServer['port'], self.localClient['peered_id']):
                    LOGGER.error('{}正在被其他人远程登录中, 无法接收新的请求!'.format(self.localClient['peered_id']))
                    QMessageBox.warning(self, '提示信息', '{}正在被其他人远程登录中, 无法接收新的请求!'.format(self.localClient['peered_id']))
                    return False

                # 把要控制的客户端的控制请求设置为True
                if postControlRequest(self.proxyServer['host'], self.proxyServer['port'], self.localClient['peered_id'], True):
                    # 告诉目标客户端主机的id
                    if postPeeredRemoteId(self.proxyServer['host'], self.proxyServer['port'], self.localClient['peered_id'],
                                          self.localClient['id']):
                        # 等待对方建立隧道
                        tryCount = 60
                        while tryCount != 0:
                            if getIsUnderControl(self.proxyServer['host'], self.proxyServer['port'],
                                                 self.localClient['peered_id']):
                                LOGGER.debug('远程客户端已经成功建立隧道, 等待连接...')
                                tunnelPort = getTunnelPort(self.proxyServer['host'], self.proxyServer['port'],
                                                           self.localClient['peered_id'])
                                if tunnelPort != -1:
                                    self.setStatusTip('已经可以连接到远程主机{}, 地址: {}:{}'.format(self.localClient['peered_id'],
                                                                                      self.proxyServer['host'], tunnelPort))
                                    LOGGER.debug('远程主机地址: {}:{}'.format(self.proxyServer['host'], tunnelPort))
                                    # QMessageBox.information(self, '提示信息', '请打开微软远程桌面软件, 输入地址: {}:{}'.
                                    #                         format(self.proxyServer['host'], tunnelPort))
                                    dialog = QInputDialog(self)
                                    dialog.setTextValue('{}:{}'.format(self.proxyServer['host'], tunnelPort))
                                    dialog.setLabelText('远程主机地址:')
                                    dialog.setOkButtonText('知道了')
                                    dialog.show()
                                    return True
                                else:
                                    LOGGER.error('获取远程主机的隧道端口号失败, 请确保网络连接正常')
                                    self.disconnectFromRemote()
                                    return False
                            QThread.sleep(2)
                            LOGGER.debug('开始新一轮的查询测试')
                            tryCount -= 1
                        QMessageBox.warning(self, '提示信息', '等待远程主机建立连接超时, 请稍等一段时间重试!')
                        return False
                    else:
                        return False
                else:
                    return False
            except Exception as err:
                LOGGER.error(str(err))
                return False
        else:
            LOGGER.error('没有被选中的项目')
            return False

    def disconnectFromRemote(self):
        """
        断开与远程计算机的连接
        :return: True/False
        """
        if self.localClient['peered_id'] != -1:
            if getIsOnline(self.proxyServer['host'], self.proxyServer['port'], self.localClient['peered_id']):
                if postIsUnderControl(self.proxyServer['host'], self.proxyServer['port'], self.localClient['peered_id'], False):
                    QMessageBox.information(self, '提示信息', '已经成功与远程客户端{}断开连接!'.format(self.localClient['peered_id']))
                    self.setStatusTip('')
                    self.localClient['peered_id'] = -1
                    return True
                else:
                    return False
            else:
                LOGGER.debug('对方已经离线')
                self.setStatusTip('')
                return True
        else:
            return False

    def setConnectionStatus(self, status):
        """
        设置连接状态
        :param status: True/False
        :return: 无
        """
        if status:
            self.ui.lineEditConnectionStatus.setText('连接正常')
            self.ui.pushButtonStartService.setEnabled(False)
            self.ui.pushButtonStopService.setEnabled(True)
            self.ui.pushButtonUpdateRemoteList.setEnabled(True)
            self.ui.pushButtonControlConnection.setEnabled(True)
        else:
            self.ui.lineEditConnectionStatus.setText('连接断开')
            self.ui.pushButtonStartService.setEnabled(True)
            self.ui.pushButtonStopService.setEnabled(False)
            self.ui.pushButtonUpdateRemoteList.setEnabled(False)
            self.ui.pushButtonControlConnection.setEnabled(False)

    def showLocalClientInfo(self):
        """
        显示本机客户端信息
        :return: 无
        """
        self.ui.lineEditLocalName.setText(self.localClient['name'])
        self.ui.lineEditClientID.setText(str(self.localClient['id']))
        self.ui.textEditLocalDescription.setPlainText(self.localClient['description'])

    def showProxyServerInfo(self):
        """
        显示服务器相关信息
        :return: 无
        """
        self.ui.lineEditServerName.setText(self.proxyServer['name'])
        self.ui.lineEditHostName.setText('{}:{}'.format(self.proxyServer['host'], self.proxyServer['port']))

    def updateRemoteList(self):
        """
        更新在线客户端列表
        :return: 无
        """
        clients = getOnlineClientList(self.proxyServer['host'], self.proxyServer['port'], self.localClient['id'])

        if isinstance(clients, dict):
            # 应当记住被选中的项目,如果有的话,那么在更新完列表后依然选中那一项
            currentItemText = None
            if self.ui.listWidgetRemoteClients.currentItem():
                currentItemText = self.ui.listWidgetRemoteClients.currentItem().text()

            self.ui.listWidgetRemoteClients.clear()
            for index, clientId in enumerate(clients.keys(), 1):
                # 更新列表显示
                self.ui.listWidgetRemoteClients.addItem('{}  {} [{}]'.format(index, clients[clientId], clientId))

            # 我们查找下在新添加的列表中有没有之前选中的项目, 如果有则选中它, 否则无需选中
            if currentItemText:
                # LOGGER.debug(currentItemText)
                result = self.ui.listWidgetRemoteClients.findItems(currentItemText, Qt.MatchExactly)

                # 如果有且仅有一项匹配才会选中它 然后滚动条追踪到选中的项目
                if len(result) == 1:
                    # LOGGER.debug('选中的项目的索引号: {}'.format(result[0].text()))
                    self.ui.listWidgetRemoteClients.setCurrentItem(result[0])
                    self.ui.listWidgetRemoteClients.scrollToItem(result[0])
        else:
            pass

    def timeoutEventHandler(self):
        """
        定时器定时到达时触发的事件, 在此处 更新keep-alive值, 同时还需要更新列表
        :param event:
        :return:
        """
        # LOGGER.debug('定时时间到达, 开始更新KeepAlive值, 同时更新列表...')
        if postKeepAliveCount(self.proxyServer['host'], self.proxyServer['port'], self.localClient['id'], 10):
            # LOGGER.debug('更新KeepAlive值为: {}'.format(10))
            # 更新列表
            self.updateRemoteList()
        else:
            LOGGER.debug('连接断开')
            self.stopService()


def main():
    app = QApplication(sys.argv)
    form = MainForm()
    form.show()
    sys.exit(app.exec_())


if __name__ == '__main__':
    main()
