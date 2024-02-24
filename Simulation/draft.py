import re
#import string

CON_STR = 'HostName=iothub-rg-az220.azure-devices.net;DeviceId=THF-CO1;SharedAccessKey=RpbYS/CWscify+xdWrQxx3kQDts3lsM+vAIoTGLlf90='

def search_device_id(value):
    temp = str(re.findall(r'\DeviceId=.+\;',value))
    value = temp[-10:-3]
    return value

print(search_device_id(CON_STR))
