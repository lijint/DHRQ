﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace Landi.FrameWorks.HardWare
{
    public class CardReader2 : HardwareBase<CardReader2, CardReader2.Status>, Landi.FrameWorks.IManagedHardware
    {
        #region CardReader2.dll
        [DllImport("CardReader2.dll")]
        protected static extern int Card_Eject();
        [DllImport("CardReader2.dll")]
        protected static extern int Card_GetStatus(out int CardPos);
        [DllImport("CardReader2.dll")]
        protected static extern int Card_Init(string port_name, int bps, int capture);
        [DllImport("CardReader2.dll")]
        protected static extern int Card_Capture();
        [DllImport("CardReader2.dll")]
        protected static extern int Card_InsertMSCard(byte[] Track1, byte[] Track2, byte[] Track3);
        [DllImport("CardReader2.dll")]
        protected static extern int Card_InsertICCard();
        [DllImport("CardReader2.dll")]
        protected static extern int Card_Close();
        [DllImport("CardReader2.dll")]
        protected static extern int Card_CancelCommand();

        [DllImport("CardReader2.dll")]
        protected static extern int Card_GetVersion(byte[] pszVersion);
        [DllImport("CardReader2.dll")]
        protected static extern int Card_CanCapture(out int pnCanCapture);
        [DllImport("CardReader2.dll")]
        protected static extern int Card_GetLastError();
        [DllImport("CardReader2.dll")]
        protected static extern int Card_ReadTrack(int nTrack, out string pszData);
        [DllImport("CardReader2.dll")]
        protected static extern int Card_WriteTrack(ref int TrackNo, ref string TrackInfo);
        [DllImport("CardReader2.dll")]
        protected static extern int Card_ReadAll(ref string pszTrack1, ref string pszTrack2, ref string pszTrack3);
        [DllImport("CardReader2.dll")]
        protected static extern int Card_InsertRFCard();
        [DllImport("CardReader2.dll")]
        protected static extern int Card_PowerUp(byte[] ubaAnswer, ref int pnLen, ref int pnChipProtocol);
        [DllImport("CardReader2.dll")]
        protected static extern int Card_PowerDown();
        [DllImport("CardReader2.dll")]
        protected static extern int Card_ChipIO(byte[] lpChipDataIn, int nChipDataInLength, byte[] lpChipDataOut, ref int nChipDataOutLength);
        [DllImport("CardReader2.dll")]
        protected static extern long GetCommHandle();

        [DllImport("CardReader2.dll")]
        protected static extern int Card_4442Act(ref int nlen, byte[] out_data);//4442卡上电
        [DllImport("CardReader2.dll")]
        protected static extern int Card_4442Deact();//4442卡下电
        [DllImport("CardReader2.dll")]
        protected static extern int Card_4442Verify(byte[] key, ref int errtime);//4442卡校验密码
        [DllImport("CardReader2.dll")]
        protected static extern int Card_4442Changekey(byte[] key);//4442卡修改密码
        [DllImport("CardReader2.dll")]
        protected static extern int Card_4442Errtime(ref int errtime);//4442卡密码错误次数
        [DllImport("CardReader2.dll")]
        protected static extern int Card_4442Write(byte address, byte len, byte[] in_data);//4442卡写卡
        [DllImport("CardReader2.dll")]
        protected static extern int Card_4442Read(byte address, byte len, byte[] out_data);//4442卡读卡
        #endregion

        public enum Status
        {
            CARD_SUCC = 0,        // 正确执行
            CARD_FAIL = 1,        // 通讯错误
            CARD_ERR_INS = 2,     // 命令处理错误,卡座返回 'N'
            CARD_WAIT = 3,        // 等待插卡
            CARD_ERR_PARAM = 4,   // 错误参数
            CARD_NOT_POWERUP = 5, // 卡未上电
            CARD_NORESPONE = 6,   // 返回的resp长度不和要求
            CARD_ERR = 7,           // 卡处理错误
        }

        public enum InitCaptureParam
        {
            CARD_INIT_RETAIN = 0,    // 初始化时不移动卡位置
            CARD_INIT_CAPTURE = 1,   // 初始化时吞卡
            CARD_INIT_EJECT = 2,     // 初始化时退卡
        }

        public enum CardStatus
        {
            CARD_POS_OUT = 0,      // 没有卡
            CARD_POS_GATE = 1,     // 卡在卡座口
            CARD_POS_INSIDE = 2,   // 卡在卡座内部
        }

        private static InitCaptureParam iniMode;
        public CardReader2()
        {
            string modeStr = ReadIniFile("InitMode");
            if (modeStr == "")
            {
                modeStr = "0";
                WriteIniFile("InitMode", modeStr);
            }
            int mode;
            if (!int.TryParse(modeStr, out mode))
                mode = 2;
            iniMode = (InitCaptureParam)mode;
        }

        #region 卡座操作
        /// <summary>
        /// 初始化卡座
        /// </summary> 
        /// <param name="port_name"></param>
        /// <param name="bps"></param>
        /// <returns></returns>
        //public static Status Open()
        //{
        //    if (!IsUse) return Status.CARD_SUCC;
        //    try
        //    {
        //        Status ret = (Status)Card_Init(Port, Bps, iniMode);
        //        if (ret != Status.CARD_SUCC)
        //        {
        //            Log.Warn("[CardReader2][Open]" + ret.ToString());
        //        }
        //        return ret;
        //    }
        //    catch (System.Exception e)
        //    {
        //        Log.Error("[CardReader2][Open]Error!\n" + e.ToString());
        //        return Status.CARD_FAIL;
        //    }
        //}

        /// <summary>
        /// 读卡状态
        /// </summary>
        /// <param name="nCardPos">端口号</param>
        /// <returns></returns>
        public static Status GetStatus(ref CardStatus cardPos)
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                int nCardPos = 0;
                Status ret = (Status)Card_GetStatus(out nCardPos);
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][GetStatus]" + ret.ToString());
                }
                else
                {
                    cardPos = (CardStatus)nCardPos;
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][GetStatus]Error!\n" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 读卡状态
        /// </summary>
        /// <returns></returns>
        public static Status ReadCardLastStatus()
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                Status ret = (Status)Card_GetLastError();
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][ReadCardLastStatus]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][ReadCardLastStatus]Error!\n" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 退卡操作
        /// </summary>        
        /// <returns></returns>
        public static Status CardOut()
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                Status ret = (Status)Card_Eject();
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][CardOut]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][CardOut]Error!\n" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 关闭卡座
        /// </summary>
        /// <returns></returns>
        //public static Status Close()
        //{
        //    if (!IsUse) return Status.CARD_SUCC;
        //    try
        //    {
        //        Status ret = (Status)Card_Close();
        //        if (ret != Status.CARD_SUCC)
        //        {
        //            Log.Warn("[CardReader2][Close]" + ret.ToString());
        //        }
        //        return ret;
        //    }
        //    catch (System.Exception e)
        //    {
        //        Log.Error("[CardReader2][Close]" + e.ToString());
        //        return Status.CARD_FAIL;
        //    }
        //}

        /// <summary>
        /// 关闭卡座
        /// </summary>
        /// <returns></returns>
        public static Status CancelCommand()
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                Status ret = (Status)Card_CancelCommand();
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][CancelCommand]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][CancelCommand]" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 写卡操作
        /// </summary>
        /// <param name="nTrackNo">磁道号</param>
        /// <param name="strTrackInfo">写入信息</param>
        /// <returns></returns>
        public static Status WriteTrack(ref int nTrackNo, ref string strTrackInfo)
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                Status ret = (Status)Card_WriteTrack(ref nTrackNo, ref strTrackInfo);
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][WriteTrack]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][WriteTrack]" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 吞卡
        /// </summary>
        /// <returns></returns>
        public static Status CardCapture()
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                Status ret = (Status)Card_Capture();
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][CardCapture]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][CardCapture]" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 插卡读卡操作
        /// </summary>        
        /// <param name="strTrack1Info">磁道1</param>
        /// <param name="strTrack2Info">磁道2</param>
        /// <param name="strTrack3Info">磁道3</param>
        /// <returns></returns>
        public static Status InsertCard(ref string strTrack1Info, ref string strTrack2Info, ref string strTrack3Info)
        {
            if (!IsUse)
            {
                strTrack1Info = GetInstance().ReadIniFile("Track1");
                strTrack2Info = GetInstance().ReadIniFile("Track2");
                strTrack3Info = GetInstance().ReadIniFile("Track3");
                System.Threading.Thread.Sleep(1000);
                return Status.CARD_SUCC;
            }
            try
            {
                Status ret = Status.CARD_SUCC;
                byte[] strTrack1 = new byte[256];
                byte[] strTrack2 = new byte[256];
                byte[] strTrack3 = new byte[256];
                ret = (Status)Card_InsertMSCard(strTrack1, strTrack2, strTrack3);
                if (ret == Status.CARD_SUCC)
                {
                    strTrack1Info = System.Text.Encoding.Default.GetString(strTrack1).Trim().Replace("\0", "");
                    strTrack2Info = System.Text.Encoding.Default.GetString(strTrack2).Trim().Replace("\0", "");
                    strTrack3Info = System.Text.Encoding.Default.GetString(strTrack3).Trim().Replace("\0", "");
                }
                else
                {
                    Log.Warn("[CardReader2][InsertCard]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][InsertCard]Error!\n" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        #endregion

        #region 4442卡操作
        /// <summary>
        /// 该函数是用线程实现的非阻塞函数，要被循环调用，直到收到CARD_SUCC返回值才是进卡成功并获取到磁道数据
        /// </summary>
        /// <returns>CARD_SUCC:	成功CARD_FAIL:失败CARD_ERR_INS	命令处理错误,卡座返回 'N'CARD_WAIT 等待插卡
        /// </returns>
        public static Status Insert_ICcard()
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                Status ret = (Status)Card_InsertICCard();
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][Insert_ICcard]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][Insert_ICcard]" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 4442卡上电
        /// </summary>
        /// <returns></returns>
        public static Status IC4442_Act()
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                int len = 0;
                byte[] data = new byte[1024]; // 满申请1K
                Status ret = (Status)Card_4442Act(ref len, data);
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][IC4442_Act]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][IC4442_Act]" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 4442卡下电
        /// </summary>
        /// <returns></returns>
        public static Status IC4442_Deact()
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                Status ret = (Status)Card_4442Deact();
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][IC4442_Deact]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][IC4442_Deact]" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 4442卡校验密码
        /// </summary>
        /// <returns></returns>
        public static Status IC4442_Verify(byte[] key)
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                int errtime = 0;
                Status ret = (Status)Card_4442Verify(key, ref errtime);
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][IC4442_Verify]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][IC4442_Verify]" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 4442卡修改密码
        /// </summary>
        /// <returns></returns>
        public static Status IC4442_Changekey(byte[] key)
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                Status ret = (Status)Card_4442Changekey(key);
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][IC4442_Changekey]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][IC4442_Changekey]" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 4442卡取密码错误次数
        /// </summary>
        /// <returns></returns>
        public static Status IC4442_Errtime(ref int errtime)
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                Status ret = (Status)Card_4442Errtime(ref errtime);
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][IC4442_Errtime]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][IC4442_Errtime]" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 4442卡读卡
        /// </summary>
        /// <returns></returns>
        public static Status IC4442_Read(byte addr, int len, byte[] data)
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                Status ret = (Status)Card_4442Read(addr, (byte)len, data);
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][IC4442_Read]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][IC4442_Read]" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        /// <summary>
        /// 4442卡写卡
        /// </summary>
        /// <returns></returns>
        public static Status IC4442_Write(byte addr, int len, byte[] data)
        {
            if (!IsUse) return Status.CARD_SUCC;
            try
            {
                Status ret = (Status)Card_4442Write(addr, (byte)len, data);
                if (ret != Status.CARD_SUCC)
                {
                    Log.Warn("[CardReader2][IC4442_Write]" + ret.ToString());
                }
                return ret;
            }
            catch (System.Exception e)
            {
                Log.Error("[CardReader2][IC4442_Write]" + e.ToString());
                return Status.CARD_FAIL;
            }
        }

        #endregion

        #region cpu卡操作
        public static Status CardPowerUp(byte[] ubaAnswer, ref int pnLen, ref int pnChipProtocol)
        {
            if (!IsUse) return Status.CARD_SUCC;
            byte[] res = new byte[500];
            Status ret = (Status)Card_PowerUp(res, ref pnLen, ref pnChipProtocol);
            byte[] resbytes = new byte[pnLen];
            Array.Copy(res, resbytes, pnLen);
            ubaAnswer = resbytes;
            return ret;
        }

        public static Status CardPowerDown()
        {
            if (!IsUse) return Status.CARD_SUCC;
            Status ret = (Status)Card_PowerDown();
            return ret;
        }

        /// <summary>
        ///获取句炳
        /// </summary>
        /// <returns></returns>
        public static long GetHandle()
        {
            if (!IsUse)
            {
                return 0;
            }

            try
            {
                return GetCommHandle();
            }
            catch (Exception err)
            {
                Log.Error("GetHandle Error");
                Log.Error("****Err.Description =" + err.Message);
            }
            return 0;
        }

        #endregion

        #region IManagedHardware
        public object Open()
        {
            if (!IsUse) return Status.CARD_SUCC;
            Status ret = (Status)Card_Init(Port, Bps, (int)iniMode);
            if (ret != Status.CARD_SUCC)
            {
                Log.Warn("[CardReader2][Open]" + ret.ToString());
            }
            return ret;
        }

        public object Close()
        {
            if (!IsUse) return Status.CARD_SUCC;
            Status ret = (Status)Card_Close();
            if (ret != Status.CARD_SUCC)
            {
                Log.Warn("[CardReader2][Close]" + ret.ToString());
            }
            return ret;
        }

        public object CheckStatus()
        {
            CardStatus status = CardStatus.CARD_POS_OUT;
            return GetStatus(ref status);
        }

        public bool MeansError(object status)
        {
            switch (AsStatus(status))
            {
                case Status.CARD_SUCC:
                case Status.CARD_WAIT:
                    return false;
                default:
                    return true;
            }
        }
        #endregion
    }
}
