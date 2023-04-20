import socket
import struct
import sys
import os
import numpy as np
import cv2
import time
import open3d as o3d
import pickle as pkl
import time
from PIL import Image
import io
import time

from body_from_image import *
from TCPClient import *


def tcp_server():
    serverHost = '' # localhost
    serverPort = 9090
    save_folder = '../../../examples/media/'

    if not os.path.isdir(save_folder):
        os.mkdir(save_folder)

    # Create a socket
    sSock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

    # Bind server to port
    try:
        sSock.bind((serverHost, serverPort))
        print('Server bind to port '+str(serverPort))
    except socket.error as msg:
        print('Bind failed. Error Code : ' + str(msg[0]) + ' Message ' + msg[1])
        return

    sSock.listen(10)
    print('Start listening...')
    sSock.settimeout(1000.0)
    while True:
        try:
            conn, addr = sSock.accept() # Blocking, wait for incoming connection
            break
        except KeyboardInterrupt:
            sys.exit(0)
        except Exception:
            continue

    print('Connected with ' + addr[0] + ':' + str(addr[1]))

    while True:
        # Receiving from client
            data = conn.recv(512*512*4+100)
            if len(data)==0:
                continue
            header = data[0:1].decode('utf-8')
            print('--------------------------\nHeader: ' + header)
            print(len(data))


            if header == 'x':
                file = open('data.txt', 'w')
                print(data)
                output = str(data, 'UTF-8')
                file.write(output)
                file.close()

                print('data saved to txt')

            if header == 's':
                # save depth sensor images
                data_length = struct.unpack(">i", data[1:5])[0]
                N = data_length
                depth_img_np = np.frombuffer(data[5:5+N], np.uint16).reshape((512,512))
                ab_img_np = np.frombuffer(data[5+N:5+2*N], np.uint16).reshape((512,512))
                timestamp = str(int(time.time()))
                file_name = timestamp
                cv2.imwrite(save_folder + file_name +'_depth.tiff', depth_img_np)
                cv2.imwrite(save_folder + file_name +'_abImage.jpg', ab_img_np)

                file = open('data.txt', 'w')
                file.write(depth_img_np)
                file.close()

                body_from_image.find_points(save_folder, file_name +'_abImage.jpg')
                body_from_image.find_hands(save_folder, file_name)

                print('Image with ts ' + timestamp + ' is saved')


            if header == 'f':

                # save spatial camera images
                data_length = struct.unpack(">i", data[1:5])[0]
                ts_left, ts_right = struct.unpack(">qq", data[5:21])

                N = int(data_length/2)
                LF_img_np = np.frombuffer(data[21:21+N], np.uint8).reshape((480,640))
                RF_img_np = np.frombuffer(data[21+N:21+2*N], np.uint8).reshape((480,640))
                file_name = str(ts_left)+'_LF.jpg'
                cv2.imwrite(save_folder + file_name, LF_img_np)
                cv2.imwrite(save_folder + str(ts_right)+'_RF.jpg', RF_img_np)
                print('Image with ts %d and %d is saved' % (ts_left, ts_right))

                time.sleep(1)
                body_from_image.find_points(save_folder, file_name)
                # body_from_image.find_hands(save_folder, file_name)


            if header == 'v':
                data_length = struct.unpack(">i", data[1:5])[0]
                N = int(data_length)


                # done by Matthew Sielecki
                bufferdata = np.frombuffer(data[1:5+N*3*4], dtype=np.uint32)
                image = Image.frombytes('RGBA', (424,240), bufferdata)
                # end of stuff done by Matthew Sielecki
                
                timestamp = str(int(time.time()))
                file_name = timestamp + '_PV.png'

                image.save(save_folder + file_name, encoding_errors='ignore')
                #print('Image from PV camera is saved')

                time.sleep(0.2)

                print("trying to send back")
                pointString = body_from_image.find_points(save_folder, file_name)
                TCPClient.sendPoints(pointString)
                # body_from_image.find_hands(save_folder, file_name)

            if header == 'p':
                print("Length of point cloud:")
                # save point cloud
                N_pointcloud = struct.unpack(">i", data[1:5])[0]
                print("Length of point cloud:" + str(N_pointcloud))
                pointcloud_np = np.frombuffer(data[5:5+N_pointcloud*3*4], np.float32).reshape((-1,3))
                
                timestamp = str(int(time.time()))
                temp_filename_pc = timestamp + '_pc.ply'
                print(pointcloud_np.shape)
                o3d_pc = o3d.geometry.PointCloud()

                o3d_pc.points = o3d.utility.Vector3dVector(pointcloud_np.astype(np.float64))
                o3d.io.write_point_cloud(save_folder + temp_filename_pc, o3d_pc, write_ascii=True)
                print('Saved  image to ' + temp_filename_pc)


            if header == 'm':
                images = [img for img in os.listdir(save_folder) if img.endswith(".png")]
                frame = cv2.imread(os.path.join(save_folder, images[0]))
                video_name = 'video.avi'
                height, width, layers = frame.shape

                video = cv2.VideoWriter(video_name, 0, 1, (width, height))

                for image in images:
                    video.write(cv2.imread(os.path.join(save_folder, image)))

                cv2.destroyAllWindows()
                video.release()
                # ImageToVideo.ConvertImageToVideo(video_name)
                time.sleep(2)
                body_from_image.find_points_video(save_folder, video_name)


            if header == 't':
                data_length = struct.unpack(">i", data[1:5])[0]
                N = int(data_length)

                PV_txt_np = np.frombuffer(data[1:5+N*3*4], dtype=np.uint32).reshape((424,240))
                timestamp = str(int(time.time()))
                file_name = timestamp + '_PV.txt'

                cv2.imwrite(save_folder + file_name, PV_txt_np)
                #print('Image from PV camera is saved')
    
                time.sleep(0.5)
            



if __name__ == "__main__":
    tcp_server()
