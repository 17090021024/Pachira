using System;

namespace TrunkCommon
{
    public static class MathHepler
    {
        private static Random _Rand = new Random();
        /// <summary>
        /// 全局随机发生器
        /// </summary>
        public static Random Rand
        {
            get { return _Rand; }
            set { _Rand = value; }
        }

        /// <summary>
        /// 计算一组数的和
        /// </summary>
        public static int Sum(params int[] nums)
        {
            if (nums == null)
                return 0;

            int sum = 0;

            foreach (int item in nums)
                sum += item;

            return sum;
        }

        /// <summary>
        /// 判断在数组里
        /// </summary>
        public static bool In(this int a, params int[] b)
        {
            if (b == null || b.Length == 0)
                return false;

            foreach (int item in b)
                if (item == a)
                    return true;

            return false;
        }

        public static int Min(params int[] nums)
        {
            int min = int.MaxValue;
            foreach (int item in nums)
                if (item < min)
                    min = item;
            return min;
        }

        public static int Max(params int[] nums)
        {
            int max = int.MinValue;
            foreach (int item in nums)
                if (item > max)
                    max = item;
            return max;
        }

        public static decimal Min(params decimal[] nums)
        {
            decimal min = decimal.MaxValue;
            foreach (decimal item in nums)
                if (item < min)
                    min = item;
            return min;
        }

        public static decimal Max(params decimal[] nums)
        {
            decimal max = decimal.MinValue;
            foreach (decimal item in nums)
                if (item > max)
                    max = item;
            return max;
        }

        /// <summary>
        /// 检查是否任意一个指定位置的位值为1
        /// </summary>
        /// <param name="flag">要操作的数</param>
        /// <param name="index">位置，自低位至高位，从0开始</param>
        /// <returns></returns>
        public static bool GetAnyBitFlag(this uint flag, params int[] index)
        {
            foreach (int item in index)
                if ((flag & (uint)Math.Pow(2, item)) != 0)
                    return true;
            return false;
        }
        /// <summary>
        /// 检查是否所有指定位置的位值为1
        /// </summary>
        /// <param name="flag">要操作的数</param>
        /// <param name="index">位置，自低位至高位，从0开始</param>
        /// <returns></returns>
        public static bool GetAllBitFlag(this uint flag, params int[] index)
        {
            foreach (int item in index)
                if ((flag & (uint)Math.Pow(2, item)) == 0)
                    return false;
            return true;
        }
        /// <summary>
        /// 设置指定位值为1
        /// </summary>
        /// <param name="flag">要操作的数</param>
        /// <param name="index">位置，自低位至高位，从0开始</param>
        /// <returns></returns>
        public static uint SetBitFlag(this uint flag, params int[] index)
        {
            foreach (int item in index)
                flag |= (uint)Math.Pow(2, item);
            return flag;
        }
        /// <summary>
        /// 设置指定位值为0
        /// </summary>
        /// <param name="flag">要操作的数</param>
        /// <param name="index">位置，自低位至高位，从0开始</param>
        /// <returns></returns>
        public static uint RemoveBitFlag(this uint flag, params int[] index)
        {
            foreach (int item in index)
                flag &= ~(uint)Math.Pow(2, item);
            return flag;
        }
        /// <summary>
        /// 检查是否任意一个指定位置的位值为1
        /// </summary>
        /// <param name="flag">要操作的数</param>
        /// <param name="index">位置，自低位至高位，从0开始</param>
        /// <returns></returns>
        public static bool GetAnyBitFlag(this ulong flag, params int[] index)
        {
            foreach (int item in index)
                if ((flag & (ulong)Math.Pow(2, item)) != 0)
                    return true;
            return false;
        }
        /// <summary>
        /// 设置指定位值为1
        /// </summary>
        /// <param name="flag">要操作的数</param>
        /// <param name="index">位置，自低位至高位，从0开始</param>
        /// <returns></returns>
        public static ulong SetBitFlag(this ulong flag, params int[] index)
        {
            foreach (int item in index)
                flag |= (ulong)Math.Pow(2, item);
            return flag;
        }
        /// <summary>
        /// 设置指定位值为0
        /// </summary>
        /// <param name="flag">要操作的数</param>
        /// <param name="index">位置，自低位至高位，从0开始</param>
        /// <returns></returns>
        public static ulong RemoveBitFlag(this ulong flag, params int[] index)
        {
            foreach (int item in index)
                flag &= ~(ulong)Math.Pow(2, item);
            return flag;
        }
        /// <summary>
        /// 检查是否所有指定位置的位值为1
        /// </summary>
        /// <param name="flag">要操作的数</param>
        /// <param name="index">位置，自低位至高位，从0开始</param>
        /// <returns></returns>
        public static bool GetAllBitFlag(this ulong flag, params int[] index)
        {
            foreach (int item in index)
                if ((flag & (ulong)Math.Pow(2, item)) == 0)
                    return false;
            return true;
        }
    }
}
