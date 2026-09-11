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

    private const ushort RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001;
    private const ushort RI_MOUSE_LEFT_BUTTON_UP = 0x0002;

    private const int GWLP_WNDPROC = -4;

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

        // 4
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

    private delegate IntPtr WndProcDelegate(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam
    );

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
    // 内部データ
    // =========================================================

    private class MouseInputData
    {
        public Vector2 delta;
        public bool leftDown;
        public bool leftUp;
    }

    // 物理マウスのデバイスID → P番号
    private Dictionary<IntPtr, int> deviceToPlayer =
        new Dictionary<IntPtr, int>();

    // P1～P4の入力
    private MouseInputData[] mouseInputs =
        new MouseInputData[4];

    // Raw Input用
    private IntPtr windowHandle = IntPtr.Zero;
    private IntPtr originalWndProc = IntPtr.Zero;

    private WndProcDelegate wndProcDelegate;

    private bool rawInputInitialized = false;

    // =========================================================
    // Awake
    // =========================================================

    void Awake()
    {
        for (int i = 0; i < 4; i++)
            mouseInputs[i] = new MouseInputData();

        if (playerManager == null)
            playerManager = FindFirstObjectByType<PlayerManager>();

        if (lobbyManager == null)
            lobbyManager = FindFirstObjectByType<ExhibitionLobbyManager>();

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        InitializeRawInput();
#else
    Debug.LogWarning("MultiMouseTransform : Windows版ではありません。");
#endif
    }

    // =========================================================
    // Raw Input 初期化
    // =========================================================

    private void InitializeRawInput()
    {
        windowHandle = GetActiveWindow();

        if (windowHandle == IntPtr.Zero)
        {
            Debug.LogError(
                "MultiMouseTransform : UnityのWindow Handleを取得できません。"
            );

            return;
        }

        RAWINPUTDEVICE[] devices =
            new RAWINPUTDEVICE[1];

        // Generic Desktop Controls
        devices[0].usUsagePage = 0x01;

        // Mouse
        devices[0].usUsage = 0x02;

        // アプリがバックグラウンドでも受け取れる
        devices[0].dwFlags = 0x00000100;

        devices[0].hwndTarget = windowHandle;

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
                "MultiMouseTransform : Raw Input登録失敗"
            );

            return;
        }

        // WndProcを差し替える
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
            "MultiMouseTransform : Raw Input 初期化完了"
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

    private void ProcessRawInput(IntPtr lParam)
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

            if (raw.header.dwType != RIM_TYPEMOUSE)
                return;

            IntPtr device =
                raw.header.hDevice;

            int playerIndex =
                GetPlayerIndex(device);

            if (playerIndex < 0 ||
                playerIndex >= 4)
            {
                return;
            }

            int x =
                raw.mouse.lLastX;

            int y =
                raw.mouse.lLastY;

            ushort buttons =
                raw.mouse.usButtonFlags;

            if (buttons != 0)
            {
                Debug.Log(
                    "P" +
                    (playerIndex + 1) +
                    " BUTTON FLAGS = " +
                    buttons
                );
            }

            mouseInputs[playerIndex].delta +=
                new Vector2(x, y);

            if ((buttons &
                 RI_MOUSE_LEFT_BUTTON_DOWN) != 0)
            {
                mouseInputs[playerIndex].leftDown = true;
            }

            if ((buttons &
                 RI_MOUSE_LEFT_BUTTON_UP) != 0)
            {
                mouseInputs[playerIndex].leftUp = true;
            }

            if (showRawInput)
            {
                Debug.Log(
                    "P" +
                    (playerIndex + 1) +
                    " Raw : (" +
                    x +
                    "," +
                    y +
                    ")"
                );
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // =========================================================
    // 物理マウス → P番号
    // =========================================================

    private int GetPlayerIndex(IntPtr device)
    {
        if (device == IntPtr.Zero)
        {
            return -1;
        }

        if (deviceToPlayer.TryGetValue(
                device,
                out int existingPlayer))
        {
            return existingPlayer;
        }

        // 新しい物理マウス
        if (deviceToPlayer.Count >= 4)
        {
            Debug.LogWarning(
                "5台目以降のマウスは使用できません。"
            );

            return -1;
        }

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

            bool hasClick =
                input.leftDown;

            // -------------------------------------------------
            // ロビー
            // -------------------------------------------------

            if (lobbyManager != null &&
                !lobbyManager.IsGameStarted())
            {
                if (hasMovement)
                {
                    lobbyManager.OnPlayerMouseInput(i);
                }

                if (hasClick)
                {
                    Debug.Log("P" + (i + 1) + " LEFT CLICK");

                    if (lobbyManager != null &&
                        !lobbyManager.IsGameStarted())
                    {
                        lobbyManager.OnPlayerMouseClick(i);
                    }
                }
            }

            // -------------------------------------------------
            // ゲーム中
            // -------------------------------------------------

            if (playerManager != null &&
                hasMovement)
            {
                MovePlayer(
                    i,
                    input.delta
                );
            }

            // -------------------------------------------------
            // 入力リセット
            // -------------------------------------------------

            input.delta = Vector2.zero;
            input.leftDown = false;
            input.leftUp = false;
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
        {
            return;
        }

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

        Debug.Log(
    "P" + (playerIndex + 1) +
    " Rigidbody = " +
    (rb != null ? "あり" : "NULL")
);


        if (rb == null)
        {
            Debug.LogError(
                "P" +
                (playerIndex + 1) +
                " : 卵にRigidbodyがありません。"
            );

            return;
        }

        EggPrefab eggPrefab =
            egg.GetComponent<EggPrefab>();

        if (eggPrefab == null)
        {
            Debug.LogError(
                "P" +
                (playerIndex + 1) +
                " : 卵にEggPrefabがありません。"
            );

            return;
        }

        EggData data =
            eggPrefab.eggData;

        if (data == null)
        {
            Debug.LogError(
                "P" +
                (playerIndex + 1) +
                " : EggDataがありません。"
            );

            return;
        }

        // -------------------------------------------------
        // 方向
        // 以前動いていた方式と同じく
        // ワールド座標で直接動かす
        // -------------------------------------------------

        float x =
            delta.x *
            xSensitivity;

        float z =
            delta.y *
            zSensitivity;

        if (invertX)
            x = -x;

        if (invertZ)
            z = -z;

        Vector3 move =
            new Vector3(
                x,
                0f,
                z
            );

        // -------------------------------------------------
        // 移動
        // -------------------------------------------------



        rb.AddForce(
            move *
            data.moveForce,
            ForceMode.Force
        );

        // -------------------------------------------------
        // 回転
        // -------------------------------------------------

        Vector3 torque =
            new Vector3(
                0f,
                x,
                -z
            );

        rb.AddTorque(
            torque *
            data.torqueForce,
            ForceMode.Force
        );

        // -------------------------------------------------
        // 最高速度
        // -------------------------------------------------

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

            // -------------------------------------------------
            // 重力追加
            // -------------------------------------------------

            rb.AddForce(
                Vector3.down *
                data.extraGravity,
                ForceMode.Force
            );

            // -------------------------------------------------
            // 卵をある程度起こす
            // -------------------------------------------------

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

            // -------------------------------------------------
            // 地面方向への安定化
            // -------------------------------------------------

            RaycastHit hit;

            float rayDistance = 1.5f;

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