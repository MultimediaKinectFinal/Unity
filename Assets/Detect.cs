using UnityEngine;
using Windows.Kinect;

public class KinectIntegratedGesture : MonoBehaviour
{
    private KinectSensor kinectSensor;
    private BodyFrameReader bodyFrameReader;
    private Body[] bodies = null;

    [Header("模式狀態 (唯讀)")]
    public Mode currentMode = Mode.Normal;
    public enum Mode { Normal, Zoom }

    [Header("1. 蹲下換彈設定 (Squat to Reload)")]
    [Tooltip("骨盆與膝蓋的垂直距離低於此數值，觸發換彈")]
    public float squatThresholdHeight = 0.25f;
    [Tooltip("蹲下後，距離必須回升大於此數值才算「完全站起」，允許下一次換彈")]
    public float standThresholdHeight = 0.4f;
    private bool isSquatting = false;

    [Header("2. 放大縮小模式 (Zoom Mode)")]
    public float chestAreaRadius = 0.35f;
    public float zoomExitYDrop = 0.3f;
    public float zoomInSensitivity = 0.02f;
    public float zoomOutSensitivity = 0.02f;
    public float zoomWaitTime = 0.3f;
    public float zoomCooldown = 1.0f;
    private float zoomWaitEndTime = 0f;
    private float cooldownEndTime = 0f;
    private float zoomBaselineDistance;

    [Header("3. 發射手勢設定 (Pull Back to Shoot)")]
    public float pullBackZVelocityThreshold = 0.025f;
    public float shootCooldown = 0.5f;
    private float shootCooldownEndTime = 0f;
    private float prevRightPosZ;

    [Header("4. 畫圈過濾設定 (Anti-Jitter & Rest)")]
    public float angleTriggerThreshold = 180f;
    [Range(0.05f, 1f)] public float smoothingFactor = 0.3f;
    public float leftVelocityThreshold = 0.005f;
    public float rightVelocityThreshold = 0.015f;
    public float waistRestOffset = 0.15f;

    private Vector3 smoothedLeftPos;
    private Vector3 smoothedRightPos;

    private bool isFirstTrack = true;
    private Vector2 prevLeftPosXY, prevLeftVelXY;
    private float leftAngleAccumulator = 0f;
    private Vector2 prevRightPosZY, prevRightVelZY;
    private float rightAngleAccumulator = 0f;

    void Start()
    {
        kinectSensor = KinectSensor.GetDefault();
        if (kinectSensor != null)
        {
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            if (!kinectSensor.IsOpen)
            {
                kinectSensor.Open();
            }
        }
    }

    void Update()
    {
        if (bodyFrameReader != null)
        {
            using (BodyFrame frame = bodyFrameReader.AcquireLatestFrame())
            {
                if (frame != null)
                {
                    if (bodies == null) bodies = new Body[kinectSensor.BodyFrameSource.BodyCount];
                    frame.GetAndRefreshBodyData(bodies);

                    foreach (Body body in bodies)
                    {
                        if (body.IsTracked)
                        {
                            ProcessGesture(body);
                            break;
                        }
                    }
                }
            }
        }
    }

