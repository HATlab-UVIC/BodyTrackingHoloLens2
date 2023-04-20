import os
os.environ['PYTHONASYNCIODEBUG'] = '1'
import asyncio
import logging
import cv2
import base64
import cv2
import numpy as np
import scipy
import scipy.misc
import matplotlib.pyplot as plt
from PIL import Image

class TCPClient:

    async def tcp_echo_client(data, loop):
        reader, writer = await asyncio.open_connection('169.254.63.129', 8080, loop=loop)
        print('Sending data of size: %r' % str(len(data)))

        #sending the size and data byte array
        print("Sending data: " + str(len(data)) + str(data))
        writer.write(str(len(data)).encode() + str(data).encode())
        await writer.drain()
        #print("Message: %r" %(data))
        print(len(data))
        print('Close the socket')
        writer.write_eof()
        #writer.close()

    def grab_frame(cap):
        ret, frame = cap.read()
        return frame
    
    def getFileData(name):
        with open(name, "rb") as imageFile:
            data = base64.b64encode((imageFile.read()))
            return data

    def sendData(folder, name):
        path = folder + name
        image = cv2.imread(path)
        path = path[:-4]
        path = path + '.jpg'

        try:
            cv2.imwrite(path, image, [int(cv2.IMWRITE_JPEG_QUALITY), 100])
        except Exception as e:
            print('Error')
            print(e)

        fileData = TCPClient.getFileData(path)
        loop = asyncio.new_event_loop()
        loop.run_until_complete(TCPClient.tcp_echo_client(fileData, loop))

        plt.ioff()
        loop.close()
    def sendPoints(data):
        loop = asyncio.new_event_loop()
        print(data)

        new_string = bytes(data, "utf-8")
        print(new_string)
        data = base64.b64encode((new_string))
        loop.run_until_complete(TCPClient.tcp_echo_client(data, loop))

        plt.ioff()
        loop.close()


#continually send frames
# while cv2.waitKey(1) & 0xFF != ord('q'):
#     frame = grab_frame(cap)

#     data = base64.b64encode((frame))

#     # in_file = open("./file.txt", "rb") # opening for [r]eading as [b]inary
#     # data = in_file.read()

#     loop = asyncio.get_event_loop()
#     loop.run_until_complete(tcp_echo_client(data, loop))
