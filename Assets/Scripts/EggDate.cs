using UnityEngine;

[CreateAssetMenu(
    fileName = "New EggData",
    menuName = "Game/Egg Data"
)]
public class EggData : ScriptableObject
{
    [Header("Šî–{î•ñ")]
    public string eggName;

    [Header("Prefab")]
    public GameObject eggPrefab;

    [Header("HP")]
    public float maxHP = 100f;

    [Header("ˆÚ“®«”\")]
    public float moveForce = 10f;
    public float torqueForce = 20f;
    public float maxSpeed = 15f;
    public float maxAngularSpeed = 12f;

    [Header("•¨—«”\")]
    public float mass = 1f;
    public float extraGravity = 20f;
    public float groundForce = 8f;

    [Header("p¨§Œä")]
    public float uprightStrength = 0.5f;
    public float uprightDamping = 1.5f;
}