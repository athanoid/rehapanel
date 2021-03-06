﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Kinect;

public class UDPServer : MonoBehaviour
{
    public GameObject BodySourceManager;
    private BodySourceManager _bodyManager;

    public GameObject UserInterface;
    private UserInterface _userInterface;

    public GameObject[] AvatarCarl;

    private float WriteFrequency = 30f;

    private KinectSensor _sensor;

    private Dictionary<JointType, String> AvatarJoint = new Dictionary<JointType, String>()
    {
        { JointType.SpineBase,       "Hips" },
        { JointType.SpineMid,        "Hips/Spine" },
        { JointType.Neck,            "Hips/Spine/Spine1/Spine2" },
        { JointType.Head,            "Hips/Spine/Spine1/Spine2/Neck/Neck1" },
        { JointType.ShoulderLeft,    "Hips/Spine/Spine1/Spine2/LeftShoulder" },
        { JointType.ElbowLeft,       "Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm" },
        { JointType.WristLeft,       "Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm" },
        { JointType.HandLeft,        "Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm/LeftHand" },
        { JointType.ShoulderRight,   "Hips/Spine/Spine1/Spine2/RightShoulder" },
        { JointType.ElbowRight,      "Hips/Spine/Spine1/Spine2/RightShoulder/RightArm" },
        { JointType.WristRight,      "Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm" },
        { JointType.HandRight,       "Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand" },
        //{ JointType.HipLeft,         "" },
        { JointType.KneeLeft,        "Hips/LeftUpLeg" },
        { JointType.AnkleLeft,       "Hips/LeftUpLeg/LeftLeg" },
        { JointType.FootLeft,        "Hips/LeftUpLeg/LeftLeg/LeftFoot" },
        //{ JointType.HipRight,        "" },
        { JointType.KneeRight,       "Hips/RightUpLeg" },
        { JointType.AnkleRight,      "Hips/RightUpLeg/RightLeg" },
        { JointType.FootRight,       "Hips/RightUpLeg/RightLeg/RightFoot" },
        //{ JointType.SpineShoulder,   "Spine1" },
        //{ JointType.HandTipLeft,     "" },
        //{ JointType.ThumbLeft,       "" },
        //{ JointType.HandTipRight,    "" },
        //{ JointType.ThumbRight,      "" },
    };

    private Dictionary<JointType, String> KinectV1Joint = new Dictionary<JointType, String>()
    {
        { JointType.SpineBase,       "waist" },
        { JointType.SpineMid,        "torso" },
        { JointType.Neck,            "neck" },
        { JointType.Head,            "head" },
        //{ JointType.ShoulderLeft,    "" },
        { JointType.ElbowLeft,       "leftshoulder" },
        { JointType.WristLeft,       "leftelbow" },
        { JointType.HandLeft,        "leftwrist" },
        //{ JointType.ShoulderRight,   "" },
        { JointType.ElbowRight,      "rightshoulder" },
        { JointType.WristRight,      "rightelbow" },
        { JointType.HandRight,       "rightwrist" },
        //{ JointType.HipLeft,         "" },
        { JointType.KneeLeft,        "lefthip" },
        { JointType.AnkleLeft,       "leftknee" },
        { JointType.FootLeft,        "leftankle" },
        //{ JointType.HipRight,        "" },
        { JointType.KneeRight,       "righthip" },
        { JointType.AnkleRight,      "rightknee" },
        { JointType.FootRight,       "rightankle" },
        //{ JointType.SpineShoulder,   "" },
        //{ JointType.HandTipLeft,     "" },
        //{ JointType.ThumbLeft,       "" },
        //{ JointType.HandTipRight,    "" },
        //{ JointType.ThumbRight,      "" },
    };

    void Start()
    {
        _userInterface = UserInterface.GetComponent<UserInterface>();
        InvokeRepeating("SendData", 0.1f, 1f / WriteFrequency);
        _sensor = KinectSensor.GetDefault();
    }

    void Update()
    {
        if (_sensor != null)
            _userInterface.ipGo = _sensor.IsOpen;
    }

