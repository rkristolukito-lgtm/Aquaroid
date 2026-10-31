using UnityEngine;

public enum BulletType { Standard, Splitter, Shockwave, Explosive }

[System.Serializable]
public struct BulletData
{
    public string bulletName;
    public BulletType type;
    public GameObject bulletPrefab;
}