    private void ProcessGesture(Body body)
    {
        CameraSpacePoint leftHand = body.Joints[JointType.HandLeft].Position;
        CameraSpacePoint rightHand = body.Joints[JointType.HandRight].Position;
        CameraSpacePoint chest = body.Joints[JointType.SpineMid].Position;
        CameraSpacePoint spineBase = body.Joints[JointType.SpineBase].Position;
        CameraSpacePoint kneeLeft = body.Joints[JointType.KneeLeft].Position;
        CameraSpacePoint kneeRight = body.Joints[JointType.KneeRight].Position;

        Vector3 rawLeftPos = new Vector3(leftHand.X, leftHand.Y, leftHand.Z);
        Vector3 rawRightPos = new Vector3(rightHand.X, rightHand.Y, rightHand.Z);
        Vector3 chestPos = new Vector3(chest.X, chest.Y, chest.Z);
        Vector3 spineBasePos = new Vector3(spineBase.X, spineBase.Y, spineBase.Z);
        Vector3 kneeLeftPos = new Vector3(kneeLeft.X, kneeLeft.Y, kneeLeft.Z);
        Vector3 kneeRightPos = new Vector3(kneeRight.X, kneeRight.Y, kneeRight.Z);

        if (isFirstTrack)
        {
            smoothedLeftPos = rawLeftPos;
            smoothedRightPos = rawRightPos;
            prevLeftPosXY = new Vector2(smoothedLeftPos.x, smoothedLeftPos.y);
            prevRightPosZY = new Vector2(smoothedRightPos.z, smoothedRightPos.y);
            prevRightPosZ = smoothedRightPos.z;
            isFirstTrack = false;
            return;
        }

        smoothedLeftPos = Vector3.Lerp(smoothedLeftPos, rawLeftPos, smoothingFactor);
        smoothedRightPos = Vector3.Lerp(smoothedRightPos, rawRightPos, smoothingFactor);

        // ========== 核心一：偵測蹲下與站起 (雙重閾值防抖) ==========
        float avgKneeY = (kneeLeftPos.y + kneeRightPos.y) / 2.0f;
        float pelvisToKneeDist = spineBasePos.y - avgKneeY;

        // 1. 檢查是否從蹲下狀態「完全站起」
        if (isSquatting && pelvisToKneeDist > standThresholdHeight)
        {
            isSquatting = false;
            Debug.Log("--- 已完全站直，解除換彈鎖定 ---");
        }

        // 2. 檢查是否「觸發蹲下」
        if (!isSquatting && pelvisToKneeDist < squatThresholdHeight)
        {
            Debug.Log("換彈");
            isSquatting = true; // 上鎖，直到下次站直前不會再觸發

            // 強制退出縮放模式並清空所有手勢累積
            currentMode = Mode.Normal;
            ResetAccumulators();
        }

        // 3. 如果正處於蹲下狀態 (包含正在緩慢站起來的半蹲過程)，直接鎖定不計算手勢！
        if (isSquatting)
        {
            prevLeftPosXY = new Vector2(smoothedLeftPos.x, smoothedLeftPos.y);
            prevRightPosZY = new Vector2(smoothedRightPos.z, smoothedRightPos.y);
            prevRightPosZ = smoothedRightPos.z;
            return;
        }


        // ========== 核心二：其餘手勢運算 (完全站立時才會執行到這) ==========

        CheckModeSwitch(smoothedLeftPos, smoothedRightPos, chestPos);

        if (currentMode == Mode.Zoom)
        {
            DetectZoom(smoothedLeftPos, smoothedRightPos);
        }
        else
        {
            DetectLeftHandCircle(smoothedLeftPos);

            bool hasShot = DetectRightHandPullBack(smoothedRightPos, chestPos, spineBasePos);
            bool isShootCooldown = Time.time < shootCooldownEndTime;
            bool isAtWaistArea = smoothedRightPos.y <= (spineBasePos.y + waistRestOffset);

            if (!hasShot && !isShootCooldown && !isAtWaistArea)
            {
                DetectRightHandCircle(smoothedRightPos);
            }
            else
            {
                rightAngleAccumulator = 0f;
                prevRightVelZY = Vector2.zero;
            }
        }

        prevLeftPosXY = new Vector2(smoothedLeftPos.x, smoothedLeftPos.y);
        prevRightPosZY = new Vector2(smoothedRightPos.z, smoothedRightPos.y);
        prevRightPosZ = smoothedRightPos.z;
    }

    private void CheckModeSwitch(Vector3 leftPos, Vector3 rightPos, Vector3 chestPos)
    {
        float distLeftToChest = Vector3.Distance(leftPos, chestPos);
        float distRightToChest = Vector3.Distance(rightPos, chestPos);
        float handsDist = Vector3.Distance(leftPos, rightPos);

        if (currentMode == Mode.Normal)
        {
            if (Time.time < cooldownEndTime) return;

            if (distLeftToChest < chestAreaRadius && distRightToChest < chestAreaRadius && handsDist < chestAreaRadius)
            {
                currentMode = Mode.Zoom;
                zoomWaitEndTime = Time.time + zoomWaitTime;
                zoomBaselineDistance = handsDist;
                Debug.Log($"--- 進入縮放模式 (等待 {zoomWaitTime} 秒穩定) ---");
            }
        }
        else if (currentMode == Mode.Zoom)
        {
            if (leftPos.y < chestPos.y - zoomExitYDrop ||
                rightPos.y < chestPos.y - zoomExitYDrop ||
                distLeftToChest > chestAreaRadius * 1.5f ||
                distRightToChest > chestAreaRadius * 1.5f)
            {
                currentMode = Mode.Normal;
                cooldownEndTime = Time.time + 0.5f;
                ResetAccumulators();
                Debug.Log("--- 取消縮放模式 ---");
            }
        }
    }

