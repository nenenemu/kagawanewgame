using UnityEngine;

[CreateAssetMenu(
    fileName = "New EggData",
    menuName = "Game/Egg Data"
)]
public class EggData : ScriptableObject
{
    [Header("基本情報")]
    public string eggName;

    [Header("Prefab")]
    public GameObject eggPrefab;

    [Header("プレビュー用Prefab")]
    public GameObject previewPrefab;

    [Header("HP")]
    public float maxHP = 100f;

    [Header("移動性能")]
    public float moveForce = 10f;
    public float torqueForce = 20f;
    public float maxSpeed = 15f;
    public float maxAngularSpeed = 12f;

    [Header("物理性能")]
    public float mass = 1f;
    public float extraGravity = 20f;
    public float groundForce = 8f;

    [Header("姿勢制御")]
    public float uprightStrength = 0.5f;
    public float uprightDamping = 1.5f;
}