    void OnApplicationQuit()
    {

    }

    void OnDisable()
    {

    }

    void SendData()
    {
        if (_userInterface.ipGo)
        {
            if (BodySourceManager == null)
            {
                return;
            }

            _bodyManager = BodySourceManager.GetComponent<BodySourceManager>();
            if (_bodyManager == null)
            {
                return;
            }

            Body[] data = _bodyManager.GetData();
            if (data == null)
            {
                return;
            }

            string message = "";

            //Finds which body is closer to the Kinect
            float[] distances = new float[6];
            int bodyindex = -1;
            foreach (var body in data)
            {
                bodyindex++;
                distances[bodyindex] = 10000;
                if (body == null) continue;
                if (!body.IsTracked) continue;
                distances[bodyindex] = body.Joints[JointType.SpineBase].Position.Z;
            }

            int closestBodyIndex = Array.IndexOf(distances, distances.Min());
            var closestBodyId = data[closestBodyIndex].TrackingId;

            bodyindex = -1;
            foreach (var body in data)
            {
                bodyindex++;
                if (body == null) continue;
                if (!body.IsTracked) continue;

                if (body.TrackingId == closestBodyId)
                {
                    foreach (JointType joint in Enum.GetValues(typeof(JointType)))
                    {
                        message = JointMensage(joint, message, "kinectdetected,", bodyindex);
                    }
                }

            }

            bodyindex = -1;
            foreach (var body in data)
            {
                bodyindex++;
                if (body == null) continue;
                if (!body.IsTracked) continue;

                // Sending the tracked body id:
                message = message + "[$]" + "tracking," + "[$$]" + "kinect," + "[$$$]" + "index," + "number," + body.TrackingId.ToString() + ";";

                foreach (JointType joint in Enum.GetValues(typeof(JointType)))
                {
                    message = JointMensage(joint, message, "kinect,", bodyindex);
                }
            }

            if (!DevicesLists.availableDev.Contains("KINECT2:TRACKING:JOINTS:ALL"))
            {
                DevicesLists.availableDev.Add("KINECT2:TRACKING:JOINTS:ALL");
            }
            if (DevicesLists.selectedDev.Contains("KINECT2:TRACKING:JOINTS:ALL") && UDPData.flag == true)
            {
                UDPData.sendString(message);
            }
        }


    }

    private string JointMensage(JointType joint, string message, string device, int bodyindex)
    {
        // Sending the tracked body joint orientation in kinect v1 format:
        if (AvatarJoint.ContainsKey(joint) && KinectV1Joint.ContainsKey(joint))
        {
            message = message + "[$]" + "tracking," + "[$$]" + device + "[$$$]";
            message = message + KinectV1Joint[joint] + ",";
            message = message + "rotation,";
            message = message + AvatarCarl[bodyindex].transform.Find(AvatarJoint[joint]).rotation.x + ",";
            message = message + AvatarCarl[bodyindex].transform.Find(AvatarJoint[joint]).rotation.y + ",";
            message = message + AvatarCarl[bodyindex].transform.Find(AvatarJoint[joint]).rotation.z + ",";
            message = message + AvatarCarl[bodyindex].transform.Find(AvatarJoint[joint]).rotation.w + ";";
        }
        // Sending the tracked body joint position in kinect v1 format:
        if (AvatarJoint.ContainsKey(joint) && KinectV1Joint.ContainsKey(joint))
        {
            message = message + "[$]" + "tracking," + "[$$]" + device + "[$$$]";
            message = message + KinectV1Joint[joint] + ",";
            message = message + "position,";
            message = message + AvatarCarl[bodyindex].transform.Find(AvatarJoint[joint]).position.x + ",";
            message = message + AvatarCarl[bodyindex].transform.Find(AvatarJoint[joint]).position.y + ",";
            message = message + AvatarCarl[bodyindex].transform.Find(AvatarJoint[joint]).position.z + ";";
        }
        return message;
    }
}
