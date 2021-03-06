﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class AvatarKinectPositionControl : MonoBehaviour
{

    public Kinect.JointType jointType;
    public GameObject BodySourceManager;
    public GameObject UserInterfaceManager;
    [Range(0, 5)]
    public int BodyIndex;

    private BodySourceManager _BodyManager;
    private UserInterface _InterfaceManager;

    // Update is called once per frame
    void Update()
    {

        if (BodySourceManager == null || UserInterfaceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        _InterfaceManager = UserInterfaceManager.GetComponent<UserInterface>();
        if (_BodyManager == null || _InterfaceManager == null)
        {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        var body = data[BodyIndex];
        //List<ulong> trackedIds = new List<ulong>();
        //{
        //    if (body != null)
        //    {
        //        if (body.IsTracked)
        //        {
        //            trackedIds.Add(body.TrackingId);
        //        }
        //    }
        //}

        if (body != null)
        {
            if (body.IsTracked)
            {
                transform.localPosition = GetVector3FromJoint(body.Joints[jointType]);
            }
            else
            {
                transform.localPosition = new Vector3(0, 0, 50);
            }
        }
        else
        {
            transform.localPosition = new Vector3(0, 0, 50);
        }

    }

    private float LimitAngleDomain(float angle)
    {
        if (angle > 360)
            angle -= 360;

        if (angle < 0)
            angle += 360;

        if (angle > 360 || angle < 0)
            LimitAngleDomain(angle);

        return angle;
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(-joint.Position.X, joint.Position.Y, joint.Position.Z);
    }
}