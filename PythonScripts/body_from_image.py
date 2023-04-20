# From Python
# It requires OpenCV installed for Python
import sys
import cv2
import os
from sys import platform
import argparse
import numpy as np
import json

class body_from_image:
    def find_points(save_folder, file_name):
        try:
            # Import Openpose (Windows/Ubuntu/OSX)
            dir_path = os.path.dirname(os.path.realpath(__file__))
            try:
                # Windows Import
                if platform == "win32":
                    # Change these variables to point to the correct folder (Release/x64 etc.)
                    sys.path.append(dir_path + '/../../python/openpose/Release')
                    os.environ['PATH']  = os.environ['PATH'] + ';' + dir_path + '/../../x64/Release;' +  dir_path + '/../../bin;'
                    os.add_dll_directory(dir_path + '/../../bin')
                    import pyopenpose as op
                else:
                    # Change these variables to point to the correct folder (Release/x64 etc.)
                    sys.path.append('../../python')
                    # If you run `make install` (default path is `/usr/local/python` for Ubuntu), you can also access the OpenPose/python module from there. This will install OpenPose and the python library at your desired installation path. Ensure that this is in your python path in order to use it.
                    # sys.path.append('/usr/local/python')
                    from openpose import pyopenpose as op
            except ImportError as e:
                print('Error: OpenPose library could not be found. Did you enable `BUILD_PYTHON` in CMake and have this Python script in the right folder?')
                raise e

            # Flags
            new_path = save_folder + file_name
            # new_path = "../../../examples/media/1679004869_PV.png"
            parser = argparse.ArgumentParser()
            parser.add_argument("--image_path", default=new_path, help="Process an image. Read all standard formats (jpg, png, bmp, etc.).")
            args = parser.parse_known_args()

            # Custom Params (refer to include/openpose/flags.hpp for more parameters)
            params = dict()
            params["model_folder"] = "../../../models/"
            params["number_people_max"] = 1

            # Add others in path?
            for i in range(0, len(args[1])):
                curr_item = args[1][i]
                if i != len(args[1])-1: next_item = args[1][i+1]
                else: next_item = "1"
                if "--" in curr_item and "--" in next_item:
                    key = curr_item.replace('-','')
                    if key not in params:  params[key] = "1"
                elif "--" in curr_item and "--" not in next_item:
                    key = curr_item.replace('-','')
                    if key not in params: params[key] = next_item

            # Construct it from system arguments
            # op.init_argv(args[1])
            # oppython = op.OpenposePython()

            # Starting OpenPose
            opWrapper = op.WrapperPython()
            opWrapper.configure(params)
            opWrapper.start()

            # Process Image
            datum = op.Datum()
            imageToProcess = cv2.imread(args[0].image_path)
            datum.cvInputData = imageToProcess
            opWrapper.emplaceAndPop(op.VectorDatum([datum]))
            return str(datum.poseKeypoints)

            # Display Image
            print("Body keypoints: \n" + str(datum.poseKeypoints))
            cv2.imshow("OpenPose 1.7.0 - Tutorial Python API", datum.cvOutputData)
            cv2.waitKey(0)
        except Exception as e:
            print(e)
            sys.exit(-1)

    def find_hands(save_folder, file_name):
        try:
            # Import Openpose (Windows/Ubuntu/OSX)
            dir_path = os.path.dirname(os.path.realpath(__file__))
            try:
                # Windows Import
                if platform == "win32":
                    # Change these variables to point to the correct folder (Release/x64 etc.)
                    sys.path.append(dir_path + '/../../python/openpose/Release');
                    os.environ['PATH']  = os.environ['PATH'] + ';' + dir_path + '/../../x64/Release;' +  dir_path + '/../../bin;'
                    os.add_dll_directory(dir_path + '/../../bin')
                    import pyopenpose as op
                else:
                    # Change these variables to point to the correct folder (Release/x64 etc.)
                    sys.path.append('../../python')
                    # If you run `make install` (default path is `/usr/local/python` for Ubuntu), you can also access the OpenPose/python module from there. This will install OpenPose and the python library at your desired installation path. Ensure that this is in your python path in order to use it.
                    # sys.path.append('/usr/local/python')
                    from openpose import pyopenpose as op
            except ImportError as e:
                print('Error: OpenPose library could not be found. Did you enable `BUILD_PYTHON` in CMake and have this Python script in the right folder?')
                raise e

            # Flags
            new_path = save_folder + file_name 
            parser = argparse.ArgumentParser()
            parser.add_argument("--image_path", default=new_path, help="Process an image. Read all standard formats (jpg, png, bmp, etc.).")
            args = parser.parse_known_args()
            
            # Custom Params (refer to include/openpose/flags.hpp for more parameters)
            params = dict()
            params["model_folder"] = "../../../models/"
            params["hand"] = True
            params["hand_detector"] = 2
            params["body"] = 0

            # Add others in path?
            for i in range(0, len(args[1])):
                curr_item = args[1][i]
                if i != len(args[1])-1: next_item = args[1][i+1]
                else: next_item = "1"
                if "--" in curr_item and "--" in next_item:
                    key = curr_item.replace('-','')
                    if key not in params:  params[key] = "1"
                elif "--" in curr_item and "--" not in next_item:
                    key = curr_item.replace('-','')
                    if key not in params: params[key] = next_item

            # Construct it from system arguments
            # op.init_argv(args[1])
            # oppython = op.OpenposePython()

            # Starting OpenPose
            opWrapper = op.WrapperPython()
            opWrapper.configure(params)
            opWrapper.start()

            # Read image and face rectangle locations
            imageToProcess = cv2.imread(args[0].image_path)
            handRectangles = [
                # Left/Right hands person 0
                [
                op.Rectangle(320.035889, 377.675049, 69.300949, 69.300949),
                op.Rectangle(0., 0., 0., 0.),
                ],
                # Left/Right hands person 1
                [
                op.Rectangle(80.155792, 407.673492, 80.812706, 80.812706),
                op.Rectangle(46.449715, 404.559753, 98.898178, 98.898178),
                ],
                # Left/Right hands person 2
                [
                op.Rectangle(185.692673, 303.112244, 157.587555, 157.587555),
                op.Rectangle(88.984360, 268.866547, 117.818230, 117.818230),
                ]
            ]

            # Create new datum
            datum = op.Datum()
            datum.cvInputData = imageToProcess
            datum.handRectangles = handRectangles

            # Process and display image
            opWrapper.emplaceAndPop(op.VectorDatum([datum]))
            print("Left hand keypoints: \n" + str(datum.handKeypoints[0]))
            print("Right hand keypoints: \n" + str(datum.handKeypoints[1]))
            cv2.imshow("OpenPose 1.7.0 - Tutorial Python API", datum.cvOutputData)
            cv2.waitKey(0)
        except Exception as e:
            print(e)
            sys.exit(-1)
    def find_points_video(save_folder, file_name):
        try:
            # Import Openpose (Windows/Ubuntu/OSX)
            dir_path = os.path.dirname(os.path.realpath(__file__))
            try:
                # Windows Import
                if platform == "win32":
                    # Change these variables to point to the correct folder (Release/x64 etc.)
                    sys.path.append(dir_path + '/../../python/openpose/Release')
                    os.environ['PATH'] = os.environ['PATH'] + ';' + dir_path + '/../../x64/Release;' + dir_path + '/../../bin;'
                    os.add_dll_directory(dir_path + '/../../bin')
                    import pyopenpose as op
                else:
                    # Change these variables to point to the correct folder (Release/x64 etc.)
                    sys.path.append('../../python');
                    # If you run `make install` (default path is `/usr/local/python` for Ubuntu), you can also access the OpenPose/python module from there. This will install OpenPose and the python library at your desired installation path. Ensure that this is in your python path in order to use it.
                    # sys.path.append('/usr/local/python')
                    from openpose import pyopenpose as op
            except ImportError as e:
                print('Error: OpenPose library could not be found. Did you enable `BUILD_PYTHON` in CMake and have this Python script in the right folder?')
                raise e

            # # Flags
            # parser = argparse.ArgumentParser()
            # parser.add_argument("--image_path", default="../../../examples/media/COCO_val2014_000000000192.jpg", help="Process an image. Read all standard formats (jpg, png, bmp, etc.).")
            # args = parser.parse_known_args()

            # Custom Params (refer to include/openpose/flags.hpp for more parameters)
            params = dict()
            params["model_folder"] = "../../../models/"

            params["hand"] = True
            params["number_people_max"] = 1
            params["disable_blending"] = False  # for black background
            # params["display"] = 0

            # # Add others in path?
            # for i in range(0, len(args[1])):
            #     curr_item = args[1][i]
            #     if i != len(args[1]) - 1:
            #         next_item = args[1][i + 1]
            #     else:
            #         next_item = "1"
            #     if "--" in curr_item and "--" in next_item:
            #         key = curr_item.replace('-', '')
            #         if key not in params:  params[key] = "1"
            #     elif "--" in curr_item and "--" not in next_item:
            #         key = curr_item.replace('-', '')
            #         if key not in params: params[key] = next_item

            # Construct it from system arguments
            # op.init_argv(args[1])
            # oppython = op.OpenposePython()

            # Starting OpenPose
            opWrapper = op.WrapperPython()
            opWrapper.configure(params)
            opWrapper.start()

            # Process Image
            datum = op.Datum()
            cap = cv2.VideoCapture("../../../examples/media/video.avi")
            fps = cap.get(cv2.CAP_PROP_FPS)
            size = (int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)), int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT)))
            framecount = cap.get(cv2.CAP_PROP_FRAME_COUNT)
            print('Total frames in this video: ' + str(framecount))
            videoWriter = cv2.VideoWriter("op720_2.avi", cv2.VideoWriter_fourcc('D', 'I', 'V', 'X'), fps, size)

            while cap.isOpened():
                hasFrame, frame = cap.read()
                if hasFrame:

                    datum.cvInputData = frame
                    opWrapper.emplaceAndPop(op.VectorDatum([datum]))
                    print(str(datum.poseKeypoints))
                    cv2.imshow("main", datum.cvOutputData)
                    videoWriter.write(datum.cvOutputData)
                    if cv2.waitKey(1) & 0xFF == ord('q'):
                        break
                else:
                    break
            cap.release()
            cv2.destroyAllWindows()

        except Exception as e:
            print(e)
            sys.exit(-1)