    private void DetectZoom(Vector3 leftPos, Vector3 rightPos)
    {
        float currentHandsDist = Vector3.Distance(leftPos, rightPos);

        if (Time.time < zoomWaitEndTime)
        {
            zoomBaselineDistance = currentHandsDist;
            return;
        }

        float deltaDist = currentHandsDist - zoomBaselineDistance;

        if (deltaDist > zoomInSensitivity)
        {
            Debug.Log("放大 (向外擴)");
            ExitZoomMode();
        }
        else if (deltaDist < -zoomOutSensitivity)
        {
            Debug.Log("縮小 (向內縮)");
            ExitZoomMode();
        }
    }

    private void ExitZoomMode()
    {
        currentMode = Mode.Normal;
        cooldownEndTime = Time.time + zoomCooldown;
        ResetAccumulators();
        Debug.Log($"--- 退出縮放模式 (冷卻 {zoomCooldown} 秒) ---");
    }

    private bool DetectRightHandPullBack(Vector3 rightPos, Vector3 chestPos, Vector3 spineBasePos)
    {
        if (rightPos.y < chestPos.y && rightPos.y > spineBasePos.y - 0.1f)
        {
            float deltaZ = rightPos.z - prevRightPosZ;

            if (deltaZ > pullBackZVelocityThreshold && Time.time > shootCooldownEndTime)
            {
                Debug.Log("發射");
                shootCooldownEndTime = Time.time + shootCooldown;
                return true;
            }
        }
        return false;
    }

    private void DetectLeftHandCircle(Vector3 leftPos)
    {
        Vector2 currentPosXY = new Vector2(leftPos.x, leftPos.y);
        Vector2 currentVelXY = currentPosXY - prevLeftPosXY;

        if (currentVelXY.magnitude > leftVelocityThreshold && prevLeftVelXY.magnitude > leftVelocityThreshold)
        {
            float angleChange = Vector2.SignedAngle(prevLeftVelXY, currentVelXY);
            leftAngleAccumulator += angleChange;

            if (leftAngleAccumulator <= -angleTriggerThreshold)
            {
                Debug.Log("左轉 (順時鐘繞圈)");
                leftAngleAccumulator = 0f;
            }
            else if (leftAngleAccumulator >= angleTriggerThreshold)
            {
                Debug.Log("右轉 (逆時鐘繞圈)");
                leftAngleAccumulator = 0f;
            }
        }

        if (currentVelXY.magnitude > leftVelocityThreshold) prevLeftVelXY = currentVelXY;
    }

    private void DetectRightHandCircle(Vector3 rightPos)
    {
        Vector2 currentPosZY = new Vector2(rightPos.z, rightPos.y);
        Vector2 currentVelZY = currentPosZY - prevRightPosZY;

        if (currentVelZY.magnitude > rightVelocityThreshold && prevRightVelZY.magnitude > rightVelocityThreshold)
        {
            float angleChange = Vector2.SignedAngle(prevRightVelZY, currentVelZY);
            rightAngleAccumulator += angleChange;

            if (rightAngleAccumulator <= -angleTriggerThreshold)
            {
                Debug.Log("抬高 (向前繞圈)");
                rightAngleAccumulator = 0f;
            }
            else if (rightAngleAccumulator >= angleTriggerThreshold)
            {
                Debug.Log("壓低 (向後繞圈)");
                rightAngleAccumulator = 0f;
            }
        }

        if (currentVelZY.magnitude > rightVelocityThreshold) prevRightVelZY = currentVelZY;
    }

    private void ResetAccumulators()
    {
        leftAngleAccumulator = 0f;
        rightAngleAccumulator = 0f;
        prevLeftVelXY = Vector2.zero;
        prevRightVelZY = Vector2.zero;
    }

    void OnDestroy()
    {
        if (bodyFrameReader != null)
        {
            bodyFrameReader.Dispose();
            bodyFrameReader = null;
        }

        if (kinectSensor != null && kinectSensor.IsOpen)
        {
            kinectSensor.Close();
            kinectSensor = null;
        }
    }
}