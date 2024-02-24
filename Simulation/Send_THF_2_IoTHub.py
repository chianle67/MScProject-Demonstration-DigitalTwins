import random  
import time  
from azure.iot.device import IoTHubDeviceClient, Message  
import re
from colorama import Fore
from datetime import datetime
  
CON_STR_THF_CO1 = 'HostName=iothub-rg-az220.azure-devices.net;DeviceId=THF-CO1;SharedAccessKey=RpbYS/CWscify+xdWrQxx3kQDts3lsM+vAIoTGLlf90='  
CON_STR_THF_CO4 = 'HostName=iothub-rg-az220.azure-devices.net;DeviceId=THF-CO4;SharedAccessKey=1fUe5h6Rv3cjU/OczmTq5+0988MkI8kEJAIoTOrziEw='
CON_STR_THF_FA1 = 'HostName=iothub-rg-az220.azure-devices.net;DeviceId=THF-FA1;SharedAccessKey=5cuIyShsukvEXLLCUJP7sOvZxuHR9z96fAIoTKk09RI='
CON_STR_THF_WA1 = 'HostName=iothub-rg-az220.azure-devices.net;DeviceId=THF-WA1;SharedAccessKey=uJB1xH90W4f2CApnLPu50pocbCF0kKZEZAIoTBiOUq0='


DESIRED_TEMPERATURE = 30
DESIRED_HUMIDITY = 50

 
MSG_TXT_THF = '{{"temperature": {temperature}, "humidity": {humidity}, "desired_temperature": {desired_temperature},\
"desired_humidity": {desired_humidity}, "temperature_alert": {temperature_alert}, "humidity_alert": {humidity_alert},\
"fan_alert": {fan_alert}}}'  

def search_device_id(value):
    temp = str(re.findall(r'\DeviceId=.+\;',value))
    return temp[-10:-3]
  
def iothub_client_telemetry_run():  
    try:   
        client_THF_CO1 = IoTHubDeviceClient.create_from_connection_string(CON_STR_THF_CO1)
        client_THF_CO4 = IoTHubDeviceClient.create_from_connection_string(CON_STR_THF_CO4)
        client_THF_FA1 = IoTHubDeviceClient.create_from_connection_string(CON_STR_THF_FA1)
        client_THF_WA1 = IoTHubDeviceClient.create_from_connection_string(CON_STR_THF_WA1)
        
        while True:  
            
            temperature = DESIRED_TEMPERATURE + (random.random()*2)  
            humidity = DESIRED_HUMIDITY + (random.random()*2)  
            desired_temperature = float(DESIRED_TEMPERATURE) 
            desired_humidity = float(DESIRED_HUMIDITY)
                           
            msg_txt_formatted_thf_co1 = MSG_TXT_THF.format(temperature=round(DESIRED_TEMPERATURE + (random.random()*10),1), 
                                                         humidity=int(round(DESIRED_HUMIDITY + (random.random()*10))),
                                                         desired_temperature=desired_temperature,
                                                         desired_humidity=int(desired_humidity),
                                                         fan_alert=0,
                                                         temperature_alert=1 if (temperature < desired_temperature - 1) or (temperature > desired_temperature + 1) else 0,
                                                         humidity_alert=1 if (humidity < desired_humidity - 1) or (humidity > desired_humidity + 1) else 0)
            
            msg_txt_formatted_thf_co4 = MSG_TXT_THF.format(temperature=round(DESIRED_TEMPERATURE + (random.random()*5),1), 
                                                         humidity=int(round(DESIRED_HUMIDITY + (random.random()*5))),
                                                         desired_temperature=desired_temperature,
                                                         desired_humidity=int(desired_humidity),
                                                         fan_alert=1,
                                                         temperature_alert=1 if (temperature < desired_temperature - 1) or (temperature > desired_temperature + 1) else 0,
                                                         humidity_alert=1 if (humidity < desired_humidity - 1) or (humidity > desired_humidity + 1) else 0)
            
            msg_txt_formatted_thf_fa_wa = MSG_TXT_THF.format(temperature=round(DESIRED_TEMPERATURE + (random.random()*7),1), 
                                                         humidity=int(round(DESIRED_HUMIDITY + (random.random()*7),1)),
                                                         desired_temperature=desired_temperature,
                                                         desired_humidity=int(desired_humidity),
                                                         fan_alert=1,
                                                         temperature_alert=1 if (temperature < desired_temperature - 1) or (temperature > desired_temperature + 1) else 0,
                                                         humidity_alert=1 if (humidity < desired_humidity - 1) or (humidity > desired_humidity + 1) else 0)  
            
            
            #message_thf_co1 = Message(msg_txt_formatted_thf_co1)
            #message_thf_co4 = Message(msg_txt_formatted_thf_co4)
            #client_THF_CO1.send_message(message_thf_co1) 
            #client_THF_CO4.send_message(message_thf_co4) 
            
            message_thf_fa_wa = Message(msg_txt_formatted_thf_fa_wa)            
            #client_THF_FA1.send_message(message_thf_fa_wa) 
            client_THF_WA1.send_message(message_thf_fa_wa)

            #rint('\n'+Fore.BLACK+"Sent message: "+Fore.YELLOW+str(message_thf_co1)+Fore.BLACK+" --> Digital Twin: "+Fore.RED+"{}".format(search_device_id(CON_STR_THF_CO1)))
            #print('\n'+Fore.BLACK+"Sent message: "+Fore.YELLOW+str(message_thf_co4)+Fore.BLACK+" --> Digital Twin: "+Fore.RED+"{}".format(search_device_id(CON_STR_THF_CO4)))
            #print('\n'+Fore.BLACK+"Sent message: "+Fore.YELLOW+str(message_thf_fa_wa)+Fore.BLACK+" --> Digital Twin: "+Fore.RED+"{}".format(search_device_id(CON_STR_THF_FA1))) 
            print('\n'+Fore.BLACK+"Sent message: "+Fore.YELLOW+str(message_thf_fa_wa)+Fore.BLACK+" --> Digital Twin: "+Fore.RED+"{}".format(search_device_id(CON_STR_THF_WA1)))    
            EVENT_DATE_TIME = datetime.now().strftime('%d-%m-%Y_%H:%M:%S_%p')
            print(Fore.BLACK+'Date time of event: '+Fore.GREEN+EVENT_DATE_TIME+'\n')
              
            time.sleep(4)  
  
    except KeyboardInterrupt:  
        print("IoTHubClient stopped !")  
        print() 
  
if __name__ == '__main__':  
    iothub_client_telemetry_run()  