import random
from random import randint  
import time  
from azure.iot.device import IoTHubDeviceClient, Message  
import re
from datetime import datetime
from colorama import Fore
  
CON_STR_RA1_FA1 = 'HostName=iothub-rg-az220.azure-devices.net;DeviceId=RA1-FA1;SharedAccessKey=DOvyVMOhLJuNzsBDqMDvxL3H/L9K8LbOYAIoTDXCdn4='  
CON_STR_RA2_FA1 = 'HostName=iothub-rg-az220.azure-devices.net;DeviceId=RA2-FA1;SharedAccessKey=GF54zELOuAVqgKN4nQ0fuq204iVcYz5a5AIoTMxR5CA='
CON_STR_RA3_FA1 = 'HostName=iothub-rg-az220.azure-devices.net;DeviceId=RA3-FA1;SharedAccessKey=NzjHLt7Wae9wR1X/WpWXSbQc5tQYQuvVBAIoTJ3O6c8='

DESIRED_HYDRAULIC_PRESSURE = 500
DESIRED_POWER = 380  

MSG_TXT_RA_FA  = '{{"desired_power": {desired_power},\
"desired_hydraulic_pressure": {desired_hydraulic_pressure},\
"hydraulic_pressure": {hydraulic_pressure}, "power": {power},\
"power_alert": {power_alert}, "hydraulic_pressure_alert": {hydraulic_pressure_alert},\
"overload_alert": {overload_alert}}}' 

def search_device_id(value):
    temp = str(re.findall(r'\DeviceId=.+\;',value))
    return temp[-10:-3]
  
def iothub_client_telemetry_sample_run():  
  
    try:  
        client_RA1_FA1 = IoTHubDeviceClient.create_from_connection_string(CON_STR_RA1_FA1)  
        client_RA2_FA1 = IoTHubDeviceClient.create_from_connection_string(CON_STR_RA2_FA1)
        client_RA3_FA1 = IoTHubDeviceClient.create_from_connection_string(CON_STR_RA3_FA1)
        
        while True:  
            
            power_1 = int(DESIRED_POWER+(random.random()*10))
            power_2 = int(DESIRED_POWER+(random.random()*20))
            power_3 = int(DESIRED_POWER+(random.random()*30))
            
            hydraulic_pressure_1=int(DESIRED_HYDRAULIC_PRESSURE+(random.random()*10))
            hydraulic_pressure_2=int(DESIRED_HYDRAULIC_PRESSURE+(random.random()*20))
            hydraulic_pressure_3=int(DESIRED_HYDRAULIC_PRESSURE+(random.random()*30))
            
                         
            msg_txt_ra1_fa1_formatted = MSG_TXT_RA_FA.format(hydraulic_pressure=int(DESIRED_HYDRAULIC_PRESSURE+(random.random()*20))  , 
                                                         power=int(DESIRED_POWER+(random.random()*10)),
                                                         desired_power=DESIRED_POWER,
                                                         desired_hydraulic_pressure=DESIRED_HYDRAULIC_PRESSURE,
                                                         hydraulic_pressure_alert=1 if (hydraulic_pressure_1 < DESIRED_HYDRAULIC_PRESSURE - 5) or (hydraulic_pressure_1 > DESIRED_HYDRAULIC_PRESSURE + 5) else 0,
                                                         power_alert=1 if (power_1 < DESIRED_POWER - 5) or (power_1 > DESIRED_POWER + 5) else 0,
                                                         overload_alert=0)
            
            msg_txt_ra2_fa1_formatted = MSG_TXT_RA_FA.format(hydraulic_pressure=int(DESIRED_HYDRAULIC_PRESSURE+(random.random()*40)) , 
                                                         power=int(DESIRED_POWER+(random.random()*20)),
                                                         desired_power=DESIRED_POWER,
                                                         desired_hydraulic_pressure=DESIRED_HYDRAULIC_PRESSURE,
                                                         hydraulic_pressure_alert=1 if (hydraulic_pressure_2 < DESIRED_HYDRAULIC_PRESSURE - 5) or (hydraulic_pressure_2 > DESIRED_HYDRAULIC_PRESSURE + 5) else 0,
                                                         power_alert=1 if (power_2 < DESIRED_POWER - 5) or (power_2 > DESIRED_POWER + 5) else 0,
                                                         overload_alert=1)
            
            msg_txt_ra3_fa1_formatted = MSG_TXT_RA_FA.format(hydraulic_pressure=int(DESIRED_HYDRAULIC_PRESSURE+(random.random()*60)) , 
                                                         power=int(DESIRED_POWER+(random.random()*30)),
                                                         desired_power=DESIRED_POWER,
                                                         desired_hydraulic_pressure=DESIRED_HYDRAULIC_PRESSURE,
                                                         hydraulic_pressure_alert=1 if (hydraulic_pressure_3 < DESIRED_HYDRAULIC_PRESSURE - 5) or (hydraulic_pressure_3 > DESIRED_HYDRAULIC_PRESSURE + 5) else 0,
                                                         power_alert=1 if (power_3 < DESIRED_POWER - 5) or (power_3 > DESIRED_POWER + 5) else 0,
                                                         overload_alert=1)  
            
            client_RA1_FA1.send_message(Message(msg_txt_ra1_fa1_formatted))
            client_RA2_FA1.send_message(Message(msg_txt_ra2_fa1_formatted))
            client_RA3_FA1.send_message(Message(msg_txt_ra3_fa1_formatted))

            print('\n'+Fore.BLACK+"Sent message: "+Fore.YELLOW+str(Message(msg_txt_ra1_fa1_formatted))+Fore.BLACK+" --> Digital Twin: "+Fore.RED+"{}".format(search_device_id(CON_STR_RA1_FA1)))
            print('\n'+Fore.BLACK+"Sent message: "+Fore.YELLOW+str(Message(msg_txt_ra2_fa1_formatted))+Fore.BLACK+" --> Digital Twin: "+Fore.RED+"{}".format(search_device_id(CON_STR_RA2_FA1))) 
            print('\n'+Fore.BLACK+"Sent message: "+Fore.YELLOW+str(Message(msg_txt_ra3_fa1_formatted))+Fore.BLACK+" --> Digital Twin: "+Fore.RED+"{}".format(search_device_id(CON_STR_RA3_FA1))) 
            EVENT_DATE_TIME = datetime.now().strftime('%d-%m-%Y_%H:%M:%S_%p')
            print(Fore.BLACK+'Date time of event: '+Fore.GREEN+EVENT_DATE_TIME+'\n') 
            time.sleep(4)  
  
    except KeyboardInterrupt:  
        print ("IoTHubClient sample stopped")  
        print() 
  
if __name__ == '__main__':  
    iothub_client_telemetry_sample_run()  