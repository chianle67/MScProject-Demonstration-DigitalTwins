import random  
import time  
from azure.iot.device import IoTHubDeviceClient, Message  
from datetime import datetime
import re
from colorama import Fore
  
CON_STR_GPS_CO1 = 'HostName=iothub-rg-az220.azure-devices.net;DeviceId=GPS-CO1;SharedAccessKey=jS+ZG/oZJy2tPIVoMEgcoHC0Zl40AX3dAAIoTFXTHR8='  
CON_STR_GPS_CO4 = 'HostName=iothub-rg-az220.azure-devices.net;DeviceId=GPS-CO4;SharedAccessKey=qfD/ucbuAxxh81lGCQ+/pWskK4YuuZSpyAIoTPuT3Vs='  


DESIRED_LATITUDE = -33.86887
DESIRED_LONGITUDE = 151.20654  

MSG_TXT_GPS_CO = '{{"latitude": {latitude}, "longitude": {longitude},\
"desired_latitude": {desired_latitude}, "desired_longitude": {desired_longitude},\
"is_arrival": {is_arrival}, "date_time": {date_time}}}'

def search_device_id(value):
    temp = str(re.findall(r'\DeviceId=.+\;',value))
    return temp[-10:-3]
  
def iothub_client_telemetry_run():  
  
    try:  
        client_GPS_CO1 = IoTHubDeviceClient.create_from_connection_string(CON_STR_GPS_CO1) 
        client_GPS_CO4 = IoTHubDeviceClient.create_from_connection_string(CON_STR_GPS_CO4) 
         
        latitude = -39.86887
        longitude = 145.20654
        
        for i in range(1,5):
            desired_latitude = DESIRED_LATITUDE
            desired_longitude = DESIRED_LONGITUDE

            msg_txt_formatted = MSG_TXT_GPS_CO.format(latitude=latitude, longitude=longitude,
                                                        desired_latitude=desired_latitude, desired_longitude=desired_longitude,
                                                        date_time=datetime.now().strftime('%Y%m%d.%H%M%S'),
                                                        is_arrival=1 if (latitude == desired_latitude) and (latitude == desired_latitude) else 0)  
            
            #message_gps_co1 = Message(msg_txt_formatted)  
            message_gps_co4 = Message(msg_txt_formatted)
            #client_GPS_CO1.send_message(message_gps_co1)
            client_GPS_CO4.send_message(message_gps_co4)

            #print('\n'+Fore.BLACK+"Sent message: "+Fore.YELLOW+str(message_gps_co1)+Fore.BLACK+" --> Digital Twin: "+Fore.RED+"{}".format(search_device_id(CON_STR_GPS_CO1)))
            print('\n'+Fore.BLACK+"Sent message: "+Fore.YELLOW+str(message_gps_co4)+Fore.BLACK+"--> Digital Twin: "+Fore.RED+"{}".format(search_device_id(CON_STR_GPS_CO4))) 
            
            EVENT_DATE_TIME = datetime.now().strftime('%d-%m-%Y_%H:%M:%S_%p')
            print(Fore.BLACK+'Date time of event: '+Fore.GREEN+EVENT_DATE_TIME+'\n') 
            
            time.sleep(8)
                    
            longitude = longitude + 2
            latitude = latitude + 2
            
    except KeyboardInterrupt:  
        print ("IoTHubClient stopped !")  
        print() 
  
if __name__ == '__main__':  
    iothub_client_telemetry_run()  