using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MultiMouseTransform : MonoBehaviour
{
    // =========================================================
    // Windows Raw Input
    // =========================================================

    private const int WM_INPUT = 0x00FF;
    private const uint RID_INPUT = 0x10000003;
    private const uint RIM_TYPEMOUSE = 0;

    // ---------------------------------------------------------
    // マウスボタン
    // ---------------------------------------------------------

    private const ushort RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001;
    private const ushort RI_MOUSE_LEFT_BUTTON_UP = 0x0002;

    private const ushort RI_MOUSE_RIGHT_BUTTON_DOWN = 0x0004;
    private const ushort RI_MOUSE_RIGHT_BUTTON_UP = 0x0008;

    private const int GWLP_WNDPROC = -4;


    // =========================================================
    // Raw Input 構造体
    // =========================================================

    [StructLayout(LayoutKind.Sequential)]
    private struct RAWINPUTDEVICE
    {
        public ushort usUsagePage;
        public ushort usUsage;
        public uint dwFlags;
        public IntPtr hwndTarget;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct RAWINPUTHEADER
    {
        public uint dwType;
        public uint dwSize;
        public IntPtr hDevice;
        public IntPtr wParam;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct RAWMOUSE_BUTTONS
    {
        public ushort usButtonFlags;
        public ushort usButtonData;
    }


    [StructLayout(LayoutKind.Explicit, Size = 24)]
    private struct RAWMOUSE
    {
        // 0
        [FieldOffset(0)]
        public ushort usFlags;

        // 2
        [FieldOffset(2)]
        public ushort padding;

        // 4
        [FieldOffset(4)]
        public ushort usButtonFlags;

        // 6
        [FieldOffset(6)]
        public ushort usButtonData;

        // 8
        [FieldOffset(8)]
        public uint ulRawButtons;

        // 12
        [FieldOffset(12)]
        public int lLastX;

        // 16
        [FieldOffset(16)]
        public int lLastY;

        // 20
        [FieldOffset(20)]
        public uint ulExtraInformation;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct RAWINPUT
    {
        public RAWINPUTHEADER header;
        public RAWMOUSE mouse;
    }


    // =========================================================
    // Window Procedure
    // =========================================================

    private delegate IntPtr WndProcDelegate(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam
    );


    // =========================================================
    // Win32 API
    // =========================================================

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterRawInputDevices(
        [In] RAWINPUTDEVICE[] pRawInputDevices,
        uint uiNumDevices,
        uint cbSize
    );


    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetRawInputData(
        IntPtr hRawInput,
        uint uiCommand,
        IntPtr pData,
        ref uint pcbSize,
        uint cbSizeHeader
    );


    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();


    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(
        IntPtr lpPrevWndFunc,
        IntPtr hWnd,
        uint Msg,
        IntPtr wParam,
        IntPtr lParam
    );


    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr64(
        IntPtr hWnd,
        int nIndex,
        IntPtr dwNewLong
    );


    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern IntPtr SetWindowLong32(
        IntPtr hWnd,
        int nIndex,
        IntPtr dwNewLong
    );


    // =========================================================
    // Unity
    // =========================================================

    [Header("Player Manager")]
    public PlayerManager playerManager;


    [Header("Lobby")]
    public ExhibitionLobbyManager lobbyManager;


    [Header("入力")]
    public bool showDebugLog = true;


    [Header("移動")]
    [Tooltip("Raw Input X の感度")]
    public float xSensitivity = 0.0001f;

    [Tooltip("Raw Input Y の感度")]
    public float zSensitivity = 0.0001f;

    [Tooltip("X方向を反転")]
    public bool invertX = false;

    [Tooltip("Z方向を反転")]
    public bool invertZ = true;


    [Header("デバッグ")]
    public bool showRawInput = false;


    // =========================================================
    // 内部入力データ
    // =========================================================

    private class MouseInputData
    {
        public Vector2 delta;

        public bool leftDown;
        public bool leftUp;

        public bool rightDown;
        public bool rightUp;
    }


    // =========================================================
    // 物理マウス → P番号
    // =========================================================

    private Dictionary<IntPtr, int> deviceToPlayer =
        new Dictionary<IntPtr, int>();


    // =========================================================
    // P1～P4入力
    // =========================================================

    private MouseInputData[] mouseInputs =
        new MouseInputData[4];


    // =========================================================
    // Raw Input
    // =========================================================

    private IntPtr windowHandle = IntPtr.Zero;

    private IntPtr originalWndProc = IntPtr.Zero;

    private WndProcDelegate wndProcDelegate;

    private bool rawInputInitialized = false;


    // =========================================================
    // Awake
    // =========================================================

    void Awake()
    {
        // -----------------------------------------------------
        // 入力データ初期化
        // -----------------------------------------------------

        for (int i = 0; i < 4; i++)
        {
            mouseInputs[i] =
                new MouseInputData();
        }


        // -----------------------------------------------------
        // PlayerManager
        // -----------------------------------------------------

        if (playerManager == null)
        {
            playerManager =
                FindFirstObjectByType<PlayerManager>();
        }


        // -----------------------------------------------------
        // LobbyManager
        // -----------------------------------------------------

        if (lobbyManager == null)
        {
            lobbyManager =
                FindFirstObjectByType<ExhibitionLobbyManager>();
        }


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        InitializeRawInput();

#else

        Debug.LogWarning(
            "MultiMouseTransform : Windows版ではありません。"
        );

#endif
    }


    // =========================================================
    // Raw Input 初期化
    // =========================================================

    private void InitializeRawInput()
    {
        windowHandle =
            GetActiveWindow();


        if (windowHandle == IntPtr.Zero)
        {
            Debug.LogError(
                "MultiMouseTransform : " +
                "UnityのWindow Handleを取得できません。"
            );

            return;
        }


        RAWINPUTDEVICE[] devices =
            new RAWINPUTDEVICE[1];


        // Generic Desktop Controls
        devices[0].usUsagePage =
            0x01;


        // Mouse
        devices[0].usUsage =
            0x02;


        // アプリがバックグラウンドでも受け取れる
        devices[0].dwFlags =
            0x00000100;


        devices[0].hwndTarget =
            windowHandle;


        bool result =
            RegisterRawInputDevices(
                devices,
                1,
                (uint)Marshal.SizeOf(
                    typeof(RAWINPUTDEVICE)
                )
            );


        if (!result)
        {
            Debug.LogError(
                "MultiMouseTransform : " +
                "Raw Input登録失敗"
            );

            return;
        }


        // -----------------------------------------------------
        // WndProc差し替え
        // -----------------------------------------------------

        wndProcDelegate =
            CustomWndProc;


        IntPtr newWndProc =
            Marshal.GetFunctionPointerForDelegate(
                wndProcDelegate
            );


        if (IntPtr.Size == 8)
        {
            originalWndProc =
                SetWindowLongPtr64(
                    windowHandle,
                    GWLP_WNDPROC,
                    newWndProc
                );
        }
        else
        {
            originalWndProc =
                SetWindowLong32(
                    windowHandle,
                    GWLP_WNDPROC,
                    newWndProc
                );
        }


        rawInputInitialized = true;


        Debug.Log(
            "MultiMouseTransform : " +
            "Raw Input 初期化完了"
        );
    }


    // =========================================================
    // WndProc
    // =========================================================

    private IntPtr CustomWndProc(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam
    )
    {
        if (msg == WM_INPUT)
        {
            ProcessRawInput(lParam);
        }


        if (originalWndProc != IntPtr.Zero)
        {
            return CallWindowProc(
                originalWndProc,
                hWnd,
                msg,
                wParam,
                lParam
            );
        }


        return IntPtr.Zero;
    }


    // =========================================================
    // Raw Input解析
    // =========================================================

    private void ProcessRawInput(
        IntPtr lParam
    )
    {
        uint size = 0;


        uint headerSize =
            (uint)Marshal.SizeOf(
                typeof(RAWINPUTHEADER)
            );


        GetRawInputData(
            lParam,
            RID_INPUT,
            IntPtr.Zero,
            ref size,
            headerSize
        );


        if (size == 0)
            return;


        IntPtr buffer =
            Marshal.AllocHGlobal(
                (int)size
            );


        try
        {
            uint result =
                GetRawInputData(
                    lParam,
                    RID_INPUT,
                    buffer,
                    ref size,
                    headerSize
                );


            if (result == 0xFFFFFFFF)
                return;


            RAWINPUT raw =
                Marshal.PtrToStructure<RAWINPUT>(
                    buffer
                );


            // -------------------------------------------------
            // マウス以外は無視
            // -------------------------------------------------

            if (raw.header.dwType !=
                RIM_TYPEMOUSE)
            {
                return;
            }


            // -------------------------------------------------
            // 物理デバイス取得
            // -------------------------------------------------

            IntPtr device =
                raw.header.hDevice;


            int playerIndex =
                GetPlayerIndex(device);


            if (playerIndex < 0 ||
                playerIndex >= 4)
            {
                return;
            }


            // -------------------------------------------------
            // 移動量
            // -------------------------------------------------

            int x =
                raw.mouse.lLastX;

            int y =
                raw.mouse.lLastY;


            // -------------------------------------------------
            // ボタン
            // -------------------------------------------------

            ushort buttons =
                raw.mouse.usButtonFlags;


            if (showDebugLog &&
                buttons != 0)
            {
                Debug.Log(
                    "P" +
                    (playerIndex + 1) +
                    " BUTTON FLAGS = " +
                    buttons
                );
            }


            // -------------------------------------------------
            // 移動量を蓄積
            // -------------------------------------------------

            mouseInputs[playerIndex].delta +=
                new Vector2(
                    x,
                    y
                );


            // =================================================
            // 左クリック
            // =================================================

            if ((buttons &
                 RI_MOUSE_LEFT_BUTTON_DOWN) != 0)
            {
                mouseInputs[playerIndex].leftDown =
                    true;
            }


            if ((buttons &
                 RI_MOUSE_LEFT_BUTTON_UP) != 0)
            {
                mouseInputs[playerIndex].leftUp =
                    true;
            }


            // =================================================
            // 右クリック
            // =================================================

            if ((buttons &
                 RI_MOUSE_RIGHT_BUTTON_DOWN) != 0)
            {
                mouseInputs[playerIndex].rightDown =
                    true;
            }


            if ((buttons &
                 RI_MOUSE_RIGHT_BUTTON_UP) != 0)
            {
                mouseInputs[playerIndex].rightUp =
                    true;
            }


            // -------------------------------------------------
            // Raw Inputデバッグ
            // -------------------------------------------------

            if (showRawInput)
            {
                Debug.Log(
                    "P" +
                    (playerIndex + 1) +
                    " Raw : (" +
                    x +
                    "," +
                    y +
                    ")" +
                    " Buttons=" +
                    buttons
                );
            }
        }
        finally
        {
            Marshal.FreeHGlobal(
                buffer
            );
        }
    }


    // =========================================================
    // 物理マウス → P番号
    // =========================================================

    private int GetPlayerIndex(
        IntPtr device
    )
    {
        if (device == IntPtr.Zero)
        {
            return -1;
        }


        // -----------------------------------------------------
        // すでに登録済み
        // -----------------------------------------------------

        if (deviceToPlayer.TryGetValue(
                device,
                out int existingPlayer))
        {
            return existingPlayer;
        }


        // -----------------------------------------------------
        // 5台目以降
        // -----------------------------------------------------

        if (deviceToPlayer.Count >= 4)
        {
            Debug.LogWarning(
                "5台目以降のマウスは使用できません。"
            );

            return -1;
        }


        // -----------------------------------------------------
        // 新しいマウス
        // -----------------------------------------------------

        int newPlayer =
            deviceToPlayer.Count;


        deviceToPlayer.Add(
            device,
            newPlayer
        );


        Debug.Log(
            "物理マウス登録 : " +
            (newPlayer + 1) +
            "P"
        );


        return newPlayer;
    }


    // =========================================================
    // Update
    // =========================================================

    void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            MouseInputData input =
                mouseInputs[i];


            bool hasMovement =
                input.delta.sqrMagnitude > 0f;


            bool hasLeftClick =
                input.leftDown;


            bool hasRightClick =
                input.rightDown;


            // =================================================
            // ロビー
            // =================================================

            if (lobbyManager != null &&
                !lobbyManager.IsGameStarted())
            {
                // -------------------------------------------------
                // ★右クリック → 参加
                // -------------------------------------------------

                if (hasRightClick)
                {
                    Debug.Log(
                        "P" +
                        (i + 1) +
                        " RIGHT CLICK"
                    );


                    lobbyManager.OnPlayerMouseRightClick(
                        i
                    );
                }


                // -------------------------------------------------
                // ★左クリック → READY切り替え
                // -------------------------------------------------

                if (hasLeftClick)
                {
                    Debug.Log(
                        "P" +
                        (i + 1) +
                        " LEFT CLICK"
                    );


                    lobbyManager.OnPlayerMouseLeftClick(
                        i
                    );
                }


                // -------------------------------------------------
                // ★マウス移動では何もしない
                // -------------------------------------------------
            }


            // =================================================
            // ゲーム中
            // =================================================

            if (playerManager != null &&
                lobbyManager != null &&
                lobbyManager.IsGameStarted() &&
                hasMovement)
            {
                MovePlayer(
                    i,
                    input.delta
                );
            }


            // =================================================
            // 入力リセット
            // =================================================

            input.delta =
                Vector2.zero;

            input.leftDown =
                false;

            input.leftUp =
                false;

            input.rightDown =
                false;

            input.rightUp =
                false;
        }
    }


    // =========================================================
    // プレイヤーを動かす
    // =========================================================

    private void MovePlayer(
        int playerIndex,
        Vector2 delta
    )
    {
        if (playerManager == null)
            return;


        if (playerIndex < 0 ||
            playerIndex >= playerManager.players.Length)
            return;


        PlayerManager.PlayerData player =
            playerManager.players[playerIndex];


        if (!player.joined)
            return;


        if (player.spawnedEgg == null)
            return;


        GameObject egg =
            player.spawnedEgg;


        Rigidbody rb =
            egg.GetComponent<Rigidbody>();


        if (rb == null)
            return;


        EggPrefab eggPrefab =
            egg.GetComponent<EggPrefab>();


        if (eggPrefab == null)
            return;


        EggData data =
            eggPrefab.eggData;


        if (data == null)
            return;


        // =====================================================
        // プレイヤー専用カメラ
        // =====================================================

        Camera cam =
            player.playerCamera;


        if (cam == null)
            return;


        // =====================================================
        // カメラ基準の前・右
        // =====================================================

        Vector3 forward =
            cam.transform.forward;

        Vector3 right =
            cam.transform.right;


        // Y方向を無視
        forward.y = 0f;
        right.y = 0f;


        forward.Normalize();
        right.Normalize();


        // =====================================================
        // マウス入力
        // =====================================================

        float horizontal =
            -delta.x * xSensitivity;

        float vertical =
            -delta.y * zSensitivity;


        // -----------------------------------------------------
        // Inspectorの反転設定
        // -----------------------------------------------------

        if (invertX)
        {
            horizontal =
                -horizontal;
        }


        if (invertZ)
        {
            vertical =
                -vertical;
        }


        // =====================================================
        // カメラ基準で移動
        // =====================================================

        Vector3 move =
            right * horizontal +
            forward * vertical;


        // =====================================================
        // 力
        // =====================================================

        rb.AddForce(
            move * data.moveForce,
            ForceMode.Force
        );


        // =====================================================
        // 回転
        // =====================================================

        Vector3 torque =
            Vector3.Cross(
                Vector3.up,
                move
            );


        rb.AddTorque(
            torque * data.torqueForce,
            ForceMode.Force
        );


        // =====================================================
        // 速度制限
        // =====================================================

        rb.linearVelocity =
            Vector3.ClampMagnitude(
                rb.linearVelocity,
                data.maxSpeed
            );


        rb.angularVelocity =
            Vector3.ClampMagnitude(
                rb.angularVelocity,
                data.maxAngularSpeed
            );
    }


    // =========================================================
    // Ground / 姿勢制御
    // =========================================================

    void FixedUpdate()
    {
        if (playerManager == null)
            return;


        for (int i = 0; i < 4; i++)
        {
            if (i >= playerManager.players.Length)
                continue;


            PlayerManager.PlayerData player =
                playerManager.players[i];


            if (!player.joined)
                continue;


            if (player.spawnedEgg == null)
                continue;


            GameObject egg =
                player.spawnedEgg;


            Rigidbody rb =
                egg.GetComponent<Rigidbody>();


            EggPrefab eggPrefab =
                egg.GetComponent<EggPrefab>();


            if (rb == null ||
                eggPrefab == null ||
                eggPrefab.eggData == null)
            {
                continue;
            }


            EggData data =
                eggPrefab.eggData;


            // =================================================
            // 重力追加
            // =================================================

            rb.AddForce(
                Vector3.down *
                data.extraGravity,
                ForceMode.Force
            );


            // =================================================
            // 卵をある程度起こす
            // =================================================

            Vector3 up =
                egg.transform.up;


            Vector3 torqueAxis =
                Vector3.Cross(
                    up,
                    Vector3.up
                );


            rb.AddTorque(
                torqueAxis *
                data.uprightStrength,
                ForceMode.Force
            );


            rb.AddTorque(
                -rb.angularVelocity *
                data.uprightDamping,
                ForceMode.Force
            );


            // =================================================
            // 地面方向への安定化
            // =================================================

            RaycastHit hit;

            float rayDistance =
                1.5f;


            if (Physics.Raycast(
                    egg.transform.position,
                    Vector3.down,
                    out hit,
                    rayDistance
                ))
            {
                if (hit.collider != null)
                {
                    rb.AddForce(
                        Vector3.down *
                        data.groundForce,
                        ForceMode.Force
                    );
                }
            }
        }
    }


    // =========================================================
    // 終了処理
    // =========================================================

    void OnDestroy()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        if (windowHandle != IntPtr.Zero &&
            originalWndProc != IntPtr.Zero)
        {
            if (IntPtr.Size == 8)
            {
                SetWindowLongPtr64(
                    windowHandle,
                    GWLP_WNDPROC,
                    originalWndProc
                );
            }
            else
            {
                SetWindowLong32(
                    windowHandle,
                    GWLP_WNDPROC,
                    originalWndProc
                );
            }
        }

#endif
    }
}