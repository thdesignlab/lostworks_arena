using System.Collections.Generic;

namespace Common
{
    public static class CO
    {
        //NPCの名前
        public const string NPC_NAME = "NPC";

        //リソースフォルダ
        public const string RESOURCE_WEAPON = "Weapon/";
        public const string RESOURCE_BULLET = "Bullet/";
        public const string RESOURCE_EFFECT = "Effect/";

        //スクリーンUI
        public const string SCREEN_CANVAS = "ScreenCanvas/";
        public const string BUTTON_LEFT_ATTACK = "FireLeft";
        public const string BUTTON_RIGHT_ATTACK = "FireRight";
        public const string BUTTON_SHOULDER_ATTACK = "FireShoulder";
        public const string BUTTON_USE_SUB = "UseSub";
        public const string BUTTON_AUTO_LOCK = "AutoLock";

        //移動モーション
        public const string MOTION_RUN = "Run";
        public const string MOTION_DASH = "Dash";
        public const string MOTION_BACK = "Back";
        public const string MOTION_JUMP = "Jump";
        public const string MOTION_LANDING = "Landing";

        //攻撃モーション
        public const string MOTION_LEFT_ATTACK = "LeftAttack";
        public const string MOTION_RIGHT_ATTACK = "RightAttack";
        public const string MOTION_SHOULDER_ATTACK = "ShoulderAttack";
        public const string MOTION_CLOSE_RANGE_ATTACK = "CloseRangeAttack";
        public const string MOTION_USE_SUB = "UseSub";

        //パーツ名称
        public const string PARTS_BODY = "Body";
        public const string PARTS_GROUNDED = "Grounded";
        public const string PARTS_LEFT_HAND = "LeftHand";
        public const string PARTS_RIGHT_HAND = "RightHand";
        public const string PARTS_SHOULDER = "Shoulder";
        public const string PARTS_SUB = "Sub";

        //全部位名
        public static string[] partsNameArray = new string[]
        {
            PARTS_LEFT_HAND,
            PARTS_RIGHT_HAND,
            PARTS_SHOULDER,
            PARTS_SUB
        };

        //武器タグ
        public const string TAG_WEAPON = "Weapon";

        //弾発射口タグ
        public const string TAG_MUZZLE = "Muzzle";

        //弾の種類(タグ)
        public const string TAG_BULLET_PHYSICS = "Bullet";
        public const string TAG_BULLET_MISSILE = "Missile";
        public const string TAG_BULLET_ENERGY = "EnergyBullet";
        public const string TAG_BULLET_LASER = "Laser";

        //弾タグ全て
        public static string[] bulletTagArray = new string[]
        {
            TAG_BULLET_PHYSICS,
            TAG_BULLET_MISSILE,
            TAG_BULLET_ENERGY,
            TAG_BULLET_LASER
        };

        //物理系の弾タグ
        public static string[] physicsBulletArray = new string[]
        {
            TAG_BULLET_PHYSICS,
            TAG_BULLET_MISSILE
        };

    }

    public static class Weapon
    {
        public static Dictionary<string, string[]> weaponLineUp = new Dictionary<string, string[]>()
        {
            { "MachineGun",  new string[]{ "マシンガン", "連射するやつ"}},
        };

    }


    public static class Func
    {
        public static bool IsPhysicsBullet(string tag)
        {
            bool isPhysicsBullet = false;
            foreach (string physicsTag in Common.CO.physicsBulletArray)
            {
                if (tag == physicsTag)
                {
                    isPhysicsBullet = true;
                    break;
                }
            }
            return isPhysicsBullet;
        }
    }
}
