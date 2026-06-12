using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("旋轉軸心設定")] private Transform yawPivot;

    [Tooltip("砲管")] public Transform pitchPivot;

    [Header("旋轉靈敏度")] public float sensitivity = 200f;

    [Header("畫圈移動量")] public float normalMoveAmount = 3f;
    public float zoomedMoveAmount = 0.2f;

    [Header("上下旋轉角度限制 (Pitch)")] public float minPitch = -5f;
    public float maxPitch = 38f;

    [Header("左右旋轉角度限制 (Yaw)")] public float limitYaw = 28f;
    private float currentPitch = 0f;
    private float currentYaw = 0f;
    private bool loaded = false;

    [Header("開鏡設定 (Zoom)")] public Camera mainCamera;
    public float normalFOV = 60f;
    public float zoomedFOV = 6f;

    private bool isZoomed = false;

    [Header("後座力設定")] public float recoilForce = 20f; // 開火時砲管抬升的角度大小
    public float recoilRecoverySpeed = 30f; // 準心回穩的速度 (越大回越快)
    public float kickSpeed = 1f; // 砲管往上暴衝的速度
    private float targetRecoil = 0f; // 目標後座力
    private float currentRecoil = 0f; // 當前實際的後座力

    void Start()
    {
        yawPivot = this.transform;

        currentYaw = yawPivot.localEulerAngles.y;
        if (currentYaw > 180f) currentYaw -= 360f;

        if (pitchPivot is not null)
        {
            currentPitch = pitchPivot.localEulerAngles.x;
            if (currentPitch > 180f) currentPitch -= 360f;
        }

        GameEvent.OnWaitingLoad?.Invoke(!loaded);
        GameEvent.KinectInput += handleInput;
    }

    private void OnDisable()
    {
        GameEvent.KinectInput -= handleInput;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) handleInput('w');
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) handleInput('s');
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) handleInput('a');
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) handleInput('d');

        UpdateAim(0f, 0f);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            handleInput('f');
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            handleInput('l');
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            handleInput('z');
        }

        // 測試用按鍵
        if (Input.GetKeyDown(KeyCode.G)) GameEvent.OnGameOver?.Invoke();
        if (Input.GetKeyDown(KeyCode.P)) GameManager.Instance.StartPlaying();
        if (Input.GetKeyDown(KeyCode.R)) GameManager.Instance.RestartGame();
    }

    void handleInput(char input)
    {
        if (input == 'f')
        {
            if (loaded && GameManager.Instance.CurrentState == GameState.Playing)
            {
                Transform cameraTransform = Camera.main.transform;

                Debug.Log($"【事件觸發】呼叫軌跡如下：\n{Environment.StackTrace}");

                Debug.Log($"【事件觸發】呼叫軌跡如下：\n{Environment.StackTrace}");

                loaded = false;

                targetRecoil += recoilForce;

                GameEvent.OnWaitingLoad?.Invoke(true);
                GameEvent.OnPlayerFire?.Invoke(cameraTransform.position, cameraTransform.forward);
            }
        }

        if (input == 'l')
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                loaded = true;
                GameEvent.OnWaitingLoad?.Invoke(false);
            }
        }

        if (input == 'z')
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                ToggleZoom();
            }
        }

        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            if (input == 'w')
            {
                currentPitch += isZoomed ? zoomedMoveAmount : normalMoveAmount;
                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            }

            if (input == 's')
            {
                currentPitch -= isZoomed ? zoomedMoveAmount : normalMoveAmount;
                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            }

            if (input == 'a')
            {
                currentYaw -= isZoomed ? zoomedMoveAmount : normalMoveAmount;
                currentYaw = Mathf.Clamp(currentYaw, -limitYaw, limitYaw);
            }

            if (input == 'd')
            {
                currentYaw += isZoomed ? zoomedMoveAmount : normalMoveAmount;
                currentYaw = Mathf.Clamp(currentYaw, -limitYaw, limitYaw);
            }
        }
    }

    private void UpdateAim(float hInput, float vInput)
    {
        float deltaYaw = hInput * sensitivity * Time.deltaTime;
        float deltaPitch = vInput * sensitivity * Time.deltaTime;

        currentYaw += deltaYaw;
        currentYaw = Mathf.Clamp(currentYaw, -limitYaw, limitYaw);

        currentPitch += deltaPitch;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // --- 👇 修正後的後座力雙重緩衝核心 ---
        // 1. 目標值持續且沉重地降回 0 (模擬液壓復位)
        targetRecoil = Mathf.MoveTowards(targetRecoil, 0f, Time.deltaTime * recoilRecoverySpeed);

        // 2. 當前視角極速追上目標值 (模擬火砲暴衝與震動)
        currentRecoil = Mathf.Lerp(currentRecoil, targetRecoil, Time.deltaTime * kickSpeed);
        // ------------------------------------

        if (yawPivot is not null)
        {
            yawPivot.localRotation = Quaternion.Euler(
                yawPivot.localEulerAngles.x,
                currentYaw,
                yawPivot.localEulerAngles.z
            );
        }

        if (pitchPivot is not null)
        {
            // 將運算好的後座力疊加到 Pitch 角度上
            float finalPitch = currentPitch + currentRecoil;

            pitchPivot.localRotation = Quaternion.Euler(
                finalPitch,
                pitchPivot.localEulerAngles.y,
                pitchPivot.localEulerAngles.z
            );
        }
    }

    private void ToggleZoom()
    {
        isZoomed = !isZoomed;

        if (mainCamera is not null)
        {
            mainCamera.fieldOfView = isZoomed ? zoomedFOV : normalFOV;
        }
    }
}