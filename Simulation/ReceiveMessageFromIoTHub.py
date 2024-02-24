import time
from azure.iot.device import IoTHubDeviceClient

RECEIVED_MESSAGES = 0

CONNECTION_STRING = "HostName=iot-az220.azure-devices.net;DeviceId=sensor-th-0055;SharedAccessKey=eWHkE6Lljf3SYm+EpZWdzUR6YkF3gZoSnAIoTE349qA="

def message_handler(message):
    global RECEIVED_MESSAGES
    RECEIVED_MESSAGES += 1
    print("")
    print("Message received:")
    # print data from both system and application (custom) properties
    for property in vars(message).items():
        print ("    {}".format(property))
    print("Total calls received: {}".format(RECEIVED_MESSAGES))
    
def main():
    # Instantiate the client
    client = IoTHubDeviceClient.create_from_connection_string(CONNECTION_STRING)

    print ("Waiting for C2D messages, press Ctrl-C to exit")
    try:
        # Attach the handler to the client
        client.on_message_received = message_handler

        while True:
            time.sleep(1000)
            
    except KeyboardInterrupt:
        print("IoT Hub C2D Messaging device sample stopped")
    finally:
        # Graceful exit
        print("Shutting down IoT Hub Client")
        client.shutdown()
        
if __name__ == '__main__':
    main()