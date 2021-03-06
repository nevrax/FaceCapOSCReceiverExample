﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace extOSC
{
    public class FaceCapOSCReceiverGeneric : MonoBehaviour
    {
        public Transform headTransform;
        public Transform eyeLTransform;
        public Transform eyeRTransform;

        Matrix4x4 headMatrix;
        Matrix4x4 eyeLMatrix;
        Matrix4x4 eyeRMatrix;

        public SkinnedMeshRenderer blendshapesGeometry;
        int blendshapesCount = 0;

        public bool usePositionData = true;
        public bool useOrientationData = true;

        public int _OSCReceiverPort = 8080;

        private OSCReceiver _OSCReceiver;

        private const string _Position = "/HT";
        private const string _EulerAngles = "/HR";
        private const string _LeftEyeEulerAngles = "/ELR";
        private const string _RightEyeEulerAngles = "/ERR";
        private const string _Blendshapes = "/W";

        protected virtual void Start()
        {
            if (headTransform == null)
            {
                Debug.Log("Error: please assign the headTransform in the inspector.");
            }
            else
            {
                headMatrix = Matrix4x4.Rotate(headTransform.localRotation);
            }

            if (eyeLTransform == null)
            {
                Debug.Log("Error: please assign the eyeLTransform in the inspector.");
            }
            else
            {
                eyeLMatrix = Matrix4x4.Rotate(eyeLTransform.localRotation);
            }

            if (eyeRTransform == null)
            {
                Debug.Log("Error: please assign the eyeRTransform in the inspector.");
            }
            else
            {
                eyeRMatrix = Matrix4x4.Rotate(eyeRTransform.localRotation);
            }


            if (blendshapesGeometry == null)
            {
                Debug.Log("Error: please assign the skinnedMeshRenderer with blendshapes in the inspector.");
            }
            else
            {
                blendshapesCount = blendshapesGeometry.sharedMesh.blendShapeCount;

                string blendShapeNames = "Blendshape (" + blendshapesCount + ") index and name:\n";

                for (int i = 0; i < blendshapesCount; i++)
                {
                    blendShapeNames += (i + ":" + blendshapesGeometry.sharedMesh.GetBlendShapeName(i).ToString() + "\n");
                }

                Debug.Log(blendShapeNames);

            // Creating an OSC receiver.
            _OSCReceiver = gameObject.AddComponent<OSCReceiver>();

            // Set local OSC port.
            _OSCReceiver.LocalPort = _OSCReceiverPort;

            // Bind OSC Addresses.
            if (usePositionData)
            {
                _OSCReceiver.Bind(_Position, PositionReceived);
            }

            if (useOrientationData)
            {
                _OSCReceiver.Bind(_EulerAngles, EulerAnglesReceived);
            }

            _OSCReceiver.Bind(_LeftEyeEulerAngles, LeftEyeEulerAnglesReceived);
            _OSCReceiver.Bind(_RightEyeEulerAngles, RightEyeEulerAnglesReceived);

            _OSCReceiver.Bind(_Blendshapes, BlendshapeReceived);

            }
        }

        protected void PositionReceived(OSCMessage message)
        {
            Vector3 value;
            if (message.ToVector3(out value) && headTransform!=null)
            {
                value.x *= -1;
                headTransform.localPosition = value;
            }
        }

        protected void EulerAnglesReceived(OSCMessage message)
        {
            Vector3 value;
            if (message.ToVector3(out value) && headTransform != null)
            {
                Matrix4x4 inMatrix = Matrix4x4.Rotate(ConvertEulerAnglesToUnitySpace(value));
                headTransform.transform.localRotation = (inMatrix * headMatrix).rotation;
            }
        }

        protected void LeftEyeEulerAnglesReceived(OSCMessage message)
        {
            Vector2 value;
            if (message.ToVector2(out value) && eyeLTransform != null)
            {
                Matrix4x4 inMatrix = Matrix4x4.Rotate(ConvertEulerAnglesToUnitySpace(new Vector3(value.x, value.y, 0)));
                eyeLTransform.transform.localRotation = (inMatrix * eyeLMatrix).rotation;
            }
        }

        protected void RightEyeEulerAnglesReceived(OSCMessage message)
        {
            Vector2 value;
            if (message.ToVector2(out value) && eyeRTransform != null)
            {
                Matrix4x4 inMatrix = Matrix4x4.Rotate(ConvertEulerAnglesToUnitySpace(new Vector3(value.x, value.y, 0)));
                eyeRTransform.transform.localRotation = (inMatrix * eyeRMatrix).rotation;
            }
        }

        protected void BlendshapeReceived(OSCMessage message)
        {
            int index = 0; 
            float value = 0;

            if (message.ToInt(out index) && message.ToFloat(out value))
            {
                if (index >= 0 && index < blendshapesCount)
                {
                    blendshapesGeometry.SetBlendShapeWeight(index, value * 100f);
                }
            }
        }

        protected Quaternion ConvertEulerAnglesToUnitySpace(Vector3 eulerAngles)
        {
            Quaternion q = Quaternion.Euler(eulerAngles);
            return new Quaternion(-q.x, q.y, q.z, -q.w);
        }

    }

}
